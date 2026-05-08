# How It Works

## One-Way Mirroring

FileMirror implements **one-way** mirroring: changes flow **Source → Target only**.

### What's Mirrored

- File content changes
- File attributes (including name, timestamps, read-only flag)
- New file/folder creation
- File/folder deletion

### What's NOT Mirrored

- Security descriptors (ACLs)
- File permissions (except read-only flag)

## Key Concepts

### Dead Paths

When a source path is removed from the configuration, the corresponding target path becomes **"dead"**:

- No longer mirrored
- Not deleted
- Not monitored

Example:
```json
{
  "sourceMappings": [
    { "sourcePath": "C:\\source\\docs", "targetPath": "C:\\backup\\docs" }
  ]
}
```

After removing this mapping:
- `C:\source\docs` continues to exist
- `C:\backup\docs` continues to exist
- No further changes are synced

### Offline Handling

If the target is unreachable:

1. Changes are **queued** in memory
2. Queue is persisted to disk
3. When target becomes available, queued changes are processed
4. Only the **latest state** is synced (not intermediate changes)

Example timeline:

```
T=0:  Source has file.txt (size 100)
T=5:  Target offline
T=10: File modified to size 200 (queued)
T=15: File modified to size 300 (queued)
T=20: Target online → only size 300 version synced
```

### Reverting Target Changes

FileMirror actively **reverts** unauthorized changes to the target:

```
Source:      file.txt → "Hello"
Target:      file.txt → "Hello"

# Someone edits target
Target:      file.txt → "World"  ← Unauthorized

# FileMirror reverts to match source
Target:      file.txt → "Hello"  ← Reverted to match source
```

## Change Types

### Created

Source creates new file/folder → Target creates corresponding file/folder

```
Source:      /folder/newfile.txt  (created)
Target:      /folder/newfile.txt  (created)
```

### Changed

Source file modified → Target file updated

```
Source:      file.txt → "New content"  (modified)
Target:      file.txt → "New content"  (reverted to match)
```

### Deleted

Source file/folder deleted → Target file/folder deleted

```
Source:      file.txt  (deleted)
Target:      file.txt  (deleted)
```

### Renamed

Source renamed → Target recreated at new name, old version deleted

```
Source:      oldname.txt  →  newname.txt
Target:      oldname.txt  (deleted), newname.txt (created)
```

## Timing and Latency

- **Detection**: Near real-time (FileSystemWatcher)
- **Batching**: Optional, 100ms timeout to group rapid changes
- **Total latency**: Typically <1 second
- **Hot-reload**: Config changes detected every 5 seconds (configurable)

## Event Processing Flow

```
1. FileSystemWatcher detects change
   ↓
2. FileSystemWatcherWrapper raises event
   ↓
3. FileMirrorEngine.ProcessChange()
   ↓
4. Calculate target path
   ↓
5. Apply operation (copy, delete, etc.)
   ↓
6. StateStore updates tracked state
```

## Thread Safety

- **Main thread**: CLI or Service handles configuration
- **FileSystemWatcher**: Runs on separate threads
- **StateStore**: Not thread-safe (single-threaded access)

For multi-threaded scenarios, consider implementing thread-safe wrappers.

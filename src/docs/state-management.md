# State Management

## Overview

FileMirror maintains state to track:
- Which files have been mirrored
- Which paths are "dead" (no longer in config)
- Changes queued during offline periods

## State Files

### MirroredState

Tracks complete state for a source→target mapping:

```json
{
  "sourcePath": "C:\\source",
  "targetPath": "C:\\target",
  "lastSynced": "2026-05-07T10:30:00.0000000Z",
  "files": {
    "C:\\source\\file1.txt": {
      "path": "C:\\source\\file1.txt",
      "lastModified": "2026-05-07T10:00:00.0000000Z",
      "length": 1024,
      "attributes": "Archive",
      "isDead": false
    }
  }
}
```

### State Properties

| Property | Type | Description |
|----------|------|-------------|
| sourcePath | string | Source directory path |
| targetPath | string | Target directory path |
| files | Dictionary<string, FileState> | Tracked files |
| lastSynced | DateTime | Last complete sync time |

### FileState Properties

| Property | Type | Description |
|----------|------|-------------|
| path | string | Full path to file |
| lastModified | DateTime | Last write timestamp |
| length | long | File size in bytes |
| attributes | FileAttributes | File attributes enum |
| isDead | bool | Whether path is dead |

## State Store Operations

### StateStore Class

`FileMirror.Core.Storage.StateStore` manages state persistence.

#### Load State

```csharp
StateStore store = new("path/to/state.json");
MirroredState state = store.Load();
```

- Loads from JSON file
- Returns new state if file doesn't exist

#### Save State

```csharp
store.Save();
```

- Serializes state to JSON
- Overwrites existing file

#### Get File State

```csharp
FileState? fileState = store.GetFileState("C:\\source\\file.txt");
```

- Returns `null` if not tracked

#### Update File State

```csharp
FileState state = new()
{
    Path = "C:\\source\\file.txt",
    LastModified = DateTime.Now,
    Length = 1024,
    Attributes = FileAttributes.Normal
};
store.UpdateFileState("C:\\source\\file.txt", state);
```

#### Mark Path Dead

```csharp
store.MarkDead("C:\\source\\removed\\path");
```

- Sets `isDead = true` for file
- Not deleted from state

#### Get Dead Paths

```csharp
List<string> deadPaths = store.GetDeadPaths();
```

- Returns all files with `isDead = true`

## Change Queue

### ChangeQueue Class

`FileMirror.Core.Storage.ChangeQueue` manages offline changes.

#### Enqueue Change

```csharp
FileSystemEvent @event = new(FileSystemEventType.Created, "C:\\source\\file.txt");
queue.Enqueue(@event);
```

#### Dequeue Change

```csharp
FileSystemEvent? @event = queue.Dequeue();
```

- Returns `null` if empty

#### Peek (without removing)

```csharp
FileSystemEvent? @event = queue.Peek();
```

#### Queue Count

```csharp
int count = queue.Count;
```

## State Persistence

### File Location

State file is specified by application:

- CLI: In-memory only (no persistence)
- Service: Configurable path

### Format

JSON serialization using Newtonsoft.Json:

```csharp
var serializerSettings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented
};
string json = JsonConvert.SerializeObject(_state, serializerSettings);
```

### Recovery

On startup:

1. Load state from disk
2. Identify dead paths
3. Resume monitoring
4. Reconcile differences

## State Lifecycle

### Normal Operation

```
1. File created in source
   ↓
2. StateStore tracks file state
   ↓
3. File mirrored to target
   ↓
4. State updated with new file
```

### Deletion

```
1. File deleted from source
   ↓
2. File deleted from target
   ↓
3. File removed from state
```

### Path Removed from Config

```
1. Mapping removed from config
   ↓
2. Path marked dead in state
   ↓
3. File no longer tracked
   ↓
4. Target path ignored (not deleted)
```

### Offline Period

```
1. Target unreachable
   ↓
2. Changes queued in memory
   ↓
3. Queue persisted to disk
   ↓
4. Target recovers
   ↓
5. Queue processed
   ↓
6. Only latest state synced
```

## Best Practices

### State Management

- Keep state files in a safe location
- Include in backups
- Monitor disk space for large states

### Cleanup

- Remove dead paths periodically
- Archive old state files
- Delete state when uninstalling

### Security

- Restrict access to state files
- Don't store sensitive data
- Encrypt if needed for compliance

## Troubleshooting

### State File Corrupted

Error: JSON parsing error

Solution: Delete state file, FileMirror will recreate:
```bash
rm C:\path\to\state.json
```

### State Out of Sync

Symptoms: Unexplained file copies or deletions

Solution: Reset state:
1. Stop FileMirror
2. Delete state file
3. Start FileMirror
4. Full sync occurs

### Dead Paths Accumulating

Solution: Implement cleanup script:
```powershell
# Example cleanup script
$state = Get-Content state.json | ConvertFrom-Json
# Process dead paths...
```

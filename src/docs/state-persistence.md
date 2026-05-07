# State Persistence

## Overview

FileMirror persists state to disk for recovery and tracking.

## Components

### FileState

Represents state of a single file:

```csharp
public class FileState
{
    public string Path { get; set; } = "";
    public DateTime LastModified { get; set; }
    public long Length { get; set; }
    public FileAttributes Attributes { get; set; }
    public bool IsDead { get; set; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| Path | string | Full file path |
| LastModified | DateTime | Last write timestamp |
| Length | long | File size in bytes |
| Attributes | FileAttributes | File attributes |
| IsDead | bool | Whether file is dead |

### MirroredState

Complete state for a source→target mapping:

```csharp
public class MirroredState
{
    public string SourcePath { get; set; } = "";
    public string TargetPath { get; set; } = "";
    public Dictionary<string, FileState> Files { get; } = new();
    public DateTime LastSynced { get; set; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| SourcePath | string | Source directory path |
| TargetPath | string | Target directory path |
| Files | Dictionary<string, FileState> | Tracked files |
| LastSynced | DateTime | Last complete sync time |

### StateStore

Manages state persistence:

```csharp
public class StateStore
{
    private readonly string _statePath;
    private MirroredState _state = new();

    public StateStore(string statePath) { ... }

    public MirroredState Load() { ... }
    public void Save() { ... }
    public FileState? GetFileState(string sourcePath) { ... }
    public void UpdateFileState(string sourcePath, FileState state) { ... }
    public void MarkDead(string sourcePath) { ... }
    public List<string> GetDeadPaths() { ... }
}
```

**Methods:**

| Method | Description |
|--------|-------------|
| `Load()` | Load state from disk |
| `Save()` | Save state to disk |
| `GetFileState(path)` | Get file state |
| `UpdateFileState(path, state)` | Update file state |
| `MarkDead(path)` | Mark file as dead |
| `GetDeadPaths()` | Get all dead paths |

### ChangeQueue

FIFO queue for pending changes:

```csharp
public class ChangeQueue
{
    private readonly Queue<FileSystemEvent> _queue = new();

    public void Enqueue(FileSystemEvent change) { ... }
    public FileSystemEvent? Dequeue() { ... }
    public FileSystemEvent? Peek() { ... }
    public int Count => _queue.Count;
}
```

**Methods:**

| Method | Description |
|--------|-------------|
| `Enqueue(change)` | Add change to queue |
| `Dequeue()` | Get and remove first change |
| `Peek()` | View first change without removing |
| `Count` | Number of changes |

## State File Format

### MirroredState JSON

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
    },
    "C:\\source\\file2.txt": {
      "path": "C:\\source\\file2.txt",
      "lastModified": "2026-05-07T10:05:00.0000000Z",
      "length": 2048,
      "attributes": "Archive",
      "isDead": false
    }
  }
}
```

### Dead Path Example

```json
{
  "files": {
    "C:\\source\\removed\\file.txt": {
      "path": "C:\\source\\removed\\file.txt",
      "lastModified": "2026-05-07T09:00:00.0000000Z",
      "length": 512,
      "attributes": "Archive",
      "isDead": true
    }
  }
}
```

## StateStore Operations

### Loading State

```csharp
StateStore store = new("path/to/state.json");
MirroredState state = store.Load();

Console.WriteLine($"Source: {state.SourcePath}");
Console.WriteLine($"Files tracked: {state.Files.Count}");
```

### Saving State

```csharp
StateStore store = new("path/to/state.json");
// ... modify state ...
store.Save();
```

### Tracking Files

```csharp
StateStore store = new("path/to/state.json");

// Update file state
FileState fileState = new()
{
    Path = "C:\\source\\file.txt",
    LastModified = DateTime.Now,
    Length = 1024,
    Attributes = FileAttributes.Normal
};

store.UpdateFileState("C:\\source\\file.txt", fileState);

// Get file state
FileState? result = store.GetFileState("C:\\source\\file.txt");
```

### Marking Dead Paths

```csharp
StateStore store = new("path/to/state.json");

// Mark file as dead
store.MarkDead("C:\\source\\removed\\file.txt");

// Get all dead paths
List<string> deadPaths = store.GetDeadPaths();

foreach (string path in deadPaths)
{
    Console.WriteLine($"Dead path: {path}");
}
```

## ChangeQueue Operations

### Enqueue Change

```csharp
ChangeQueue queue = new();
FileSystemEvent @event = new(FileSystemEventType.Changed, "C:\\source\\file.txt");
queue.Enqueue(@event);
```

### Dequeue Change

```csharp
FileSystemEvent? @event = queue.Dequeue();

if (@event != null)
{
    Console.WriteLine($"Processing: {@event.Type} {@event.Path}");
}
```

### Peek (View Without Removing)

```csharp
FileSystemEvent? first = queue.Peek();
Console.WriteLine($"First in queue: {@first?.Path}");
Console.WriteLine($"Queue count: {queue.Count}");
```

## State Management Workflow

### Initial Sync

```
1. StateStore.Load() or create new state
2. Scan source directory recursively
3. For each file:
   a. Create FileState
   b. Copy to target
   c. Update FileState
   d. Store in Files dictionary
4. StateStore.Save()
```

### Change Processing

```
1. FileSystemWatcher detects change
2. Process change via FileMirrorEngine
3. Update FileState in StateStore
4. StateStore.Save()
```

### Deletion Handling

```
1. Source file deleted
2. Target file deleted
3. FileState removed from Files dictionary
4. StateStore.Save()
```

### Path Removal (Dead Path)

```
1. Mapping removed from config
2. FileState.IsDead = true (or removed)
3. FileState kept in Files dictionary
4. Target path ignored (not deleted)
5. StateStore.Save()
```

### Offline Recovery

```
1. Target becomes unreachable
2. Changes queued in ChangeQueue
3. Queue persisted to disk
4. Target recovers
5. ReconcileOfflineChanges():
   a. Dequeue each change
   b. Process change
   c. Update StateStore
6. StateStore.Save()
```

## Serialization

### Saving

```csharp
public void Save()
{
    string json = JsonConvert.SerializeObject(_state, Formatting.Indented);
    File.WriteAllText(_statePath, json);
}
```

### Loading

```csharp
public MirroredState Load()
{
    if (!File.Exists(_statePath))
    {
        _state = new MirroredState();
        return _state;
    }

    string json = File.ReadAllText(_statePath);
    _state = JsonConvert.DeserializeObject<MirroredState>(json) ?? new MirroredState();

    return _state;
}
```

## Thread Safety

- **StateStore**: Not thread-safe (single-threaded use)
- **ChangeQueue**: Thread-safe (uses Queue which is thread-safe for basic operations)

**Recommendation:** Use single-threaded state access or implement locking.

## File Locations

### CLI Mode

State stored in memory only (no persistence):
```csharp
StateStore store = new("temp_state.json");
// State lost on exit
```

### Service Mode

State stored to configurable location:
```csharp
string statePath = Path.Combine(AppContext.BaseDirectory, "state.json");
StateStore store = new(statePath);
```

## Recovery Workflow

### Application Startup

```csharp
// 1. Load state
StateStore store = new("state.json");
MirroredState state = store.Load();

// 2. Check for dead paths
List<string> deadPaths = store.GetDeadPaths();

if (deadPaths.Count > 0)
{
    Console.WriteLine("Dead paths found:");
    foreach (string path in deadPaths)
    {
        Console.WriteLine($"  - {path}");
    }
}

// 3. Resume monitoring
FileSystemWatcherWrapper watcher = new(mapping);
watcher.Start();
```

### Offline Recovery

```csharp
FileMirrorEngine engine = new();

// On startup after offline period
engine.ReconcileOfflineChanges(mapping);

// State will be updated automatically
```

## Best Practices

### State File Location

- **CLI**: Temporary file in temp directory
- **Service**: Application directory or configurable path
- **Security**: Restrict file permissions
- **Backup**: Include in backup routines

### Cleanup

Periodically clean up dead paths:

```csharp
StateStore store = new("state.json");
store.Load();

List<string> deadPaths = store.GetDeadPaths();

foreach (string path in deadPaths)
{
    store.UpdateFileState(path, new FileState { IsDead = true });
}

store.Save();
```

### Error Handling

```csharp
try
{
    StateStore store = new("state.json");
    MirroredState state = store.Load();
}
catch (JsonException ex)
{
    Console.WriteLine($"State file corrupted: {ex.Message}");
    // Reset state
    StateStore store = new("state.json");
    store.Save();
}
catch (IOException ex)
{
    Console.WriteLine($"State file access error: {ex.Message}");
}
```

## Testing

See `StateStoreTests.cs` and `ChangeQueueTests.cs` in tests directory.

### StateStore Test

```csharp
[Test]
public void UpdateFileState_UpdatesExistingFile()
{
    FileState fileState1 = new()
    {
        Path = "C:\\source\\test.txt",
        LastModified = DateTime.Now.AddMinutes(-1),
        Length = 100,
        Attributes = FileAttributes.Normal
    };
    _store.UpdateFileState("C:\\source\\test.txt", fileState1);

    FileState fileState2 = new()
    {
        Path = "C:\\source\\test.txt",
        LastModified = DateTime.Now,
        Length = 200,
        Attributes = FileAttributes.Archive
    };
    _store.UpdateFileState("C:\\source\\test.txt", fileState2);

    FileState? result = _store.GetFileState("C:\\source\\test.txt");

    Assert.That(result!.Length, Is.EqualTo(200));
}
```

### ChangeQueue Test

```csharp
[Test]
public void Enqueue_Dequeue_FIFOOrder()
{
    FileSystemEvent event1 = new(FileSystemEventType.Created, "C:\\source\\test1.txt");
    FileSystemEvent event2 = new(FileSystemEventType.Created, "C:\\source\\test2.txt");

    _queue.Enqueue(event1);
    _queue.Enqueue(event2);

    FileSystemEvent? result1 = _queue.Dequeue();
    FileSystemEvent? result2 = _queue.Dequeue();

    Assert.That(result1!.Path, Is.EqualTo("C:\\source\\test1.txt"));
    Assert.That(result2!.Path, Is.EqualTo("C:\\source\\test2.txt"));
}
```

## Troubleshooting

### State File Corrupted

Error: JSON parsing error

Solution: Delete and recreate:
```bash
rm state.json
# FileMirror will create new on next save
```

### State Out of Sync

Symptoms: Unexplained file operations

Solution:
1. Stop FileMirror
2. Delete state file
3. Start FileMirror
4. Full sync occurs

### Dead Paths Not Recognized

Check:
1. `IsDead` flag is set
2. `GetDeadPaths()` filters correctly

Example:
```csharp
List<string> deadPaths = _state.Files
    .Where(kv => kv.Value.IsDead)
    .Select(kv => kv.Key)
    .ToList();
```

# File System Monitor

## Overview

FileMirror uses .NET's `FileSystemWatcher` to detect file system changes in real-time.

## Components

### FileSystemEventType

Enum of file system event types:

```csharp
public enum FileSystemEventType
{
    Changed,
    Created,
    Deleted,
    Renamed,
    Error
}
```

### FileSystemEvent

Wrapper around file system events:

```csharp
public class FileSystemEvent
{
    public FileSystemEventType Type { get; }
    public string Path { get; }
    public string? OldPath { get; }
    public DateTime Timestamp { get; }
}
```

**Properties:**
- `Type`: Event type (Changed/Created/Deleted/Renamed/Error)
- `Path`: File/folder path
- `OldPath`: Old path (for renames, null otherwise)
- `Timestamp`: When event occurred

### FileSystemWatcherWrapper

Wrapper around `System.IO.FileSystemWatcher`:

```csharp
public class FileSystemWatcherWrapper : IDisposable
{
    public FileSystemWatcherWrapper(SourceMapping mapping) { ... }

    public event Action<FileSystemEvent>? OnFileChanged;
    public event Action<string>? OnErrorLogged;

    public void Start() { ... }
    public void Stop() { ... }
}
```

**Events:**
- `OnFileChanged`: Raised when file system event occurs
- `OnErrorLogged`: Raised when FileSystemWatcher reports error

**Methods:**
- `Start()`: Enable event raising
- `Stop()`: Disable event raising

**Properties:**
- `_recursive`: Monitor subdirectories
- `_watcher`: Internal FileSystemWatcher

## Usage

### Basic Setup

```csharp
SourceMapping mapping = new("C:\\source", "C:\\target");
FileSystemWatcherWrapper watcher = new(mapping);

watcher.OnFileChanged += @event =>
{
    Console.WriteLine($"{@event.Type}: {@event.Path}");
};

watcher.Start();
```

### Event Handling

```csharp
watcher.OnFileChanged += @event =>
{
    switch (@event.Type)
    {
        case FileSystemEventType.Created:
            Console.WriteLine($"Created: {@event.Path}");
            break;
        case FileSystemEventType.Changed:
            Console.WriteLine($"Changed: {@event.Path}");
            break;
        case FileSystemEventType.Deleted:
            Console.WriteLine($"Deleted: {@event.Path}");
            break;
        case FileSystemEventType.Renamed:
            Console.WriteLine($"Renamed: {@event.OldPath} → {@event.Path}");
            break;
        case FileSystemEventType.Error:
            watcher.OnErrorLogged?.Invoke($"Watcher error: {@event.Path}");
            break;
    }
};
```

### Error Handling

```csharp
watcher.OnErrorLogged += message =>
{
    Console.WriteLine($"Watcher error: {message}");
    // Log to file, alert, etc.
};
```

### Disposal

```csharp
FileSystemWatcherWrapper watcher = new(mapping);

// Use watcher...

watcher.Stop();
watcher.Dispose(); // Cleanup resources
```

## Event Flow

```
1. FileSystemWatcher detects change
   ↓
2. Native event raised (Changed/Created/Deleted/Renamed/Error)
   ↓
3. FileSystemWatcherWrapper event handler invoked
   ↓
4. FileSystemEvent created
   ↓
5. OnFileChanged event raised
```

## Event Types

### Changed

Raised when file content or attributes change:

- File modified (write, append)
- File attributes changed (read-only, archive, etc.)
- File timestamp changed

Example:
```
Changed: C:\source\file.txt
```

### Created

Raised when new file or folder is created:

- New file
- New directory
- File moved into watched directory

Example:
```
Created: C:\source\newfile.txt
```

### Deleted

Raised when file or folder is deleted:

- File removed
- Directory removed
- File moved out of watched directory

Example:
```
Deleted: C:\source\file.txt
```

### Renamed

Raised when file or folder is renamed:

- File renamed
- Directory renamed

`OldPath` contains the previous path:

```
Renamed: C:\source\oldname.txt
OldPath: C:\source\oldname.txt
```

### Error

Raised when FileSystemWatcher encounters error:

- Access denied
- Path not found
- Too many changes (buffer overflow)

Error info passed via `OnErrorLogged` event:

```
OnErrorLogged: Access to the path is denied
```

## Configuration

### Watcher Filters

`FileSystemWatcherWrapper` configures watcher with:

```csharp
_watcher = new FileSystemWatcher(mapping.SourcePath)
{
    IncludeSubdirectories = _recursive,
    NotifyFilter = NotifyFilters.LastWrite | 
                   NotifyFilters.FileName | 
                   NotifyFilters.DirectoryName | 
                   NotifyFilters.Size | 
                   NotifyFilters.Attributes,
    Filter = "*.*"
};
```

**NotifyFilters:**
- `LastWrite`: Last write timestamp
- `FileName`: File name changes
- `DirectoryName`: Directory name changes
- `Size`: File size changes
- `Attributes`: File attribute changes

### Recursive vs Non-Recursive

```csharp
// Recursive (default)
_watcher.IncludeSubdirectories = true;

// Non-recursive
_watcher.IncludeSubdirectories = false;
```

## Change Batching

### Why Batch?

Without batching, rapid changes create many events:

```
Write file (Changed)
Write file again (Changed)
Write file again (Changed)
Write file again (Changed)
Write file again (Changed)
```

With batching, these are grouped:

```
[Changed, Changed, Changed, Changed, Changed] → Process once
```

## Network Shares

### Requirements

- FileMirror must run on the **host** machine (not client)
- Network path format: `\\server\share`

### Example

```csharp
SourceMapping mapping = new(
    sourcePath: "\\\\server\\shared",
    targetPath: "C:\\backup\\shared"
);

FileSystemWatcherWrapper watcher = new(mapping);
watcher.Start();
```

### Limitations

- Network latency affects detection speed
- Disconnections cause Error events
- May miss rapid changes during disconnection

## Testing

See `FileSystemWatcherWrapperTests.cs` in tests directory.

### Event Detection Test

```csharp
[Test]
public void Constructor_SubscribesToEvents()
{
    bool eventFired = false;
    _watcher.OnFileChanged += (_) => eventFired = true;

    _watcher.Start();

    string testFile = Path.Combine(SourcePath, "test.txt");
    File.WriteAllText(testFile, "test content");

    Thread.Sleep(100);

    _watcher.Stop();

    Assert.That(eventFired, Is.True);
}
```

### Start/Stop Test

```csharp
[Test]
public void Start_EnablesRaisingEvents()
{
    _watcher.Start();
    // Verify events raised...
}

[Test]
public void Stop_DisablesRaisingEvents()
{
    _watcher.Start();
    _watcher.Stop();
    // Verify events stopped...
}
```

## Troubleshooting

### Events Not Raised

Check:
1. Directory exists
2. Watcher started (`Start()` called)
3. Event handler subscribed
4. Network share accessible (if applicable)

### Access Denied

Solution: Run FileMirror with appropriate permissions
- CLI: Run as user with access
- Service: Configure service account

### Buffer Overflow

Error: "The internal buffer is full"

Solution:
1. Reduce events (disable some NotifyFilters)
2. Increase buffer (not directly possible)
3. Reduce event frequency by filtering (e.g., only Changed events)

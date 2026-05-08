# Mirroring Engine

## Overview

The mirroring engine applies source file system changes to target directories.

## Components

### IFileMirrorOperation

Interface for file operations:

```csharp
public interface IFileMirrorOperation
{
    void ApplyToTarget(FileInfo source, FileInfo target);
    void RevertToSource(FileInfo target, FileInfo source);
}
```

**Methods:**
- `ApplyToTarget()`: Copy source to target
- `RevertToSource()`: Revert target to match source

### IDirectoryMirrorOperation

Interface for directory operations:

```csharp
public interface IDirectoryMirrorOperation
{
    void Create(DirectoryInfo source, DirectoryInfo target);
    void Delete(DirectoryInfo target);
    void SyncAttributes(DirectoryInfo source, DirectoryInfo target);
}
```

**Methods:**
- `Create()`: Create target directory
- `Delete()`: Delete target directory
- `SyncAttributes()`: Sync directory attributes

### FileMirrorEngine

Main mirroring engine:

```csharp
public class FileMirrorEngine
{
    private readonly ChangeQueue _changeQueue = new();

    public void ProcessChange(SourceMapping mapping, FileSystemEvent change) { ... }
    public void QueueForOffline(FileSystemEvent change) { ... }
    public void ReconcileOfflineChanges(SourceMapping mapping) { ... }
}
```

**Properties:**
- `_changeQueue`: Queue for offline changes
**Methods:**
- `ProcessChange()`: Handle source change
- `QueueForOffline()`: Queue change during offline
- `ReconcileOfflineChanges()`: Process queued changes

## Usage

### Basic Operation

```csharp
FileMirrorEngine engine = new();
SourceMapping mapping = new("C:\\source", "C:\\target");

FileSystemEvent @event = new(FileSystemEventType.Created, "C:\\source\\file.txt");
engine.ProcessChange(mapping, @event);
```

### Event Processing

```csharp
FileMirrorEngine engine = new();

watcher.OnFileChanged += @event =>
{
    engine.ProcessChange(mapping, @event);
};
```

### Offline Handling

```csharp
FileMirrorEngine engine = new();

// While target offline
FileSystemEvent @event = new(FileSystemEventType.Changed, "C:\\source\\file.txt");
engine.QueueForOffline(@event);

// When target available
engine.ReconcileOfflineChanges(mapping);
```

## Change Types

### Created

Source creates file/folder → Target creates matching file/folder.

#### Files

```
Source:      C:\source\file.txt (created)
Target:      C:\target\file.txt (created)
             Content copied
             Attributes copied
```

#### Directories

```
Source:      C:\source\folder (created)
Target:      C:\target\folder (created)
             Last write time synced
```

### Changed

Source file modified → Target file updated.

```
Source:      file.txt → "New content"
Target:      file.txt → "New content"
             (overwritten)
             Attributes synced
```

### Deleted

Source file/folder deleted → Target file/folder deleted.

```
Source:      file.txt (deleted)
Target:      file.txt (deleted)
```

### Renamed

Source renamed → Target recreated at new name.

```
Source:      oldname.txt → newname.txt
Target:      oldname.txt (deleted)
             newname.txt (created)
```

## FileMirrorEngine Deep Dive

### ProcessChange Method

Handles all change types:

```csharp
public void ProcessChange(SourceMapping mapping, FileSystemEvent change)
{
    string sourcePath = change.Path;
    string targetPath = mapping.TargetPath + sourcePath[mapping.SourcePath.Length..];

    switch (change.Type)
    {
        case FileSystemEventType.Created:
            HandleCreated(sourcePath, targetPath);
            break;
        case FileSystemEventType.Changed:
            HandleChanged(sourcePath, targetPath);
            break;
        case FileSystemEventType.Deleted:
            HandleDeleted(sourcePath, targetPath);
            break;
        case FileSystemEventType.Renamed:
            HandleRenamed(sourcePath, targetPath, change.OldPath);
            break;
    }
}
```

**Path Calculation:**

```csharp
string sourcePath = change.Path;        // "C:\source\folder\file.txt"
string targetPath = mapping.TargetPath + sourcePath[mapping.SourcePath.Length..];
// "C:\target" + "\folder\file.txt"
// = "C:\target\folder\file.txt"
```

### HandleCreated

```csharp
private void HandleCreated(string sourcePath, string targetPath)
{
    if (Directory.Exists(sourcePath))
    {
        Directory.CreateDirectory(targetPath);
        Directory.SetLastWriteTime(targetPath, File.GetLastWriteTime(sourcePath));
    }
    else if (File.Exists(sourcePath))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        File.Copy(sourcePath, targetPath, true);
        File.SetAttributes(targetPath, File.GetAttributes(sourcePath));
    }
}
```

- Checks if source is directory or file
- Creates target directory/file
- Copies attributes (including read-only flag)

### HandleChanged

```csharp
private void HandleChanged(string sourcePath, string targetPath)
{
    if (File.Exists(sourcePath))
    {
        File.Copy(sourcePath, targetPath, true);
        File.SetAttributes(targetPath, File.GetAttributes(sourcePath));
    }
}
```

- Overwrites target with source content
- Syncs attributes

### HandleDeleted

```csharp
private void HandleDeleted(string sourcePath, string targetPath)
{
    if (File.Exists(targetPath))
    {
        File.Delete(targetPath);
    }
    else if (Directory.Exists(targetPath))
    {
        Directory.Delete(targetPath, true);
    }
}
```

- Deletes target file/folder if exists

### HandleRenamed

```csharp
private void HandleRenamed(string sourcePath, string targetPath, string? oldPath)
{
    if (oldPath == null)
        return;

    string oldTargetPath = targetPath[..^Path.GetFileName(targetPath).Length] +
                           Path.GetFileName(oldPath);

    if (File.Exists(oldPath))
    {
        if (File.Exists(oldTargetPath))
            File.Delete(oldTargetPath);
        HandleCreated(sourcePath, targetPath);
    }
    else if (Directory.Exists(oldPath))
    {
        if (Directory.Exists(oldTargetPath))
            Directory.Delete(oldTargetPath, true);
        HandleCreated(sourcePath, targetPath);
    }
}
```

- Deletes old target
- Creates new target
```

**Note:** Only syncs last write time (directories don't have same attributes as files).

## Change Queue

### Queue Changes

```csharp
FileSystemEvent @event = new(FileSystemEventType.Changed, "C:\\source\\file.txt");
engine.QueueForOffline(@event);
```

### Reconcile Queue

```csharp
while (engine.ChangeQueue.Count > 0)
{
    FileSystemEvent? change = engine.ChangeQueue.Dequeue();
    if (change != null)
    {
        engine.ProcessChange(mapping, change);
    }
}
```

## Attributes Preserved

FileMirror preserves:

- **Read-only flag**
- **Archive flag**
- **Hidden flag**
- **System flag**
- **Last write time**
- **File name**

FileMirror ignores:

- **Security descriptors (ACLs)**
- **Permissions**
- **Ownership**

## Thread Safety

- **FileMirrorEngine**: Not thread-safe
- **ChangeQueue**: Thread-safe (uses Queue internally)

**Recommendation:** Use single-threaded processing or implement locking.

## Testing

### ProcessChange Test

```csharp
[Test]
public void ProcessChange_Created_FileMirrored()
{
    // Arrange
    string sourceDir = Path.Combine(TestBasePath, "source");
    string targetDir = Path.Combine(TestBasePath, "target");
    Directory.CreateDirectory(sourceDir);
    
    SourceMapping mapping = new(sourceDir, targetDir);
    FileMirrorEngine engine = new();
    
    string sourceFile = Path.Combine(sourceDir, "test.txt");
    File.WriteAllText(sourceFile, "test content");
    
    // Act
    FileSystemEvent @event = new(FileSystemEventType.Created, sourceFile);
    engine.ProcessChange(mapping, @event);
    
    // Assert
    string targetFile = Path.Combine(targetDir, "test.txt");
    Assert.That(File.Exists(targetFile), Is.True);
    Assert.That(File.ReadAllText(targetFile), Is.EqualTo("test content"));
}
```



## Troubleshooting

### Target Not Updated

Check:
1. Source file exists
2. Source path and target path calculated correctly
3. No access denied errors
4. File not locked by other process

### Wrong File Mirrored

Debug:
```csharp
Console.WriteLine($"Source: {sourcePath}");
Console.WriteLine($"Target: {targetPath}");
```

Verify path calculation:

```csharp
string targetPath = mapping.TargetPath + sourcePath[mapping.SourcePath.Length..];
```

### Renamed Files Not Handled

Check `OldPath` is not null:

```csharp
if (change.OldPath == null)
{
    Console.WriteLine("Rename event has null OldPath");
    return;
}
```

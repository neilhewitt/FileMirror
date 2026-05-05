# State Persistence and Recovery

## Goal
Implement state persistence for tracking mirrored files and queued changes.

## Requirements

### State Model
1. Create `FileState` class:
   - `Path` (string)
   - `LastModified` (DateTime)
   - `Length` (long)
   - `Attributes` (FileAttributes)
   - `IsDead` (bool)

2. Create `MirroredState` class:
   - `SourcePath` (string)
   - `TargetPath` (string)
   - `Files` (Dictionary<string, FileState>)
   - `LastSynced` (DateTime)

### State Store
1. Create `StateStore` class:
   - `Load(string path)` → MirroredState
   - `Save(string path, MirroredState state)`
   - `GetFileState(string sourcePath)` → FileState?
   - `UpdateFileState(string sourcePath, FileState state)`
   - `MarkDead(string sourcePath)`
   - `GetDeadPaths()` → List<string>

### Change Queue
1. Create `ChangeQueue` class:
   - `Enqueue(FileSystemEvent change)`
   - `Dequeue()` → FileSystemEvent?
   - `Peek()` → FileSystemEvent?
   - `Count` property
   - Persist to disk for recovery

### Recovery
1. On startup:
   - Load state from disk
   - Identify dead paths
   - Resume monitoring
   - Reconcile differences

## Instructions
1. Create in `src/FileMirror.Core/Storage/`
2. Use JSON for persistence
3. Write tests for state operations

## Acceptance Criteria
- State persists across restarts
- Dead paths remembered
- Change queue survives restart
- Recovery syncs to latest state
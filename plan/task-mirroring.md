# Mirroring Core Logic

## Goal
Implement the core one-way mirroring logic that applies source changes to targets.

## Requirements

### File Operations
1. Create `FileMirrorOperation` interface:
   - `ApplyToTarget(FileInfo source, FileInfo target)`
   - `RevertToSource(FileInfo target, FileInfo source)` (for reverting target changes)

2. Implement concrete operations:
   - `CopyFileOperation` - full file copy with attributes
   - `DiffCopyOperation` - file content diff application
   - `MoveOperation` - rename handling
   - `DeleteOperation` - target file deletion
   - `AttributeOperation` - sync attributes (including read-only flag)

### Directory Operations
1. Create `DirectoryMirrorOperation`:
   - `Create` - create target directory
   - `Delete` - delete target directory (if source deleted)
   - `SyncAttributes` - sync directory attributes

### Mirroring Strategy
1. Create `MirrorEngine` class:
   - `ProcessChange(SourceMapping config, FileSystemEvent change)`
   - Handle deletions: mark path as "dead" (stop mirroring, not delete)
   - Handle offline targets: queue changes, reconcile on recovery
   - For offline reconciliation: sync to latest state only

2. Create `RevertEngine` class:
   - Detects target changes vs source
   - Reverts target changes to match source
   - Handles renames correctly

## Instructions
1. Create classes in `src/FileMirror.Core/Mirroring/`
2. Use FileInfo/DirectoryInfo for path operations
3. Preserve read-only flag, ignore security descriptors
4. Write tests for each operation type

## Implementation Status
- ✅ `IFileMirrorOperation` interface
- ✅ `IDirectoryMirrorOperation` interface
- ✅ `FileMirrorEngine` class
- ✅ `RevertEngine` class

## Notes
- ✅ FileMirrorEngine.ProcessChange() - handles Created, Changed, Deleted, Renamed
- ✅ FileMirrorEngine.ReconcileOfflineChanges() - dequeues and processes queued changes
- ✅ RevertEngine.DetectAndRevert() - compares source/target and reverts differences
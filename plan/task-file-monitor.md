# File System Monitor

## Goal
Create file system monitoring using FileSystemWatcher with change batching.

## Requirements

### Change Detection
1. Create `FileSystemWatcherWrapper` class:
   - Wraps System.IO.FileSystemWatcher
   - Supports recursive and non-recursive monitoring
   - Handles network share paths
   - Event args: `FileSystemEvent` with Type, Path, OldPath (renames), Timestamp

2. Create `FileSystemEvent` enum:
   - Changed, Created, Deleted, Renamed, Error

### Change Batching
1. Create `ChangeBatcher` class:
   - `AddEvent(FileSystemEvent @event)` 
   - `GetBatch()` → List<FileSystemEvent>
   - De-duplicate overlapping changes (e.g., create then delete = no net change)
   - Timeout-based flush (default 100ms)

### Error Handling
1. Handle network share unavailability gracefully
2. Queue events when target is unreachable
3. Log errors without stopping monitoring

## Instructions
1. Create classes in `src/FileMirror.Core/Monitoring/`
2. Use FileSystemWatcher with proper filters
3. Implement batching logic to reduce redundant operations
4. Write tests for batching logic (edge cases: rapid create/delete, renames)

## Implementation Status
- ✅ `FileSystemEventType` enum
- ✅ `FileSystemEvent` class
- ✅ `FileSystemWatcherWrapper` class with all file system events
- ✅ `ChangeBatcher` class with timeout-based batching
- ✅ Error handling via `OnErrorLogged` event

## Notes
- Batch deduplication logic still TODO
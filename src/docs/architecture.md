# Architecture Overview

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      FileMirror Application                   │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────┐ │
│  │   Config     │────>│   File       │────>│   Mirroring  │ │
│  │   System     │     │   Monitor    │     │   Engine     │ │
│  └──────────────┘     └──────────────┘     └──────────────┘ │
│                              │                               │
│                              ▼                               │
│                    ┌──────────────┐                         │
│                    │   State      │                         │
│                    │   Persistence│                         │
│                    └──────────────┘                         │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

## Component Breakdown

### Configuration System (`FileMirror.Core.Config`)

Manages all configuration aspects:

- **Config**: Main configuration object with mappings and settings
- **SourceMapping**: Defines a source→target pair with recursion flag
- **ConfigParser**: JSON parsing and validation
- **ConfigStore**: Load/save config, watch for changes (hot-reload)

### File System Monitor (`FileMirror.Core.Monitoring`)

Detects changes in source directories:

- **FileSystemWatcherWrapper**: Wraps .NET FileSystemWatcher
- **FileSystemEvent**: Encapsulates a single change event
- **FileSystemEventType**: Enum (Changed, Created, Deleted, Renamed, Error)

### Mirroring Engine (`FileMirror.Core.Mirroring`)

Applies source changes to targets:

- **FileMirrorEngine**: Main engine, processes events and applies changes

### State Persistence (`FileMirror.Core.Storage`)

Tracks mirrored state and handles offline periods:

- **FileState**: Tracks individual file state (path, timestamp, size, attributes)
- **MirroredState**: Complete state for a source→target pair
- **StateStore**: Load/save state to disk, track dead paths
- **ChangeQueue**: FIFO queue for queued changes during offline periods

## Data Flow

### Change Detection → Mirroring

1. **FileSystemWatcherWrapper** detects file system changes
2. Events converted to **FileSystemEvent** objects
3. **FileMirrorEngine** processes each event:
   - Calculates target path from source path
   - Applies appropriate operation (copy, delete, etc.)
4. **StateStore** updates internal state

### Offline Recovery

1. Changes detected while target unreachable → queued
2. Queue persisted to disk
3. On recovery, **ReconcileOfflineChanges** processes queue
4. Only latest state synced (not intermediate states)

## Design Principles

- **YAGNI**: No factories, services, or complex patterns
- **Concrete over abstract**: Prefer classes to interfaces
- **Simple inheritance**: Use where beneficial
- **Testable**: All components unit-testable
- **One-way only**: Source → Target, target changes reverted
- **Dead paths**: Removed source paths become "dead" (not deleted)

## Project Structure

```
src/
├── FileMirror.Core/          # Core domain logic
│   ├── Config/              # Configuration system
│   ├── Monitoring/          # File system monitoring
│   ├── Mirroring/           # Core mirroring logic
│   └── Storage/             # State persistence
├── FileMirror/              # CLI application
└── FileMirror.Service/      # Windows Service wrapper
```

## Key Behaviors

- **Latency**: <1 second for changes to propagate
- **Revert**: Target changes detected and reverted immediately
- **Dead paths**: Source removal → target ignored (not deleted)
- **Offline**: Changes queued, recovered on reconnect
- **Hot-reload**: Config changes detected and applied

# Release Notes

## Version 1.0.0 (Current)

Initial release.

### Features

- **Real-time monitoring**: Detect file changes <1s latency
- **One-way mirroring**: Source → Target only
- **Target reversion**: Unauthorized changes detected and reverted
- **Hot-reload**: Config changes detected without restart
- **Change batching**: Groups rapid changes to reduce redundant operations
- **State persistence**: Tracks mirrored files, supports offline recovery
- **Dead path handling**: Removed source paths become "dead" (not deleted)
- **Offline recovery**: Queues changes during offline periods
- **CLI mode**: Run in foreground or background
- **Windows Service**: Install as Windows Service
- **Network shares**: Mirror to network paths (host machine required)

### Components

- **Config System**: JSON parsing, validation, hot-reload
- **File System Monitor**: FileSystemWatcher wrapper with batching
- **Mirroring Engine**: Core mirroring logic
- **State Persistence**: Track files, handle offline periods
- **CLI**: Command-line interface
- **Windows Service**: Service wrapper

### Known Issues

- No filtering of file types (mirrors all files)
- No deduplication of overlapping changes in batch
- No incremental file copy (full copy each change)
- No conflict resolution for bidirectional scenarios

### Breaking Changes

None (first release).

---

## Version History

### Upcoming: v1.1.0

**Planned Features:**

- File type filtering
- Incremental file copy (differences only)
- Conflict detection
- Performance metrics

**Planned Enhancements:**

- Configurable batching timeout
- Custom logging providers
- More detailed state tracking

---

## Migration Guide

### From v0.x to v1.0.0

No previous versions exist. Fresh install required.

### From v1.0.0 to v1.1.0 (Future)

When v1.1.0 releases:

1. Backup current state file
2. Update FileMirror binary
3. Test in development environment
4. Deploy to production
5. Verify all mappings working

---

## Deprecations

None (first release).

---

## Bug Fixes

None (first release).

---

## Security

### Security Considerations

- Run service with minimal privileges
- Restrict access to config files
- Encrypt sensitive config data
- Audit file access

### Known Vulnerabilities

None reported.

---

## Changelog Format

Based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

### Categories

- **Added**: New features
- **Changed**: Changes in existing functionality
- **Deprecated**: Soon-to-be removed features
- **Removed**: Removed features
- **Fixed**: Bug fixes
- **Security**: Security changes

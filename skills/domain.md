# FileMirror Domain Concepts

## Mirroring Behavior

**One-way only**: Source → Target. Target changes are reverted.

**Dead paths**: When source path is removed from config, target becomes "dead" (not deleted, just ignored).

**Offline handling**: Changes queue during offline periods, reconcile to latest state on recovery.

**Latency**: <1s for mirrored changes.

## File Operations

Mirrored:
- File content changes (using diffs where possible)
- File attributes (including name)
- New file/folder additions
- File/folder deletions

Not mirrored:
- Security descriptors
- File system permissions (except read-only flag)

## Configuration

JSON config file with source→target mappings:
- Local paths: Direct mirroring
- Network shares: FileMirror must run on the host machine
- Recursive flag: Optional per-path
- Hot-reloaded: Config changes picked up immediately

## Windows Service

- Install/uninstall via PowerShell script
- No GUI required
- Config file path specified at install time
- Hot-reload supported
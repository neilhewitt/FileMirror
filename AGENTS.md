# FileMirror

## Quick start

- **Target framework**: .NET 10
- **Distribution**: Single self-contained binary
- **Hosting**: Windows Service or CLI
- **Language**: C# with style from global AGENTS.md

## Architecture

- Simple, concrete classes only - no factories, services, or DDD
- Prefer inheritance over composition
- Testable design without over-architecting
- YAGNI - keep it simple

## Coding style

Follow the global AGENTS.md coding standards rigidly. These are non-negotiable and must be enforced:

- **No `var`** (except anonymous types where absolutely required)
- **Constants use SCREAMING_SNAKE_CASE**
- **No comments** unless explicitly requested for clarity
- **Explicit types only**

Failure to follow these rules is a code quality failure that will be caught in code review. Do not deviate without explicit approval.

## What it does

Mirrors file changes (contents, attributes, names, adds, deletes) one-way from source to target path(s) in real time (<1s latency). Target changes are reverted to match source. No GUI - config-only via JSON file.

## Configuration

Config file paths define source → target mappings. Network shares require FileMirror running on the host machine. Read-only flag is preserved; security descriptors are not.

## Operational notes

- Deletions from source make target paths "dead" (no longer mirrored, not deleted)
- Offline target: changes queue and reconcile to latest state on recovery
- Config changes hot-reloaded; service restart usually unnecessary

## Test strategy

- NUnit for all tests
- Test base classes for shared setup
- Parameterized `[TestCase]` for data-driven tests
- No mocking frameworks - use real objects

## Implementation tasks

Run tasks in this order (each has dedicated prompt in `plan/task-*.md`):

1. **Project Structure** - `plan/task-project-structure.md`
   - Create .NET 10 projects with minimal dependencies
   - Directory structure: `FileMirror.Core`, `FileMirror`, `FileMirror.Service`

2. **Configuration** - `plan/task-configuration.md`
   - JSON config model with source→target mappings
   - Hot-reload support via file watching

3. **File Monitor** - `plan/task-file-monitor.md`
   - FileSystemWatcher wrapper with change batching
   - Handles network shares and offline scenarios

4. **Mirroring** - `plan/task-mirroring.md`
   - One-way mirroring logic (source → target)
   - Revert target changes, handle deletions as "dead" paths

5. **State & Recovery** - `plan/task-state.md`
   - Persist mirrored state to disk
   - Change queue for offline period
   - Recovery to latest state

6. **CLI Host** - `plan/task-cli.md`
   - Command-line interface with --config option
   - Foreground/background modes

7. **Service Host** - `plan/task-service.md`
   - Windows Service wrapper
   - PowerShell install/uninstall scripts

## Skills

- **Testing**: `skills/testing.md` - NUnit setup, no mocking
- **Build**: `skills/build.md` - Build and publish commands
- **Domain**: `skills/domain.md` - Mirroring behavior and config

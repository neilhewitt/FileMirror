# CLI Host

## Goal
Create command-line interface for FileMirror.

## Requirements

### CLI Interface
1. Create `CommandLineOptions` class:
   - `--config` (required): Path to config file
   - `--foreground`: Run in foreground (don't detach)
   - `--version`: Show version
   - `--help`: Show usage

2. Create `Program` class with:
   - Parse command line args
   - Validate config path exists
   - Start mirroring in foreground or background
   - Handle SIGINT/SIGTERM gracefully

3. Run modes:
   - **Foreground**: Block main thread, log to console
   - **Background**: Detach, log to file

### Logging
1. Simple console/file logging
2. Log levels: Info, Warning, Error
3. Log rotation (daily or 10MB)

## Instructions
1. Create in `src/FileMirror/`
2. Use System.CommandLine or simple argument parsing
3. Logging via Console.WriteLine for now
4. Write tests for argument parsing

## Acceptance Criteria
- CLI accepts all required options
- Config file validated on startup
- Graceful shutdown on signal
- Logs output appropriately
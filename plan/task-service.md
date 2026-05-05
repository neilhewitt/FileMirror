# Windows Service Host

## Goal
Create Windows Service wrapper for FileMirror.

## Requirements

### Service Wrapper
1. Create `FileMirrorService` class:
   - Inherits from `ServiceBase`
   - `OnStart(string[] args)`
   - `OnStop()`
   - `OnContinue()`
   - `OnPause()`
   - Config file path from args

2. Service lifecycle:
   - Load config on start
   - Start monitoring all configured source paths
   - Graceful shutdown stops all watchers
   - Hot-reload config changes without restart

### Installation Script
1. Create PowerShell script `install-service.ps1`:
   - Copy binary to installation directory
   - Register Windows Service
   - Set service to auto-start
   - Log installation actions

2. Create `uninstall-service.ps1`:
   - Stop service if running
   - Unregister Windows Service
   - Clean up (optional: preserve config)

## Instructions
1. Create in `src/FileMirror.Service/`
2. Use Microsoft.Extensions.Hosting.WindowsServices
3. PowerShell scripts in repo root
4. Write integration tests for service lifecycle

## Acceptance Criteria
- Service installs/uninstalls via PowerShell
- Service starts/stops cleanly
- Config hot-reload works in service mode
- No GUI required
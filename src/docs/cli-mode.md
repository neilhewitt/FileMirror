# CLI Mode

## Overview

FileMirror CLI provides command-line interface for running in foreground or background modes.

## Command-Line Options

### Basic Usage

```bash
FileMirror --config path\to\config.json --foreground
```

### Options

| Option | Short | Description | Required |
|--------|-------|-------------|----------|
| `--config` | `-c` | Path to config file | Yes |
| `--foreground` | `-f` | Run in foreground mode | No |
| `--version` | `-v` | Show version information | No |
| `--help` | `-h` | Show help message | No |

## Running Modes

### Foreground Mode

Block the main thread, log to console. Exit on Ctrl+C.

```bash
FileMirror --config config.json --foreground
```

**Output:**
```
Starting FileMirror in foreground mode...
Press Ctrl+C to stop.
```

**Exit:**
- Press Ctrl+C
- Close console window

**Use cases:**
- Development/testing
- Docker containers
- Debugging

### Background Mode

Run detached from console. Exit only on process termination.

```bash
FileMirror --config config.json
```

**Output:**
```
Starting FileMirror in background mode...
```

**Exit:**
- Close console (process continues)
- Kill process (Windows: Task Manager, Linux: kill)

**Use cases:**
- Quick testing
- Temporary runs

### Version Info

```bash
FileMirror --version
```

Output:
```
FileMirror version 1.0.0
```

### Help

```bash
FileMirror --help
```

Shows usage information.

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | Error (file not found, config errors, etc.) |

## Examples

### Start Monitoring

```bash
FileMirror --config C:\backup\config.json --foreground
```

### Check Version

```bash
FileMirror --version
```

### Show Help

```bash
FileMirror --help
```

## Config File

### Required

```bash
FileMirror --config path\to\config.json
```

### Validation

Config file is validated before starting:

- At least one sourceMapping required
- sourcePath and targetPath must be specified

### Errors

```bash
FileMirror --config config.json
```

If invalid:
```
Configuration errors:
  - At least one source mapping must be configured
```

## Logging

### Console Output

- Info messages: "Starting FileMirror..."
- Error messages: "Error: Config file not found..."

### Log Levels

- **Info**: Normal operations
- **Warning**: Non-critical issues
- **Error**: Critical errors (exits)

## Signal Handling

### Windows (Ctrl+C)

```csharp
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};
```

- Graceful shutdown
- Stops all watchers
- Exits cleanly

## Process Management

### Windows PowerShell

```powershell
# Run in foreground
FileMirror --config config.json --foreground

# Run in background (detached)
Start-Process -FilePath "FileMirror.exe" -ArgumentList "--config config.json"
```

### Windows Command Prompt

```cmd
REM Run in foreground
FileMirror --config config.json --foreground

REM Run in background
FileMirror --config config.json
```

### Linux/macOS (WSL)

```bash
# Run in foreground
./FileMirror --config config.json --foreground

# Run in background
./FileMirror --config config.json &
```

## Integration with Other Tools

### Windows Task Scheduler

```powershell
# Create scheduled task
$action = New-ScheduledTaskAction -Execute "FileMirror.exe" -Argument "--config C:\backup\config.json --foreground"
$trigger = New-ScheduledTaskTrigger -AtStartup
Register-ScheduledTask -Action $action -Trigger $trigger -TaskName "FileMirror"
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/10.0/runtime-deps:windowsservercore-ltsc2022

COPY FileMirror.exe /
COPY config.json /config/

ENTRYPOINT ["FileMirror.exe", "--config", "/config/config.json", "--foreground"]
```

### CI/CD Pipelines

```yaml
# GitHub Actions
- name: Run FileMirror
  run: FileMirror --config config.json --foreground
  working-directory: src/bin/Release/net10.0/publish
```

## Troubleshooting

### Config File Not Found

Error: `Error: Config file not found: C:\path\config.json`

Solution: Verify path exists:
```bash
Test-Path C:\path\config.json
```

### Invalid JSON

Error: Config parsing error (silent in current implementation)

Solution: Validate JSON:
```powershell
Get-Content config.json | ConvertFrom-Json
```

### Access Denied

Error: Access to path denied

Solution: Run with appropriate permissions:
```powershell
# Run PowerShell as Administrator
```

### Port Already in Use

Not applicable for CLI (no network ports).

## Comparison: CLI vs Service

| Feature | CLI | Service |
|---------|-----|---------|
| Foreground/Background | Both | Background only |
| Console output | Yes | No |
| Auto-start | No | Yes |
| User interaction | Yes | No |
| Ease of setup | Simple | Complex |
| Recommended for | Development | Production |

## Best Practices

### Development

```bash
# Use foreground for debugging
FileMirror --config config.json --foreground
```

### Testing

```bash
# Quick test
FileMirror --config config.json --foreground
```

### Production

Use Windows Service instead:
```bash
# Install service
.\install-service.ps1 -ConfigPath config.json

# Start service
net start FileMirror
```

## API Reference

### Program.Main

Entry point:

```csharp
public static async Task<int> Main(string[] args)
{
    // Parse args
    // Validate config
    // Run mode (foreground/background)
    return 0; // Exit code
}
```

### CommandLineOptions

Command-line argument model:

```csharp
public class CommandLineOptions
{
    public string ConfigPath { get; set; } = "";
    public bool Foreground { get; set; }
    public bool Version { get; set; }
    public bool Help { get; set; }
}
```

## See Also

- [Windows Service](windows-service.md) - For production deployment
- [Configuration](configuration.md) - Config file format
- [Building](building.md) - Build and publish

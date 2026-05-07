# Windows Service

## Overview

FileMirror can be installed as a Windows Service for production use.

## Installation

### Prerequisites

- Windows 10/11 or Windows Server 2016+
- Administrator privileges
- .NET 10 Runtime (or self-contained binary)

### PowerShell Install Script

```powershell
.\install-service.ps1 -ConfigPath "C:\path\to\config.json"
```

**Script parameters:**

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `ConfigPath` | Yes | - | Path to config file |
| `ServiceName` | No | "FileMirror" | Service name |
| `BinaryPath` | No | Current dir | Path to FileMirror.exe |

### Manual Installation

1. Copy FileMirror.exe to installation directory
2. Open PowerShell as Administrator
3. Create service:

```powershell
New-Service -Name "FileMirror" `
    -BinaryPathName "C:\path\to\FileMirror.exe" `
    -DisplayName "FileMirror Service" `
    -Description "Mirrors files from source to target paths" `
    -StartupType Automatic
```

4. Start service:

```powershell
Start-Service FileMirror
```

## Service Lifecycle

### Start

```
1. Service control manager invokes OnStart()
2. Load config from specified path
3. Create FileSystemWatcherWrapper for each mapping
4. Start all watchers
5. Service enters running state
```

### Stop

```
1. Service control manager invokes OnStop()
2. Stop all FileSystemWatcherWrapper instances
3. Release resources
4. Service enters stopped state
```

### Pause/Continue

Not implemented (placeholders in code).

## PowerShell Scripts

### Install Service

```powershell
# install-service.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$ConfigPath,
    
    [string]$ServiceName = "FileMirror",
    
    [string]$BinaryPath = "."
)

# Copy binary to system directory
$serviceDir = "C:\Program Files\FileMirror"
if (!(Test-Path $serviceDir)) {
    New-Item -ItemType Directory -Path $serviceDir
}

Copy-Item "$BinaryPath\FileMirror.exe" $serviceDir

# Create service
$binaryFullPath = Join-Path $serviceDir "FileMirror.exe"
$binaryWithConfig = "`"$binaryFullPath`" `"$ConfigPath`""

New-Service -Name $ServiceName `
    -BinaryPathName $binaryWithConfig `
    -DisplayName $ServiceName `
    -Description "FileMirror - Real-time file mirroring service" `
    -StartupType Automatic

Write-Host "Service installed successfully"
Write-Host "Start with: Start-Service $ServiceName"
```

### Uninstall Service

```powershell
# uninstall-service.ps1
param(
    [string]$ServiceName = "FileMirror"
)

# Stop service if running
if (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue) {
    Stop-Service -Name $ServiceName
    Write-Host "Service stopped"
}

# Remove service
Remove-Service -Name $ServiceName

Write-Host "Service uninstalled"
```

## Service Management

### Start Service

```powershell
Start-Service FileMirror
```

### Stop Service

```powershell
Stop-Service FileMirror
```

### Restart Service

```powershell
Restart-Service FileMirror
```

### Check Status

```powershell
Get-Service FileMirror
```

Output:
```
Status   Name               DisplayName
------   ----               -----------
Running  FileMirror         FileMirror Service
```

### View Logs

```powershell
# Event Viewer
Get-EventLog -LogName System -Source "FileMirror"

# Or check Windows Logs → Applications and Services Logs
```

## Configuration

### Config File

Specify config path at install time:

```powershell
.\install-service.ps1 -ConfigPath "C:\config\filemirror.json"
```

Config is loaded on service start.

### Hot-Reload

Service watches config file for changes:

- Detects changes every 5 seconds (configurable)
- Applies new config without restart
- Only affects new operations

## Troubleshooting

### Service Won't Start

Check:
1. Config file path is valid
2. FileMirror.exe exists
3. No access denied errors
4. Config is valid JSON

### Service Crashes

Check:
1. Config file valid
2. Source/target paths accessible
3. Event Viewer for error details

### Event Viewer

View service events:

```powershell
# Get all FileMirror events
Get-EventLog -LogName Application -Source "FileMirror" | Format-List

# Get recent errors
Get-EventLog -LogName Application -Source "FileMirror" -EntryType Error | Select-Object -Last 10
```

## Service Account

### Default Account

- Runs as Local System
- Full local access
- Network access if configured

### Custom Account

```powershell
# Create service with custom account
$cred = Get-Credential
$binaryPath = "C:\path\to\FileMirror.exe"

sc.exe create FileMirror `
    binPath= "`"$binaryPath`"" `
    obj= "$($cred.UserName)" `
    password= $($cred.GetNetworkCredential().Password) `
    start= auto
```

## Best Practices

### Production Deployment

1. Use self-contained binary
2. Install in `C:\Program Files\FileMirror`
3. Use dedicated service account
4. Configure auto-start
5. Monitor in Event Viewer

### Security

- Run with minimal privileges
- Restrict config file access
- Encrypt sensitive config data
- Audit file access

### Maintenance

1. Schedule regular reboots
2. Monitor disk space
3. Review logs periodically
4. Backup config files

## Comparison: Service vs CLI

| Feature | Service | CLI |
|---------|---------|-----|
| Auto-start | Yes | No |
| Background | Always | Optional |
| Console | No | Yes |
| User login | Not required | Not required |
| Setup complexity | High | Low |
| Recommended for | Production | Development |

## API Reference

### FileMirrorService Class

```csharp
public class FileMirrorService : ServiceBase
{
    public FileMirrorService()
    {
        ServiceName = "FileMirror";
    }

    protected override void OnStart(string[] args) { ... }
    protected override void OnStop() { ... }
    protected override void OnContinue() { ... }
    protected override void OnPause() { ... }
}
```

### Program Class

```csharp
public static class Program
{
    public static void Main(string[] args)
    {
        ServiceBase.Run(new FileMirrorService());
    }
}
```

## See Also

- [CLI Mode](cli-mode.md) - For development and testing
- [Configuration](configuration.md) - Config file format
- [Building](building.md) - Build and publish

# Quick Start

## Prerequisites

- .NET 10 SDK
- Windows 10/11 or Windows Server 2016+

## Installation

### Option 1: Build from Source

```bash
cd src
dotnet build -c Release
dotnet publish -c Release --self-contained true /p:PublishSingleFile=true
```

The published binary is at:
```
src/bin/Release/net10.0/publish/
```

### Option 2: Download Pre-built

(If available from releases page)

## Configuration

Create a config file `config.json`:

```json
{
  "sourceMappings": [
    {
      "sourcePath": "C:\\source",
      "targetPath": "C:\\target",
      "recursive": true
    }
  ],
  "reloadInterval": "00:00:05",
  "batchChanges": true
}
```

## Running

### CLI Mode (Foreground)

```bash
./FileMirror --config config.json --foreground
```

Press Ctrl+C to stop.

### CLI Mode (Background)

```bash
./FileMirror --config config.json
```

### Windows Service

```powershell
# Install service (run as Administrator)
.\install-service.ps1 -ConfigPath "C:\path\to\config.json"

# Start service
net start FileMirror

# Stop service
net stop FileMirror

# Uninstall service
.\uninstall-service.ps1
```

## Verify It's Working

1. Create a file in the source directory
2. Check the target directory - file should appear
3. Modify the file in source
4. Changes should reflect in target within 1 second

## First Steps

- Read [Configuration](../docs/configuration.md) for detailed config options
- Read [Windows Service](../docs/windows-service.md) for service setup
- Read [Testing](../docs/testing.md) to verify functionality

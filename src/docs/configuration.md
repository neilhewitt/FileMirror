# Configuration

## Config File Format

Config file is JSON with the following structure:

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

## Configuration Options

### sourceMappings (Required)

Array of source→target mapping objects.

#### SourceMapping Properties

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| sourcePath | string | Yes | - | Source directory path |
| targetPath | string | Yes | - | Target directory path |
| recursive | boolean | No | true | Monitor subdirectories |

#### Examples

Single local mapping:
```json
{
  "sourceMappings": [
    {
      "sourcePath": "C:\\source",
      "targetPath": "C:\\backup"
    }
  ]
}
```

Multiple mappings:
```json
{
  "sourceMappings": [
    {
      "sourcePath": "C:\\docs",
      "targetPath": "D:\\backup\\docs"
    },
    {
      "sourcePath": "C:\\projects",
      "targetPath": "D:\\backup\\projects",
      "recursive": false
    }
  ]
}
```

Network share:
```json
{
  "sourceMappings": [
    {
      "sourcePath": "\\\\server\\shared",
      "targetPath": "C:\\backup\\shared"
    }
  ]
}
```

### reloadInterval (Optional)

TimeSpan string specifying how often to check for config changes.

Default: `"00:00:05"` (5 seconds)

Examples:
```json
"reloadInterval": "00:00:05"   // 5 seconds
"reloadInterval": "00:01:00"   // 1 minute
"reloadInterval": "00:05:00"   // 5 minutes
```

### batchChanges (Optional)

Whether to batch rapid file system events.

Default: `true`

When `true`:
- Events within 100ms are grouped
- Reduces redundant operations
- May delay changes by up to 100ms

When `false`:
- Each event processed immediately
- Slightly higher overhead
- Lower latency

Example:
```json
"batchChanges": true
```

## Validation

ConfigParser validates configuration:

- At least one sourceMapping required
- sourcePath must be specified for each mapping
- targetPath must be specified for each mapping

Invalid config example:
```json
{
  "sourceMappings": []  // ERROR: Empty mappings
}
```

Error output:
```
Configuration errors:
  - At least one source mapping must be configured
```

## Hot-Reload

Config file is watched for changes:

- Uses FileSystemWatcher
- Detects content/size changes
- Applies new config without restart
- Timeout: 5 seconds (configurable via reloadInterval)

Hot-reload limitations:
- Changes apply to new operations only
- Currently monitoring sessions continue with old settings
- New monitoring sessions use updated config

## File Locations

### CLI Mode

Specify via `--config` option:

```bash
./FileMirror --config C:\path\to\config.json
```

### Windows Service

Specify at install time:

```powershell
.\install-service.ps1 -ConfigPath "C:\path\to\config.json"
```

## Example Configs

### Development Setup

```json
{
  "sourceMappings": [
    {
      "sourcePath": "C:\\dev\\source",
      "targetPath": "C:\\dev\\backup"
    }
  ],
  "reloadInterval": "00:00:02",
  "batchChanges": true
}
```

### Production Setup

```json
{
  "sourceMappings": [
    {
      "sourcePath": "C:\\data",
      "targetPath": "\\\\backup-server\\data",
      "recursive": true
    },
    {
      "sourcePath": "C:\\logs",
      "targetPath": "\\\\backup-server\\logs",
      "recursive": true
    }
  ],
  "reloadInterval": "00:01:00",
  "batchChanges": true
}
```

### Non-Recursive Backup

```json
{
  "sourceMappings": [
    {
      "sourcePath": "C:\\documents",
      "targetPath": "D:\\backup\\documents",
      "recursive": false
    }
  ]
}
```

Only files directly in `C:\documents` are mirrored, not subfolders.

## Troubleshooting

### Path Not Found

Error: `sourcePath` or `targetPath` doesn't exist

Solution: Create directories before starting FileMirror:
```bash
mkdir C:\source
mkdir C:\target
```

### Network Share Access Denied

Error: Access to network path denied

Solution: Ensure FileMirror runs with appropriate credentials:
- CLI: Run as user with network access
- Service: Configure service logon account

### Invalid JSON

Error: JSON parsing error

Solution: Validate JSON syntax:
```bash
# Use PowerShell to validate
Get-Content config.json | ConvertFrom-Json
```

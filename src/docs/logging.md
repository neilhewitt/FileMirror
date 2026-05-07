# Logging

## Overview

FileMirror provides basic logging for monitoring and debugging.

## Log Outputs

### Console Output

CLI mode logs to console:

```csharp
Console.WriteLine("Starting FileMirror in foreground mode...");
```

**Output:**
```
Starting FileMirror in foreground mode...
Press Ctrl+C to stop.
```

### Event Viewer

Windows Service logs to Event Viewer:

- **Application log**: Service start/stop
- **System log**: Service control events

## Log Levels

| Level | Description | Example |
|-------|-------------|---------|
| **Info** | Normal operations | "Starting FileMirror" |
| **Warning** | Non-critical issues | "Config file not found" |
| **Error** | Critical errors | "Access denied" |

## Log Messages

### Info Messages

```
Starting FileMirror in foreground mode...
Starting FileMirror in background mode...
Stopping FileMirror...
Config file: C:\path\to\config.json
Source mapping: C:\source → C:\target
```

### Warning Messages

```
Config file not found: C:\path\config.json
SourcePath is required
TargetPath is required
```

### Error Messages

```
Error: Config file not found: {path}
Watcher error: Access to the path is denied
```

## Config Change Detection

### Hot-Reload Logging

```
Config changed: C:\path\config.json
New config loaded
```

## Error Handling

### Watcher Errors

```csharp
watcher.OnErrorLogged += message =>
{
    Console.WriteLine($"Watcher error: {message}");
};
```

**Output:**
```
Watcher error: The internal buffer is full
```

### File Access Errors

```csharp
try
{
    File.Copy(sourcePath, targetPath, true);
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"Access denied: {ex.Message}");
}
```

## Custom Logging

### Add File Logging

```csharp
public static class Logger
{
    private static readonly object _lock = new();
    
    public static void Info(string message)
    {
        var log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [INFO] {message}";
        Console.WriteLine(log);
        File.AppendAllText("filemirror.log", log + Environment.NewLine);
    }
    
    public static void Error(string message)
    {
        var log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR] {message}";
        Console.WriteLine(log);
        File.AppendAllText("filemirror-error.log", log + Environment.NewLine);
    }
}
```

### Use Custom Logger

```csharp
Logger.Info("Starting FileMirror");
Logger.Error($"Config error: {error}");
```

## Log Rotation

### Manual Rotation

```csharp
public static class RotatingLogger
{
    private static readonly int MaxFileSize = 10 * 1024 * 1024; // 10MB
    private static readonly string LogDirectory = "logs";
    
    static RotatingLogger()
    {
        Directory.CreateDirectory(LogDirectory);
    }
    
    public static void Log(string message, bool isError = false)
    {
        string filename = isError ? "error.log" : "info.log";
        string filepath = Path.Combine(LogDirectory, filename);
        
        // Check file size
        if (File.Exists(filepath) && new FileInfo(filepath).Length > MaxFileSize)
        {
            string rotated = Path.Combine(LogDirectory, 
                $"{DateTime.Now:yyyyMMdd-HHmmss}-{filename}");
            File.Move(filepath, rotated);
        }
        
        string log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";
        File.AppendAllText(filepath, log + Environment.NewLine);
    }
}
```

## Log Analysis

### Common Patterns

```powershell
# Count events by type
Get-Content filemirror.log | Group-Object

# Find recent errors
Get-Content filemirror.log | Where-Object { $_ -like "*ERROR*" } | Select-Object -Last 10

# Find specific path
Get-Content filemirror.log | Where-Object { $_ -like "*C:\source*" }
```

## Debug Logging

### Temporary Debug

Add debug statements:

```csharp
// Add to key methods
Console.WriteLine($"[DEBUG] ProcessChange: {@event.Type} {@event.Path}");
Console.WriteLine($"[DEBUG] Target path: {targetPath}");
Console.WriteLine($"[DEBUG] Source exists: {File.Exists(sourcePath)}");
Console.WriteLine($"[DEBUG] Target exists: {File.Exists(targetPath)}");
```

### Environment Variable

```csharp
public static class Logger
{
    private static readonly bool DebugEnabled = 
        Environment.GetEnvironmentVariable("FILEMIRROR_DEBUG") == "true";
    
    public static void Debug(string message)
    {
        if (DebugEnabled)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }
    }
}
```

Use:
```bash
$env:FILEMIRROR_DEBUG="true"
FileMirror --config config.json
```

## Performance Logging

### Timing Logs

```csharp
Stopwatch sw = Stopwatch.StartNew();

// Operation
ProcessChange(mapping, @event);

sw.Stop();
Console.WriteLine($"Processed in {sw.ElapsedMilliseconds}ms");
```

### Statistics

```csharp
public class PerformanceMonitor
{
    private int _totalEvents;
    private DateTime _startTime;
    
    public void OnEventProcessed()
    {
        _totalEvents++;
        Console.WriteLine($"Events processed: {_totalEvents}");
    }
}
```

## Best Practices

### Production

- Use File logging (not console)
- Implement log rotation
- Set appropriate log levels
- Monitor disk space

### Development

- Use console logging
- Enable debug mode
- Add timing logs
- Track event flow

## Example: Full Logging Setup

```csharp
public static class FileMirrorLogger
{
    private static readonly string LogDir = "logs";
    private static readonly object Lock = new();
    
    static FileMirrorLogger()
    {
        Directory.CreateDirectory(LogDir);
    }
    
    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        lock (Lock)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logLine = $"{timestamp} [{level}] {message}";
            
            // Console
            Console.WriteLine(logLine);
            
            // File
            string logFile = Path.Combine(LogDir, $"{level}.log");
            File.AppendAllText(logFile, logLine + Environment.NewLine);
        }
    }
}

public enum LogLevel { Info, Warning, Error }

// Usage
FileMirrorLogger.Log("Starting FileMirror");
FileMirrorLogger.Error("Access denied to path");
```

## See Also

- [Troubleshooting](troubleshooting.md) - Debugging common issues
- [CLI Mode](cli-mode.md) - Console output
- [Windows Service](windows-service.md) - Event Viewer logs

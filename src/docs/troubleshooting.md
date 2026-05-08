# Troubleshooting

## Common Issues

### 1. FileMirror Won't Start

**Error:** "Config file not found"

**Cause:** Config path incorrect or file missing

**Solution:**
```bash
# Verify config exists
Test-Path C:\path\to\config.json

# Check config syntax
Get-Content config.json | ConvertFrom-Json
```

---

### 2. Target Files Not Being Updated

**Symptoms:** Source changes not appearing in target

**Causes:**

1. **FileSystemWatcher not detecting changes**
   - Directory may not exist
   - Watcher not started
   - Event handler not subscribed

2. **Path calculation error**
   - Source and target paths may not match
   - Check path lengths align correctly

3. **Access denied**
   - Insufficient permissions
   - File locked by other process

**Solution:**
```csharp
// Verify paths
Console.WriteLine($"Source: {sourcePath}");
Console.WriteLine($"Target: {targetPath}");

// Check permissions
try
{
    File.Copy(sourcePath, targetPath, true);
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"Access denied: {ex.Message}");
}
```

---

### 3. Hot-Reload Not Working

**Symptoms:** Config changes not applied

**Causes:**

1. **FileSystemWatcher not watching config**
   - Watch not set up correctly
   - File changes not detected

2. **Config not reloaded**
   - onChange callback not invoked
   - Load failed silently

**Solution:**
```csharp
// Verify watcher
ConfigStore store = new();
store.Watch("config.json", path =>
{
    Console.WriteLine($"Config changed: {path}");
    // Reload logic...
});
```

---

### 4. Network Share Access Denied

**Error:** "Access to the path '\\server\share' is denied"

**Causes:**

1. **Incorrect credentials**
   - Service may not have network access
   - Client machine vs host machine confusion

2. **Network unavailable**
   - Share may be offline
   - Firewall blocking access

**Solution:**
```bash
# Verify network access
Test-Path \\server\share

# Check credentials
# Service must run with account that has share access
```

**Critical:** FileMirror must run on the **host** machine for network shares.

---

### 5. Dead Paths Not Recognized

**Symptoms:** Files still being mirrored after removal from config

**Causes:**

1. **Mapping not marked as dead**
   - ConfigParser may not remove mapping
   - State not updated

2. **Old state file**
   - State not cleared after config change

**Solution:**
```csharp
// Verify mapping removed
List<SourceMapping> mappings = config.SourceMappings;
foreach (SourceMapping mapping in mappings)
{
    Console.WriteLine($"Mapping: {mapping.SourcePath} → {mapping.TargetPath}");
}

// Clear state if needed
File.Delete("state.json");
```

---

### 6. State File Corrupted

**Error:** JSON parsing error

**Causes:**

1. **Incomplete write**
   - Application crashed during save
   - Disk full

2. **Manual modification**
   - JSON syntax error
   - Invalid characters

**Solution:**
```bash
# Delete state file
rm state.json

# FileMirror will recreate on next save
```

---

### 7. Offline Recovery Not Working

**Symptoms:** Changes lost during offline period

**Causes:**

1. **Queue not persisted**
   - State not saved before crash
   - In-memory only

2. **Reconciliation not called**
   - ReconcileOfflineChanges not invoked
   - Exception during processing

**Solution:**
```csharp
// Ensure queue is persisted
StateStore stateStore = new("state.json");
stateStore.Save(); // Call before exit

// Ensure reconciliation called
FileMirrorEngine engine = new();
engine.ReconcileOfflineChanges(mapping);
```

---

### 8. Events Duplicated

**Symptoms:** Same change processed multiple times

**Causes:**

1. **Multiple watchers on same path**
   - Duplicate SourceMapping entries
   - Overlapping paths

**Solution:**
```csharp
// Check for duplicates
HashSet<string> processedPaths = new();

watcher.OnFileChanged += @event =>
{
    if (processedPaths.Contains(@event.Path))
    {
        Console.WriteLine($"Duplicate: {@event.Path}");
        return;
    }
    
    processedPaths.Add(@event.Path);
    // Process event...
};
```

---

### 9. High CPU Usage

**Symptoms:** FileMirror uses 100% CPU

**Causes:**

1. **Loop in change handler**
   - Target changes trigger source changes
   - Infinite loop

2. **Too many events**
   - Large directory changes
   - Rapid file modifications

**Solution:**
```csharp
// Add throttling
DateTime lastProcess = DateTime.MinValue;
TimeSpan minInterval = TimeSpan.FromSeconds(1);

watcher.OnFileChanged += @event =>
{
    if (DateTime.Now - lastProcess < minInterval)
        return;
    
    lastProcess = DateTime.Now;
    // Process event...
};
```

---

### 10. Memory Leak

**Symptoms:** Memory usage constantly increases

**Causes:**

1. **Unsubscribed events**
   - Event handlers not cleaned up
   - Watchers not disposed

2. **Queued events not processed**
   - ChangeQueue growing indefinitely

**Solution:**
```csharp
// Proper disposal
FileSystemWatcherWrapper watcher = new(mapping);
// Use in using statement or dispose properly
watcher.Dispose();

// Clear queue when done
ChangeQueue queue = new();
while (queue.Count > 0)
{
    queue.Dequeue();
}
```

---

## Diagnostic Tools

### Event Viewer

View Windows events:

```powershell
Get-EventLog -LogName Application -Source "FileMirror"
```

### Process Monitor

Use Sysinternals Process Monitor:

1. Filter by process name: `FileMirror.exe`
2. Watch for file system events
3. Check access denied errors

### Logging

Add temporary logging:

```csharp
Console.WriteLine($"[DEBUG] Processing: {@event.Type} {@event.Path}");
```

### Network Trace

Use Wireshark or tcpview to check:

1. Network share connectivity
2. Port usage
3. Connection drops

---

## Debug Mode

### Enable Verbose Logging

```csharp
// Add to Program.cs
Console.WriteLine("[INFO] Starting FileMirror");
Console.WriteLine("[DEBUG] Config loaded");
Console.WriteLine("[DEBUG] Watcher started");
```

### Attach Debugger

1. Start FileMirror with --foreground
2. Attach Visual Studio debugger
3. Set breakpoints in key methods
4. Debug step-by-step

---

## Common Scenarios

### Scenario 1: Development Testing

**Problem:** Hard to test real-time monitoring

**Solution:**
```bash
# Use foreground mode
FileMirror --config config.json --foreground

# Add temporary logging
Console.WriteLine($"Event: {@event.Type} {@event.Path}");
```

### Scenario 2: Production Monitoring

**Problem:** Need to monitor service health

**Solution:**
```powershell
# Check service status
Get-Service FileMirror

# View recent errors
Get-EventLog -LogName Application -Source "FileMirror" -EntryType Error | Select-Object -Last 5
```

### Scenario 3: Migration Testing

**Problem:** Test new config without breaking production

**Solution:**
```bash
# Test with separate paths
FileMirror --config test-config.json --foreground

# Verify before switching
Test-Path C:\test-target
```

---

## Support

If issues persist:

1. Check documentation
2. Review logs
3. Test with minimal config
4. Enable debug logging
5. Report with:
   - FileMirror version
   - Config file (sanitized)
   - Error messages
   - Steps to reproduce

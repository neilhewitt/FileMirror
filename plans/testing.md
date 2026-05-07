# FileMirror Testing Guide

## Current Test Status

**27/27 tests passing**

- Config module: 7/7 ✅
- Monitoring module: 7/7 ✅  
- Storage module: 13/13 ✅

Run tests with: `dotnet test`

## Test Environment

Tests use isolated directories to prevent affecting host files.

### Default Location
Tests run in `%TEMP%\FileMirrorTest_{GUID}` - e.g.:
```
C:\Users\neilh\AppData\Local\Temp\FileMirrorTest_xxx
```

### Custom Location
Set environment variable `FILEMIRROR_TEST_PATH` to your preferred directory:
```powershell
$env:FILEMIRROR_TEST_PATH="D:\FileMirrorTest"
```

## Test Structure

### Unit Tests (in progress)
- **Config** - Complete
- **Monitoring** - Complete (FileSystemWatcherWrapper, ChangeBatcher)
- **Storage** - Complete (StateStore, ChangeQueue)
- **Mirroring** - Not yet implemented

### Integration Tests (planned)
End-to-end tests for mirroring scenarios

## Guidelines

1. Use NUnit (no xUnit)
2. TestBase for common setup/cleanup
3. No mocking - use real objects
4. Arrange, Act, Assert format
5. Naming: `MethodName_Scenario_ExpectedBehavior`

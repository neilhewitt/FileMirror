# Configuration Model and Parsing

## Goal
Create a simple configuration model and JSON parser for FileMirror.

## Requirements

### Configuration Model
1. Create `SourceMapping` class:
   - `SourcePath` (string, required)
   - `TargetPath` (string, required)
   - `Recursive` (bool, default true)
   - `IsDead` (bool, set only when removed from config)

2. Create `Config` class:
   - `SourceMappings` (List<SourceMapping>)
   - `ReloadInterval` (TimeSpan, default 5s)
   - `BatchChanges` (bool, default true)

### JSON Parsing
1. Create `ConfigParser` class with:
   - `Parse(string json)` → Config
   - `Validate(Config config)` → List<string> (validation errors)

2. JSON format:
```json
{
  "sourceMappings": [
    {
      "sourcePath": "C:\\source",
      "targetPath": "\\\\server\\target",
      "recursive": true
    }
  ],
  "reloadInterval": "00:00:05"
}
```

### Persistence
1. Create `ConfigStore` class:
   - `Load(string path)` → Config
   - `Save(string path, Config config)`
   - `Watch(string path, Action<string> onChange)`

## Instructions
1. Create classes in `src/FileMirror.Core/Config/`
2. Implement JSON parsing using Newtonsoft.Json
3. Implement file watching for hot-reload
4. Write tests in `tests/FileMirror.Core.Tests/Config/`

## Acceptance Criteria
- Config model represents all required fields
- JSON parsing handles missing optional fields with defaults
- File watcher triggers reload callback
- All parsing edge cases covered by tests
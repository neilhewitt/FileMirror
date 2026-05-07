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
4. Write tests in `src/tests/FileMirror.Core.Tests/Config/`

## Implementation Status
- ✅ `SourceMapping` - stores source/target paths and recursion setting
- ✅ `Config` - holds collection of mappings and settings
- ✅ `ConfigParser` - parses JSON, validates config
- ✅ `ConfigStore` - loads/saves config, watches for changes
- ✅ All tests pass (7/7)

## Style reminder
Follow the global AGENTS.md coding style. Constants use SCREAMING_SNAKE_CASE, no `var` (except for anonymous types where required), no comments unless explicitly requested.
# Configuration System

## Components

### Config

Main configuration object containing all settings.

```csharp
public class Config
{
    public List<SourceMapping> SourceMappings { get; } = new();
    public TimeSpan ReloadInterval { get; set; } = TimeSpan.FromSeconds(5);
    public bool BatchChanges { get; set; } = true;
}
```

**Properties:**
- `SourceMappings`: List of source→target mappings
- `ReloadInterval`: Config change detection frequency
- `BatchChanges`: Whether to batch rapid events

### SourceMapping

Defines a single source→target pair.

```csharp
public class SourceMapping
{
    public string SourcePath { get; set; } = null!;
    public string TargetPath { get; set; } = null!;
    public bool Recursive { get; set; }
}
```

**Properties:**
- `SourcePath`: Source directory (required)
- `TargetPath`: Target directory (required)
- `Recursive`: Monitor subdirectories (default: true)

**Constructors:**
- Parameterless: Default values
- Parameterized: Set all properties

**Methods:**
- `MarkDead()`: Mark mapping as removed from config

### ConfigParser

JSON parsing and validation.

```csharp
public class ConfigParser
{
    public Config Parse(string json) { ... }
    public List<string> Validate(Config config) { ... }
}
```

**Methods:**
- `Parse(json)`: Deserialize JSON to Config
- `Validate(config)`: Returns list of error strings

**Validation Rules:**
- At least one sourceMapping required
- sourcePath must not be empty/whitespace
- targetPath must not be empty/whitespace

### ConfigStore

Load/save config, watch for changes.

```csharp
public class ConfigStore
{
    public Config Load(string path) { ... }
    public void Save(string path, Config config) { ... }
    public void Watch(string path, Action<string> onChange) { ... }
}
```

**Methods:**
- `Load(path)`: Read config from file
- `Save(path, config)`: Write config to file
- `Watch(path, onChange)`: Monitor file for changes

## Usage

### Loading Config

```csharp
ConfigStore store = new();
Config config = store.Load("config.json");

if (config.SourceMappings.Count == 0)
{
    Console.WriteLine("No source mappings configured");
    return;
}
```

### Parsing from String

```csharp
string json = File.ReadAllText("config.json");
ConfigParser parser = new();
Config config = parser.Parse(json);
```

### Validating Config

```csharp
List<string> errors = parser.Validate(config);

if (errors.Count > 0)
{
    foreach (string error in errors)
    {
        Console.WriteLine($"Error: {error}");
    }
    return;
}
```

### Hot-Reload Setup

```csharp
ConfigStore store = new();

// Initial load
Config config = store.Load("config.json");

// Start watchers
store.Watch("config.json", path =>
{
    Config newConfig = store.Load(path);
    // Apply new config...
});
```

## JSON Format

### Complete Example

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

### Property Names

JSON uses camelCase, C# uses PascalCase:

| JSON Property | C# Property |
|---------------|-------------|
| sourcePath | SourcePath |
| targetPath | TargetPath |
| recursive | Recursive |
| reloadInterval | ReloadInterval |
| batchChanges | BatchChanges |

### TimeSpan Format

```
"00:00:05"   // 5 seconds
"00:01:00"   // 1 minute
"00:05:00"   // 5 minutes
"1.00:00:00" // 1 day
```

## Configuration Lifecycle

### Application Start

```
1. ConfigStore.Load() → Config object
2. ConfigParser.Validate() → Check for errors
3. If valid: Apply config to FileMirror
4. Start watchers
```

### Hot-Reload

```
1. FileSystemWatcher detects config change
2. onChange callback invoked
3. ConfigStore.Load() → New Config object
4. Validate new config
5. If valid: Apply to FileMirror
```

### Saving Config

```csharp
Config config = new();
config.SourceMappings.Add(new SourceMapping("C:\\source", "C:\\target"));
config.ReloadInterval = TimeSpan.FromSeconds(10);

ConfigStore store = new();
store.Save("config.json", config);
```

## Serialization

### Save Format

```csharp
var obj = new
{
    sourceMappings = config.SourceMappings.Select(m => new
    {
        sourcePath = m.SourcePath,
        targetPath = m.TargetPath,
        recursive = m.Recursive
    }),
    reloadInterval = config.ReloadInterval.ToString()
};
string json = JsonConvert.SerializeObject(obj, settings);
```

Output:
```json
{
  "sourceMappings": [
    {
      "sourcePath": "C:\\source",
      "targetPath": "C:\\target",
      "recursive": true
    }
  ],
  "reloadInterval": "00:00:05"
}
```

### Load Format

```csharp
string json = File.ReadAllText(path);
var settings = new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};
Config config = JsonConvert.DeserializeObject<Config>(json, settings)
                ?? new Config();
```

## Error Handling

### File Not Found

```csharp
ConfigStore store = new();
Config config = store.Load("nonexistent.json");
// Returns: new Config() with empty SourceMappings
```

### Invalid JSON

```csharp
string json = "{ invalid json }";
ConfigParser parser = new();
Config config = parser.Parse(json);
// Returns: new Config() (deserialization fails gracefully)
```

### Validation Errors

```csharp
Config config = new();
List<string> errors = parser.Validate(config);
// Returns: ["At least one source mapping must be configured"]
```

## Testing

See `ConfigParserTests.cs` and `ConfigStoreTests.cs` in tests directory.

### Parse Tests

```csharp
[Test]
public void Parse_FullConfig_ParsesAllFields()
{
    string json = @"{
        ""sourceMappings"": [
            {
                ""sourcePath"": ""C:\\source"",
                ""targetPath"": ""C:\\target"",
                ""recursive"": false
            }
        ],
        ""reloadInterval"": ""00:00:10"",
        ""batchChanges"": false
    }";

    ConfigParser parser = new();
    Config config = parser.Parse(json);

    Assert.That(config.SourceMappings.Count, Is.EqualTo(1));
    Assert.That(config.ReloadInterval, Is.EqualTo(TimeSpan.FromSeconds(10)));
}
```

### Validation Tests

```csharp
[Test]
public void Validate_EmptyMappings_ReturnsError()
{
    Config config = new();
    ConfigParser parser = new();
    List<string> errors = parser.Validate(config);

    Assert.That(errors.Count, Is.GreaterThan(0));
}
```

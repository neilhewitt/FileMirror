# Coding Style

## Overview

FileMirror follows the global OpenCode AGENTS.md coding standards with project-specific overrides.

## C# Style

### Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Classes | PascalCase | `FileMirrorEngine`, `ConfigParser` |
| Interfaces | `I` + PascalCase | `IFileMirrorOperation`, `IDirectoryMirrorOperation` |
| Enums | PascalCase | `FileSystemEventType`, ` FileMode` |
| Private fields | `_camelCase` | `_watcher`, `_changeQueue` |
| Public properties | PascalCase | `SourcePath`, `IsDead` |
| Public methods | PascalCase | `ProcessChange()`, `Validate()` |
| Constants | SCREAMING_SNAKE_CASE | `VERSION`, `DEFAULT_TIMEOUT` |

### Examples

```csharp
// Classes
public class FileMirrorEngine { }

// Interfaces
public interface IFileMirrorOperation { }

// Enums
public enum FileSystemEventType
{
    Changed,
    Created,
    Deleted,
    Renamed,
    Error
}

// Constants
public static class Constants
{
    public const string VERSION = "1.0.0";
    public static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromMilliseconds(100);
}
```

## Code Structure

### File Organization

- **One class per file**
- **Filename matches class name** (case-sensitive)
- **Exception**: Empty custom exception types can be grouped

Example:
```
Config.cs       → Config class
ConfigParser.cs → ConfigParser class
ConfigStore.cs  → ConfigStore class
```

### Class Layout

Within each class (static first, then instance):

1. **Static consts** (top of class)
2. **Static fields**
3. **Instance fields**
4. **Properties**
5. **Public methods**
6. **Internal methods**
7. **Protected methods**
8. **Protected internal methods**
9. **Private methods**
10. **Constructors** (bottom of class)

### Example

```csharp
public class FileMirrorEngine
{
    // 1. Static consts (top)
    public const string DEFAULT_CONFIG_NAME = "config.json";
    public static readonly TimeSpan DEFAULT_BATCH_TIMEOUT = TimeSpan.FromMilliseconds(100);

    // 2. Instance fields
    private readonly ChangeQueue _changeQueue = new();
    private readonly RevertEngine _revertEngine = new();

    // 3. Properties
    public TimeSpan BatchTimeout { get; set; }

    // 4. Public methods
    public void ProcessChange(SourceMapping mapping, FileSystemEvent change) { }

    // 5. Internal methods
    internal void HandleCreated(string sourcePath, string targetPath) { }

    // 6. Private methods
    private void HandleDeleted(string sourcePath, string targetPath) { }

    // 7. Constructors (bottom)
    public FileMirrorEngine()
    {
        BatchTimeout = DEFAULT_BATCH_TIMEOUT;
    }
}
```

## Syntax Rules

### No `var`

Always use explicit types:

```csharp
// BAD - Don't do this:
var config = new Config();
var files = new List<string>();

// GOOD - Use explicit types:
Config config = new();
List<string> files = new();
```

**Exception**: Anonymous types require `var`:

```csharp
// Required for anonymous types:
var obj = new { sourcePath = "C:\\source", targetPath = "C:\\target" };
```

### Braces Always

Always use braces for control blocks:

```csharp
// BAD:
if (condition)
    DoSomething();

// GOOD:
if (condition)
{
    DoSomething();
}

// Exception: Ternary operator
string result = condition ? "yes" : "no";
```

### Ternary Operator

Use for simple conditional assignments:

```csharp
// GOOD:
string path = isDirectory ? sourcePath + "\\" : sourcePath;

// BAD: (complex, use if instead)
string path = condition1 ? value1 : condition2 ? value2 : condition3 ? value3 : defaultValue;
```

### Expression-Bodied Members

For simple members:

```csharp
// GOOD - Expression-bodied:
public int Count => _queue.Count;
public bool IsEmpty => _queue.Count == 0;
public string SourcePath { get; set; } = "";

// Use traditional for complex:
public void Process()
{
    // Multiple statements
    Validate();
    Execute();
    Log();
}
```

### Pattern Matching with Switch Expressions

```csharp
// GOOD:
public string GetStatus(FileSystemEventType type) => type switch
{
    FileSystemEventType.Created => "Created",
    FileSystemEventType.Changed => "Changed",
    FileSystemEventType.Deleted => "Deleted",
    FileSystemEventType.Renamed => "Renamed",
    FileSystemEventType.Error => "Error",
    _ => "Unknown"
};
```

### Tuple Return Values

For multiple return values:

```csharp
// GOOD:
public (string Path, bool Exists) GetInfo(string path)
{
    bool exists = File.Exists(path);
    return (path, exists);
}

// Usage:
(string path, bool exists) = GetInfo("C:\\source");
```

### Extension Methods

For domain operations:

```csharp
public static class PathExtensions
{
    public static bool IsDirectory(this string path)
    {
        return Directory.Exists(path);
    }

    public static bool IsFile(this string path)
    {
        return File.Exists(path);
    }
}

// Usage:
if ("C:\\source".IsDirectory())
{
    // Handle directory
}
```

### Immutability

Prefer immutable objects:

```csharp
// GOOD - Immutable:
public class Config
{
    public List<SourceMapping> SourceMappings { get; } = new();
    public TimeSpan ReloadInterval { get; set; }
    public bool BatchChanges { get; set; }
}

// BAD - Mutable collection:
public class Config
{
    public List<SourceMapping> SourceMappings { get; set; } = new();  // Can be replaced
}
```

### Readonly Fields

For immutable fields:

```csharp
public class FileMirrorEngine
{
    private readonly ChangeQueue _changeQueue = new();
    private readonly RevertEngine _revertEngine = new();

    public FileMirrorEngine()
    {
        _changeQueue = new ChangeQueue();  // Can only be set in constructor
    }
}
```

## FileMirror Specific Rules

### Constants

Use SCREAMING_SNAKE_CASE:

```csharp
public static class Constants
{
    public const string VERSION = "1.0.0";
    public static readonly TimeSpan DEFAULT_BATCH_TIMEOUT = TimeSpan.FromMilliseconds(100);
    public static readonly string DEFAULT_STATE_FILE = "state.json";
}
```

### No Comments

**Do not add comments unless explicitly requested for clarity.**

```csharp
// BAD:
public void Process()  // Process the change
{
    // Validate input
    if (input == null)
        throw new ArgumentNullException();
}

// GOOD:
public void Process()
{
    if (input == null)
        throw new ArgumentNullException();
}
```

**Exception**: Complex algorithms may need explanation:

```csharp
// Path calculation: replace source prefix with target prefix
string targetPath = mapping.TargetPath + sourcePath[mapping.SourcePath.Length..];
```

### No Factories/Services

Prefer direct instantiation:

```csharp
// BAD:
Config config = ConfigFactory.Create();
FileSystemWatcher watcher = new FileSystemWatcherService();

// GOOD:
Config config = new();
FileSystemWatcher watcher = new();
```

### Inheritance Over Composition

```csharp
// GOOD:
public class FileSystemWatcherWrapper : IDisposable { }

// BAD:
public class MyWatcher
{
    private readonly FileSystemWatcher _watcher;
    // Complex composition...
}
```

## Formatting

### Line Length

Maximum 120 characters per line.

### Spacing

- One space around operators: `x = y + z`
- No spaces after cast: `(string)value`
- No spaces after method name: `Method()`
- One blank line between members

### Indentation

Use **4 spaces** (not tabs):

```csharp
public class MyClass
{
    public void Method()
    {
        if (condition)
        {
            DoSomething();
        }
    }
}
```

### Using Statements

At top of file, alphabetically:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
```

### Namespace Formatting

```csharp
namespace FileMirror.Core.Mirroring;

public class FileMirrorEngine
{
    // ...
}
```

## Testing Style

### Test Naming

Format: `MethodName_Scenario_ExpectedBehavior`

```csharp
[Test]
public void ProcessChange_Created_FileMirrored()
{
    // Arrange, Act, Assert
}
```

### Test Structure

Always use Arrange, Act, Assert:

```csharp
[Test]
public void Method_Scenario_ExpectedBehavior()
{
    // Arrange - Setup
    var subject = new Subject();

    // Act - Execute
    var result = subject.Method();

    // Assert - Verify
    Assert.That(result, Is.EqualTo(expected));
}
```

### No Mocking

Use real objects:

```csharp
// BAD:
var mock = new Mock<IFileOperation>();
mock.Setup(x => x.Apply()).Returns(true);

// GOOD:
var operation = new FileMirrorOperation();
```

## Example Complete File

```csharp
using System;
using System.Collections.Generic;
using System.IO;

namespace FileMirror.Core.Config;

public class Config
{
    public static readonly TimeSpan DEFAULT_RELOAD_INTERVAL = TimeSpan.FromSeconds(5);

    public List<SourceMapping> SourceMappings { get; } = new();
    public TimeSpan ReloadInterval { get; set; } = DEFAULT_RELOAD_INTERVAL;

    public Config()
    {
        ReloadInterval = DEFAULT_RELOAD_INTERVAL;
    }
}

public class ConfigParser
{
    private readonly JsonSerializerSettings _settings = new();

    public Config Parse(string json)
    {
        return JsonConvert.DeserializeObject<Config>(json, _settings) ?? new Config();
    }

    public List<string> Validate(Config config)
    {
        List<string> errors = new();

        if (config.SourceMappings.Count == 0)
            errors.Add("At least one source mapping must be configured");

        return errors;
    }
}
```

## Linting

Run analyzers:

```bash
dotnet build -c Release
```

Or run analyzers directly:

```bash
dotnet build --no-restore
```

## Validation Checklist

Before committing:

- [ ] No `var` (except anonymous types)
- [ ] All methods have braces
- [ ] No comments unless explicitly requested
- [ ] Public members PascalCase
- [ ] Private fields `_camelCase`
- [ ] Constants SCREAMING_SNAKE_CASE
- [ ] One class per file
- [ ] Filename matches class name
- [ ] Tests use NUnit (not xUnit)
- [ ] Tests follow Arrange, Act, Assert
- [ ] No mocking frameworks used

## Further Reading

- Global AGENTS.md: C# coding standards
- NUnit documentation: Testing best practices
- Microsoft C# Coding Conventions: General style guide

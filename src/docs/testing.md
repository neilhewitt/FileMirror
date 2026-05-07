# Testing

## Overview

FileMirror uses **NUnit** for all testing. No mocking frameworks.

## Test Projects

```
src/
└── tests/
    └── FileMirror.Core.Tests/
        ├── TestBase.cs           # Test infrastructure
        ├── Config/               # Config tests
        │   ├── ConfigParserTests.cs
        │   └── ConfigStoreTests.cs
        ├── Monitoring/           # Monitoring tests
        │   ├── FileSystemWatcherWrapperTests.cs
        │   └── ChangeBatcherTests.cs
        └── Storage/              # Storage tests
            ├── StateStoreTests.cs
            └── ChangeQueueTests.cs
```

## Test Commands

### Run All Tests

```bash
cd src
dotnet test
```

### Run Specific Test Project

```bash
dotnet test tests/FileMirror.Core.Tests/
```

### Run Specific Test Class

```bash
dotnet test tests/FileMirror.Core.Tests/ --filter "FullyQualifiedName~ConfigParserTests"
```

### Run in Watch Mode (re-run on change)

```bash
dotnet watch test
```

### Run with Coverage

Install coverlet:
```bash
dotnet tool install --global coverlet.console
```

Run with coverage:
```bash
cd tests/FileMirror.Core.Tests
coverlet . --format cobertura
```

Output: `coverage.cobertura.xml`

## Test Infrastructure

### TestBase

Abstract base class for tests with common setup.

```csharp
public abstract class TestBase : IDisposable
{
    protected string TestBasePath { get; }
    protected string SourcePath { get; }
    protected string TargetPath { get; }
    protected string StatePath { get; }
}
```

**Features:**
- Isolated test directories
- Automatic cleanup
- Configurable test path

**Usage:**

```csharp
[TestFixture]
public class MyTests : TestBase
{
    [SetUp]
    public void Setup()
    {
        // TestBase creates SourcePath, TargetPath, StatePath
    }

    [Test]
    public void MyTest()
    {
        // Use TestBase.SourcePath, etc.
    }

    // TearDown automatically cleans up
}
```

### Default Test Location

Tests run in `%TEMP%\FileMirrorTest_{GUID}`:
```
C:\Users\neilh\AppData\Local\Temp\FileMirrorTest_xxx
```

### Custom Test Location

Set environment variable:

```powershell
$env:FILEMIRROR_TEST_PATH="D:\FileMirrorTest"
```

**Safety:** Tests refuse to run in repository directory.

## Test Naming Convention

Format: `MethodName_Scenario_ExpectedBehavior`

### Good Examples

```csharp
[Test]
public void Parse_FullConfig_ParsesAllFields()
{
    // Arrange
    // Act
    // Assert
}

[Test]
public void Validate_EmptyMappings_ReturnsError()
{
    // Arrange
    // Act
    // Assert
}

[Test]
public void Enqueue_Dequeue_FIFOOrder()
{
    // Arrange
    // Act
    // Assert
}
```

### Bad Examples

```csharp
// Don't do this:
[Test]
public void Test1() { }  // Too generic

[Test]
public void TestParse() { }  // Doesn't specify scenario

[Test]
public void ShouldWork() { }  // Doesn't specify expected behavior
```

## Test Types

### Unit Tests

Test individual components in isolation.

#### Config Parser Test

```csharp
[Test]
public void Parse_FullConfig_ParsesAllFields()
{
    // Arrange
    string json = @"{
        ""sourceMappings"": [{
            ""sourcePath"": ""C:\\source"",
            ""targetPath"": ""C:\\target"",
            ""recursive"": false
        }],
        ""reloadInterval"": ""00:00:10"",
        ""batchChanges"": false
    }";

    ConfigParser parser = new();

    // Act
    Config config = parser.Parse(json);

    // Assert
    Assert.That(config.SourceMappings.Count, Is.EqualTo(1));
    Assert.That(config.SourceMappings[0].SourcePath, Is.EqualTo("C:\\source"));
    Assert.That(config.ReloadInterval, Is.EqualTo(TimeSpan.FromSeconds(10)));
}
```

### State Persistence Tests

```csharp
[Test]
public void UpdateFileState_UpdatesExistingFile()
{
    // Arrange
    FileState fileState1 = new()
    {
        Path = "C:\\source\\test.txt",
        LastModified = DateTime.Now.AddMinutes(-1),
        Length = 100,
        Attributes = FileAttributes.Normal
    };
    _store.UpdateFileState("C:\\source\\test.txt", fileState1);

    FileState fileState2 = new()
    {
        Path = "C:\\source\\test.txt",
        LastModified = DateTime.Now,
        Length = 200,
        Attributes = FileAttributes.Archive
    };

    // Act
    _store.UpdateFileState("C:\\source\\test.txt", fileState2);

    // Assert
    FileState? result = _store.GetFileState("C:\\source\\test.txt");
    Assert.That(result!.Length, Is.EqualTo(200));
}
```

### Monitoring Tests

```csharp
[Test]
public void Constructor_SubscribesToEvents()
{
    // Arrange
    bool eventFired = false;
    _watcher.OnFileChanged += (_) => eventFired = true;

    _watcher.Start();

    // Act
    string testFile = Path.Combine(SourcePath, "test.txt");
    File.WriteAllText(testFile, "test content");
    Thread.Sleep(100);

    _watcher.Stop();

    // Assert
    Assert.That(eventFired, Is.True);
}
```

## Parameterized Tests

Use `[TestCase]` for data-driven testing.

### Multiple Configurations

```csharp
[TestCase("C:\\source", "C:\\target", true)]
[TestCase("D:\\data", "E:\\backup", false)]
public void Parse_SourceMapping_ParsesCorrectly(string source, string target, bool recursive)
{
    // Arrange
    string json = $@"{{ ""sourceMappings"": [{{ ""sourcePath"": ""{source}"", ""targetPath"": ""{target}"", ""recursive"": {recursive.ToLower()} }}] }}";

    ConfigParser parser = new();

    // Act
    Config config = parser.Parse(json);

    // Assert
    Assert.That(config.SourceMappings.Count, Is.EqualTo(1));
    Assert.That(config.SourceMappings[0].SourcePath, Is.EqualTo(source));
    Assert.That(config.SourceMappings[0].Recursive, Is.EqualTo(recursive));
}
```

## Test Coverage

### Current Coverage

- **Config module**: 100%
- **Monitoring module**: 100%
- **Storage module**: 100%

### Target Coverage

- Minimum: 80%
- Ideal: 95%

### Checking Coverage

Install coverlet:
```bash
dotnet tool install --global coverlet.console
```

Generate report:
```bash
cd tests/FileMirror.Core.Tests
coverlet . --format cobertura --output ../TestResults/
```

## Test Guidelines

### 1. Arrange, Act, Assert

```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up test
    var subject = new Subject();
    
    // Act - Perform action
    var result = subject.Method();
    
    // Assert - Verify result
    Assert.That(result, Is.EqualTo(expected));
}
```

### 2. Use NUnit Asserts

```csharp
Assert.That(actual, Is.EqualTo(expected));
Assert.That(actual, Is.Not.Null);
Assert.That(collection, Has.Count.EqualTo(3));
Assert.That(collection, Has.Some.EqualTo(expected));
Assert.That(exception, Is.TypeOf<ArgumentException>());
```

### 3. Clean Up Resources

```csharp
[Test]
public void TestWithFiles()
{
    string tempFile = Path.Combine(Path.GetTempPath(), "test.txt");
    
    try
    {
        // Test code...
    }
    finally
    {
        if (File.Exists(tempFile))
            File.Delete(tempFile);
    }
}
```

### 4. Test Edge Cases

```csharp
[Test]
public void Parse_EmptyConfig_ReturnsDefault()
{
    // Test empty JSON
    string json = "{}";
    
    ConfigParser parser = new();
    Config config = parser.Parse(json);
    
    Assert.That(config.SourceMappings.Count, Is.EqualTo(0));
    Assert.That(config.ReloadInterval, Is.EqualTo(TimeSpan.FromSeconds(5)));
}

[Test]
public void Parse_InvalidJson_ReturnsDefault()
{
    // Test malformed JSON
    string json = "{ invalid }";
    
    ConfigParser parser = new();
    Config config = parser.Parse(json);
    
    Assert.That(config.SourceMappings.Count, Is.EqualTo(0));
}
```

### 5. Test Error Conditions

```csharp
[Test]
public void Validate_MissingSourcePath_ReturnsError()
{
    Config config = new();
    config.SourceMappings.Add(new SourceMapping
    {
        SourcePath = "",  // Empty
        TargetPath = "C:\\target"
    });

    ConfigParser parser = new();
    List<string> errors = parser.Validate(config);

    Assert.That(errors, Has.Some.Contains("SourcePath is required"));
}
```

## CI/CD Integration

### GitHub Actions

```yaml
- name: Run Tests
  run: dotnet test src -c Release
```

### Azure DevOps

```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: 'src/tests/FileMirror.Core.Tests/FileMirror.Core.Tests.csproj'
```

## Troubleshooting

### Tests Not Found

Check:
1. Test project references NUnit
2. Test methods have `[Test]` attribute
3. Test class has `[TestFixture]` attribute

### Tests Hanging

Check:
1. Resources properly disposed
2. No blocking operations
3. TestTimeout attribute used:

```csharp
[Test, Timeout(5000)]
public void SlowOperation_Test()
{
    // Test code...
}
```

### Test Output Not Showing

Enable output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Permission Errors

Check:
1. Test directory exists
2. User has write permissions
3. No antivirus blocking file operations

## Best Practices

### Test Isolation

Each test should:
- Use unique test data
- Not depend on other tests
- Clean up after itself

### Test Speed

- Unit tests: <100ms
- Integration tests: <1s
- E2E tests: <10s

### Test Reliability

- Deterministic (same result every run)
- No external dependencies
- No timing assumptions (use mocks for timing)

## Example Test Suite

```csharp
[TestFixture]
public class ConfigParserTests : TestBase
{
    private ConfigParser _parser = null!;

    [SetUp]
    public void Setup()
    {
        _parser = new ConfigParser();
    }

    [Test]
    public void Parse_MinimalConfig_CreatesDefaultConfig()
    {
        string json = @"{ ""sourceMappings"": [] }";
        Config config = _parser.Parse(json);

        Assert.That(config.SourceMappings.Count, Is.EqualTo(0));
        Assert.That(config.ReloadInterval, Is.EqualTo(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Validate_ValidConfig_ReturnsEmpty()
    {
        Config config = new();
        config.SourceMappings.Add(new SourceMapping("C:\\source", "C:\\target"));
        
        List<string> errors = _parser.Validate(config);

        Assert.That(errors.Count, Is.EqualTo(0));
    }
}
```

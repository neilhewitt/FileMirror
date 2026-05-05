# Testing Skill

When writing or modifying tests for FileMirror:

1. **Use NUnit only** - never xUnit
2. **Test base classes** - create base test classes for shared setup
3. **Parameterized tests** - use `[TestCase]` for data-driven testing
4. **No mocking frameworks** - use real objects only
5. **Structure** - arrange, act, assert in that order
6. **Test naming** - `MethodName_Scenario_ExpectedBehavior`
7. **Test files** - in `tests/` directory, match source file path structure

Example test structure:

```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var subject = new Subject();
    
    // Act
    var result = subject.Method();
    
    // Assert
    Assert.That(result, Is.EqualTo(expected));
}
```

Always run tests after code changes:
```bash
dotnet test
```
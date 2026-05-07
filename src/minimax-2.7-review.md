# Code Review: FileMirror

**Review Date:** May 6, 2026
**Reviewer:** MiniMax-IQ3
**Target Framework:** .NET 10

---

## Executive Summary

The codebase implements a file mirroring tool with real-time file monitoring. The architecture is straightforward but has significant issues with resource management, coding standard violations, and a potential bug. The code needs moderate refactoring before production use.

---

## Critical Issues

### 1. Duplicate File.Copy Bug

**File:** `src/FileMirror.Core/Mirroring/FileMirrorEngine.cs:49`

```csharp
File.Copy(sourcePath, targetPath, true);
File.Copy(sourcePath, targetPath, true);  // Duplicate!
```

The `HandleCreated` method calls `File.Copy` twice for the same operation. This is wasteful and could cause performance issues.

**Fix:**
```csharp
Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
File.Copy(sourcePath, targetPath, true);
File.SetAttributes(targetPath, File.GetAttributes(sourcePath));
```

---

### 2. Resource Leaks - Undisposed FileSystemWatcher and Timer Objects

Multiple classes create `IDisposable` objects that are never disposed:

- **FileSystemWatcherWrapper.cs:16** - Creates `FileSystemWatcher` but never disposes it
- **ChangeBatcher.cs:19** - Creates `Timer` but never disposes it
- **ConfigStore.cs:56** - Creates `FileSystemWatcher` in `Watch()` but never disposes it
- **FileMirror.cs:117-125** - Creates watchers in `RunForeground` but never disposes them
- **FileMirrorService.cs:27-33** - Creates watchers in `OnStart` but never disposes them in `OnStop`

**Fix Prompt:**
```
Create a fix for the resource leaks in FileSystemWatcherWrapper.cs. The class should implement IDisposable and properly dispose the _watcher field. Apply the same pattern to ChangeBatcher.cs for the _timer field, and update ConfigStore.cs Watch() method to return a disposable watcher or use a different pattern for hot-reloading config.
```

---

### 3. DateTime Equality Comparison

**File:** `src/FileMirror.Core/Mirroring/RevertEngine.cs:21`

```csharp
if (source.LastWriteTime != target.LastWriteTime || source.Length != target.Length)
```

Comparing `DateTime` values for exact equality is problematic due to file system precision limitations. Should use a tolerance.

**Fix Prompt:**
```
Fix the DateTime comparison in RevertEngine.cs:21. Use a tolerance (e.g., 1 second) when comparing LastWriteTime values instead of exact equality. This accounts for file system precision limitations.
```

---

## Coding Standards Violations

### Global AGENTS.md Violations

#### 1. Using `var` Instead of Explicit Types

The AGENTS.md specifies: **"Never use `var`"** and **"Always use explicit type with target-typed `new()`"**

**Files with violations:**
- `FileMirrorEngine.cs:12-14` - `Dictionary<string, FileState>`, `ChangeQueue`, `RevertEngine`
- `Config.cs:8` - `List<SourceMapping>`
- `MirroredState.cs:11` - `Dictionary<string, FileState>`
- `ConfigParserTests.cs:75,85` - `new Config()`
- `ConfigStoreTests.cs:44,50` - `new Config()`
- `FileMirror.cs:43-45,70,117,122` - Multiple instances
- `FileMirrorService.cs:22,25,30` - Multiple instances

**Fix Prompt:**
```
Enforce explicit type declarations throughout the codebase. Replace all `new()` calls with explicit types (e.g., `ConfigParser parser = new ConfigParser()` instead of `var parser = new()`). This applies to all files in src/FileMirror.Core/, src/FileMirror/, and src/FileMirror.Service/.
```

#### 2. Using `Substring` Instead of Modern Range Syntax

The AGENTS.md specifies: **"Use indices/ranges instead (`text[5..10]` not `text.Substring(5, 5)`)"**

**Files with violations:**
- `FileMirrorEngine.cs:19` - `sourcePath.Substring(mapping.SourcePath.Length)`
- `FileMirrorEngine.cs:99-100` - `targetPath.Substring(0, targetPath.Length - ...)`

**Fix Prompt:**
```
Replace Substring calls with modern range syntax in FileMirrorEngine.cs. Line 19 should use `sourcePath[mapping.SourcePath.Length..]` and lines 99-100 should use range syntax instead of Substring.
```

#### 3. Missing Braces on If Statements

**Files with violations:**
- `ConfigParser.cs:25-26` - Single line if without braces
- `ConfigStore.cs:53-54` - Single line if without braces

**Fix Prompt:**
```
Add braces to single-line if statements in ConfigParser.cs:25-26 and ConfigStore.cs:53-54 to comply with coding standards.
```

---

### FileMirror AGENTS.md Violations

#### 1. Extensive Use of `new()` Without Explicit Type

The local AGENTS.md reinforces the global rule about not using `var` and using explicit types with `new()`. The codebase violates this extensively.

---

## Architecture Issues

### 1. Unused Interfaces

**Files:**
- `src/FileMirror.Core/Mirroring/IDirectoryMirrorOperation.cs`
- `src/FileMirror.Core/Mirroring/IFileMirrorOperation.cs`

These interfaces are defined but never implemented or used anywhere in the codebase.

**Fix Prompt:**
```
Remove the unused interfaces IDirectoryMirrorOperation.cs and IFileMirrorOperation.cs, or implement them if they were intended for a plugin system. Unused dead code should be removed.
```

### 2. Incomplete Background Mode

**File:** `src/FileMirror/Program.cs:133-136`

```csharp
private static void RunBackground(Config config)
{
    Console.WriteLine("Starting FileMirror in background mode...");
}
```

This method doesn't actually run anything in background mode - it just prints a message and returns.

**Fix Prompt:**
```
Implement the RunBackground method in FileMirror/Program.cs to actually run the file mirroring service in background mode. This likely involves using a proper hosting mechanism or background service pattern.
```

### 3. Inconsistent RevertEngine Logic

**File:** `src/FileMirror.Core/Mirroring/RevertEngine.cs:44-46`

```csharp
if (source.Attributes != target.Attributes)
{
    Directory.SetLastWriteTime(target.FullName, source.LastWriteTime);
}
```

The condition checks `Attributes` but sets `LastWriteTime`. This appears inconsistent.

**Fix Prompt:**
```
Review and fix the logic in RevertEngine.cs:32-48 (DetectAndRevert for DirectoryInfo). The condition checks Attributes but sets LastWriteTime - these should be consistent.
```

---

## Security Issues

### 1. Path Traversal Potential

**File:** `src/FileMirror.Core/Mirroring/FileMirrorEngine.cs:19`

```csharp
string targetPath = mapping.TargetPath + sourcePath.Substring(mapping.SourcePath.Length);
```

If `sourcePath` doesn't start with `mapping.SourcePath`, this could lead to unexpected target paths. No validation is performed.

**Fix Prompt:**
```
Add path validation in FileMirrorEngine.cs ProcessChange method. Verify that the source path actually starts with the mapping's source path before constructing the target path. Throw an exception or log an error if path validation fails.
```

### 2. Using Newtonsoft.Json Instead of System.Text.Json

**Files:**
- `StateStore.cs:7`
- `ConfigParser.cs:5-6`
- `ConfigStore.cs:5`

The codebase uses `Newtonsoft.Json` (v13.0.3) instead of `System.Text.Json` which is the modern .NET approach and has better performance and security characteristics.

**Fix Prompt:**
```
Replace Newtonsoft.Json usage with System.Text.Json throughout the codebase. Update StateStore.cs, ConfigParser.cs, and ConfigStore.cs to use System.Text.Json instead. This requires changing JsonConvert.DeserializeObject to JsonSerializer.Deserialize, JsonConvert.SerializeObject to JsonSerializer.Serialize, and related changes.
```

---

## Dependency Issues

### 1. Unused Packages

**File:** `src/FileMirror.Core/FileMirror.Core.csproj`

- `System.IO.Abstractions` (v21.0.10) - Listed but not used anywhere in the code

**File:** `src/FileMirror/FileMirror.csproj`

- `System.CommandLine` (v2.0.0-beta4.22272.1) - Listed but the code manually parses arguments instead of using this library

**Fix Prompt:**
```
Remove unused packages from the project files: System.IO.Abstractions from FileMirror.Core.csproj and System.CommandLine from FileMirror.csproj. If these packages are needed for future functionality, they should be used properly.
```

---

## Summary of Fix Prompts

1. **Fix duplicate File.Copy bug in FileMirrorEngine.cs:49**

2. **Fix resource leaks - implement IDisposable on FileSystemWatcherWrapper, ChangeBatcher, and ConfigStore**

3. **Fix DateTime comparison with tolerance in RevertEngine.cs:21**

4. **Replace all `new()` with explicit types throughout the codebase**

5. **Replace Substring with range syntax in FileMirrorEngine.cs**

6. **Add braces to single-line if statements in ConfigParser.cs and ConfigStore.cs**

7. **Remove unused interfaces IDirectoryMirrorOperation.cs and IFileMirrorOperation.cs**

8. **Implement RunBackground method in FileMirror/Program.cs**

9. **Fix inconsistent logic in RevertEngine.cs DetectAndRevert for DirectoryInfo**

10. **Add path validation in FileMirrorEngine.cs ProcessChange method**

11. **Replace Newtonsoft.Json with System.Text.Json**

12. **Remove unused packages System.IO.Abstractions and System.CommandLine**
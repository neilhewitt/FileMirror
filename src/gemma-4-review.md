# FileMirror Code Review Findings

## Summary of Issues

### 1. Design & Implementation Issues

#### `FileMirror.Core`
- **`FileMirrorEngine.cs`**:
    - **Bug**: Duplicate `File.Copy` in `HandleCreated`.
    - **Bug**: Uses `.Substring()` instead of indices/ranges (Standard violation).
    - **Logic Error**: Redundant/confusing logic in `HandleDeleted`.
    - **Unused Code**: `_fileStates` field is unused.
- **`ConfigParser.cs`**:
    - **Robustness**: `TimeSpan.Parse` can throw exceptions; use `TryParse`.
    - **Implementation**: Manual JToken traversal instead of direct deserialization to `Config`.
- **`StateStore.cs`**:
    - **Critical Bug**: `Load` does not update the internal `_state` field.
    - **Design Inconsistency**: Ambiguous path management between constructor and methods.
- **`RevertEngine.cs`**:
    - **Incomplete Logic**: Directory attribute synchronization is insufficient.

#### `FileMirror` (CLI)
- **Incomplete Feature**: `RunBackground` is not implemented.
- **Implementation**: `RunForeground` uses a busy-wait loop and lacks graceful shutdown.
- **Implementation**: Manual argument parsing instead of using `System.CommandLine`.

#### `FileMirror.Service`
- **Bug**: `OnStart` method has an incorrect signature.

### 2. Coding Standards Adherence
- **Indices/Ranges**: Violated in `FileMirrorEngine.cs`.
- **No `var`**: Generally followed.
- **Constants**: Followed.

---

## Proposed Agent Tasks

### Task 1: Fix Core Engine and Logic Errors
**Agent Type**: `general`
**Prompt**: 
Refactor `src/FileMirror.Core/Mirroring/FileMirrorEngine.cs` to:
1. Remove the duplicate `File.Copy` call in `HandleCreated`.
2. Replace all uses of `.Substring()` with C# indices and ranges (e.g., `[start..end]`) to adhere to coding standards.
3. Simplify the logic in `HandleDeleted` to be more readable and remove redundant checks.
4. Remove the unused `_fileStates` dictionary.
5. Ensure `HandleRenamed` correctly calculates the old target path using ranges.

### Task 2: Improve Configuration and State Management
**Agent Type**: `general`
**Prompt**: 
Improve the robustness and implementation of the following files:
1. `src/FileMirror.Core/Config/ConfigParser.cs`: 
    - Use `TimeSpan.TryParse` for `ReloadInterval`.
    - Replace manual `JObject` traversal with direct `JsonConvert.DeserializeObject<Config>(json)`.
2. `src/FileMirror.Core/Storage/StateStore.cs`:
    - Fix the `Load` method so that it correctly updates the internal `_state` field.
    - Unify path handling so the class uses a single source of truth for the state file path (prefer the constructor-provided path).

### Task 3: Complete CLI and Service Implementation
**Agent Type**: `general`
**Prompt**: 
Complete the host implementations:
1. `src/FileMirror/Program.cs`:
    - Implement `RunBackground` (this might require integration with the Service host or a background process logic).
    - Refactor `ParseArguments` to use the `System.CommandLine` library already present in the project.
    - Improve `RunForeground` to avoid the busy-wait loop and handle `Ctrl+C` gracefully to stop watchers.

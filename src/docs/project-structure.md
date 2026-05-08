# Project Structure

## Directory Layout

```
src/
├── FileMirror/                      # Main CLI application
│   ├── Program.cs                  # Entry point, CLI parsing
│   │   ├── Config/                     # Configuration system
│   │   ├── Config.cs               # Main configuration object
│   │   ├── ConfigParser.cs         # JSON parsing and validation
│   │   ├── ConfigStore.cs          # Load/save config, hot-reload
│   │   └── SourceMapping.cs        # Single source→target mapping
│   │
│   ├── Monitoring/                 # File system monitoring
│   │   ├── FileSystemWatcherWrapper.cs  # FileSystemWatcher wrapper
│   │   ├── FileSystemEvent.cs      # Change event encapsulation
│   │   └── FileSystemEventType.cs  # Event type enum
│   │
│   ├── Mirroring/                  # Core mirroring logic
│   │   ├── FileMirrorEngine.cs     # Main mirroring engine
│   │
│   └── Storage/                    # State persistence
│       ├── FileState.cs            # Individual file state
│       ├── MirroredState.cs        # Complete state for mapping
│       ├── StateStore.cs           # Load/save state to disk
│       └── ChangeQueue.cs          # FIFO queue for offline changes
│
├── FileMirror.Service/             # Windows Service wrapper
│   ├── Program.cs                  # Service entry point
│   └── FileMirrorService.cs        # Service implementation
│
├── tests/
│   └── FileMirror.Core.Tests/      # Core logic tests
│       ├── TestBase.cs             # Test infrastructure
│       ├── Config/                 # Config tests
│       │   ├── ConfigParserTests.cs
│       │   └── ConfigStoreTests.cs
│       ├── Monitoring/             # Monitoring tests
│       │   └── FileSystemWatcherWrapperTests.cs
│       └── Storage/                # Storage tests
│           ├── StateStoreTests.cs
│           └── ChangeQueueTests.cs
│
├── docs/                           # This documentation
├── plans/                          # Implementation plans (internal)
└── plans/                          # Implementation plans (internal)
```

## Project Dependencies

### FileMirror.Core.csproj

- System.IO.Abstractions (testability)
- Newtonsoft.Json (config serialization)

### FileMirror.csproj

- FileMirror.Core
- System.CommandLine (CLI parsing)

### FileMirror.Service.csproj

- FileMirror.Core
- Microsoft.Extensions.Hosting.WindowsServices

### FileMirror.Core.Tests.csproj

- FileMirror.Core
- NUnit (testing framework)
- NUnit.Analyzers (style checks)
- NUnit3TestAdapter (test runner)
- Microsoft.NET.Test.Sdk (test infrastructure)

## Namespace Mapping

| Folder | Namespace |
|--------|-----------|
| Config | `FileMirror.Core.Config` |
| Monitoring | `FileMirror.Core.Monitoring` |
| Mirroring | `FileMirror.Core.Mirroring` |
| Storage | `FileMirror.Core.Storage` |
| Tests | `FileMirror.Core.Tests` |

## Building

```bash
# Build all projects
dotnet build

# Build specific project
dotnet build FileMirror/FileMirror.csproj
dotnet build FileMirror.Core/FileMirror.Core.csproj
dotnet build FileMirror.Service/FileMirror.Service.csproj

# Build release
dotnet build -c Release

# Publish single binary
dotnet publish -c Release --self-contained true /p:PublishSingleFile=true
```

## Adding New Features

1. Add new classes to appropriate `src/FileMirror.Core/[Area]/` folder
2. Add tests to `src/tests/FileMirror.Core.Tests/[Area]/`
3. Follow naming conventions (PascalCase for classes, methods, properties)
4. Run tests: `dotnet test`
5. Build: `dotnet build`

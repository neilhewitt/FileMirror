# Project Structure Setup

## Goal
Create the initial .NET 10 project structure for FileMirror following YAGNI principles.

## Requirements

### Project Structure
```
FileMirror/
├── src/
│   ├── FileMirror/              # Main CLI application project
│   │   ├── CommandLineOptions.cs
│   │   └── Program.cs
│   ├── FileMirror.Core/         # Core domain logic
│   │   ├── Config/              # Configuration handling
│   │   │   ├── Config.cs
│   │   │   ├── ConfigParser.cs
│   │   │   ├── ConfigStore.cs
│   │   │   └── SourceMapping.cs
│   │   ├── Monitoring/          # File system monitoring
│   │   │   ├── ChangeBatcher.cs
│   │   │   ├── FileSystemEvent.cs
│   │   │   ├── FileSystemEventType.cs
│   │   │   └── FileSystemWatcherWrapper.cs
│   │   ├── Mirroring/           # Core mirroring logic
│   │   │   ├── FileMirrorEngine.cs
│   │   │   ├── IFileMirrorOperation.cs
│   │   │   ├── IDirectoryMirrorOperation.cs
│   │   │   └── RevertEngine.cs
│   │   └── Storage/             # State persistence
│   │       ├── ChangeQueue.cs
│   │       ├── FileState.cs
│   │       ├── MirroredState.cs
│   │       └── StateStore.cs
│   ├── FileMirror.Service/      # Windows Service wrapper
│   │   ├── FileMirrorService.cs
│   │   └── Program.cs
│   └── tests/
│       ├── FileMirror.Core.Tests/   # Core logic tests
│       │   └── Config/
│       │       ├── ConfigParserTests.cs
│       │       └── ConfigStoreTests.cs
│       └── FileMirror.Service.Tests/# Service tests
├── AGENTS.md
└── AGENTS/
    ├── skill-testing.md
    ├── skill-build.md
    └── skill-domain.md
```

### Project Files
1. Create `FileMirror.Core/FileMirror.Core.csproj` - class library, .NET 10
2. Create `FileMirror/FileMirror.csproj` - console app, .NET 10, self-contained
3. Create `FileMirror.Service/FileMirror.Service.csproj` - Windows Service, .NET 10

### Dependencies
- System.IO.Abstractions (for testability without mocking)
- Microsoft.Extensions.Hosting.WindowsServices (for Windows Service)
- Newtonsoft.Json (for config)

## Instructions
1. Create directory structure matching above
2. Create .csproj files with correct TargetFramework and OutputType
3. Add project references (FileMirror → FileMirror.Core, FileMirror.Service → FileMirror.Core)
4. Add NuGet package references
5. Create placeholder Program.cs files with Main methods
6. Run `dotnet build` to verify structure

## Acceptance Criteria
- ✅ All projects build without errors
- ✅ Test project structure is ready for NUnit tests
- ✅ No code generation or complex scaffolding

## Implementation Status
- ✅ Project structure created
- ✅ All .csproj files configured
- ✅ Project references added
- ✅ NuGet packages added
- ✅ All programs have Main methods
# Build Skill

FileMirror build requirements:

## Building
```bash
# Build solution
dotnet build

# Build release configuration
dotnet build -c Release

# Publish self-contained single binary
dotnet publish -c Release --self-contained true /p:PublishSingleFile=true
```

## Directory Structure
```
FileMirror/
├── src/              # Source code
│   ├── FileMirror/  # Main application
│   └── FileMirror.Core/ # Core domain logic
├── tests/           # NUnit tests
└── AGENTS.md        # This file
```

## Publishing
- Target framework: .NET 10
- Single self-contained binary for distribution
- Windows Service and CLI hosting supported

## Build Artifacts
- Binary location: `bin/Release/net10.0/publish/`
- Test results: `TestResults/`
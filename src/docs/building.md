# Building

## Prerequisites

- .NET 10 SDK
- Windows 10/11 or Windows Server 2016+

## Build Commands

### Build All Projects

```bash
cd src
dotnet build
```

### Build Release Configuration

```bash
dotnet build -c Release
```

### Build Specific Project

```bash
# CLI application
dotnet build FileMirror/FileMirror.csproj

# Core library
dotnet build FileMirror.Core/FileMirror.Core.csproj

# Windows Service
dotnet build FileMirror.Service/FileMirror.Service.csproj
```

## Publishing

### CLI Application (Single Binary)

```bash
dotnet publish -c Release --self-contained true /p:PublishSingleFile=true
```

Output location:
```
src/bin/Release/net10.0/publish/FileMirror.exe
```

### Windows Service

```bash
cd src
dotnet build FileMirror.Service -c Release
```

Output location:
```
src/bin/Release/net10.0/FileMirror.Service.exe
```

## Build Output

```
src/
├── bin/
│   └── Release/
│       └── net10.0/
│           ├── FileMirror.Core.dll
│           ├── FileMirror.Core.pdb
│           ├── FileMirror.exe
│           ├── FileMirror.pdb
│           ├── FileMirror.Service.exe
│           └── FileMirror.Service.pdb
└── obj/
    └── Release/
        └── net10.0/
            └── [build artifacts]
```

## Dependencies

### FileMirror.Core

```xml
<ItemGroup>
  <PackageReference Include="System.IO.Abstractions" Version="21.0.22" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
</ItemGroup>
```

### FileMirror

```xml
<ItemGroup>
  <ProjectReference Include="..\FileMirror.Core\FileMirror.Core.csproj" />
</ItemGroup>
<ItemGroup>
  <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
</ItemGroup>
```

### FileMirror.Service

```xml
<ItemGroup>
  <ProjectReference Include="..\FileMirror.Core\FileMirror.Core.csproj" />
</ItemGroup>
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Hosting.WindowsServices" Version="9.0.0" />
</ItemGroup>
```

### FileMirror.Core.Tests

```xml
<ItemGroup>
  <PackageReference Include="NUnit" Version="4.2.2" />
  <PackageReference Include="NUnit.Analyzers" Version="4.3.0" />
  <PackageReference Include="NUnit3TestAdapter" Version="4.6.0" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
</ItemGroup>
```

## Build Scripts

### Windows PowerShell

```powershell
# Build release
dotnet build -c Release

# Publish single binary
dotnet publish -c Release --self-contained true /p:PublishSingleFile=true
```

### Bash (WSL)

```bash
# Build release
dotnet build -c Release

# Publish single binary
dotnet publish -c Release --self-contained true /p:PublishSingleFile=true
```

## Verification

### Check Build Output

```bash
# List published files
ls src/bin/Release/net10.0/publish/

# Should include:
# FileMirror.exe
```

### Verify Single Binary

```bash
# Check file size (should be ~30-40 MB)
ls -lh FileMirror.exe

# Should show standalone executable
```

## Troubleshooting

### Build Fails

Check:
1. .NET 10 SDK installed:
   ```bash
   dotnet --version
   # Should show: 10.0.x
   ```

2. All projects in solution:
   ```bash
   dotnet list src/FileMirror.slnx
   ```

### Missing Dependencies

Restore packages:
```bash
dotnet restore
```

### Permission Denied

Run with elevated privileges (Windows):
```bash
# Run PowerShell as Administrator
dotnet build
```

## CI/CD

### GitHub Actions Example

```yaml
name: Build

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET 10
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore dependencies
      run: dotnet restore src
    
    - name: Build
      run: dotnet build src -c Release --no-restore
    
    - name: Test
      run: dotnet test src -c Release --no-build
    
    - name: Publish
      run: dotnet publish src/FileMirror/FileMirror.csproj -c Release --self-contained true /p:PublishSingleFile=true
```

### Azure DevOps Example

```yaml
pool:
  vmImage: 'windows-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '10.0.x'

- task: DotNetCoreCLI@2
  inputs:
    command: 'restore'
    projects: 'src/FileMirror.slnx'

- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    projects: 'src/FileMirror.slnx'
    arguments: '-c Release'

- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: 'src/tests/FileMirror.Core.Tests/FileMirror.Core.Tests.csproj'

- task: DotNetCoreCLI@2
  inputs:
    command: 'publish'
    projects: 'src/FileMirror/FileMirror.csproj'
    arguments: '-c Release --self-contained true /p:PublishSingleFile=true'
```

## Release Notes

### Version 1.0.0

- Single self-contained binary
- Windows Service support
- Hot-reload configuration
- State persistence
- Change batching
- One-way mirroring
- Dead path handling
- Offline recovery

## Performance

### Build Times

- Clean build: ~30 seconds
- Incremental: ~3-5 seconds
- Publish: ~10-15 seconds

### Binary Size

- FileMirror.exe: ~35 MB (self-contained)

### Build Optimization

```bash
# Faster builds (debug, no publish)
dotnet build -c Debug

# Enable ready-to-run for faster startup
dotnet publish -c Release /p:PublishReadyToRun=true
```

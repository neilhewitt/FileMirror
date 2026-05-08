# FileMirror Deployment Scripts

## FileMirror-Deploy-CLI.ps1

Builds and deploys the FileMirror CLI to a specified folder.

### Usage

```powershell
.\FileMirror-Deploy-CLI.ps1 -OutputPath "C:\path\to\deploy"
```

### Parameters

- `-OutputPath` (Required): Destination folder for the deployed files
- `-Configuration`: Build configuration (Debug or Release, default: Release)
- `-ProjectPath`: Path to the CLI project file (default: src/FileMirror/FileMirror.csproj)

### Output

Deploys the following files to the output folder:
- `FileMirror.exe` - Self-contained single-file executable
- `FileMirror.Core.pdb` - Debug symbols (optional)

### Running the CLI

```cmd
FileMirror.exe --config <path-to-config.ini>
```

## FileMirror-Deploy-Service.ps1

Builds and deploys the FileMirror Windows Service to a specified folder, along with an installation script.

### Usage

```powershell
.\FileMirror-Deploy-Service.ps1 -OutputPath "C:\path\to\deploy"
```

### Parameters

- `-OutputPath` (Required): Destination folder for the deployed files
- `-Configuration`: Build configuration (Debug or Release, default: Release)
- `-ProjectPath`: Path to the service project file (default: src/FileMirror.Service/FileMirror.Service.csproj)
- `-InstallScriptName`: Name of the installation script (default: Install-FileMirrorService.ps1)

### Output

Deploys the following files to the output folder:
- `FileMirror.Service.exe` - Self-contained single-file executable
- `FileMirror.Core.pdb` - Debug symbols (optional)
- `Install-FileMirrorService.ps1` - PowerShell script to install the Windows Service

### Installing the Service

Run the installation script with administrator privileges:

```powershell
# In an elevated PowerShell session
.\Install-FileMirrorService.ps1
```

The service is installed as "FileMirror" and configured to start automatically. Log files are written to a `logs` subfolder.

### Uninstalling the Service

```powershell
sc.exe delete FileMirror
```

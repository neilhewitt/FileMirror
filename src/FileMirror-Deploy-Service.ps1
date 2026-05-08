# FileMirror-Deploy-Service.ps1
# Builds and deploys the FileMirror Windows Service to a specified folder,
# along with the installation script (not executed)

param(
    [Parameter(Mandatory=$true)]
    [string]$OutputPath,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$ProjectPath = "C:\projects\FileMirror\src\FileMirror.Service\FileMirror.Service.csproj",

    [string]$InstallScriptName = "Install-FileMirrorService.ps1"
)

# Validate output path
if (-not (Test-Path $OutputPath)) {
    Write-Host "Creating output directory: $OutputPath"
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

Write-Host "Building FileMirror Service in $Configuration mode..."

# Temporarily enable self-contained for the service build to create a single executable
# We'll need to modify the project file or use a different approach
# For now, build without self-contained as the service requires Windows dependencies

dotnet publish $ProjectPath -c $Configuration -o $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create installation script
$installScriptPath = Join-Path $OutputPath $InstallScriptName

@'
# Install-FileMirrorService.ps1
# Installs the FileMirror Windows Service

param(
    [string]$ServiceName = "FileMirror",
    [string]$BinaryPath = "$(Join-Path $OutputPath "FileMirror.Service.exe")",
    [string]$LogPath = "$(Join-Path $OutputPath "logs")"
)

Write-Host "Installing FileMirror Service..."

# Create log directory if it doesn't exist
if (-not (Test-Path $LogPath)) {
    New-Item -ItemType Directory -Path $LogPath | Out-Null
}

# Check if service already exists
if (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue) {
    Write-Host "Service '$ServiceName' already exists. Uninstalling..." -ForegroundColor Yellow
    
    # Stop the service if running
    $service = Get-Service -Name $ServiceName
    if ($service.Status -eq 'Running') {
        Stop-Service -Name $ServiceName
        Start-Sleep -Seconds 2
    }
    
    sc.exe delete $ServiceName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to delete existing service!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Existing service uninstalled."
}

# Install the service
Write-Host "Installing service from: $BinaryPath"
sc.exe create $ServiceName binPath= "`"$BinaryPath`"" start= "auto"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to create service!" -ForegroundColor Red
    exit 1
}

# Set service description
sc.exe description $ServiceName "Mirrors file changes from source to target paths"

Write-Host "Service installed successfully." -ForegroundColor Green
Write-Host "To start the service, run: Start-Service $ServiceName"
Write-Host "To uninstall, run: sc.exe delete $ServiceName"
'@ | Set-Content -Path $installScriptPath -Encoding UTF8

Write-Host "Deployment complete. Files in: $OutputPath"
Get-ChildItem -Path $OutputPath | Select-Object Name, Length

Write-Host ""
Write-Host "Installation script created: $installScriptPath"
Write-Host "Run the installation script with administrator privileges to install the service."

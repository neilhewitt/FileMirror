# FileMirror-Deploy-CLI.ps1
# Builds and deploys the FileMirror CLI to a specified folder

param(
    [Parameter(Mandatory=$true)]
    [string]$OutputPath,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$ProjectPath = "C:\projects\FileMirror\src\FileMirror\FileMirror.csproj"
)

# Validate output path
if (-not (Test-Path $OutputPath)) {
    Write-Host "Creating output directory: $OutputPath"
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

Write-Host "Building FileMirror CLI in $Configuration mode..."
dotnet publish $ProjectPath -c $Configuration -o $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Deployment complete. Files in: $OutputPath"
Get-ChildItem -Path $OutputPath | Select-Object Name, Length

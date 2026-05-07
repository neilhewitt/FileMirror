#Requires -RunAsAdministrator

param(
    [string]$BinaryPath = ".\FileMirror.exe",
    [string]$ServiceName = "FileMirror",
    [string]$ConfigPath = ""
)

Write-Host "Installing FileMirror service..."

$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($service) {
    Write-Host "Service $ServiceName already exists."
    exit 1
}

try {
    New-Service -Name $ServiceName -BinaryPathName $BinaryPath -DisplayName "FileMirror" -StartupType Automatic

    if ($ConfigPath) {
        # TODO: Store config path in registry or config file
    }

    Write-Host "FileMirror service installed successfully."
}
catch {
    Write-Error "Failed to install service: $_"
    exit 1
}
#Requires -RunAsAdministrator

param(
    [string]$ServiceName = "FileMirror"
)

Write-Host "Uninstalling FileMirror service..."

$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if (-not $service) {
    Write-Host "Service $ServiceName does not exist."
    exit 0
}

try {
    Stop-Service -Name $ServiceName -Force
    Uninstall-Service -Name $ServiceName
    Write-Host "FileMirror service uninstalled successfully."
}
catch {
    Write-Error "Failed to uninstall service: $_"
    exit 1
}
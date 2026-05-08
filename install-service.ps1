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
    $binaryPathEscaped = $BinaryPath -replace '\\', '\\'
    if ($ConfigPath) {
        $configPathEscaped = $ConfigPath -replace '\\', '\\'
        $binaryPathWithArgs = "`"{0}`" `"{1}`"" -f $binaryPathEscaped, $configPathEscaped
        New-Service -Name $ServiceName -BinaryPathName $binaryPathWithArgs -DisplayName "FileMirror" -StartupType Automatic
    }
    else {
        New-Service -Name $ServiceName -BinaryPathName $binaryPathEscaped -DisplayName "FileMirror" -StartupType Automatic
    }

    Write-Host "FileMirror service installed successfully."
    if ($ConfigPath) {
        Write-Host "Config path: $ConfigPath"
    }
}
catch {
    Write-Error "Failed to install service: $_"
    exit 1
}
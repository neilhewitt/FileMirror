$ErrorActionPreference = "Stop"

Write-Host "=== FileMirror Re-Sync Test ==="

# Setup
$source = "C:\projects\FileMirror\src\Source"
$target = "C:\projects\FileMirror\src\Target"
$config = "C:\projects\FileMirror\src\config.ini"
$binary = "C:\projects\FileMirror\src\publish\FileMirror.exe"

# Clean everything
Get-ChildItem -Path $source -File -Force | Remove-Item -Force
Get-ChildItem -Path $target -File -Force | Remove-Item -Force

# Create test file
"test content" | Out-File (Join-Path $source "test.txt")

# Start FileMirror in background
Write-Host "Starting FileMirror..."
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $binary
$psi.Arguments = "--config `"$config`" --foreground"
$psi.UseShellExecute = $false
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$process = [System.Diagnostics.Process]::Start($psi)

# Wait for initial sync
Start-Sleep -Seconds 6

# Verify initial sync
if (Test-Path (Join-Path $target "test.txt")) {
    Write-Host "[PASS] Initial sync: File replicated"
} else {
    Write-Host "[FAIL] Initial sync: File NOT replicated"
    $process.Kill()
    exit 1
}

# Delete from target
Write-Host "Deleting file from Target..."
Remove-Item (Join-Path $target "test.txt") -Force
Start-Sleep -Seconds 1

# Wait for re-sync
Write-Host "Waiting for re-sync..."
Start-Sleep -Seconds 6

# Verify re-sync
if (Test-Path (Join-Path $target "test.txt")) {
    Write-Host "[PASS] Re-sync: File restored"
} else {
    Write-Host "[FAIL] Re-sync: File NOT restored"
    $process.Kill()
    exit 1
}

# Stop FileMirror
Write-Host "Stopping FileMirror..."
$process.Kill()
$process.WaitForExit()

Write-Host "=== All tests passed ==="

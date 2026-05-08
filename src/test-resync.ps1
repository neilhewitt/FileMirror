param(
    [string]$ConfigPath = "C:\projects\FileMirror\src\config.ini",
    [string]$SourcePath = "C:\projects\FileMirror\src\Source",
    [string]$TargetPath = "C:\projects\FileMirror\src\Target"
)

# Build the project
Write-Host "Building FileMirror..."
dotnet build "C:\projects\FileMirror\src\FileMirror\FileMirror.csproj" --configuration Release --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed"
    exit 1
}

# Create test directories
Write-Host "Creating test directories..."
New-Item -ItemType Directory -Path $SourcePath -Force | Out-Null
New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null

# Clean up any existing test files
Get-ChildItem -Path $SourcePath -File | Remove-Item -Force
Get-ChildItem -Path $TargetPath -File | Remove-Item -Force

# Create test file in source
Write-Host "Creating test file in Source..."
"test content" | Out-File -FilePath (Join-Path $SourcePath "test.txt") -Encoding utf8

# Create a process to run FileMirror
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "dotnet"
$psi.Arguments = "run --project `"C:\projects\FileMirror\src\FileMirror\FileMirror.csproj`" --configuration Release -- --config `"$ConfigPath`" --foreground"
$psi.UseShellExecute = $false
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true

Write-Host "Starting FileMirror..."
$process = [System.Diagnostics.Process]::Start($psi)

# Wait for initial sync (about 5 seconds)
Start-Sleep -Seconds 6

# Check if file was mirrored
if (Test-Path (Join-Path $TargetPath "test.txt")) {
    Write-Host "[PASS] Initial sync: File was mirrored to Target"
} else {
    Write-Host "[FAIL] Initial sync: File was NOT mirrored to Target"
}

# Delete the file from target
Write-Host "Deleting test file from Target..."
Remove-Item -Path (Join-Path $TargetPath "test.txt") -Force

# Wait for re-sync (5 second interval + small buffer)
Start-Sleep -Seconds 6

# Check if file was restored
if (Test-Path (Join-Path $TargetPath "test.txt")) {
    Write-Host "[PASS] Re-sync: File was restored to Target"
} else {
    Write-Host "[FAIL] Re-sync: File was NOT restored to Target"
}

# Stop FileMirror
Write-Host "Stopping FileMirror..."
$process.Kill()
$process.WaitForExit()

Write-Host "Test complete"

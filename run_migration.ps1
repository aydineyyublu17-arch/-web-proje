$env:DOTNET_ROOT = "C:\Program Files\dotnet"
$UserProfile = $env:USERPROFILE
$ToolPath = Join-Path $UserProfile ".dotnet\tools\dotnet-ef.exe"
# Start-Process avoids some console encoding issues by launching a fresh process
Write-Host "Running dotnet-ef from: $ToolPath"
& $ToolPath database update
if ($LASTEXITCODE -eq 0) {
    Write-Host "SUCCESS: Database updated."
} else {
    Write-Host "FAILURE: Database update failed."
}

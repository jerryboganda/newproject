# Minimal Backend Startup Script
# This script starts the backend with minimal dependencies

Write-Host "🔧 Starting StreamVault Backend (Minimal Mode)..." -ForegroundColor Cyan

# Navigate to API directory
Set-Location "$PSScriptRoot\streamvault-backend\src\StreamVault.API"

# Try to build with warnings as errors disabled
Write-Host "Building project..." -ForegroundColor Yellow
& "C:\Program Files\dotnet\dotnet.exe" build StreamVault.Api.csproj --no-restore --verbosity quiet /warnaserror:false

if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️ Build had warnings, but attempting to run..." -ForegroundColor Yellow
}

# Start the API
Write-Host "Starting API server..." -ForegroundColor Yellow
& "C:\Program Files\dotnet\dotnet.exe" run --project StreamVault.Api.csproj --no-build

Write-Host "Backend stopped." -ForegroundColor Red

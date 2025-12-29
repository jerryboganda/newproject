# Quick Fix and Start Script
# Run as Administrator

Write-Host "🔧 StreamVault Quick Start Fix" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan

# Check and add .NET to PATH
$dotnetPath = "C:\Program Files\dotnet"
if (Test-Path $dotnetPath) {
    $env:PATH += ";$dotnetPath"
    Write-Host "✅ Added .NET to PATH" -ForegroundColor Green
    & "$dotnetPath\dotnet.exe" --version
} else {
    Write-Host "❌ .NET not found at $dotnetPath" -ForegroundColor Red
    Write-Host "Please install .NET 8 SDK from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
}

# Check and add Node.js to PATH
$nodePath = "C:\Program Files\nodejs"
if (Test-Path $nodePath) {
    $env:PATH += ";$nodePath"
    Write-Host "✅ Added Node.js to PATH" -ForegroundColor Green
    & "$nodePath\node.exe" --version
    & "$nodePath\npm.cmd" --version
} else {
    Write-Host "❌ Node.js not found at $nodePath" -ForegroundColor Red
    Write-Host "Please install Node.js from: https://nodejs.org/" -ForegroundColor Yellow
}

# Check services
Write-Host "`n🔍 Checking Services..." -ForegroundColor Yellow

# PostgreSQL
$pgService = Get-Service postgresql* -ErrorAction SilentlyContinue
if ($pgService -and $pgService.Status -eq 'Running') {
    Write-Host "✅ PostgreSQL is running" -ForegroundColor Green
} else {
    Write-Host "❌ PostgreSQL is not running" -ForegroundColor Red
}

# Memurai
$redisProcess = Get-Process memurai -ErrorAction SilentlyContinue
if ($redisProcess) {
    Write-Host "✅ Memurai is running" -ForegroundColor Green
} else {
    Write-Host "❌ Memurai is not running" -ForegroundColor Red
}

# Start services if tools are available
if ((Test-Path $dotnetPath) -and (Test-Path $nodePath)) {
    Write-Host "`n🚀 Starting Services..." -ForegroundColor Yellow
    
    # Create a new PowerShell window for backend
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\streamvault-backend\src\StreamVault.API'; dotnet run"
    Write-Host "✅ Starting backend in new window..." -ForegroundColor Green
    
    # Wait a moment
    Start-Sleep -Seconds 3
    
    # Create a new PowerShell window for frontend
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\streamvault-admin-dashboard'; npm run dev"
    Write-Host "✅ Starting frontend in new window..." -ForegroundColor Green
    
    Write-Host "`n⏳ Waiting for services to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    Write-Host "`n🌐 Try accessing:" -ForegroundColor Cyan
    Write-Host "   Frontend: http://localhost:3000" -ForegroundColor White
    Write-Host "   Backend: http://localhost:5000" -ForegroundColor White
    Write-Host "   Swagger: http://localhost:5000/swagger" -ForegroundColor White
} else {
    Write-Host "`n❌ Cannot start services - missing tools" -ForegroundColor Red
}

Write-Host "`nPress any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

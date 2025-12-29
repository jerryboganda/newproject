# StreamVault Runner - Fixed Version
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "     StreamVault Native Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if backend is already running
Write-Host "[1/5] Checking existing services..." -ForegroundColor Yellow
$backendRunning = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
$frontendRunning = Get-NetTCPConnection -LocalPort 3000 -ErrorAction SilentlyContinue

if ($backendRunning) {
    Write-Host "✓ Backend is already running on port 5000" -ForegroundColor Green
} else {
    Write-Host "✗ Backend not running" -ForegroundColor Red
}

if ($frontendRunning) {
    Write-Host "✓ Frontend is already running on port 3000" -ForegroundColor Green
} else {
    Write-Host "✗ Frontend not running" -ForegroundColor Red
}

# Start backend if needed
if (-not $backendRunning) {
    Write-Host ""
    Write-Host "[2/5] Starting Backend API..." -ForegroundColor Yellow
    Set-Location "C:\Users\Admin\CascadeProjects\newproject\WorkingBackend"
    Start-Process -FilePath "dotnet" -ArgumentList "run", "--urls=http://localhost:5000" -WindowStyle Minimized
    Write-Host "✓ Backend starting..." -ForegroundColor Green
    Start-Sleep 5
}

# Check dependencies
Write-Host ""
Write-Host "[3/5] Checking frontend dependencies..." -ForegroundColor Yellow
Set-Location "C:\Users\Admin\CascadeProjects\newproject\streamvault-frontend"
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing dependencies..." -ForegroundColor Cyan
    npm install
} else {
    Write-Host "✓ Dependencies already installed" -ForegroundColor Green
}

# Start frontend if needed
if (-not $frontendRunning) {
    Write-Host ""
    Write-Host "[4/5] Starting Frontend..." -ForegroundColor Yellow
    Start-Process -FilePath "npm" -ArgumentList "run", "dev" -WindowStyle Minimized
    Write-Host "✓ Frontend starting..." -ForegroundColor Green
    Start-Sleep 10
}

# Verify services
Write-Host ""
Write-Host "[5/5] Verifying services..." -ForegroundColor Yellow
try {
    $backendCheck = Invoke-RestMethod -Uri "http://localhost:5000/health" -TimeoutSec 5
    Write-Host "✓ Backend is responding!" -ForegroundColor Green
} catch {
    Write-Host "✗ Backend not responding" -ForegroundColor Red
}

try {
    $frontendCheck = Invoke-WebRequest -Uri "http://localhost:3000" -TimeoutSec 5
    Write-Host "✓ Frontend is responding!" -ForegroundColor Green
} catch {
    Write-Host "✗ Frontend not responding (may still be loading)" -ForegroundColor Yellow
}

# Final message
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "    StreamVault Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host "🔧 Backend:  http://localhost:5000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Login:" -ForegroundColor Yellow
Write-Host "  Email: admin@streamvault.com" -ForegroundColor White
Write-Host "  Password: Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "Press Enter to open the application..." -ForegroundColor Gray
Read-Host

Start-Process "http://localhost:3000"
Write-Host ""
Write-Host "✨ Application opened in browser!" -ForegroundColor Green
Write-Host ""
Write-Host "To stop services: Close the minimized windows or run:" -ForegroundColor Gray
Write-Host "taskkill /F /IM dotnet.exe /IM node.exe" -ForegroundColor Gray

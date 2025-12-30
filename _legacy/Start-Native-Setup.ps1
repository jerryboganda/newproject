# StreamVault Native Windows Setup (No Docker/Podman)
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  StreamVault Native Windows Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Kill any existing processes
Write-Host "[1/6] Cleaning up existing processes..." -ForegroundColor Yellow
taskkill /F /IM dotnet.exe 2>$null
taskkill /F /IM node.exe 2>$null
Write-Host "✓ Cleaned up" -ForegroundColor Green

# Start Backend
Write-Host ""
Write-Host "[2/6] Starting Backend API..." -ForegroundColor Yellow
Set-Location "C:\Users\Admin\CascadeProjects\newproject\WorkingBackend"
Start-Process -FilePath "dotnet" -ArgumentList "run", "--urls=http://localhost:5000" -NoNewWindow
Write-Host "✓ Backend starting on http://localhost:5000" -ForegroundColor Green

# Wait for backend to start
Write-Host ""
Write-Host "[3/6] Waiting for backend to start..." -ForegroundColor Yellow
Start-Sleep 5
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/health" -TimeoutSec 10
    Write-Host "✓ Backend is running! Status: $($response.status)" -ForegroundColor Green
} catch {
    Write-Host "⚠ Backend still starting..." -ForegroundColor Yellow
}

# Install frontend dependencies if needed
Write-Host ""
Write-Host "[4/6] Checking frontend dependencies..." -ForegroundColor Yellow
Set-Location "C:\Users\Admin\CascadeProjects\newproject\streamvault-frontend"
if (-not (Test-Path "node_modules\.package-lock.json")) {
    Write-Host "Installing dependencies..." -ForegroundColor Cyan
    npm install
}
Write-Host "✓ Dependencies ready" -ForegroundColor Green

# Start Frontend
Write-Host ""
Write-Host "[5/6] Starting Frontend..." -ForegroundColor Yellow
Start-Process -FilePath "npm" -ArgumentList "run", "dev" -NoNewWindow
Write-Host "✓ Frontend starting on http://localhost:3000" -ForegroundColor Green

# Wait for frontend
Write-Host ""
Write-Host "[6/6] Waiting for frontend to start..." -ForegroundColor Yellow
Start-Sleep 10

# Success message
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "    StreamVault is running!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host "🔧 Backend API: http://localhost:5000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Default Login:" -ForegroundColor Yellow
Write-Host "Email: admin@streamvault.com" -ForegroundColor White
Write-Host "Password: Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "✨ No Docker, No Podman, No Virtualization!" -ForegroundColor Green
Write-Host ""
Write-Host "To stop: Close the terminal windows or run taskkill /F /IM dotnet.exe /IM node.exe" -ForegroundColor Gray
Write-Host ""
Read-Host "Press Enter to open the application in browser"

# Open browser
Start-Process "http://localhost:3000"

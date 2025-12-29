# StreamVault Startup Script
# Run this script in PowerShell

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "     STREAMVAULT STARTUP SCRIPT" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the right directory
if (-not (Test-Path "streamvault-backend")) {
    Write-Host "ERROR: Please run this from the newproject folder!" -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)"
    Write-Host ""
    Write-Host "Make sure you can see the streamvault-backend folder"
    Read-Host "Press Enter to exit"
    exit 1
}

# Create logs directory
if (-not (Test-Path "logs")) {
    New-Item -ItemType Directory -Path "logs" | Out-Null
}

Write-Host "[1/3] Setting up Backend..." -ForegroundColor Yellow
Set-Location "streamvault-backend\src\StreamVault.Api"

# Check if required files exist
if (-not (Test-Path "Program.Production.cs")) {
    Write-Host "ERROR: Program.Production.cs not found!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Use production program
Copy-Item "Program.Production.cs" "Program.cs" -Force

# Install required packages
Write-Host "Installing required packages..." -ForegroundColor Gray
dotnet add package Microsoft.EntityFrameworkCore.Sqlite | Out-Null
dotnet add package BCrypt.Net-Next | Out-Null
dotnet add package Swashbuckle.AspNetCore | Out-Null

# Build the project
Write-Host "Building backend..." -ForegroundColor Gray
dotnet build --no-restore | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Backend build failed!" -ForegroundColor Red
    Write-Host "Check the error messages above"
    Read-Host "Press Enter to exit"
    exit 1
}

# Start backend
Write-Host "Starting backend server..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host '===== STREAMVAULT BACKEND =====' -ForegroundColor Cyan; Write-Host 'Database: SQLite (Persistent)' -ForegroundColor Gray; dotnet run"

# Wait for backend
Write-Host ""
Write-Host "[2/3] Waiting for backend to initialize..." -ForegroundColor Yellow
Write-Host "Please wait 20 seconds..." -ForegroundColor Gray
Start-Sleep -Seconds 20

# Setup frontend
Write-Host ""
Write-Host "[3/3] Setting up Frontend..." -ForegroundColor Yellow
Set-Location "..\..\..\streamvault-frontend"

# Create .env.local
"NEXT_PUBLIC_API_URL=http://localhost:5000" | Out-File -FilePath ".env.local" -Encoding UTF8

# Install dependencies if needed
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Gray
    npm install
}

# Start frontend
Write-Host "Starting frontend server..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host '===== STREAMVAULT FRONTEND =====' -ForegroundColor Cyan; npm run dev"

# Go back to root
Set-Location ".."

# Final message
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "     STREAMVAULT IS STARTING!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Please wait 30 seconds for full startup" -ForegroundColor Yellow
Write-Host ""
Write-Host "URLs:" -ForegroundColor Cyan
Write-Host "- Frontend: http://localhost:3000" -ForegroundColor White
Write-Host "- Backend:  http://localhost:5000" -ForegroundColor White
Write-Host "- API Docs: http://localhost:5000/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Login:" -ForegroundColor Cyan
Write-Host "- Email: admin@streamvault.com" -ForegroundColor White
Write-Host "- Password: Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Backend and Frontend are running in separate PowerShell windows" -ForegroundColor Gray
Write-Host "Keep those windows open to keep StreamVault running" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to exit this launcher..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

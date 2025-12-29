# StreamVault Simple Startup Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting StreamVault (Simple Mode)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`nStep 1: Preparing backend..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\streamvault-backend\src\StreamVault.Api"

# Backup original Program.cs if it exists
if (Test-Path "Program.cs") {
    Copy-Item "Program.cs" "Program.cs.backup" -Force
}

# Use simplified version
Copy-Item "Program.Simple.cs" "Program.cs" -Force

Write-Host "`nStep 2: Installing required packages..." -ForegroundColor Yellow
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package BCrypt.Net-Next
dotnet add package Swashbuckle.AspNetCore

Write-Host "`nStep 3: Starting Backend API..." -ForegroundColor Yellow
Write-Host "This will take 10-15 seconds..." -ForegroundColor Gray

# Start backend in new window
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\streamvault-backend\src\StreamVault.Api'; Write-Host 'Starting Backend...' -ForegroundColor Green; dotnet run"

Write-Host "`nStep 4: Waiting for backend to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "`nStep 5: Starting Frontend..." -ForegroundColor Yellow
# Start frontend in new window
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\streamvault-frontend'; Write-Host 'Starting Frontend...' -ForegroundColor Green; npm run dev"

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "READY! Services are starting..." -ForegroundColor Green
Write-Host "`nURLs:" -ForegroundColor Cyan
Write-Host "- Frontend: http://localhost:3000" -ForegroundColor White
Write-Host "- Backend API: http://localhost:5000" -ForegroundColor White
Write-Host "- Swagger Docs: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "`nLogin Credentials:" -ForegroundColor Cyan
Write-Host "Email: admin@streamvault.com" -ForegroundColor White
Write-Host "Password: Admin123!" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green

Write-Host "`nPlease wait 30 seconds for everything to load" -ForegroundColor Yellow
Write-Host "`nTo stop: Close the Backend and Frontend PowerShell windows" -ForegroundColor Gray
Write-Host "`nPress any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

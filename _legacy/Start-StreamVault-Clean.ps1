# StreamVault Clean Start - PowerShell Version
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "     STREAMVAULT CLEAN START" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to backend
Set-Location "streamvault-backend\src\StreamVault.Api"

# Backup original if exists
if (Test-Path "Program.cs" -and -not (Test-Path "Program.cs.original")) {
    Write-Host "Backing up original Program.cs..." -ForegroundColor Yellow
    Copy-Item "Program.cs" "Program.cs.original"
}

# Use working version
Write-Host "Using simplified working version..." -ForegroundColor Yellow
Copy-Item "Program.SimpleWorking.cs" "Program.cs" -Force

# Clean project
Write-Host "Cleaning project..." -ForegroundColor Yellow
dotnet clean | Out-Null

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore | Out-Null

# Install required packages
Write-Host "Installing required packages..." -ForegroundColor Yellow
dotnet add package Microsoft.EntityFrameworkCore.Sqlite | Out-Null
dotnet add package BCrypt.Net-Next | Out-Null
dotnet add package Swashbuckle.AspNetCore | Out-Null

# Build project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build --no-restore | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    Write-Host "Check the error messages above."
    Read-Host "Press Enter to exit"
    exit 1
}

# Start backend
Write-Host "Starting backend..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host '===== STREAMVAULT BACKEND =====' -ForegroundColor Cyan; Write-Host 'Database: SQLite (Persistent)' -ForegroundColor Gray; Write-Host ''; dotnet run"

# Wait for backend
Write-Host ""
Write-Host "Waiting 20 seconds for backend to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 20

# Start frontend
Write-Host "Starting frontend..." -ForegroundColor Yellow
Set-Location "..\..\..\streamvault-frontend"

# Create .env.local
"NEXT_PUBLIC_API_URL=http://localhost:5000" | Out-File -FilePath ".env.local" -Encoding UTF8
"NEXT_PUBLIC_WS_URL=ws://localhost:5000" | Out-File -FilePath ".env.local" -Encoding UTF8 -Append

# Install dependencies if needed
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
    npm install
}

# Start frontend
Write-Host "Starting frontend..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host '===== STREAMVAULT FRONTEND =====' -ForegroundColor Cyan; npm run dev"

# Done
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
Write-Host "- Health:   http://localhost:5000/health" -ForegroundColor White
Write-Host ""
Write-Host "Login:" -ForegroundColor Cyan
Write-Host "- Email: admin@streamvault.com" -ForegroundColor White
Write-Host "- Password: Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "Database: SQLite (streamvault.db)" -ForegroundColor Gray
Write-Host "Data is persisted between restarts" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# StreamVault Podman Setup Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    StreamVault Podman Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Podman is installed
Write-Host "[1/4] Checking Podman installation..." -ForegroundColor Yellow
try {
    $podmanVersion = podman --version 2>$null
    Write-Host "✓ Podman is installed: $podmanVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: Podman is not installed" -ForegroundColor Red
    Write-Host "Please install Podman from: https://podman.io/docs/installation" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Initialize Podman machine if needed
Write-Host ""
Write-Host "[2/4] Initializing Podman machine..." -ForegroundColor Yellow
try {
    podman machine list | findstr "running" >$null 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Starting Podman machine..." -ForegroundColor Cyan
        podman machine init
        podman machine start
    } else {
        Write-Host "✓ Podman machine is already running" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ ERROR: Failed to initialize Podman machine" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Build and start services
Write-Host ""
Write-Host "[3/4] Building and starting services..." -ForegroundColor Yellow
Set-Location $PSScriptRoot

# Stop any existing containers
podman-compose -f podman-compose.yml down 2>$null

# Build images
Write-Host "Building Podman images..." -ForegroundColor Cyan
podman-compose -f podman-compose.yml build --no-cache
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERROR: Failed to build services" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Start containers
Write-Host ""
Write-Host "[4/4] Starting all containers..." -ForegroundColor Yellow
podman-compose -f podman-compose.yml up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERROR: Failed to start containers" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Success message
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "    StreamVault is now running!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Frontend: http://localhost:3001" -ForegroundColor Cyan
Write-Host "🔧 Backend API: http://localhost:5000" -ForegroundColor Cyan
Write-Host "💾 MinIO Console: http://localhost:9001" -ForegroundColor Cyan
Write-Host ""
Write-Host "Default Login:" -ForegroundColor Yellow
Write-Host "Email: admin@streamvault.com" -ForegroundColor White
Write-Host "Password: Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "To stop all services:" -ForegroundColor Gray
Write-Host "podman-compose -f podman-compose.yml down" -ForegroundColor Gray
Write-Host ""
Write-Host "To view logs:" -ForegroundColor Gray
Write-Host "podman-compose -f podman-compose.yml logs -f" -ForegroundColor Gray
Write-Host ""
Read-Host "Press Enter to continue"

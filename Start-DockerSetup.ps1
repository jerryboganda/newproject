# StreamVault Docker Setup Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    StreamVault Docker Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is installed
Write-Host "[1/4] Checking Docker Desktop installation..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version 2>$null
    Write-Host "✓ Docker Desktop is installed: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: Docker Desktop is not installed" -ForegroundColor Red
    Write-Host "Please install Docker Desktop from: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    Write-Host "After installation, restart this script" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Check if Docker is running
Write-Host ""
Write-Host "[2/4] Checking if Docker Desktop is running..." -ForegroundColor Yellow
try {
    docker info >$null 2>&1
    Write-Host "✓ Docker Desktop is running!" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: Docker Desktop is not running" -ForegroundColor Red
    Write-Host "Please start Docker Desktop from the Start Menu" -ForegroundColor Yellow
    Write-Host "Wait for it to fully start, then run this script again" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Build and start services
Write-Host ""
Write-Host "[3/4] Building and starting services..." -ForegroundColor Yellow
Set-Location $PSScriptRoot

# Stop any existing containers
docker-compose -f docker-compose-simple.yml down

# Build images
Write-Host "Building Docker images..." -ForegroundColor Cyan
docker-compose -f docker-compose-simple.yml build --no-cache
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERROR: Failed to build services" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Start containers
Write-Host ""
Write-Host "[4/4] Starting all containers..." -ForegroundColor Yellow
docker-compose -f docker-compose-simple.yml up -d
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
Write-Host "docker-compose -f docker-compose-simple.yml down" -ForegroundColor Gray
Write-Host ""
Write-Host "To view logs:" -ForegroundColor Gray
Write-Host "docker-compose -f docker-compose-simple.yml logs -f" -ForegroundColor Gray
Write-Host ""
Read-Host "Press Enter to continue"

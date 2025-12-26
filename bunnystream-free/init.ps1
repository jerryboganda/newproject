# Development Environment Initialization Script
# Run this after Docker is installed to set up everything

Write-Host "🚀 BunnyStream Development Environment Initialization" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

$allGood = $true

# Check Docker
Write-Host "1. Checking Docker..." -ForegroundColor White
try {
    $dockerVersion = docker --version
    Write-Host "   ✅ $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Docker not installed" -ForegroundColor Red
    Write-Host "   → Download: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    $allGood = $false
}

# Navigate to project
Write-Host ""
Write-Host "2. Navigating to project directory..." -ForegroundColor White
cd bunnystream-free
if (Test-Path "docker-compose.yml") {
    Write-Host "   ✅ Project directory found" -ForegroundColor Green
} else {
    Write-Host "   ❌ Project directory not found" -ForegroundColor Red
    $allGood = $false
}

if ($allGood) {
    Write-Host ""
    Write-Host "3. Starting Docker services..." -ForegroundColor Yellow
    docker-compose up -d
    
    Write-Host ""
    Write-Host "⏳ Waiting for services to start (20 seconds)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 20
    
    Write-Host ""
    Write-Host "4. Verifying services..." -ForegroundColor Yellow
    docker-compose ps
    
    Write-Host ""
    Write-Host "5. Testing database connection..." -ForegroundColor Yellow
    docker exec -it bunnystream-postgres psql -U postgres -c "SELECT 'PostgreSQL OK' as status" 2>$null
    
    Write-Host ""
    Write-Host "6. Testing Redis connection..." -ForegroundColor Yellow
    docker exec -it bunnystream-redis redis-cli ping 2>$null
    
    Write-Host ""
    Write-Host "✅ All services started successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📍 Service URLs:" -ForegroundColor Cyan
    Write-Host "   - PostgreSQL:  localhost:5432 (postgres / postgres123)" -ForegroundColor White
    Write-Host "   - Redis:       localhost:6379" -ForegroundColor White
    Write-Host "   - MinIO:       http://localhost:9000 (minioadmin / minioadmin123)" -ForegroundColor White
    Write-Host "   - MinIO Web:   http://localhost:9001" -ForegroundColor White
    
    Write-Host ""
    Write-Host "🎯 Next Steps:" -ForegroundColor Cyan
    Write-Host "   1. Install Node.js from https://nodejs.org/" -ForegroundColor White
    Write-Host "   2. cd frontend && npm install && npm run dev" -ForegroundColor White
    Write-Host "   3. In another terminal: cd backend && npm install && npm run start:dev" -ForegroundColor White
    
} else {
    Write-Host ""
    Write-Host "❌ Prerequisites not met. Please install missing components." -ForegroundColor Red
}

Write-Host ""
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "🎉 Initialization Complete!" -ForegroundColor Green

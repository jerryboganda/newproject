# StreamVault Launch Script with Local Services
# Run this script to start the complete system without containers

Write-Host "🚀 Starting StreamVault System (Local Mode)..." -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Check if PostgreSQL is running
Write-Host "`n🔍 Checking PostgreSQL..." -ForegroundColor Yellow
try {
    $pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
    if ($pgService -and $pgService.Status -eq 'Running') {
        Write-Host "✅ PostgreSQL is running" -ForegroundColor Green
    } elseif ($pgService) {
        Write-Host "⚠️ PostgreSQL is stopped. Starting it..." -ForegroundColor Yellow
        Start-Service -Name $pgService.Name
        Write-Host "✅ PostgreSQL started" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL service not found. Please install PostgreSQL 15" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "❌ PostgreSQL not found. Please install PostgreSQL 15" -ForegroundColor Red
    exit 1
}

# Check if Memurai (Redis) is running
Write-Host "`n🔍 Checking Memurai (Redis)..." -ForegroundColor Yellow
try {
    $redisProcess = Get-Process -Name "memurai" -ErrorAction SilentlyContinue
    if ($redisProcess) {
        Write-Host "✅ Memurai is running" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Memurai not running. Please start Memurai manually." -ForegroundColor Yellow
        Write-Host "   Run: memurai.exe" -ForegroundColor Gray
        Write-Host "   Press Enter to continue after starting Memurai..." -ForegroundColor Gray
        Read-Host
    }
}
catch {
    Write-Host "❌ Memurai not found. Please install Memurai for Windows." -ForegroundColor Red
    exit 1
}

# Create database if it doesn't exist
Write-Host "`n🗄️ Setting up database..." -ForegroundColor Yellow
$env:PGPASSWORD = "password"
$checkDb = psql -U postgres -h localhost -c "SELECT 1 FROM pg_database WHERE datname='streamvault_dev'" -t -A 2>$null
if ($checkDb -ne "1") {
    Write-Host "Creating database streamvault_dev..." -ForegroundColor Gray
    psql -U postgres -h localhost -c "CREATE DATABASE streamvault_dev;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database created" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to create database" -ForegroundColor Red
    }
} else {
    Write-Host "✅ Database already exists" -ForegroundColor Green
}

# Run database migrations
Write-Host "`n🗄️ Running database migrations..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\streamvault-backend"
dotnet ef database update --project src\StreamVault.Infrastructure --startup-project src\StreamVault.API

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Database migrations completed" -ForegroundColor Green
} else {
    Write-Host "❌ Database migrations failed" -ForegroundColor Red
    Write-Host "Continuing anyway - the database might already be up to date..." -ForegroundColor Yellow
}

# Start Backend API
Write-Host "`n🔧 Starting Backend API..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\streamvault-backend\src\StreamVault.API"
$backendJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    & dotnet run
}
Write-Host "✅ Backend starting at http://localhost:5000" -ForegroundColor Green

# Wait for backend to start
Write-Host "`n⏳ Waiting for backend to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check if backend is responding
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/health" -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Backend is ready" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Backend might still be starting..." -ForegroundColor Yellow
    }
}
catch {
    Write-Host "⚠️ Backend might still be starting... Give it a moment" -ForegroundColor Yellow
}

# Start Frontend
Write-Host "`n🎨 Starting Frontend..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\streamvault-admin-dashboard"
$frontendJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    & npm run dev
}
Write-Host "✅ Frontend starting at http://localhost:3000" -ForegroundColor Green

# Wait for frontend to start
Start-Sleep -Seconds 5

# Display access information
Write-Host "`n🎉 System Started Successfully!" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green
Write-Host "`n📱 Access URLs:" -ForegroundColor Cyan
Write-Host "   Frontend: http://localhost:3000" -ForegroundColor White
Write-Host "   Backend API: http://localhost:5000" -ForegroundColor White
Write-Host "   Swagger Docs: http://localhost:5000/swagger" -ForegroundColor White

Write-Host "`n👤 Default Login Credentials:" -ForegroundColor Cyan
Write-Host "   Super Admin: superadmin@streamvault.app / SuperAdmin123!" -ForegroundColor White
Write-Host "   Tenant Admin: admin@tenant1.com / TenantAdmin123!" -ForegroundColor White
Write-Host "   Regular User: user@tenant1.com / User123!" -ForegroundColor White

Write-Host "`n📝 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Open http://localhost:3000 in your browser" -ForegroundColor Gray
Write-Host "2. Login as Super Admin" -ForegroundColor Gray
Write-Host "3. Go to Settings > Integrations" -ForegroundColor Gray
Write-Host "4. Configure your Bunny.net credentials" -ForegroundColor Gray
Write-Host "5. Test video upload functionality" -ForegroundColor Gray

Write-Host "`n💡 To stop the system:" -ForegroundColor Magenta
Write-Host "   1. Close this PowerShell window" -ForegroundColor Gray
Write-Host "   2. Or press Ctrl+C in each terminal" -ForegroundColor Gray

Write-Host "`nPress Ctrl+C to stop monitoring..." -ForegroundColor Gray

# Monitor jobs
try {
    while ($true) {
        Receive-Job $backendJob -ErrorAction SilentlyContinue | Out-Null
        Receive-Job $frontendJob -ErrorAction SilentlyContinue | Out-Null
        Start-Sleep -Seconds 1
    }
}
finally {
    # Cleanup jobs when script is stopped
    Remove-Job $backendJob -Force -ErrorAction SilentlyContinue
    Remove-Job $frontendJob -Force -ErrorAction SilentlyContinue
    Write-Host "`n🛑 Services stopped" -ForegroundColor Red
}

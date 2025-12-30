# StreamVault Launch Script with Podman and Local Services
# Run this script to start the complete system

Write-Host "🚀 Starting StreamVault System..." -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Check if Podman is available
try {
    podman --version | Out-Null
    Write-Host "✅ Podman is available" -ForegroundColor Green
    $podmanAvailable = $true
}
catch {
    Write-Host "❌ Podman is not available" -ForegroundColor Red
    $podmanAvailable = $false
}

# Start PostgreSQL with Podman
if ($podmanAvailable) {
    Write-Host "`n🐳 Starting PostgreSQL with Podman..." -ForegroundColor Yellow
    podman run -d --name streamvault-postgres `
        -e POSTGRES_DB=streamvault_dev `
        -e POSTGRES_USER=postgres `
        -e POSTGRES_PASSWORD=password `
        -p 5432:5432 `
        -v postgres_data:/var/lib/postgresql/data `
        postgres:15-alpine
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ PostgreSQL started on port 5432" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to start PostgreSQL" -ForegroundColor Red
    }
}

# Start RabbitMQ with Podman
if ($podmanAvailable) {
    Write-Host "`n🐳 Starting RabbitMQ with Podman..." -ForegroundColor Yellow
    podman run -d --name streamvault-rabbitmq `
        -e RABBITMQ_DEFAULT_USER=guest `
        -e RABBITMQ_DEFAULT_PASS=guest `
        -p 5672:5672 `
        -p 15672:15672 `
        -v rabbitmq_data:/var/lib/rabbitmq `
        rabbitmq:3-management-alpine
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ RabbitMQ started on ports 5672/15672" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to start RabbitMQ" -ForegroundColor Red
    }
}

# Start MinIO with Podman
if ($podmanAvailable) {
    Write-Host "`n🐳 Starting MinIO with Podman..." -ForegroundColor Yellow
    podman run -d --name streamvault-minio `
        -e MINIO_ROOT_USER=minioadmin `
        -e MINIO_ROOT_PASSWORD=minioadmin `
        -p 9000:9000 `
        -p 9001:9001 `
        -v minio_data:/data `
        minio/minio:latest server /data --console-address ":9001"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ MinIO started on ports 9000/9001" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to start MinIO" -ForegroundColor Red
    }
}

# Check if Memurai (Redis) is running
Write-Host "`n🔍 Checking Memurai (Redis) status..." -ForegroundColor Yellow
try {
    $redisProcess = Get-Process -Name "memurai" -ErrorAction SilentlyContinue
    if ($redisProcess) {
        Write-Host "✅ Memurai is running" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Memurai not running. Please start Memurai manually." -ForegroundColor Yellow
        Write-Host "   Run: memurai.exe" -ForegroundColor Gray
    }
}
catch {
    Write-Host "❌ Memurai not found. Please install Memurai for Windows." -ForegroundColor Red
}

# Wait for services to be ready
Write-Host "`n⏳ Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Run database migrations
Write-Host "`n🗄️ Running database migrations..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\streamvault-backend"
dotnet ef database update --project src\StreamVault.Infrastructure --startup-project src\StreamVault.API

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Database migrations completed" -ForegroundColor Green
} else {
    Write-Host "❌ Database migrations failed" -ForegroundColor Red
}

# Start Backend API
Write-Host "`n🔧 Starting Backend API..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\streamvault-backend\src\StreamVault.API"
Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow
Write-Host "✅ Backend starting at http://localhost:5000" -ForegroundColor Green

# Wait for backend to start
Start-Sleep -Seconds 5

# Start Frontend
Write-Host "`n🎨 Starting Frontend..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\streamvault-admin-dashboard"
Start-Process -FilePath "npm" -ArgumentList "run", "dev" -NoNewWindow
Write-Host "✅ Frontend starting at http://localhost:3000" -ForegroundColor Green

# Display access information
Write-Host "`n🎉 System Started Successfully!" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green
Write-Host "`n📱 Access URLs:" -ForegroundColor Cyan
Write-Host "   Frontend: http://localhost:3000" -ForegroundColor White
Write-Host "   Backend API: http://localhost:5000" -ForegroundColor White
Write-Host "   Swagger Docs: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "   RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host "   MinIO Console: http://localhost:9001 (minioadmin/minioadmin)" -ForegroundColor White

Write-Host "`n👤 Default Login Credentials:" -ForegroundColor Cyan
Write-Host "   Super Admin: superadmin@streamvault.app / SuperAdmin123!" -ForegroundColor White
Write-Host "   Tenant Admin: admin@tenant1.com / TenantAdmin123!" -ForegroundColor White
Write-Host "   Regular User: user@tenant1.com / User123!" -ForegroundColor White

Write-Host "`n📝 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Login as Super Admin" -ForegroundColor Gray
Write-Host "2. Go to Settings > Integrations" -ForegroundColor Gray
Write-Host "3. Configure your Bunny.net credentials" -ForegroundColor Gray
Write-Host "4. Test video upload functionality" -ForegroundColor Gray

Write-Host "`nPress any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

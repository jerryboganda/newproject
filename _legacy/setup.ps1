# StreamVault Local Setup Script
# Run this script as Administrator in PowerShell

Write-Host "🚀 StreamVault Local Setup Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "Please run this script as Administrator!"
    pause
    exit 1
}

# Check prerequisites
Write-Host "`n📋 Checking prerequisites..." -ForegroundColor Yellow

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version 2>$null
    Write-Host "✅ .NET SDK: $dotnetVersion" -ForegroundColor Green
    $dotnetAvailable = $true
}
catch {
    Write-Host "❌ .NET is not installed" -ForegroundColor Red
    Write-Host "Please install .NET 8 SDK from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    $dotnetAvailable = $false
}

# Check if Node.js is installed
try {
    $nodeVersion = node --version 2>$null
    Write-Host "✅ Node.js: $nodeVersion" -ForegroundColor Green
    $nodeAvailable = $true
}
catch {
    Write-Host "❌ Node.js is not installed" -ForegroundColor Red
    Write-Host "Please install Node.js from: https://nodejs.org/" -ForegroundColor Yellow
    $nodeAvailable = $false
}

# Check if PostgreSQL is installed
try {
    $pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
    if ($pgService) {
        Write-Host "✅ PostgreSQL service found: $($pgService.Name)" -ForegroundColor Green
        $pgAvailable = $true
    } else {
        Write-Host "❌ PostgreSQL not found" -ForegroundColor Red
        Write-Host "Please install PostgreSQL from: https://www.postgresql.org/download/windows/" -ForegroundColor Yellow
        $pgAvailable = $false
    }
}
catch {
    Write-Host "❌ PostgreSQL not found" -ForegroundColor Red
    Write-Host "Please install PostgreSQL from: https://www.postgresql.org/download/windows/" -ForegroundColor Yellow
    $pgAvailable = $false
}

# Check if Redis is installed
try {
    $redisProcess = Get-Process -Name "redis-server" -ErrorAction SilentlyContinue
    if ($redisProcess) {
        Write-Host "✅ Redis is running" -ForegroundColor Green
        $redisAvailable = $true
    } else {
        Write-Host "⚠️ Redis not running. Please start Redis server" -ForegroundColor Yellow
        $redisAvailable = $false
    }
}
catch {
    Write-Host "❌ Redis not found" -ForegroundColor Red
    Write-Host "Please install Redis from: https://redis.io/download" -ForegroundColor Yellow
    $redisAvailable = $false
}

# If all prerequisites are available, proceed with setup
if ($dotnetAvailable -and $nodeAvailable -and $pgAvailable) {
    Write-Host "`n🔧 Creating environment files..." -ForegroundColor Yellow
    
    # Create backend appsettings.Development.json
    $backendConfig = @{
        ConnectionStrings = @{
            DefaultConnection = "Host=localhost;Database=streamvault;Username=streamvault;Password=streamvault123"
        }
        Redis = @{
            ConnectionString = "localhost:6379"
        }
        Jwt = @{
            SecretKey = "StreamVaultSuperSecretKey1234567890ABCDEF"  # 32 chars
            Issuer = "StreamVault"
            Audience = "StreamVault"
            ExpiryMinutes = 60
            RefreshExpiryDays = 7
        }
        RabbitMQ = @{
            HostName = "localhost"
            UserName = "guest"
            Password = "guest"
            VirtualHost = "/"
        }
        MinIO = @{
            EndPoint = "localhost:9000"
            AccessKey = "minioadmin"
            SecretKey = "minioadmin"
            Bucket = "streamvault"
        }
        Stripe = @{
            SecretKey = "sk_test_your_stripe_key_here"
            PublishableKey = "pk_test_your_stripe_key_here"
            WebhookSecret = "whsec_your_webhook_secret"
        }
        SendGrid = @{
            ApiKey = "SG.your_sendgrid_key_here"
            FromEmail = "noreply@streamvault.app"
            FromName = "StreamVault"
        }
    }
    
    $backendConfigPath = "$PSScriptRoot\streamvault-backend\src\StreamVault.API\appsettings.Development.json"
    $backendConfig | ConvertTo-Json -Depth 4 | Out-File -FilePath $backendConfigPath -Encoding UTF8
    Write-Host "✅ Created backend configuration: appsettings.Development.json" -ForegroundColor Green
    
    # Create frontend .env.local
    $frontendConfig = @"
NEXT_PUBLIC_API_URL=http://localhost:5000/api
NEXT_PUBLIC_WS_URL=ws://localhost:5000/ws
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_your_stripe_key_here
NEXT_PUBLIC_APP_URL=http://localhost:3000
"@
    
    $frontendConfigPath = "$PSScriptRoot\streamvault-admin-dashboard\.env.local"
    $frontendConfig | Out-File -FilePath $frontendConfigPath -Encoding UTF8
    Write-Host "✅ Created frontend configuration: .env.local" -ForegroundColor Green
    
    Write-Host "`n📊 Database Setup Instructions:" -ForegroundColor Yellow
    Write-Host "1. Open pgAdmin or psql as postgres user"
    Write-Host "2. Run these SQL commands:"
    Write-Host "   CREATE DATABASE streamvault;"
    Write-Host "   CREATE USER streamvault WITH PASSWORD 'streamvault123';"
    Write-Host "   GRANT ALL PRIVILEGES ON DATABASE streamvault TO streamvault;"
    
    Write-Host "`n🚀 Launch Commands:" -ForegroundColor Yellow
    Write-Host "1. Start Redis: redis-server"
    Write-Host "2. Start Backend: cd streamvault-backend\src\StreamVault.API && dotnet run"
    Write-Host "3. Start Frontend: cd streamvault-admin-dashboard && npm run dev"
    
    Write-Host "`n👤 Default Users:" -ForegroundColor Cyan
    Write-Host "Super Admin: superadmin@streamvault.app / SuperAdmin123!" -ForegroundColor White
    Write-Host "Tenant Admin: admin@tenant1.com / TenantAdmin123!" -ForegroundColor White
    Write-Host "Regular User: user@tenant1.com / User123!" -ForegroundColor White
    
    Write-Host "`n🌐 Access URLs:" -ForegroundColor Cyan
    Write-Host "Super Admin Panel: http://localhost:3000/admin" -ForegroundColor White
    Write-Host "Tenant Dashboard: http://localhost:3000" -ForegroundColor White
    Write-Host "API Documentation: http://localhost:5000/swagger" -ForegroundColor White
    
    Write-Host "`n✨ Setup complete! Please follow the instructions above to launch the system." -ForegroundColor Green
} else {
    Write-Host "`n❌ Please install all prerequisites before continuing." -ForegroundColor Red
    Write-Host "1. .NET 8 SDK: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    Write-Host "2. Node.js 18+: https://nodejs.org/" -ForegroundColor Yellow
    Write-Host "3. PostgreSQL 15: https://www.postgresql.org/download/windows/" -ForegroundColor Yellow
    Write-Host "4. Redis: https://redis.io/download" -ForegroundColor Yellow
}

pause

Write-Host "`n📚 For more information, see SETUP.md" -ForegroundColor Yellow
Write-Host "`nPress any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

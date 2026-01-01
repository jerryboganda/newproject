#!/usr/bin/env pwsh
<#
.SYNOPSIS
    StreamVault - Professional Podman Container Launcher (v2)
.DESCRIPTION
    Launches StreamVault services as individual Podman containers with proper networking
#>

param(
    [ValidateSet('start', 'stop', 'logs')]
    [string]$Action = 'start'
)

$PODMAN = "C:\Program Files\RedHat\Podman\podman.exe"

function Write-Status($msg, $type = "info") {
    $colors = @{info = "Cyan"; success = "Green"; warn = "Yellow"; error = "Red" }
    Write-Host "[$([datetime]::Now.ToString('HH:mm:ss'))] " -NoNewline
    Write-Host $msg -ForegroundColor $colors[$type]
}

function Start-Environment {
    Write-Status "🚀 Starting StreamVault Professional Container Environment"
    Write-Status "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    # Create network
    Write-Status "🔗 Creating streamvault-network..."
    & $PODMAN network create streamvault-network 2>/dev/null | Where-Object { $_ }
    
    # Start PostgreSQL (already running, but ensure it's on network)
    Write-Status "🗄️  PostgreSQL already running on port 5432"
    
    # Start Redis (already running)
    Write-Status "⚡ Redis already running on port 6379"
    
    # Connect existing containers to network
    Write-Status "🔗 Connecting PostgreSQL to network..."
    & $PODMAN network connect streamvault-network streamvault-postgres 2>&1 | Where-Object { $_ -notmatch "already" }
    
    Write-Status "🔗 Connecting Redis to network..."
    & $PODMAN network connect streamvault-network streamvault-redis 2>&1 | Where-Object { $_ -notmatch "already" }
    
    # Build and run backend
    Write-Status ""
    Write-Status "🏗️  Building backend image..."
    Push-Location "c:\Users\Admin\Desktop\newproject\streamvault-backend"
    & $PODMAN build -t streamvault-backend:latest -f Dockerfile . 2>&1 | Select-Object -Last 5
    Pop-Location
    
    if ($LASTEXITCODE -eq 0) {
        Write-Status "✅ Backend image built" "success"
        Write-Status "▶️  Starting backend container..."
        & $PODMAN run -d `
            --name streamvault-backend `
            --network streamvault-network `
            -p 8080:8080 `
            -e ASPNETCORE_ENVIRONMENT=Production `
            -e ASPNETCORE_URLS=http://+:8080 `
            -e "ConnectionStrings__DefaultConnection=Host=streamvault-postgres;Port=5432;Database=streamvault_db;Username=streamvault;Password=StreamVault123!" `
            -e "Redis__ConnectionString=streamvault-redis:6379,password=StreamVault123!" `
            --restart unless-stopped `
            streamvault-backend:latest 2>&1
        
        Start-Sleep -Seconds 3
        Write-Status "✅ Backend container started" "success"
    } else {
        Write-Status "❌ Backend build failed" "error"
    }
    
    # Build and run frontend
    Write-Status ""
    Write-Status "🏗️  Building frontend image..."
    Push-Location "c:\Users\Admin\Desktop\newproject\streamvault-admin-dashboard"
    & $PODMAN build -t streamvault-frontend:latest -f Dockerfile . 2>&1 | Select-Object -Last 5
    Pop-Location
    
    if ($LASTEXITCODE -eq 0) {
        Write-Status "✅ Frontend image built" "success"
        Write-Status "▶️  Starting frontend container..."
        & $PODMAN run -d `
            --name streamvault-frontend `
            --network streamvault-network `
            -p 3000:3000 `
            -e NODE_ENV=production `
            -e NEXT_PUBLIC_API_URL=http://streamvault-backend:8080 `
            -e PORT=3000 `
            --restart unless-stopped `
            streamvault-frontend:latest 2>&1
        
        Start-Sleep -Seconds 3
        Write-Status "✅ Frontend container started" "success"
    } else {
        Write-Status "❌ Frontend build failed" "error"
    }
    
    # Show status
    Write-Status ""
    Show-Complete-Status
}

function Stop-Environment {
    Write-Status "🛑 Stopping StreamVault containers..."
    & $PODMAN stop streamvault-frontend streamvault-backend 2>&1 | Where-Object { $_ }
    & $PODMAN rm streamvault-frontend streamvault-backend 2>&1 | Where-Object { $_ }
    Write-Status "✅ Containers stopped" "success"
}

function Show-Complete-Status {
    Write-Status "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Status "✨ STREAMVAULT PROFESSIONAL ENVIRONMENT READY" "success"
    Write-Status "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    Write-Status ""
    Write-Status "🌐 SERVICE ENDPOINTS" "success"
    Write-Status "   Frontend:  http://localhost:3000 (shadcn/ui Admin Dashboard)"
    Write-Status "   Backend:   http://localhost:8080 (ASP.NET Core API)"
    Write-Status "   Database:  localhost:5432 (PostgreSQL 15)"
    Write-Status "   Cache:     localhost:6379 (Redis 7)"
    
    Write-Status ""
    Write-Status "👤 LOGIN CREDENTIALS" "success"
    Write-Status "   Super Admin:"
    Write-Status "     Email: admin@streamvault.com"
    Write-Status "     Pass:  SuperAdmin123!"
    Write-Status ""
    Write-Status "   Business Admin:"
    Write-Status "     Email: business@streamvault.com"
    Write-Status "     Pass:  BusinessAdmin123!"
    
    Write-Status ""
    Write-Status "📦 RUNNING CONTAINERS"
    & $PODMAN ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | Where-Object { $_ }
    
    Write-Status ""
    Write-Status "💾 DATABASE" "success"
    Write-Status "   Host:     streamvault-postgres"
    Write-Status "   Database: streamvault_db"
    Write-Status "   User:     streamvault"
    Write-Status "   Pass:     StreamVault123!"
    
    Write-Status ""
    Write-Status "⚡ REDIS" "success"
    Write-Status "   Host:     streamvault-redis"
    Write-Status "   Port:     6379"
    Write-Status "   Password: StreamVault123!"
    
    Write-Status ""
    Write-Status "🔗 CONTAINER NETWORK"
    Write-Status "   All containers connected to: streamvault-network"
    
    Write-Status ""
    Write-Status "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Status "✅ Open http://localhost:3000 to access dashboard" "success"
    Write-Status "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
}

function Show-Logs {
    param([string]$Container = "")
    if ($Container) {
        Write-Status "📋 Logs from $Container"
        & $PODMAN logs -f $Container
    } else {
        Write-Status "📋 Logs from all containers"
        & $PODMAN logs -f streamvault-frontend streamvault-backend streamvault-postgres streamvault-redis
    }
}

# Execution
switch ($Action) {
    'start' { Start-Environment }
    'stop' { Stop-Environment }
    'logs' { Show-Logs }
}

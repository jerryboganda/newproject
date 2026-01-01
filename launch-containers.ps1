#!/usr/bin/env pwsh
<#
.SYNOPSIS
    StreamVault - Professional Container-Based Development Environment
.DESCRIPTION
    Launches StreamVault backend and frontend as containers using Podman
.EXAMPLE
    .\launch-containers.ps1
#>

param(
    [ValidateSet('start', 'stop', 'restart', 'status', 'logs')]
    [string]$Action = 'start'
)

# Podman executable path
$PODMAN = "C:\Program Files\RedHat\Podman\podman.exe"
$COMPOSE_FILE = "docker-compose.prod.yml"

function Write-Status($message, $type = "info") {
    $colors = @{
        "info"    = "Cyan"
        "success" = "Green"
        "warning" = "Yellow"
        "error"   = "Red"
    }
    Write-Host "[$([datetime]::Now.ToString('HH:mm:ss'))] " -NoNewline
    Write-Host $message -ForegroundColor $colors[$type]
}

function Invoke-Podman($args) {
    & $PODMAN @args
}

function Start-ContainerEnvironment {
    Write-Status "🚀 Starting StreamVault container environment..."
    Write-Status "📋 Using compose file: $COMPOSE_FILE"
    
    # Check if compose file exists
    if (-not (Test-Path $COMPOSE_FILE)) {
        Write-Status "❌ Compose file not found: $COMPOSE_FILE" "error"
        return $false
    }
    
    # Check Podman machine is running
    Write-Status "📡 Checking Podman machine status..."
    $machineStatus = Invoke-Podman info 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Status "⚠️  Podman machine might not be running. Attempting to start..." "warning"
        # Try to start machine if available
    }
    
    # Create network if doesn't exist
    Write-Status "🔗 Ensuring network exists..."
    Invoke-Podman network create streamvault-network 2>&1 | Where-Object { $_ -notmatch "already exists" }
    
    # Build and start containers
    Write-Status "🏗️  Building images..."
    Invoke-Podman compose -f $COMPOSE_FILE build --no-cache 2>&1 | Select-Object -Last 20
    
    if ($LASTEXITCODE -eq 0) {
        Write-Status "✅ Images built successfully" "success"
    } else {
        Write-Status "❌ Build failed" "error"
        return $false
    }
    
    Write-Status "▶️  Starting containers..."
    Invoke-Podman compose -f $COMPOSE_FILE up -d 2>&1 | Select-Object -Last 10
    
    if ($LASTEXITCODE -eq 0) {
        Write-Status "✅ Containers started" "success"
        Show-Environment-Status
        return $true
    } else {
        Write-Status "❌ Failed to start containers" "error"
        return $false
    }
}

function Stop-ContainerEnvironment {
    Write-Status "🛑 Stopping StreamVault containers..."
    Invoke-Podman compose -f $COMPOSE_FILE down 2>&1 | Select-Object -Last 5
    Write-Status "✅ Containers stopped" "success"
}

function Restart-ContainerEnvironment {
    Stop-ContainerEnvironment
    Start-Sleep -Seconds 2
    Start-ContainerEnvironment
}

function Show-Environment-Status {
    Write-Status "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Status "📊 STREAMVAULT CONTAINER ENVIRONMENT STATUS" "success"
    Write-Status "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    Write-Status ""
    Write-Status "🌐 SERVICES"
    Write-Status "  Frontend Dashboard: http://localhost:3000" "success"
    Write-Status "  Backend API:        http://localhost:8080" "success"
    Write-Status "  PostgreSQL:         localhost:5432" "success"
    Write-Status "  Redis Cache:        localhost:6379" "success"
    
    Write-Status ""
    Write-Status "👤 TEST CREDENTIALS"
    Write-Status "  Super Admin:"
    Write-Status "    Email:    admin@streamvault.com"
    Write-Status "    Password: SuperAdmin123!"
    Write-Status ""
    Write-Status "  Business Admin:"
    Write-Status "    Email:    business@streamvault.com"
    Write-Status "    Password: BusinessAdmin123!"
    
    Write-Status ""
    Write-Status "📦 CONTAINER STATUS"
    Invoke-Podman ps --filter "label!=io.podman.compose.project!=streamvault" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    Write-Status ""
    Write-Status "🔍 HEALTH CHECKS"
    $containers = @("streamvault-postgres", "streamvault-redis", "streamvault-backend", "streamvault-frontend")
    foreach ($container in $containers) {
        $status = Invoke-Podman inspect $container --format "{{.State.Health.Status}}" 2>/dev/null
        if ($status) {
            $healthStatus = if ($status -eq "healthy") { "✅" } else { "⚠️" }
            Write-Status "  $healthStatus $container : $status"
        }
    }
    
    Write-Status ""
    Write-Status "💾 DATABASE CREDENTIALS"
    Write-Status "  Host:     postgres (streamvault-postgres)"
    Write-Status "  Port:     5432"
    Write-Status "  User:     streamvault"
    Write-Status "  Password: StreamVault123!"
    Write-Status "  Database: streamvault_db"
    
    Write-Status ""
    Write-Status "⚡ REDIS CREDENTIALS"
    Write-Status "  Host:     redis (streamvault-redis)"
    Write-Status "  Port:     6379"
    Write-Status "  Password: StreamVault123!"
    
    Write-Status ""
    Write-Status "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
}

function Show-Logs {
    param([string]$Container = "")
    
    if ($Container) {
        Write-Status "📋 Showing logs for: $Container"
        Invoke-Podman logs -f $Container
    } else {
        Write-Status "📋 Showing logs for all services..."
        Invoke-Podman compose -f $COMPOSE_FILE logs -f
    }
}

function Show-Status {
    Write-Status "Checking container status..."
    Invoke-Podman ps -a --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    Write-Status ""
    Invoke-Podman compose -f $COMPOSE_FILE ps
}

# Main execution
switch ($Action) {
    'start' {
        Start-ContainerEnvironment
    }
    'stop' {
        Stop-ContainerEnvironment
    }
    'restart' {
        Restart-ContainerEnvironment
    }
    'status' {
        Show-Status
        Show-Environment-Status
    }
    'logs' {
        Show-Logs
    }
    default {
        Write-Status "Unknown action: $Action" "error"
    }
}

Write-Status ""
Write-Status "For more commands:"
Write-Status "  .\launch-containers.ps1 start    - Start all containers"
Write-Status "  .\launch-containers.ps1 stop     - Stop all containers"
Write-Status "  .\launch-containers.ps1 restart  - Restart all containers"
Write-Status "  .\launch-containers.ps1 status   - Show container status"
Write-Status "  .\launch-containers.ps1 logs     - Stream container logs"

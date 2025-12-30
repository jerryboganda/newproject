@echo off
echo ========================================
echo    StreamVault Docker Setup Script
echo ========================================
echo.

echo [1/4] Checking Docker Desktop installation...
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Docker Desktop is not installed or not in PATH
    echo Please install Docker Desktop from: https://www.docker.com/products/docker-desktop
    echo After installation, restart this script
    pause
    exit /b 1
)
echo Docker Desktop is installed!

echo.
echo [2/4] Starting Docker Desktop...
echo Please wait for Docker to start...
:wait_docker
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo Docker is starting... Please wait...
    timeout /t 5 >nul
    goto wait_docker
)
echo Docker Desktop is running!

echo.
echo [3/4] Building and starting services...
cd /d "%~dp0"
docker-compose down
docker-compose build --no-cache
if %errorlevel% neq 0 (
    echo ERROR: Failed to build services
    pause
    exit /b 1
)

echo.
echo [4/4] Starting all containers...
docker-compose up -d
if %errorlevel% neq 0 (
    echo ERROR: Failed to start containers
    pause
    exit /b 1
)

echo.
echo ========================================
echo    StreamVault is now running!
echo ========================================
echo.
echo Frontend: http://localhost:3001
echo Backend API: http://localhost:5000
echo MinIO Console: http://localhost:9001
echo PostgreSQL: localhost:5432
echo Redis: localhost:6379
echo.
echo Default Login:
echo Email: admin@streamvault.com
echo Password: Admin123!
echo.
echo To stop all services, run: docker-compose down
echo.
pause

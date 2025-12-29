@echo off
echo ========================================
echo    StreamVault Podman Setup
echo ========================================
echo.

echo [1/4] Checking Podman...
podman --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Podman not found in PATH
    echo Please restart your terminal after installation
    echo Or add to PATH: C:\Program Files\RedHat\Podman
    pause
    exit /b 1
)
echo Podman found!

echo.
echo [2/4] Checking Podman machine...
podman machine list | findstr "running" >nul 2>&1
if %errorlevel% neq 0 (
    echo Starting Podman machine...
    podman machine init
    podman machine start
) else (
    echo Podman machine is running!
)

echo.
echo [3/4] Building services...
cd /d "%~dp0"
podman-compose -f podman-compose.yml down
podman-compose -f podman-compose.yml build --no-cache
if %errorlevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo [4/4] Starting containers...
podman-compose -f podman-compose.yml up -d
if %errorlevel% neq 0 (
    echo ERROR: Failed to start
    pause
    exit /b 1
)

echo.
echo ========================================
echo    StreamVault is running!
echo ========================================
echo.
echo Frontend: http://localhost:3001
echo Backend: http://localhost:5000
echo MinIO: http://localhost:9001
echo.
echo Login: admin@streamvault.com / Admin123!
echo.
pause

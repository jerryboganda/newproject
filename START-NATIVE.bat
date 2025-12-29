@echo off
title StreamVault Native Setup
color 0A
echo ========================================
echo   StreamVault Native Windows Setup
echo ========================================
echo.

echo [1/4] Stopping any existing services...
taskkill /F /IM dotnet.exe >nul 2>&1
taskkill /F /IM node.exe >nul 2>&1
echo Done!

echo.
echo [2/4] Starting Backend API...
cd /d "C:\Users\Admin\CascadeProjects\newproject\WorkingBackend"
start "Backend API" cmd /k "dotnet run --urls=http://localhost:5000"
echo Backend starting on http://localhost:5000

echo.
echo [3/4] Installing frontend dependencies...
cd /d "C:\Users\Admin\CascadeProjects\newproject\streamvault-frontend"
if not exist "node_modules" (
    echo Installing npm packages...
    npm install
)

echo.
echo [4/4] Starting Frontend...
start "Frontend" cmd /k "npm run dev"
echo Frontend starting on http://localhost:3000

echo.
echo ========================================
echo     StreamVault is running!
echo ========================================
echo.
echo Frontend: http://localhost:3000
echo Backend:  http://localhost:5000
echo.
echo Login: admin@streamvault.com / Admin123!
echo.
echo No Docker, No Podman, No Virtualization!
echo.
echo Opening browser in 10 seconds...
timeout /t 10

start http://localhost:3000
echo.
echo Done! Keep the terminal windows open to run the services.
pause

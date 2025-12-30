@echo off
title StreamVault Setup
color 0A
echo ========================================
echo     StreamVault Native Setup
echo ========================================
echo.

echo [1/3] Starting Backend API...
start "Backend" /min cmd /c "cd /d C:\Users\Admin\CascadeProjects\newproject\WorkingBackend && dotnet run --urls=http://localhost:5000"
echo Backend starting on port 5000...

echo.
echo [2/3] Starting Frontend...
cd /d C:\Users\Admin\CascadeProjects\newproject\streamvault-frontend
if not exist "node_modules" (
    echo Installing dependencies...
    call npm install
)
start "Frontend" /min cmd /c "npm run dev"
echo Frontend starting on port 3000...

echo.
echo [3/3] Waiting for services to start...
timeout /t 15 /nobreak >nul

echo.
echo ========================================
echo     StreamVault is Running!
echo ========================================
echo.
echo Frontend: http://localhost:3000
echo Backend:  http://localhost:5000
echo.
echo Login: admin@streamvault.com
echo        Admin123!
echo.
echo Opening browser...
start http://localhost:3000

echo.
echo Services are running in minimized windows.
echo Close them or press Ctrl+C to stop.
pause

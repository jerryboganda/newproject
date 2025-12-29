@echo off
echo Starting StreamVault Mock API Server...
cd /d "%~dp0"
"C:\Program Files\nodejs\node.exe" mock-api-server.js
pause

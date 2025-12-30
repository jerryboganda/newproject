# 🔧 StreamVault Troubleshooting Guide

## Issue: Connection Refused (ERR_CONNECTION_REFUSED)

### Root Cause:
The backend and frontend services are not running because .NET and Node.js are not in the system PATH.

## 🔍 Diagnostic Steps:

### 1. Check if .NET is Installed
```powershell
# Check default installation paths
dir "C:\Program Files\dotnet"
dir "C:\Program Files (x86)\dotnet"
```

### 2. Check if Node.js is Installed
```powershell
# Check default installation paths
dir "C:\Program Files\nodejs"
dir "C:\Program Files (x86)\nodejs"
dir "%APPDATA%\npm"
```

### 3. Add to PATH (Temporary for current session)
```powershell
# Add .NET to PATH
$env:PATH += ";C:\Program Files\dotnet"

# Add Node.js to PATH
$env:PATH += ";C:\Program Files\nodejs"

# Verify
dotnet --version
node --version
npm --version
```

## 🚀 Quick Fix Script:

### Option 1: Run with Full Paths
```powershell
# Start Backend (in PowerShell as Admin)
cd C:\Users\Admin\CascadeProjects\newproject\streamvault-backend\src\StreamVault.API
& "C:\Program Files\dotnet\dotnet.exe" run

# Start Frontend (in new PowerShell as Admin)
cd C:\Users\Admin\CascadeProjects\newproject\streamvault-admin-dashboard
& "C:\Program Files\nodejs\node.exe" "C:\Program Files\nodejs\npm.cmd" run dev
```

### Option 2: Update PATH Permanently
```powershell
# Add to system PATH (run as Administrator)
[Environment]::SetEnvironmentVariable("PATH", $env:PATH + ";C:\Program Files\dotnet;C:\Program Files\nodejs", "Machine")
```

## 📝 Step-by-Step Solution:

### Step 1: Open PowerShell as Administrator
- Right-click Start Menu
- Select "Windows PowerShell (Admin)"

### Step 2: Add tools to PATH
```powershell
$env:PATH += ";C:\Program Files\dotnet;C:\Program Files\nodejs"
```

### Step 3: Verify Installation
```powershell
dotnet --version
node --version
npm --version
```

### Step 4: Start Services
```powershell
# Terminal 1 - Backend
cd C:\Users\Admin\CascadeProjects\newproject\streamvault-backend\src\StreamVault.API
dotnet run

# Terminal 2 - Frontend (new window)
cd C:\Users\Admin\CascadeProjects\newproject\streamvault-admin-dashboard
npm run dev
```

## 🔍 If Still Not Working:

### Check PostgreSQL:
```powershell
Get-Service postgresql*
# Should show "Running" status
```

### Check Memurai:
```powershell
Get-Process memurai
# Should show the running process
```

### Check Ports:
```powershell
netstat -an | findstr :5000  # Backend
netstat -an | findstr :3000  # Frontend
```

## 🆘 Alternative Solutions:

### 1. Use Visual Studio
- Open `streamvault-backend\StreamVault.sln`
- Press F5 to run

### 2. Use VS Code
- Open the project folder
- Use the integrated terminal
- Install .NET and Node.js extensions

### 3. Reinstall Tools
If tools are not found in default paths:
- Download .NET 8 SDK: https://dotnet.microsoft.com/download
- Download Node.js LTS: https://nodejs.org/

## 📞 Common Issues:

1. **"dotnet not found"** → .NET not installed or not in PATH
2. **"npm not found"** → Node.js not installed or not in PATH
3. **"Database connection failed"** → PostgreSQL not running
4. **"Redis connection failed"** → Memurai not running

## ✅ Success Indicators:
- Backend: "info: Microsoft.Hosting.Lifetime[14]" in console
- Frontend: "ready - started server on 0.0.0.0:3000" in console
- Can access http://localhost:5000/swagger
- Can access http://localhost:3000

Once services are running, you should be able to access the URLs without errors!

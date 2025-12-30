# 🚀 StreamVault System Startup Guide

## Current Status ✅
- PostgreSQL: Running (postgresql-x64-18)
- Memurai (Redis): Running (Process ID: 1852)
- Environment files: Configured
- .gitignore: Updated to allow .env.local

## 📋 To Start the System:

### Step 1: Open PowerShell as Administrator
```powershell
# Right-click PowerShell -> "Run as Administrator"
```

### Step 2: Navigate to Project Directory
```powershell
cd C:\Users\Admin\CascadeProjects\newproject
```

### Step 3: Start Backend API
```powershell
# In PowerShell Terminal 1
cd streamvault-backend\src\StreamVault.API
dotnet run
```

### Step 4: Start Frontend (in new PowerShell window)
```powershell
# Open new PowerShell window
cd C:\Users\Admin\CascadeProjects\newproject\streamvault-admin-dashboard
npm run dev
```

### Step 5: Run Database Migrations (if needed)
```powershell
# In another PowerShell window
cd C:\Users\Admin\CascadeProjects\newproject\streamvault-backend
dotnet ef database update --project src\StreamVault.Infrastructure --startup-project src\StreamVault.API
```

## 🔑 Login Credentials

| Role | Email | Password | URL |
|------|-------|----------|-----|
| Super Admin | superadmin@streamvault.app | SuperAdmin123! | http://localhost:3000/admin |
| Tenant Admin | admin@tenant1.com | TenantAdmin123! | http://localhost:3000 |
| Regular User | user@tenant1.com | User123! | http://localhost:3000 |

## 🌐 Access URLs

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Swagger Documentation**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/api/health

## ⚙️ Configuration Files Created

1. **Backend Config**: `streamvault-backend\src\StreamVault.API\appsettings.Development.json`
   - Database: PostgreSQL on localhost:5432
   - Redis: Memurai on localhost:6379
   
2. **Frontend Config**: `streamvault-admin-dashboard\.env.local`
   - API URL: http://localhost:5000/api
   - WebSocket URL: ws://localhost:5000/ws

## 🐛 Troubleshooting

### If .NET command not found:
```powershell
# Close and reopen PowerShell as Administrator
# Or run:
& "C:\Program Files\dotnet\dotnet.exe" --version
```

### If npm command not found:
```powershell
# Close and reopen PowerShell
# Or check Node.js installation:
node --version
npm --version
```

### Database Connection Issues:
1. Ensure PostgreSQL service is running
2. Check connection string in appsettings.Development.json
3. Verify database 'streamvault_dev' exists

### Redis Connection Issues:
1. Ensure Memurai is running
2. Check if port 6379 is available
3. Verify Redis connection string

## 📝 After Login

1. **Configure Bunny.net** (as Super Admin):
   - Go to Settings > Integrations
   - Add Bunny Stream API Key
   - Add Library ID
   - Add Pull Zone ID

2. **Test Features**:
   - Upload a video
   - Create collections
   - Manage users
   - View analytics
   - Test billing features

## 🛑 To Stop the System

1. Press Ctrl+C in backend terminal
2. Press Ctrl+C in frontend terminal
3. Close PowerShell windows

## 📚 Additional Notes

- The system will automatically seed initial data on first run
- All services are running locally (no containers needed)
- Logs will appear in the respective terminal windows
- The system supports hot reload for development

Happy testing! 🎉

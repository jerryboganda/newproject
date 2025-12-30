# StreamVault System Launch Guide

## Prerequisites Installation

Since the development tools are not installed, please install the following:

### 1. Install .NET 8.0 SDK
Download and install from: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. Install Node.js 18+
Download and install from: https://nodejs.org/

### 3. Install PostgreSQL 15
Download and install from: https://www.postgresql.org/download/windows/

### 4. Install Redis
Download and install from: https://redis.io/download

## Launch Instructions

### Step 1: Start PostgreSQL
```powershell
# Start PostgreSQL service
net start postgresql-x64-15

# Create database
psql -U postgres -c "CREATE DATABASE streamvault;"
psql -U postgres -c "CREATE USER streamvault WITH PASSWORD 'streamvault123';"
psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE streamvault TO streamvault;"
```

### Step 2: Start Redis
```powershell
# Start Redis server
redis-server
```

### Step 3: Launch Backend API
```powershell
cd C:\Users\Admin\CascadeProjects\newproject\streamvault-backend\src\StreamVault.API
dotnet run
```

### Step 4: Launch Frontend
```powershell
cd C:\Users\Admin\CascadeProjects\newproject\streamvault-admin-dashboard
npm run dev
```

## Default Users and Access URLs

### 1. Super Admin Panel
- **URL**: http://localhost:3000/admin
- **Username**: superadmin@streamvault.app
- **Password**: SuperAdmin123!

### 2. Business Admin Panel (Tenant)
- **URL**: http://localhost:3000
- **Username**: admin@tenant1.com
- **Password**: TenantAdmin123!

### 3. Regular User
- **URL**: http://localhost:3000
- **Username**: user@tenant1.com
- **Password**: User123!

### 4. API Documentation
- **URL**: http://localhost:5000/swagger

## Database Seed Data

The system will automatically create:
- 1 Super Admin user
- 1 Tenant (Acme Corporation)
- 1 Tenant Admin user
- 2 Regular users
- Sample videos and collections

## Bunny.net Configuration

After logging in as Super Admin:
1. Go to Settings > Integrations
2. Add your Bunny.net Stream API Key
3. Configure your Library ID and Pull Zone
4. Save settings

## Testing the System

### 1. Video Upload
- Login as Tenant Admin
- Navigate to Videos > Upload
- Test video upload functionality

### 2. User Management
- Login as Tenant Admin
- Go to Users > Manage Users
- Create, edit, and deactivate users

### 3. Analytics
- View dashboard analytics
- Check video views and usage statistics

### 4. Billing
- View subscription plans
- Check usage tracking

### 5. Support System
- Create support tickets
- Test knowledge base

## Troubleshooting

### Backend Issues
- Check PostgreSQL is running on port 5432
- Verify Redis is running on port 6379
- Check appsettings.json for correct connection strings

### Frontend Issues
- Clear browser cache
- Check console for errors
- Verify API URL is correct in .env.local

### Database Issues
- Run migrations: `dotnet ef database update`
- Check connection string in appsettings.json

## Environment Variables

Create `.env.local` in the frontend directory:
```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
NEXT_PUBLIC_WS_URL=ws://localhost:5000/ws
```

Create `appsettings.Development.json` in the backend:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=streamvault;Username=streamvault;Password=streamvault123"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-here-must-be-32-chars",
    "Issuer": "StreamVault",
    "Audience": "StreamVault"
  }
}
```

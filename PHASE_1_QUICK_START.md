# StreamVault Quick Start Guide - Phase 1

## Prerequisites

- **Podman Desktop** installed (Windows)
- **.NET 8 SDK** (for local development without containers)
- **Node.js 18+** (for frontend development)
- **PostgreSQL 15** (if running outside container)

## Quick Start (5 minutes)

### 1. Clone and Setup
```bash
cd c:\Users\Admin\Desktop\newproject

# Copy environment file
cp .env.example .env

# Edit .env file with your Stripe test keys and email settings
# (or use defaults for local development)
```

### 2. Start Services with Podman
```bash
# Start all services
podman-compose up -d

# Verify services are running
podman ps
# You should see:
# - streamvault-postgres
# - streamvault-redis
# - streamvault-api
# - streamvault-frontend
# - streamvault-nginx
```

### 3. Access the Application
```
Frontend: http://localhost:3000
API: http://localhost:5000
API Swagger: http://localhost:5000/swagger
```

### 4. Test Login
```
Workspace: default (or leave empty)
Email: admin@example.com
Password: Password123!
```

## Local Development (Alternative)

If you want to run without containers:

### Backend (ASP.NET Core)
```bash
cd streamvault-backend

# Install dependencies
dotnet restore

# Run migrations
dotnet ef database update -p src/StreamVault.Infrastructure -s src/StreamVault.Api

# Start API
dotnet run --project src/StreamVault.Api/StreamVault.Api.csproj
# API runs on http://localhost:5000
```

### Frontend (Next.js)
```bash
cd streamvault-frontend

# Install dependencies
npm install

# Start dev server
npm run dev
# Frontend runs on http://localhost:3000
```

## Docker-to-Podman Command Equivalents

```bash
# Instead of:      Use:
docker ps         podman ps
docker logs       podman logs
docker exec       podman exec
docker build      podman build
docker-compose    podman-compose
```

## Environment Configuration

### Required for Production
```bash
# Authentication
JWT_SECRET_KEY=your_super_secret_key_min_32_chars

# Stripe (test keys for development)
STRIPE_SECRET_KEY=sk_test_YOUR_KEY
STRIPE_PUBLISHABLE_KEY=pk_test_YOUR_KEY

# Email Service (use Mailtrap for testing)
EMAIL_SMTP_SERVER=smtp.mailtrap.io
EMAIL_SMTP_PORT=2525
EMAIL_SMTP_USER=your_user
EMAIL_SMTP_PASSWORD=your_password

# Bunny.net (get from bunnycdn.com)
BUNNY_API_KEY=your_api_key
BUNNY_LIBRARY_ID=your_library_id
```

### Optional
```bash
# Database
POSTGRES_PASSWORD=streamvault_secure_pass_2025

# Redis
REDIS_PASSWORD=streamvault_redis_2025

# Application URLs
APP_URL=http://localhost
API_URL=http://localhost:5000
FRONTEND_URL=http://localhost:3000
```

## Features Implemented

### Authentication ✅
- [x] Email/password login
- [x] 2FA email verification
- [x] User registration
- [x] Email verification
- [x] Password reset
- [x] Refresh token rotation
- [x] Super admin impersonation
- [x] JWT with claims

### Authorization ✅
- [x] 40+ core permissions
- [x] Role-based access control
- [x] Custom role creation
- [x] Wildcard permissions (videos.*)
- [x] Permission seeding
- [x] Multi-tenant isolation
- [x] Policy-based authorization

### Infrastructure ✅
- [x] PostgreSQL 15 database
- [x] Redis caching
- [x] Nginx reverse proxy
- [x] Podman container orchestration
- [x] Health checks
- [x] Logging and monitoring hooks

## API Endpoints

### Authentication
```
POST   /api/v1/auth/login              - Login
POST   /api/v1/auth/login/verify-2fa   - Verify 2FA
POST   /api/v1/auth/register           - Register
POST   /api/v1/auth/verify-email       - Verify email
POST   /api/v1/auth/refresh            - Refresh token
POST   /api/v1/auth/logout             - Logout
POST   /api/v1/auth/forgot-password    - Request reset
POST   /api/v1/auth/reset-password     - Reset password
POST   /api/v1/auth/impersonate        - Impersonate user
GET    /api/v1/auth/me                 - Current user
```

### Health
```
GET    /api/health                     - Health check
```

## Database

### Connection String
```
Server=localhost;Database=streamvault;User Id=streamvault;Password=streamvault_secure_pass_2025;
```

### Default Database
```
Database: streamvault
User: streamvault
Password: streamvault_secure_pass_2025
Port: 5432
```

### Connect with pgAdmin
```
Host: localhost:5432
Username: streamvault
Password: streamvault_secure_pass_2025
Database: streamvault
```

## Troubleshooting

### Services won't start
```bash
# Check Podman status
podman ps -a

# View container logs
podman logs streamvault-api
podman logs streamvault-postgres

# Remove and restart
podman-compose down
podman-compose up -d
```

### Database connection issues
```bash
# Check PostgreSQL is running
podman logs streamvault-postgres

# Verify connection string in .env
# Test connection from API container
podman exec streamvault-api curl http://postgres:5432
```

### Frontend can't reach API
```bash
# Check API is responding
curl http://localhost:5000/api/health

# Check CORS configuration
# Verify API_BASE_URL in frontend .env
```

### Podman permission denied
```bash
# On Windows, ensure Podman Desktop is running
# And your user is in the podman group

# Check socket
ls -la /run/podman/podman.sock
```

## Development Workflow

### 1. Make changes to backend
```bash
# Option A: With containers
podman-compose restart api

# Option B: Local development
cd streamvault-backend
dotnet watch run --project src/StreamVault.Api
```

### 2. Make changes to frontend
```bash
# Option A: With containers
podman-compose restart frontend

# Option B: Local development (auto-reload)
cd streamvault-frontend
npm run dev
```

### 3. Run database migrations
```bash
cd streamvault-backend
dotnet ef migrations add YourMigrationName -p src/StreamVault.Infrastructure -s src/StreamVault.Api
dotnet ef database update -p src/StreamVault.Infrastructure -s src/StreamVault.Api
```

### 4. Seed test data
```csharp
// In Program.cs:
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
    await PermissionSeeder.SeedPermissionsAsync(dbContext);
}
```

## Testing

### Manual API Testing with cURL
```bash
# Login
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "Password123!",
    "tenantSlug": "default"
  }'

# Get current user (with token)
curl -X GET http://localhost:5000/api/v1/auth/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"

# Health check
curl http://localhost:5000/api/health
```

### Swagger UI
Open: http://localhost:5000/swagger/index.html

## Next Steps

1. **Test Authentication:**
   - Try login with test credentials
   - Verify 2FA email receives code
   - Test password reset flow

2. **Test Authorization:**
   - Create custom roles
   - Assign permissions
   - Test endpoint access

3. **Prepare for Phase 2:**
   - Review Bunny.net API documentation
   - Set up Bunny.net library per tenant
   - Plan video upload implementation

## Support

### Logs
```bash
# API logs
podman logs -f streamvault-api

# Frontend logs
podman logs -f streamvault-frontend

# Database logs
podman logs -f streamvault-postgres

# View all logs
podman-compose logs -f
```

### Database Inspection
```bash
# Access PostgreSQL CLI
podman exec -it streamvault-postgres psql -U streamvault -d streamvault

# Common SQL commands
\dt                          # List tables
SELECT * FROM "Users";       # View users
SELECT * FROM "Roles";       # View roles
SELECT * FROM "Permissions"; # View permissions
```

---

**Quick Start Complete!**

Next: Phase 2 - Bunny.net Integration & Video Management

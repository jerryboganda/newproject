# StreamVault Development Setup Guide

## Week 1 Completed Tasks

### Backend ✅
- [x] ASP.NET Core solution structure set up
- [x] Entity Framework Core configured with PostgreSQL
- [x] Project layers established (API, Application, Domain, Infrastructure)
- [x] Dependency injection container configured
- [x] Serilog logging configured
- [x] Base entities created
- [x] Initial database migration created

### Frontend ✅
- [x] Next.js project initialized with TypeScript
- [x] Tailwind CSS configured
- [x] ESLint and Prettier set up
- [x] Base layout components created
- [x] API client (Axios wrapper) implemented
- [x] Zustand state management configured

### DevOps ✅
- [x] Docker development environment configured
- [x] PostgreSQL and Redis containers set up
- [x] Docker Compose configuration ready

## Getting Started

### Prerequisites
- .NET 7.0 SDK
- Node.js 18+
- Docker Desktop
- Git

### Running the Application

1. **Start the infrastructure services:**
   ```bash
   docker-compose up -d postgres redis minio
   ```

2. **Run database migrations:**
   ```bash
   cd streamvault-backend
   dotnet ef database update --project src/StreamVault.Infrastructure --startup-project src/StreamVault.Api
   ```

3. **Start the backend API:**
   ```bash
   cd src/StreamVault.Api
   dotnet run
   ```

4. **Start the frontend:**
   ```bash
   cd streamvault-frontend
   npm install
   npm run dev
   ```

### Access Points
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- API Health Check: http://localhost:5000/health
- MinIO Console: http://localhost:9001

### Environment Variables

**Backend (.env):**
```env
ASPNETCORE_ENVIRONMENT=Development
POSTGRES_CONNECTION_STRING="Host=localhost;Port=5432;Database=streamvault_master;Username=streamvault;Password=streamvault"
REDIS_CONNECTION_STRING="localhost:6379"
JWT_ISSUER="streamvault"
JWT_AUDIENCE="streamvault"
JWT_SIGNING_KEY="your-secret-key-here"
```

**Frontend (.env.local):**
```env
NEXT_PUBLIC_API_BASE_URL="http://localhost:5000"
```

## Project Structure

```
streamvault/
├── streamvault-backend/
│   └── src/
│       ├── StreamVault.Api/          # Web API layer
│       ├── StreamVault.Application/  # Business logic
│       ├── StreamVault.Domain/       # Entities and interfaces
│       └── StreamVault.Infrastructure/# Data access and external services
├── streamvault-frontend/
│   ├── app/                          # Next.js app directory
│   ├── components/                   # React components
│   ├── lib/                          # Utility libraries
│   └── stores/                       # Zustand stores
└── docker-compose.yml                # Development containers
```

## Next Steps (Week 2)

1. Implement authentication endpoints
2. Build login/registration pages
3. Set up JWT token handling
4. Implement tenant resolution middleware
5. Create user management features

## Common Commands

**Backend:**
```bash
# Create new migration
dotnet ef migrations add <MigrationName> --project src/StreamVault.Infrastructure --startup-project src/StreamVault.Api

# Apply migrations
dotnet ef database update --project src/StreamVault.Infrastructure --startup-project src/StreamVault.Api

# Build solution
dotnet build

# Run tests
dotnet test
```

**Frontend:**
```bash
# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build

# Run tests
npm test
```

## Troubleshooting

1. **Database connection issues:**
   - Ensure PostgreSQL container is running
   - Check connection string in appsettings.json
   - Verify database exists

2. **CORS issues:**
   - Check CORS configuration in Program.cs
   - Ensure frontend URL is allowed

3. **Build errors:**
   - Run `dotnet clean` then `dotnet build`
   - Check for missing NuGet packages

4. **Node.js issues:**
   - Clear node_modules and reinstall: `rm -rf node_modules && npm install`
   - Check Node.js version: `node --version`

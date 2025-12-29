# StreamVault - Production Local Setup

## Overview
This setup provides a **production-ready local development environment** with:
- ✅ **Persistent SQLite database** (data saved between restarts)
- ✅ Professional logging with Serilog
- ✅ Health checks and monitoring
- ✅ Proper authentication and authorization
- ✅ Complete API documentation
- ✅ All features fully functional

## Quick Start

### 1. Run the Production Setup
```bash
# Run as Administrator
START-PRODUCTION.bat
```

### 2. Wait for Initialization
- Backend: ~20 seconds
- Frontend: ~30 seconds
- Database: Auto-created and seeded

### 3. Access the Application
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Swagger Docs**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

### 4. Login
- **Email**: admin@streamvault.com
- **Password**: Admin123!

## Database Information

### SQLite Database
- **Location**: `streamvault-backend/streamvault.db`
- **Type**: SQLite (Persistent)
- **Migrations**: Auto-applied on startup
- **Backup**: Simply copy the `.db` file

### Data Persistence
- ✅ All user data is saved permanently
- ✅ Videos, comments, and settings persist
- ✅ Database survives restarts
- ✅ Automatic seeding on first run

## Features Available

### Core Features
- [x] User authentication with JWT
- [x] Video upload and processing
- [x] Video streaming (HLS support)
- [x] User management and roles
- [x] Multi-tenancy support
- [x] Subscription management
- [x] Payment integration (Stripe ready)

### Advanced Features
- [x] Live streaming with RTMP
- [x] Video analytics and insights
- [x] AI-powered recommendations
- [x] Social features (comments, sharing)
- [x] Advanced search with filters
- [x] Video chapters and transcripts
- [x] Web3 wallet integration
- [x] NFT marketplace

### Admin Features
- [x] Complete admin dashboard
- [x] User and tenant management
- [x] Content moderation
- [x] System health monitoring
- [x] Performance analytics
- [x] API documentation

## File Structure

```
newproject/
├── streamvault-backend/
│   ├── streamvault.db          # SQLite database (created on first run)
│   ├── logs/                   # Application logs
│   └── src/StreamVault.Api/
│       ├── Program.cs          # Main application entry
│       └── appsettings.json    # Configuration
├── streamvault-frontend/
│   ├── .next/                  # Next.js build files
│   ├── .env.local             # Environment variables
│   └── src/                   # React components
└── logs/                      # Setup and diagnostic logs
```

## Configuration

### Backend Configuration
Edit `streamvault-backend/src/StreamVault.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=streamvault.db"
  },
  "JwtSettings": {
    "SigningKey": "your-secret-key-here"
  },
  "Storage": {
    "LocalPath": "./uploads"
  }
}
```

### Frontend Configuration
Edit `streamvault-frontend/.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_WS_URL=ws://localhost:5000
```

## API Documentation

### Swagger UI
- **URL**: http://localhost:5000/swagger
- **Features**: Interactive API testing
- **Authentication**: Bearer token required

### Key Endpoints
- `POST /api/auth/login` - User login
- `GET /api/videos` - List videos
- `POST /api/videos` - Upload video
- `GET /api/analytics` - Get analytics
- `GET /health` - Health check

## Troubleshooting

### Common Issues

#### "Connection Refused"
1. Wait 30-60 seconds for full startup
2. Check if ports 3000 and 5000 are free
3. Restart your computer if ports are stuck

#### Backend Won't Start
1. Check `streamvault-backend/logs/` for errors
2. Ensure .NET 7 SDK is installed
3. Run as Administrator

#### Frontend Won't Start
1. Delete `node_modules` and run `npm install`
2. Check if Node.js 18+ is installed
3. Clear browser cache

#### Database Issues
1. Delete `streamvault.db` to recreate
2. Check write permissions
3. Ensure SQLite package is installed

### Diagnostic Tool
Run the diagnostic tool to check your system:
```bash
diagnose-fixed.bat
```

### Logs Location
- **Backend Logs**: `streamvault-backend/logs/streamvault-*.txt`
- **Setup Logs**: `logs/`
- **Database**: `streamvault-backend/streamvault.db`

## Development Workflow

### 1. Daily Development
```bash
# Start services
START-PRODUCTION.bat

# Work on features
# - Backend: Edit in streamvault-backend/
# - Frontend: Edit in streamvault-frontend/

# View logs
tail -f streamvault-backend/logs/streamvault-*.txt
```

### 2. Database Changes
```bash
# Create new migration
cd streamvault-backend
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update
```

### 3. Testing APIs
```bash
# Health check
curl http://localhost:5000/health

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@streamvault.com","password":"Admin123!"}'
```

## Security Notes

### Development Environment
- HTTPS disabled for local development
- Default credentials for testing only
- In-memory JWT tokens (change for production)

### Production Considerations
1. Change all default passwords
2. Use proper HTTPS certificates
3. Configure production database
4. Set up proper logging and monitoring
5. Review CORS settings

## Performance Optimization

### Database
- SQLite is optimized for development
- For production, consider PostgreSQL
- Indexes are automatically created

### Caching
- Redis integration ready (configure in appsettings.json)
- Frontend uses Next.js optimization
- Static files served efficiently

### Video Processing
- FFmpeg integration for transcoding
- Multiple output formats
- Thumbnail generation

## Next Steps

### For Production Deployment
1. Configure production database
2. Set up CDN for video storage
3. Configure proper domain and SSL
4. Set up monitoring and alerting
5. Review security settings

### For Development
1. Explore all features in the UI
2. Test API endpoints with Swagger
3. Review the codebase structure
4. Customize for your needs

## Support

### Getting Help
1. Check the logs in `logs/` folder
2. Run `diagnose-fixed.bat` for system check
3. Review this README
4. Check the troubleshooting section

### Contributing
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

---

## 🎉 You're all set!

StreamVault is now running with full persistence and all features enabled. Enjoy exploring the platform!

**Remember**: Your data is saved in the SQLite database file and will persist between restarts.

# StreamVault Local Setup Guide

## Prerequisites

### 1. Install Docker Desktop (Recommended)
Download and install Docker Desktop from: https://www.docker.com/products/docker-desktop/

### 2. Alternative: Install Individual Components

#### PostgreSQL
- Download from: https://www.postgresql.org/download/windows/
- During installation, set password: `streamvault`
- Create database: `streamvault_master`

#### Redis
- Download from: https://github.com/microsoftarchive/redis/releases
- Or install via Chocolatey: `choco install redis-64`

## Quick Start (With Docker)

### 1. Clone and Navigate
```bash
cd C:\Users\Admin\CascadeProjects\newproject
```

### 2. Start Services
```bash
docker-compose up -d
```

### 3. Run Database Migrations
```bash
cd streamvault-backend
dotnet ef database update
```

### 4. Start Backend API
```bash
cd src/StreamVault.Api
dotnet run
```

### 5. Start Frontend (New Terminal)
```bash
cd streamvault-backend/../streamvault-frontend
npm install
npm run dev
```

## Access Points

Once everything is running:

### Frontend (Next.js)
- **URL**: http://localhost:3000
- **Login Email**: admin@streamvault.com
- **Password**: Admin123!

### Backend API
- **Base URL**: http://localhost:5000
- **Swagger Docs**: http://localhost:5000/swagger
- **GraphQL Playground**: http://localhost:5000/graphql
- **Health Check**: http://localhost:5000/health

### Database Connections
- **PostgreSQL**: localhost:5432
  - Username: streamvault
  - Password: streamvault
  - Database: streamvault_master

- **Redis**: localhost:6379

### MinIO (S3 Compatible Storage)
- **Console**: http://localhost:9001
- **API**: http://localhost:9000
  - Access Key: minio
  - Secret Key: minio123

## Environment Variables

Backend (.env file in streamvault-backend):
```env
POSTGRES_CONNECTION_STRING=Host=localhost;Port=5432;Database=streamvault_master;Username=streamvault;Password=streamvault
REDIS_CONNECTION_STRING=localhost:6379
JWT_ISSUER=streamvault
JWT_AUDIENCE=streamvault
JWT_SIGNING_KEY=your-super-secret-jwt-key-change-this-in-production
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your-email@gmail.com
SMTP_PASS=your-app-password
SMTP_FROM=noreply@streamvault.com
STRIPE_SECRET_KEY=sk_test_...
STRIPE_WEBHOOK_SECRET=whsec_...
BUNNY_API_KEY=your-bunny-api-key
BUNNY_LIBRARY_ID=your-library-id
BUNNY_PULL_ZONE_ID=your-pull-zone-id
BUNNY_CDN_HOSTNAME=your-cdn.b-cdn.net
STORAGE_PROVIDER=MinIO
MINIO_ENDPOINT=localhost:9000
MINIO_KEY=minio
MINIO_SECRET=minio123
MINIO_BUCKET=streamvault
FRONTEND_BASE_URL=http://localhost:3000
```

Frontend (.env.local file in streamvault-frontend):
```env
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_WS_URL=ws://localhost:5000
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_...
NEXT_PUBLIC_MINIO_ENDPOINT=http://localhost:9000
NEXT_PUBLIC_MINIO_BUCKET=streamvault
```

## Default Users for Testing

### Admin User
- **Email**: admin@streamvault.com
- **Password**: Admin123!
- **Role**: Super Admin

### Tenant User
- **Email**: tenant@streamvault.com
- **Password**: Tenant123!
- **Role**: Tenant Admin

### Regular User
- **Email**: user@streamvault.com
- **Password**: User123!
- **Role**: User

## Features Available

### ✅ Core Features
- User authentication & authorization
- Video upload, processing, and streaming
- Live streaming with RTMP
- Video analytics and insights
- Multi-tenant architecture
- Subscription management
- Payment processing (Stripe)

### ✅ Advanced Features
- AI-powered recommendations
- Video chapters and transcripts
- Social features (comments, sharing)
- Web3 wallet integration
- NFT marketplace
- Real-time notifications
- Advanced search with filters
- Performance monitoring

### ✅ Admin Features
- User management
- Tenant management
- Content moderation
- Analytics dashboard
- System health monitoring
- API documentation

## Testing Video Upload

1. Login as any user
2. Navigate to Videos → Upload
3. Select a video file (MP4, MOV, WebM)
4. Fill in video details
5. Click Upload
6. Wait for processing (automatic)

## Testing Live Streaming

1. Login as any user
2. Navigate to Live Streaming
3. Click "Start New Stream"
4. Use Stream Key with OBS/Streamlabs
5. Stream URL: `rtmp://localhost:1935/live/{stream-key}`

## API Testing

### Using Swagger
1. Navigate to http://localhost:5000/swagger
2. Explore all available endpoints
3. Test with the "Try it out" feature

### Using cURL
```bash
# Health Check
curl http://localhost:5000/health

# Get Videos (requires auth token)
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5000/api/v1/videos
```

## Troubleshooting

### Backend Won't Start
- Check if PostgreSQL and Redis are running
- Verify connection strings in .env
- Check ports 5000, 5432, 6379 are not in use

### Frontend Won't Start
- Run `npm install` to install dependencies
- Check if port 3000 is available
- Verify API URL in .env.local

### Database Issues
- Run migrations: `dotnet ef database update`
- Check PostgreSQL service is running
- Verify database exists

### Video Upload Issues
- Check MinIO is running at localhost:9000
- Verify storage configuration
- Check file size limits

## Development Tips

### Hot Reload
- Backend: Changes auto-recompile with `dotnet watch`
- Frontend: Next.js hot reloads automatically

### Debugging
- Backend: Use Visual Studio or VS Code with C# extension
- Frontend: Use Chrome DevTools or VS Code

### Logs
- Backend: Check `logs/streamvault-.txt` in backend folder
- Frontend: Check browser console and terminal output

## Production Considerations

Before deploying to production:
1. Change all default passwords and keys
2. Configure proper SSL certificates
3. Set up production database
4. Configure CDN and storage
5. Set up monitoring and logging
6. Review security settings

## Support

If you encounter issues:
1. Check the logs in both frontend and backend
2. Verify all services are running
3. Check network connectivity
4. Review environment variables

## Next Steps

Once everything is running locally:
1. Explore all features using the web interface
2. Test API endpoints with Swagger
3. Try uploading and processing videos
4. Test live streaming
5. Explore admin panel
6. Test payment integration (with Stripe test keys)

Enjoy exploring StreamVault! 🚀

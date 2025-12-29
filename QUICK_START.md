# 🚀 StreamVault Quick Start Guide

## Prerequisites

Before launching the system, ensure you have installed:

1. **.NET 8.0 SDK** - https://dotnet.microsoft.com/download/dotnet/8.0
2. **Node.js 18+** - https://nodejs.org/
3. **PostgreSQL 15** - https://www.postgresql.org/download/windows/
4. **Redis** - https://redis.io/download

## Step 1: Database Setup

Open PostgreSQL (pgAdmin or psql) and run:

```sql
CREATE DATABASE streamvault;
CREATE USER streamvault WITH PASSWORD 'streamvault123';
GRANT ALL PRIVILEGES ON DATABASE streamvault TO streamvault;
```

## Step 2: Environment Configuration

### Backend Configuration
Create `streamvault-backend/src/StreamVault.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=streamvault;Username=streamvault;Password=streamvault123"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "SecretKey": "StreamVaultSuperSecretKey1234567890ABCDEF",
    "Issuer": "StreamVault",
    "Audience": "StreamVault",
    "ExpiryMinutes": 60,
    "RefreshExpiryDays": 7
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "MinIO": {
    "EndPoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "Bucket": "streamvault"
  },
  "Stripe": {
    "SecretKey": "sk_test_your_stripe_key_here",
    "PublishableKey": "pk_test_your_stripe_key_here",
    "WebhookSecret": "whsec_your_webhook_secret"
  },
  "SendGrid": {
    "ApiKey": "SG.your_sendgrid_key_here",
    "FromEmail": "noreply@streamvault.app",
    "FromName": "StreamVault"
  }
}
```

### Frontend Configuration
Create `streamvault-admin-dashboard/.env.local`:

```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
NEXT_PUBLIC_WS_URL=ws://localhost:5000/ws
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_your_stripe_key_here
NEXT_PUBLIC_APP_URL=http://localhost:3000
```

## Step 3: Launch Services

1. **Start Redis**:
   ```powershell
   redis-server
   ```

2. **Start Backend API** (in a new terminal):
   ```powershell
   cd streamvault-backend\src\StreamVault.API
   dotnet run
   ```

3. **Start Frontend** (in a new terminal):
   ```powershell
   cd streamvault-admin-dashboard
   npm run dev
   ```

## Step 4: Access the System

### 🔑 Default Users

| Role | Email | Password | Access URL |
|------|-------|----------|------------|
| Super Admin | superadmin@streamvault.app | SuperAdmin123! | http://localhost:3000/admin |
| Tenant Admin | admin@tenant1.com | TenantAdmin123! | http://localhost:3000 |
| Regular User | user@tenant1.com | User123! | http://localhost:3000 |

### 🌐 Important URLs

- **Super Admin Panel**: http://localhost:3000/admin
- **Tenant Dashboard**: http://localhost:3000
- **API Documentation**: http://localhost:5000/swagger
- **API Health Check**: http://localhost:5000/api/health

## Step 5: Configure Bunny.net

1. Login as **Super Admin**
2. Navigate to **Settings > Integrations**
3. Add your Bunny.net credentials:
   - Stream API Key
   - Library ID
   - Pull Zone ID
4. Save configuration

## Step 6: Test System Functions

### Video Upload Test
1. Login as Tenant Admin
2. Go to **Videos > Upload**
3. Upload a test video
4. Verify encoding completes

### User Management Test
1. Login as Tenant Admin
2. Go to **Users > Manage Users**
3. Create a new user
4. Test role assignments

### Analytics Test
1. View dashboard statistics
2. Check video analytics
3. Verify usage tracking

### Billing Test
1. Navigate to **Billing > Plans**
2. View subscription details
3. Check usage metrics

### Support System Test
1. Create a support ticket
2. Test knowledge base search
3. Verify ticket responses

## Troubleshooting

### Backend Issues
- Ensure PostgreSQL is running on port 5432
- Verify Redis is running on port 6379
- Check database connection string
- Run migrations: `dotnet ef database update`

### Frontend Issues
- Clear browser cache
- Check console for errors
- Verify API URL in .env.local
- Ensure backend is running

### Database Issues
- Verify database was created
- Check user permissions
- Confirm connection string matches

### Port Conflicts
- Backend uses port 5000
- Frontend uses port 3000
- PostgreSQL uses port 5432
- Redis uses port 6379

## Development Tips

1. **Hot Reload**: Both frontend and backend support hot reload
2. **Debugging**: Use browser dev tools for frontend, Visual Studio for backend
3. **Logs**: Check backend console for API logs
4. **Database**: Use pgAdmin to inspect data

## Production Considerations

1. Change all default passwords
2. Use HTTPS in production
3. Configure proper CORS origins
4. Set up proper backup strategies
5. Configure monitoring and logging

## Need Help?

- Check the API documentation at `/swagger`
- Review the database schema in the documentation
- Check browser console for JavaScript errors
- Review backend logs for API issues

Happy testing! 🎉

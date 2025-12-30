# StreamVault - REAL SYSTEM IMPLEMENTATION PLAN

## Overview
StreamVault is a multi-tenant video streaming platform with the following core features:
- Video upload, processing, and streaming via Bunny CDN
- Multi-tenancy with tenant isolation
- User authentication and authorization
- Video management (upload, edit, delete, analytics)
- Subscription/billing management via Stripe
- Live streaming capabilities
- AI-powered features (auto-tagging, transcripts, captions)

## Architecture
```
Frontend: Next.js 16 with TypeScript
Backend: .NET 8 Web API
Database: PostgreSQL 15
Cache: Redis (Memurai on Windows)
Message Queue: RabbitMQ
File Storage: Bunny CDN
Payments: Stripe
Email: SendGrid
```

## Phase 1: Core Backend Setup (Day 1-2)

### 1.1 Clean Backend Project
```bash
# Create new clean solution
dotnet new sln -n StreamVault
dotnet new webapi -n StreamVault.Api
dotnet new classlib -n StreamVault.Application
dotnet new classlib -n StreamVault.Domain
dotnet new classlib -n StreamVault.Infrastructure
dotnet new classlib -n StreamVault.Shared

# Add to solution
dotnet sln add StreamVault.Api/StreamVault.Api.csproj
dotnet sln add StreamVault.Application/StreamVault.Application.csproj
dotnet sln add StreamVault.Domain/StreamVault.Domain.csproj
dotnet sln add StreamVault.Infrastructure/StreamVault.Infrastructure.csproj
dotnet sln add StreamVault.Shared/StreamVault.Shared.csproj
```

### 1.2 Core NuGet Packages
```xml
<!-- StreamVault.Api -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" />
<PackageReference Include="Swashbuckle.AspNetCore" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
<PackageReference Include="StackExchange.Redis" />
<PackageReference Include="RabbitMQ.Client" />
<PackageReference Include="Stripe.net" />
<PackageReference Include="SendGrid" />
<PackageReference Include="AWSSDK.S3" />

<!-- StreamVault.Infrastructure -->
<PackageReference Include="Microsoft.EntityFrameworkCore" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
<PackageReference Include="StackExchange.Redis" />
<PackageReference Include="RabbitMQ.Client" />
<PackageReference Include="Stripe.net" />
<PackageReference Include="SendGrid" />
```

### 1.3 Database Schema
```sql
-- Tenants
CREATE TABLE tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(50) UNIQUE NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_active BOOLEAN DEFAULT true
);

-- Users
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES tenants(id),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    avatar_url VARCHAR(500),
    role VARCHAR(20) NOT NULL, -- SuperAdmin, Admin, User
    is_email_verified BOOLEAN DEFAULT false,
    two_factor_enabled BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Videos
CREATE TABLE videos (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES tenants(id),
    user_id UUID REFERENCES users(id),
    title VARCHAR(255) NOT NULL,
    description TEXT,
    thumbnail_url VARCHAR(500),
    video_url VARCHAR(500),
    duration INTEGER, -- seconds
    file_size BIGINT, -- bytes
    status VARCHAR(20) DEFAULT 'Processing', -- Uploading, Processing, Ready, Failed
    is_public BOOLEAN DEFAULT false,
    view_count INTEGER DEFAULT 0,
    tags TEXT[], -- PostgreSQL array
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Video Processing Jobs
CREATE TABLE video_processing_jobs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    video_id UUID REFERENCES videos(id),
    job_type VARCHAR(50), -- Thumbnail, Transcoding, Caption, AI_Tags
    status VARCHAR(20) DEFAULT 'Pending',
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    error_message TEXT,
    metadata JSONB
);

-- Subscriptions
CREATE TABLE subscriptions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES tenants(id),
    plan_type VARCHAR(50), -- Free, Pro, Enterprise
    stripe_subscription_id VARCHAR(100),
    status VARCHAR(20), -- Active, Canceled, Past_Due
    current_period_start TIMESTAMP,
    current_period_end TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW()
);
```

## Phase 2: Authentication & Authorization (Day 2-3)

### 2.1 JWT Authentication Setup
```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Multi-tenancy middleware
app.UseMiddleware<TenantResolutionMiddleware>();
```

### 2.2 Auth Controllers
```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
}
```

## Phase 3: Video Management (Day 3-4)

### 3.1 Bunny CDN Integration
```csharp
public class BunnyCDNService
{
    public async Task<string> UploadVideoAsync(Stream videoStream, string fileName)
    {
        // Upload to Bunny CDN storage
        // Return video URL
    }
    
    public async Task<string> UploadThumbnailAsync(Stream thumbnailStream, string fileName)
    {
        // Upload thumbnail to Bunny CDN
        // Return thumbnail URL
    }
    
    public async Task<string> GetPullZoneUrlAsync(string videoId)
    {
        // Generate streaming URL from pull zone
    }
}
```

### 3.2 Video Processing Pipeline
```csharp
public class VideoProcessingService
{
    [Queue("video-processing")]
    public async Task ProcessVideoAsync(Guid videoId)
    {
        // 1. Generate thumbnail
        // 2. Transcode to multiple resolutions
        // 3. Extract audio for transcription
        // 4. Generate captions
        // 5. AI tagging
        // 6. Update video status
    }
}
```

### 3.3 Video Controllers
```csharp
[ApiController]
[Route("api/videos")]
[Authorize]
public class VideosController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    
    [HttpGet]
    public async Task<IActionResult> GetVideos([FromQuery] VideoFilter filter)
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVideo(Guid id)
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVideo(Guid id, UpdateVideoRequest request)
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVideo(Guid id)
    
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishVideo(Guid id)
}
```

## Phase 4: Frontend Development (Day 4-5)

### 4.1 Clean Frontend Setup
```bash
npx create-next-app@latest streamvault-frontend --typescript --tailwind --app
cd streamvault-frontend
npm install @radix-ui/react-* lucide-react zustand axios
```

### 4.2 Key Frontend Components
- Video upload with progress bar
- Video player with custom controls
- Dashboard with analytics
- Settings page for API keys
- User management (for admins)

### 4.3 State Management
```typescript
// stores/auth-store.ts
export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  token: null,
  login: async (email, password) => { /* API call */ },
  logout: () => { /* Clear state */ },
}));

// stores/video-store.ts
export const useVideoStore = create<VideoState>((set, get) => ({
  videos: [],
  uploadVideo: async (file) => { /* Upload with progress */ },
  fetchVideos: async () => { /* Get user videos */ },
}));
```

## Phase 5: Advanced Features (Day 6-7)

### 5.1 Live Streaming
```csharp
public class LiveStreamService
{
    public async Task<StreamKey> CreateStreamAsync(Guid userId)
    {
        // Create stream via Bunny Stream API
        // Return RTMP endpoint and stream key
    }
    
    public async Task StartRecordingAsync(Guid streamId)
    {
        // Start recording live stream
    }
}
```

### 5.2 AI Features
```csharp
public class AIService
{
    public async Task<string[]> GenerateTagsAsync(string title, string description)
    {
        // Use OpenAI API to generate relevant tags
    }
    
    public async Task<string> GenerateTranscriptAsync(string audioUrl)
    {
        // Use Whisper API for transcription
    }
    
    public async Task<Caption[]> GenerateCaptionsAsync(string transcript)
    {
        // Generate timed captions from transcript
    }
}
```

### 5.3 Analytics
```csharp
public class AnalyticsService
{
    public async Task<VideoAnalytics> GetVideoAnalyticsAsync(Guid videoId)
    {
        // Views, watch time, engagement metrics
    }
    
    public async Task<TenantAnalytics> GetTenantAnalyticsAsync(Guid tenantId)
    {
        // Storage usage, bandwidth, costs
    }
}
```

## Phase 6: Deployment & Production (Day 8-10)

### 6.1 Docker Setup
```dockerfile
# Dockerfile.backend
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "StreamVault.Api.dll"]

# Dockerfile.frontend
FROM node:18-alpine
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build
FROM node:18-alpine AS runner
WORKDIR /app
COPY --from=builder /app/next.config.js ./
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
EXPOSE 3000
CMD ["node", "server.js"]
```

### 6.2 Docker Compose
```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: streamvault
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    volumes:
      - postgres_data:/var/lib/postgresql/data
  
  redis:
    image: redis:7-alpine
    
  rabbitmq:
    image: rabbitmq:3-management
    
  backend:
    build: ./backend
    depends_on:
      - postgres
      - redis
      - rabbitmq
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=streamvault;Username=postgres;Password=password
      - Redis__ConnectionString=redis:6379
      - RabbitMQ__HostName=rabbitmq
      
  frontend:
    build: ./frontend
    depends_on:
      - backend
```

## Implementation Priority

1. **Day 1**: Set up clean backend project, database, basic entities
2. **Day 2**: Implement authentication with JWT
3. **Day 3**: Video upload and Bunny CDN integration
4. **Day 4**: Video management endpoints
5. **Day 5**: Frontend setup and auth flow
6. **Day 6**: Video upload UI and dashboard
7. **Day 7**: Analytics and settings
8. **Day 8**: Live streaming basics
9. **Day 9**: AI features (tags, transcripts)
10. **Day 10**: Docker deployment

## Required Services & API Keys

1. **Bunny CDN** (Storage + Stream)
   - Storage API Key
   - Library ID
   - Pull Zone ID
   - Stream API Key

2. **Stripe**
   - Publishable Key
   - Secret Key
   - Webhook Secret

3. **SendGrid**
   - API Key

4. **OpenAI** (for AI features)
   - API Key

## Next Steps

1. **Start with Phase 1** - Create a clean backend project
2. **Set up PostgreSQL** - Create the database schema
3. **Implement authentication** - JWT with multi-tenancy
4. **Build video upload** - Integrate with Bunny CDN
5. **Develop frontend** - Clean, modern UI with all features

This is a REAL implementation plan for a production-ready StreamVault system, not a mock.

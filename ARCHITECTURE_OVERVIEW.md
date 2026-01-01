# StreamVault Architecture Overview - Phase 1

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          CLIENT LAYER (Browser)                              │
│  ┌────────────────────────────────┐  ┌────────────────────────────────┐    │
│  │ Login Page                     │  │ 2FA Verification Modal         │    │
│  │ - Email Input                  │  │ - 6-digit Code Input          │    │
│  │ - Password Input               │  │ - Backend Verification         │    │
│  │ - Workspace Selection          │  │ - Redirect on Success          │    │
│  │ - Form Validation (Zod)        │  └────────────────────────────────┘    │
│  └────────────────────────────────┘                                         │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ HTTP/HTTPS
                                    │
┌─────────────────────────────────────────────────────────────────────────────┐
│                   REVERSE PROXY LAYER (Nginx)                                │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │ Port 80/443                                                             │ │
│  │ - SSL/TLS Termination (Let's Encrypt Ready)                            │ │
│  │ - Static File Serving                                                  │ │
│  │ - Request Routing                                                      │ │
│  │ - Gzip Compression                                                     │ │
│  │ - Rate Limiting Hooks                                                  │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
              │                                        │
              │                                        │
        ┌─────┴─────┐                          ┌──────┴───────┐
        │            │                         │              │
┌───────▼──────┐  ┌──────────────────┐  ┌──────▼──────┐  ┌───▼──────────┐
│ Frontend     │  │                  │  │ Backend API │  │ API Docs     │
│ :3000        │  │                  │  │ :5000       │  │ :5000/swagger│
└──────────────┘  └──────────────────┘  └─────────────┘  └──────────────┘
```

## Application Layer Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      BACKEND (ASP.NET Core 8)                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                         API Layer (Controllers)                       │  │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐   │  │
│  │  │ AuthController   │  │ VideosController │  │ OtherControllers │   │  │
│  │  │                  │  │                  │  │                  │   │  │
│  │  │ • login          │  │ • GetVideos      │  │ • Collections    │   │  │
│  │  │ • register       │  │ • CreateVideo    │  │ • Analytics      │   │  │
│  │  │ • refresh        │  │ • DeleteVideo    │  │ • Users          │   │  │
│  │  │ • verify-2fa     │  │ • UpdateVideo    │  │ • Billing        │   │  │
│  │  │ • logout         │  │                  │  │                  │   │  │
│  │  │ • reset-password │  │                  │  │                  │   │  │
│  │  └──────────────────┘  └──────────────────┘  └──────────────────┘   │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                    │                                        │
│  ┌──────────────────────────────────┴──────────────────────────────────┐  │
│  │                    Middleware Pipeline                              │  │
│  │  ┌─────────────────────────────────────────────────────────────┐   │  │
│  │  │ 1. TenantResolutionMiddleware                              │   │  │
│  │  │    - Extract tenant from subdomain/header                 │   │  │
│  │  │    - Validate tenant exists                                │   │  │
│  │  │    - Set tenant context in HttpContext.Items              │   │  │
│  │  └─────────────────────────────────────────────────────────────┘   │  │
│  │  ┌─────────────────────────────────────────────────────────────┐   │  │
│  │  │ 2. Authentication Middleware (JWT Bearer)                  │   │  │
│  │  │    - Extract JWT from Authorization header                │   │  │
│  │  │    - Validate signature                                    │   │  │
│  │  │    - Validate claims (tenant_id matches)                  │   │  │
│  │  │    - Set User.Principal with claims                        │   │  │
│  │  └─────────────────────────────────────────────────────────────┘   │  │
│  │  ┌─────────────────────────────────────────────────────────────┐   │  │
│  │  │ 3. PermissionAuthorizationMiddleware                       │   │  │
│  │  │    - Check [RequirePermission] attributes                 │   │  │
│  │  │    - Load user permissions from database                  │   │  │
│  │  │    - Match wildcards (videos.* = videos.view, etc)        │   │  │
│  │  │    - Allow/Deny endpoint access                            │   │  │
│  │  └─────────────────────────────────────────────────────────────┘   │  │
│  │  ┌─────────────────────────────────────────────────────────────┐   │  │
│  │  │ 4. CORS & Security Headers                                 │   │  │
│  │  │    - Add security headers                                  │   │  │
│  │  │    - CORS validation                                       │   │  │
│  │  └─────────────────────────────────────────────────────────────┘   │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                    │                                        │
│  ┌──────────────────────────────────┴──────────────────────────────────┐  │
│  │                    Service Layer (Application)                      │  │
│  │  ┌────────────────────────────────────────────────────────────┐    │  │
│  │  │ AuthService                                               │    │  │
│  │  │ - Login (email/password validation)                       │    │  │
│  │  │ - Register (new tenant + user)                            │    │  │
│  │  │ - GenerateTokens (JWT + refresh)                          │    │  │
│  │  │ - RefreshToken (rotate tokens)                            │    │  │
│  │  │ - VerifyEmail (token validation)                          │    │  │
│  │  └────────────────────────────────────────────────────────────┘    │  │
│  │  ┌────────────────────────────────────────────────────────────┐    │  │
│  │  │ PermissionSeeder                                          │    │  │
│  │  │ - SeedPermissionsAsync() - Insert 40+ core permissions   │    │  │
│  │  │ - SeedDefaultRolesAsync() - Create default roles         │    │  │
│  │  │ - AssignPermissionsToRoles() - Link permissions          │    │  │
│  │  └────────────────────────────────────────────────────────────┘    │  │
│  │  ┌────────────────────────────────────────────────────────────┐    │  │
│  │  │ Future Services (Phase 2+)                                │    │  │
│  │  │ - VideoService, BunnyNetService                           │    │  │
│  │  │ - BillingService, AnalyticsService                        │    │  │
│  │  │ - SupportService, NotificationService                     │    │  │
│  │  └────────────────────────────────────────────────────────────┘    │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                    │                                        │
│  ┌──────────────────────────────────┴──────────────────────────────────┐  │
│  │                    Data Access Layer (Entity Framework)             │  │
│  │  ┌────────────────────────────────────────────────────────────┐    │  │
│  │  │ StreamVaultDbContext                                      │    │  │
│  │  │ - DbSet<User>, DbSet<Role>, DbSet<Permission>            │    │  │
│  │  │ - DbSet<Tenant>, DbSet<Video>, DbSet<Invoice>            │    │  │
│  │  │ - 40+ other DbSets (full entity coverage)                 │    │  │
│  │  │ - OnModelCreating() - Relationships & indexes             │    │  │
│  │  │ - Interceptors - Tenant filtering on all queries          │    │  │
│  │  └────────────────────────────────────────────────────────────┘    │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │
┌───────────────────────────────────┼───────────────────────────────────────┐
│                    DATABASE LAYER                                          │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                   PostgreSQL 15                                  │   │
│  │                   Port: 5432                                     │   │
│  │                   Database: streamvault                          │   │
│  │                                                                  │   │
│  │  ┌─────────────────────────────────────────────────────────┐   │   │
│  │  │ SYSTEM TABLES (Master Database)                         │   │   │
│  │  │ - Tenants (multi-tenant account info)                  │   │   │
│  │  │ - TenantBranding (customization per tenant)            │   │   │
│  │  │ - SubscriptionPlans (plan definitions)                 │   │   │
│  │  │ - TenantSubscriptions (subscription status)            │   │   │
│  │  │ - Permissions (core 40+ permissions)                   │   │   │
│  │  │ - SystemUsers (super admins)                           │   │   │
│  │  │ - Invoices, PlatformSettings, etc.                     │   │   │
│  │  └─────────────────────────────────────────────────────────┘   │   │
│  │                                                                  │   │
│  │  ┌─────────────────────────────────────────────────────────┐   │   │
│  │  │ TENANT TABLES (Row-Level Isolation)                     │   │   │
│  │  │ - Users (has TenantId - filters all queries)            │   │   │
│  │  │ - Roles (per-tenant custom roles)                       │   │   │
│  │  │ - UserRoles (junction for N:M)                          │   │   │
│  │  │ - RolePermissions (role-permission mapping)             │   │   │
│  │  │ - EmailVerificationTokens (2FA, verify tokens)          │   │   │
│  │  │ - TwoFactorAuthCodes (6-digit codes)                    │   │   │
│  │  │ - UserSessions (refresh tokens)                         │   │   │
│  │  │ - Videos, Collections, Captions (future phases)         │   │   │
│  │  └─────────────────────────────────────────────────────────┘   │   │
│  │                                                                  │   │
│  │  Indexes:                                                       │   │
│  │  - PK on all Id columns (UUID)                                │   │
│  │  - UNIQUE on email per tenant                                │   │
│  │  - FK indexes for joins                                       │   │
│  │  - Composite indexes for common queries                       │   │
│  │  - TenantId on all tenant tables for filtering                │   │
│  │                                                                  │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                            │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                      Redis 7 (Cache)                            │   │
│  │                      Port: 6379                                 │   │
│  │                                                                  │   │
│  │  - Session cache (store refresh tokens)                         │   │
│  │  - Permission cache (avoid DB lookup per request)               │   │
│  │  - API response cache (aggregate queries)                       │   │
│  │  - Rate limiting counter                                        │   │
│  │                                                                  │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

## Authentication Flow

```
User Input (Login Page)
│
│ Email + Password + Workspace
│
▼
POST /api/v1/auth/login
│
├─► TenantResolutionMiddleware
│   └─► Load tenant by slug
│
├─► Verify email exists in tenant
│
├─► Verify password (BCrypt)
│
├─► Check if 2FA enabled
│   ├─► YES: Generate 6-digit code, send email, return "requires2FA"
│   │       User enters code → POST /api/v1/auth/login/verify-2fa
│   │
│   └─► NO: Continue to token generation
│
├─► Load user roles and permissions from database
│
├─► Generate JWT access token
│   {
│     "sub": "user-id",
│     "tenant_id": "tenant-id",
│     "email": "user@example.com",
│     "roles": ["Admin"],
│     "permissions": ["videos.*", "users.manage"],
│     "exp": 1234567890
│   }
│
├─► Generate refresh token (random 32 bytes)
│
├─► Store refresh token in database
│
├─► Store tokens in localStorage
│
▼
User logged in, redirected to dashboard
```

## Permission Checking Flow

```
Request to protected endpoint
│
│ Authorization: Bearer {access_token}
│
▼
Parse & Validate JWT Token
│
├─► Check signature
├─► Check expiration
├─► Verify tenant_id in claims
│
▼
Load Endpoint's Required Permission (via [RequirePermission] attribute)
│
▼
PermissionAuthorizationHandler
│
├─► Get user from claims
├─► Load user roles from database
├─► Load all permissions for each role
│
▼
Permission Matching
│
├─► Check exact match (videos.view == videos.view) → ALLOW
├─► Check wildcard (videos.* matches videos.view) → ALLOW
├─► No match → DENY (403 Forbidden)
│
▼
Execute endpoint or return 403
```

## Data Isolation Example

```
User A from Tenant X logs in
│
└─► JWT contains: tenant_id = "tenant-x-id"
    │
    └─► All queries filtered by:
        WHERE TenantId = "tenant-x-id"
        │
        └─► Can only see their own users, videos, roles, etc.

User B from Tenant Y logs in
│
└─► JWT contains: tenant_id = "tenant-y-id"
    │
    └─► All queries filtered by:
        WHERE TenantId = "tenant-y-id"
        │
        └─► Cannot see anything from Tenant X

Database level isolation:
┌─────────────────────────┐
│ Users                   │
├─────────────────────────┤
│ Id | Email      | Tenant│
├────┼─────────────┼───────┤
│ 1  | user@a.com | X     │ ◄── User A (tenant X)
│ 2  | user@b.com | Y     │ ◄── User B (tenant Y)
│ 3  | admin@a.com| X     │ ◄── Admin A (tenant X)
└─────────────────────────┘

Query for User A:
SELECT * FROM Users WHERE TenantId = X
→ Only sees rows 1 and 3

Query for User B:
SELECT * FROM Users WHERE TenantId = Y
→ Only sees row 2
```

## Security Layers

```
Layer 1: Transport
├─► HTTPS/TLS via Nginx
├─► Certificate validation
└─► Forward secrecy

Layer 2: Authentication
├─► Email verification (token-based)
├─► Password hashing (BCrypt)
├─► 2FA (6-digit email code)
├─► Refresh token rotation
└─► JWT signing and validation

Layer 3: Authorization
├─► Permission-based access control
├─► Role-based enforcement
├─► Multi-tenant row-level isolation
└─► Attribute-based endpoint protection

Layer 4: Data Protection
├─► Row-level filtering (TenantId)
├─► Encrypted sensitive fields (API keys)
├─► Column-level security (SQL)
└─► Audit logging for sensitive operations
```

## API Request/Response Flow

```
1. FRONTEND REQUEST
───────────────────────────────────────────────────────────────────
POST /api/v1/auth/login
Content-Type: application/json
Authorization: Bearer {token}
X-Tenant-Slug: my-workspace

{
  "email": "user@example.com",
  "password": "secure123",
  "tenantSlug": "my-workspace"
}


2. MIDDLEWARE PROCESSING
───────────────────────────────────────────────────────────────────
1. TenantResolutionMiddleware
   └─► Load tenant by slug "my-workspace"
   
2. Authentication Middleware
   └─► Validate token (if provided)
   
3. PermissionAuthorizationMiddleware
   └─► Check [RequirePermission] attribute
   
4. CORS Middleware
   └─► Validate origin


3. CONTROLLER ACTION
───────────────────────────────────────────────────────────────────
AuthController.Login()
├─► Validate input
├─► Query database for user
├─► Verify password
├─► Generate tokens
└─► Return response


4. BACKEND RESPONSE
───────────────────────────────────────────────────────────────────
200 OK
Content-Type: application/json
X-Tenant-Id: tenant-uuid
X-Tenant-Name: My Workspace

{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_long_string_here",
  "user": {
    "id": "user-uuid",
    "email": "user@example.com",
    "firstName": "John",
    "tenantId": "tenant-uuid",
    "roles": ["Admin"],
    "permissions": ["videos.*", "users.manage"]
  }
}


5. FRONTEND HANDLING
───────────────────────────────────────────────────────────────────
1. Store tokens in localStorage
2. Update auth store (Zustand)
3. Set Authorization header for future requests
4. Redirect to dashboard
```

---

## Component Interactions

```
┌─────────────────────────────────────────────────────────┐
│                   Frontend (React/Next.js)              │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  LoginPage ──┐                                          │
│             │ React Hook Form + Zod validation         │
│  useAuthStore ─┼─► API Client ──┐                      │
│             │                   │                      │
│  apiClient.post() ──────────────┼──────────────────► API
│             │                   │
│  useAuthStore.login() ◄─────────┘
│                                                         │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                    Backend (ASP.NET Core)               │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Request ─► Middleware Pipeline ─┐                    │
│             (Tenant, Auth, Perms) │                    │
│                                   │                    │
│                                   ▼                    │
│                            AuthController              │
│                                   │                    │
│                                   ▼                    │
│                            AuthService                 │
│                                   │                    │
│                                   ▼                    │
│                      StreamVaultDbContext              │
│                                   │                    │
│                                   ▼                    │
│                            PostgreSQL DB               │
│                                   │                    │
│                    ┌──────────────┼──────────────┐     │
│                    ▼              ▼              ▼     │
│                  Users          Tenants       Tokens   │
│                    │              │              │     │
│                    └──────────────┼──────────────┘     │
│                                   ▼                    │
│                      StreamVaultDbContext              │
│                                   │                    │
│                                   ▼                    │
│                            AuthService                 │
│                                   │                    │
│                                   ▼                    │
│                            AuthController              │
│                                   │                    │
│                                   ▼                    │
│                      { token, user } Response          │
│                                                        │
└─────────────────────────────────────────────────────────┘
```

---

**Phase 1 Architecture is production-ready and scalable for Phase 2+ implementation.**

# Comprehensive Development Plan

## SaaS Multi-Tenant DRM Video Hosting Platform

---

# EXECUTIVE SUMMARY

**Project Name:** StreamVault \- Multi-Tenant Video Hosting SaaS Platform

**Core Concept:** A white-label video hosting platform that operates as a reseller of Bunny.net Stream services, where tenants have no visibility into the underlying infrastructure provider. The platform offers subscription-based access with usage-based overage billing, complete analytics (with configurable usage multipliers), and comprehensive video management capabilities.

**Key Differentiators:**

- Complete white-label reseller model (Bunny.net completely abstracted)  
- Configurable usage multiplier for business margin management  
- Fully customizable RBAC per tenant  
- Advanced support system with knowledge base  
- Self-hosted on private VPS (cost-effective, full control)

---

# PART 1: SYSTEM ARCHITECTURE

## 1.1 High-Level Architecture Diagram

┌─────────────────────────────────────────────────────────────────────────────────┐

│                              INTERNET / CLIENTS                                  │

│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │

│  │ Tenant Admin │  │ Tenant Users │  │ Video Viewers│  │ Super Admin  │         │

│  │   (Web App)  │  │  (Web App)   │  │   (Player)   │  │   (Web App)  │         │

│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘         │

└─────────┼─────────────────┼─────────────────┼─────────────────┼─────────────────┘

          │                 │                 │                 │

          ▼                 ▼                 ▼                 ▼

┌─────────────────────────────────────────────────────────────────────────────────┐

│                           NGINX REVERSE PROXY (SSL)                             │

│                    ┌────────────────────────────────────┐                       │

│                    │  Let's Encrypt SSL Certificates    │                       │

│                    │  Rate Limiting, Gzip Compression   │                       │

│                    │  Static File Serving               │                       │

│                    └────────────────────────────────────┘                       │

└─────────────────────────────────────────────────────────────────────────────────┘

          │

          ▼

┌─────────────────────────────────────────────────────────────────────────────────┐

│                              APPLICATION LAYER                                   │

│                                                                                  │

│  ┌─────────────────────────────┐    ┌─────────────────────────────┐            │

│  │       FRONTEND (Next.js)    │    │      BACKEND (ASP.NET Core) │            │

│  │  ┌───────────────────────┐  │    │  ┌───────────────────────┐  │            │

│  │  │ Super Admin Dashboard │  │    │  │    API Gateway        │  │            │

│  │  ├───────────────────────┤  │    │  │   /api/v1/\*           │  │            │

│  │  │ Tenant Admin Dashboard│  │◄──►│  ├───────────────────────┤  │            │

│  │  ├───────────────────────┤  │    │  │  Authentication Svc   │  │            │

│  │  │ Video Player (Embed)  │  │    │  │  (JWT \+ Refresh)      │  │            │

│  │  ├───────────────────────┤  │    │  ├───────────────────────┤  │            │

│  │  │ Public Pages          │  │    │  │  Multi-Tenancy Svc    │  │            │

│  │  └───────────────────────┘  │    │  │  (Tenant Resolution)  │  │            │

│  │  Port: 3000                 │    │  ├───────────────────────┤  │            │

│  └─────────────────────────────┘    │  │  Video Proxy Service  │  │            │

│                                      │  │  (Bunny.net Abstract) │  │            │

│                                      │  ├───────────────────────┤  │            │

│                                      │  │  Billing Service      │  │            │

│                                      │  │  (Stripe \+ Manual)    │  │            │

│                                      │  ├───────────────────────┤  │            │

│                                      │  │  Analytics Service    │  │            │

│                                      │  │  (Usage Multiplier)   │  │            │

│                                      │  ├───────────────────────┤  │            │

│                                      │  │  Support Ticket Svc   │  │            │

│                                      │  ├───────────────────────┤  │            │

│                                      │  │  Notification Service │  │            │

│                                      │  │  (Email via SMTP)     │  │            │

│                                      │  ├───────────────────────┤  │            │

│                                      │  │  Background Jobs      │  │            │

│                                      │  │  (Hangfire)           │  │            │

│                                      │  └───────────────────────┘  │            │

│                                      │  Port: 5000                 │            │

│                                      └─────────────────────────────┘            │

└─────────────────────────────────────────────────────────────────────────────────┘

          │                                    │

          ▼                                    ▼

┌─────────────────────────────────────────────────────────────────────────────────┐

│                               DATA LAYER                                         │

│                                                                                  │

│  ┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐     │

│  │    PostgreSQL       │  │      Redis          │  │    File Storage     │     │

│  │    (Primary DB)     │  │   (Cache \+ Queue)   │  │   (Local/MinIO)     │     │

│  │  ┌───────────────┐  │  │  ┌───────────────┐  │  │  ┌───────────────┐  │     │

│  │  │ Master DB     │  │  │  │ Session Cache │  │  │  │ Thumbnails    │  │     │

│  │  │ (System Data) │  │  │  │ API Response  │  │  │  │ Exports       │  │     │

│  │  ├───────────────┤  │  │  │ Cache         │  │  │  │ Temp Files    │  │     │

│  │  │ Tenant DBs    │  │  │  │ Rate Limiting │  │  │  │ Logs          │  │     │

│  │  │ (Enterprise)  │  │  │  │ Job Queue     │  │  │  └───────────────┘  │     │

│  │  └───────────────┘  │  │  └───────────────┘  │  └─────────────────────┘     │

│  │  Port: 5432         │  │  Port: 6379         │                               │

│  └─────────────────────┘  └─────────────────────┘                               │

└─────────────────────────────────────────────────────────────────────────────────┘

          │

          ▼

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         EXTERNAL SERVICES (Hidden from Tenants)                  │

│                                                                                  │

│  ┌──────────────────────────────────────────────────────────────────────────┐   │

│  │                         BUNNY.NET STREAM API                              │   │

│  │                    (Proxied through Video Proxy Service)                  │   │

│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐          │   │

│  │  │ Video Library   │  │ Video Upload    │  │ Video Playback  │          │   │

│  │  │ Management      │  │ & Encoding      │  │ & Streaming     │          │   │

│  │  ├─────────────────┤  ├─────────────────┤  ├─────────────────┤          │   │

│  │  │ Collections     │  │ Direct Upload   │  │ HLS/DASH        │          │   │

│  │  │ Thumbnails      │  │ TUS Resumable   │  │ Token Auth      │          │   │

│  │  │ Captions        │  │ Pull from URL   │  │ Geo-blocking    │          │   │

│  │  │ Chapters        │  │ Webhooks        │  │ Watermarking    │          │   │

│  │  └─────────────────┘  └─────────────────┘  └─────────────────┘          │   │

│  │                                                                          │   │

│  │  API Base: https://video.bunnycdn.com (NEVER exposed to tenants)        │   │

│  └──────────────────────────────────────────────────────────────────────────┘   │

│                                                                                  │

│  ┌─────────────────────┐  ┌─────────────────────┐                              │

│  │   Stripe API        │  │   SMTP Server       │                              │

│  │  (Subscriptions)    │  │  (Notifications)    │                              │

│  │  \- Checkout         │  │  \- Mailjet/         │                              │

│  │  \- Webhooks         │  │    Sendgrid/        │                              │

│  │  \- Customer Portal  │  │    Self-hosted      │                              │

│  └─────────────────────┘  └─────────────────────┘                              │

└─────────────────────────────────────────────────────────────────────────────────┘

## 1.2 Multi-Tenancy Architecture

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         MULTI-TENANCY DATA FLOW                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

                              ┌─────────────────────┐

                              │   Incoming Request  │

                              │   (HTTP Header or   │

                              │    Subdomain)       │

                              └──────────┬──────────┘

                                         │

                                         ▼

                              ┌─────────────────────┐

                              │  Tenant Resolution  │

                              │     Middleware      │

                              └──────────┬──────────┘

                                         │

                    ┌────────────────────┼────────────────────┐

                    │                    │                    │

                    ▼                    ▼                    ▼

        ┌───────────────────┐ ┌───────────────────┐ ┌───────────────────┐

        │  Small Tenant     │ │  Medium Tenant    │ │  Enterprise       │

        │  (Shared Schema)  │ │  (Shared Schema)  │ │  (Dedicated DB)   │

        ├───────────────────┤ ├───────────────────┤ ├───────────────────┤

        │ TenantId: abc123  │ │ TenantId: def456  │ │ Database: ent\_xyz │

        │ Uses: Master DB   │ │ Uses: Master DB   │ │ Connection Pool   │

        │ Row-Level Filter  │ │ Row-Level Filter  │ │ Full Isolation    │

        └───────────────────┘ └───────────────────┘ └───────────────────┘

                    │                    │                    │

                    ▼                    ▼                    ▼

┌─────────────────────────────────────────────────────────────────────────────────┐

│                              MASTER DATABASE                                     │

│  ┌─────────────────────────────────────────────────────────────────────────────┐│

│  │ SYSTEM TABLES (No TenantId \- Platform Level)                                ││

│  │ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            ││

│  │ │  Tenants    │ │ SystemUsers │ │ Subscript-  │ │ Platform    │            ││

│  │ │             │ │ (SuperAdmin)│ │ ionPlans    │ │ Settings    │            ││

│  │ └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘            ││

│  └─────────────────────────────────────────────────────────────────────────────┘│

│  ┌─────────────────────────────────────────────────────────────────────────────┐│

│  │ TENANT TABLES (Has TenantId \- Shared Schema Multi-Tenancy)                  ││

│  │ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            ││

│  │ │ Users       │ │ Videos      │ │ Collections │ │ Analytics   │            ││

│  │ │ TenantId:FK │ │ TenantId:FK │ │ TenantId:FK │ │ TenantId:FK │            ││

│  │ └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘            ││

│  └─────────────────────────────────────────────────────────────────────────────┘│

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         ENTERPRISE TENANT DATABASE                               │

│  (Completely Separate PostgreSQL Schema or Database)                            │

│  ┌─────────────────────────────────────────────────────────────────────────────┐│

│  │ Same structure as Tenant Tables but in isolated database                    ││

│  │ Connection string stored in Tenants table in Master DB                      ││

│  └─────────────────────────────────────────────────────────────────────────────┘│

└─────────────────────────────────────────────────────────────────────────────────┘

## 1.3 Bunny.net Abstraction Layer (White-Label Reseller Architecture)

┌─────────────────────────────────────────────────────────────────────────────────┐

│                    BUNNY.NET ABSTRACTION LAYER (RESELLER MODEL)                  │

│                                                                                  │

│  CRITICAL: Tenants must NEVER see any reference to "Bunny.net" or "BunnyCDN"   │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│                              TENANT PERSPECTIVE                                  │

│                                                                                  │

│  What Tenant Sees:                                                              │

│  ┌────────────────────────────────────────────────────────────────────────────┐ │

│  │ • API Endpoints: https://api.yourplatform.com/v1/media/\*                   │ │

│  │ • Video URLs: https://stream.yourplatform.com/v/{video\_id}/playlist.m3u8   │ │

│  │ • Upload URLs: https://upload.yourplatform.com/video                       │ │

│  │ • Thumbnail URLs: https://assets.yourplatform.com/thumb/{id}.jpg           │ │

│  │ • Player: Custom branded player (no Bunny.net references)                  │ │

│  │ • Dashboard: Your platform branding only                                   │ │

│  │ • Documentation: Your API docs (rebranded from Bunny.net concepts)         │ │

│  │ • Error Messages: Custom error messages (no Bunny.net references)          │ │

│  │ • Analytics Labels: "Storage", "Bandwidth", "Views" (not Bunny terms)      │ │

│  └────────────────────────────────────────────────────────────────────────────┘ │

└─────────────────────────────────────────────────────────────────────────────────┘

                                         │

                                         ▼

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         VIDEO PROXY SERVICE (ABSTRACTION)                        │

│                                                                                  │

│  ┌─────────────────────────────────────────────────────────────────────────────┐│

│  │                         REQUEST TRANSFORMATION                               ││

│  │                                                                              ││

│  │  Incoming (Your API)              Outgoing (Bunny.net API)                  ││

│  │  ─────────────────────────────────────────────────────────────────          ││

│  │  POST /v1/media/upload     →     POST /library/{lib}/videos                 ││

│  │  GET /v1/media/{id}        →     GET /library/{lib}/videos/{bunny\_id}       ││

│  │  GET /v1/media/list        →     GET /library/{lib}/videos                  ││

│  │  DELETE /v1/media/{id}     →     DELETE /library/{lib}/videos/{bunny\_id}    ││

│  │  PUT /v1/media/{id}        →     POST /library/{lib}/videos/{bunny\_id}      ││

│  │  POST /v1/media/{id}/caption →   PUT /library/{lib}/videos/{id}/captions    ││

│  │  GET /v1/analytics/usage   →     GET /library/{lib}/statistics (+ multiply) ││

│  │                                                                              ││

│  └─────────────────────────────────────────────────────────────────────────────┘│

│                                                                                  │

│  ┌─────────────────────────────────────────────────────────────────────────────┐│

│  │                         RESPONSE TRANSFORMATION                              ││

│  │                                                                              ││

│  │  • Remove all Bunny.net references from responses                           ││

│  │  • Replace Bunny CDN URLs with your proxy URLs                              ││

│  │  • Transform field names to your terminology                                ││

│  │  • Add tenant-specific metadata                                             ││

│  │  • Apply usage multiplier to analytics data                                 ││

│  │                                                                              ││

│  │  Example Response Transformation:                                           ││

│  │  ─────────────────────────────────────────────────────────────────          ││

│  │  Bunny Response:                    Your Response:                          ││

│  │  {                                  {                                       ││

│  │    "guid": "abc-123",                "id": "vid\_abc123",                    ││

│  │    "libraryId": 12345,      →        "tenant\_id": "tenant\_xyz",             ││

│  │    "title": "My Video",              "title": "My Video",                   ││

│  │    "dateUploaded": "...",            "created\_at": "...",                   ││

│  │    "storageSize": 1073741824         "storage\_bytes": 1073741824,           ││

│  │  }                                    "storage\_display": "1 GB"             ││

│  │                                     }                                       ││

│  └─────────────────────────────────────────────────────────────────────────────┘│

│                                                                                  │

│  ┌─────────────────────────────────────────────────────────────────────────────┐│

│  │                         URL REWRITING / PROXY                                ││

│  │                                                                              ││

│  │  Video Playback Flow:                                                       ││

│  │  ─────────────────────────────────────────────────────────────────          ││

│  │                                                                              ││

│  │  1\. Tenant requests: https://stream.yourplatform.com/v/{your\_video\_id}      ││

│  │                                                                              ││

│  │  2\. Your system:                                                            ││

│  │     a. Validates token/authentication                                       ││

│  │     b. Looks up Bunny video GUID from your\_video\_id                         ││

│  │     c. Generates signed Bunny.net URL with token                            ││

│  │     d. Returns HLS playlist with rewritten segment URLs                     ││

│  │        OR redirects to CDN with signed URL (302 redirect)                   ││

│  │                                                                              ││

│  │  Option A: Full Proxy (Higher bandwidth cost, complete hiding)              ││

│  │     All video segments flow through your server                             ││

│  │                                                                              ││

│  │  Option B: Signed Redirect (Recommended \- Cost effective)                   ││

│  │     Generate time-limited signed URLs, redirect to CDN                      ││

│  │     URL still shows pull zone but is temporary and unguessable              ││

│  │                                                                              ││

│  │  Option C: Custom Hostname on Bunny.net                                     ││

│  │     Configure stream.yourplatform.com as custom hostname in Bunny.net       ││

│  │     DNS CNAME points to Bunny.net CDN                                        ││

│  │     Best of both worlds \- your domain, CDN performance                      ││

│  │                                                                              ││

│  └─────────────────────────────────────────────────────────────────────────────┘│

└─────────────────────────────────────────────────────────────────────────────────┘

                                         │

                                         ▼

┌─────────────────────────────────────────────────────────────────────────────────┐

│                              BUNNY.NET STREAM API                                │

│                         (Hidden \- Backend Only Access)                           │

│                                                                                  │

│  Your Bunny.net Account Configuration:                                          │

│  ┌────────────────────────────────────────────────────────────────────────────┐ │

│  │ • Multiple Video Libraries (one per tenant OR shared with folders)         │ │

│  │ • Pull Zone with Custom Hostname: stream.yourplatform.com                   │ │

│  │ • Token Authentication Enabled                                              │ │

│  │ • Geo-blocking configured per tenant needs                                  │ │

│  │ • Watermarking enabled (your platform logo or tenant logo)                  │ │

│  │ • Webhook URL: https://api.yourplatform.com/webhooks/encoding              │ │

│  └────────────────────────────────────────────────────────────────────────────┘ │

│                                                                                  │

│  API Keys Storage (ENCRYPTED in database):                                      │

│  ┌────────────────────────────────────────────────────────────────────────────┐ │

│  │ • Main API Key: For library management, video CRUD                         │ │

│  │ • Read-Only API Key: For fetching data only                                │ │

│  │ • Token Authentication Key: For signed URL generation                       │ │

│  │ • Per-Library API Keys: If using library-per-tenant model                  │ │

│  └────────────────────────────────────────────────────────────────────────────┘ │

└─────────────────────────────────────────────────────────────────────────────────┘

## 1.4 Authentication & Authorization Flow

┌─────────────────────────────────────────────────────────────────────────────────┐

│                    AUTHENTICATION & AUTHORIZATION FLOW                           │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│                              LOGIN FLOW                                          │

│                                                                                  │

│  ┌──────────┐         ┌──────────┐         ┌──────────┐         ┌──────────┐   │

│  │  User    │         │ Next.js  │         │ ASP.NET  │         │ Database │   │

│  │ Browser  │         │ Frontend │         │ Core API │         │          │   │

│  └────┬─────┘         └────┬─────┘         └────┬─────┘         └────┬─────┘   │

│       │                    │                    │                    │          │

│       │ 1\. Enter credentials                   │                    │          │

│       │────────────────────►                   │                    │          │

│       │                    │                    │                    │          │

│       │                    │ 2\. POST /api/auth/login                │          │

│       │                    │────────────────────►                   │          │

│       │                    │                    │                    │          │

│       │                    │                    │ 3\. Validate user   │          │

│       │                    │                    │────────────────────►          │

│       │                    │                    │                    │          │

│       │                    │                    │ 4\. Get user \+ roles│          │

│       │                    │                    │◄────────────────────          │

│       │                    │                    │                    │          │

│       │                    │ 5\. Return JWT \+ Refresh Token          │          │

│       │                    │◄────────────────────                   │          │

│       │                    │                    │                    │          │

│       │ 6\. Store tokens,   │                    │                    │          │

│       │    redirect to dashboard               │                    │          │

│       │◄────────────────────                   │                    │          │

│       │                    │                    │                    │          │

└───────┴────────────────────┴────────────────────┴────────────────────┴──────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         JWT TOKEN STRUCTURE                                      │

│                                                                                  │

│  Access Token (Short-lived: 15 minutes):                                        │

│  ┌────────────────────────────────────────────────────────────────────────────┐ │

│  │ {                                                                          │ │

│  │   "sub": "user\_abc123",                     // User ID                     │ │

│  │   "tenant\_id": "tenant\_xyz",                // Tenant ID                   │ │

│  │   "tenant\_db": "shared",                    // or "dedicated"              │ │

│  │   "email": "user@tenant.com",                                              │ │

│  │   "roles": \["admin", "content\_manager"\],    // User roles in tenant        │ │

│  │   "permissions": \[                          // Resolved permissions        │ │

│  │     "videos.create",                                                       │ │

│  │     "videos.update",                                                       │ │

│  │     "videos.delete",                                                       │ │

│  │     "analytics.view",                                                      │ │

│  │     "users.manage"                                                         │ │

│  │   \],                                                                       │ │

│  │   "is\_super\_admin": false,                  // Super admin flag            │ │

│  │   "is\_impersonating": false,                // Impersonation flag          │ │

│  │   "original\_user\_id": null,                 // Set during impersonation    │ │

│  │   "exp": 1234567890,                        // Expiration                  │ │

│  │   "iat": 1234567890,                        // Issued at                   │ │

│  │   "iss": "yourplatform.com"                 // Issuer                      │ │

│  │ }                                                                          │ │

│  └────────────────────────────────────────────────────────────────────────────┘ │

│                                                                                  │

│  Refresh Token (Long-lived: 7 days):                                            │

│  ┌────────────────────────────────────────────────────────────────────────────┐ │

│  │ Stored in database, linked to user \+ device                                │ │

│  │ Used to obtain new access tokens without re-login                          │ │

│  │ Can be revoked (logout, security breach)                                   │ │

│  └────────────────────────────────────────────────────────────────────────────┘ │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│                    SUPER ADMIN IMPERSONATION FLOW                                │

│                                                                                  │

│  ┌──────────────┐                    ┌──────────────┐                           │

│  │ Super Admin  │                    │    System    │                           │

│  │  Dashboard   │                    │              │                           │

│  └──────┬───────┘                    └──────┬───────┘                           │

│         │                                   │                                    │

│         │ 1\. Click "Login as" on Tenant X, User Y                               │

│         │───────────────────────────────────►                                    │

│         │                                   │                                    │

│         │                   2\. Verify Super Admin permissions                    │

│         │                   3\. Log impersonation attempt (Audit)                 │

│         │                   4\. Generate impersonation token with:                │

│         │                      \- is\_impersonating: true                          │

│         │                      \- original\_user\_id: super\_admin\_id               │

│         │                      \- All target user's permissions                   │

│         │                                   │                                    │

│         │ 5\. Return impersonation token     │                                    │

│         │◄───────────────────────────────────                                    │

│         │                                   │                                    │

│         │ 6\. Redirect to Tenant Dashboard   │                                    │

│         │   (with banner "Viewing as User Y")                                    │

│         │                                   │                                    │

│         │ 7\. All actions logged with        │                                    │

│         │    impersonation context          │                                    │

│         │                                   │                                    │

│         │ 8\. "Exit Impersonation" button    │                                    │

│         │    returns to Super Admin view    │                                    │

│         │                                   │                                    │

└─────────┴───────────────────────────────────┴────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         RBAC PERMISSION STRUCTURE                                │

│                                                                                  │

│  Permission Format: {resource}.{action}                                         │

│                                                                                  │

│  ┌────────────────────────────────────────────────────────────────────────────┐ │

│  │ AVAILABLE PERMISSIONS (Per Tenant \- Customizable)                          │ │

│  │                                                                            │ │

│  │ Videos:                          Users:                                    │ │

│  │   videos.view                      users.view                              │ │

│  │   videos.create                    users.create                            │ │

│  │   videos.update                    users.update                            │ │

│  │   videos.delete                    users.delete                            │ │

│  │   videos.upload                    users.invite                            │ │

│  │   videos.bulk\_operations           users.roles.manage                      │ │

│  │   videos.export                                                            │ │

│  │   videos.settings                Billing:                                  │ │

│  │                                    billing.view                            │ │

│  │ Collections:                       billing.manage                          │ │

│  │   collections.view                 billing.invoices                        │ │

│  │   collections.create                                                       │ │

│  │   collections.update             Settings:                                 │ │

│  │   collections.delete               settings.view                           │ │

│  │                                    settings.update                         │ │

│  │ Analytics:                         settings.branding                       │ │

│  │   analytics.view                   settings.api\_keys                       │ │

│  │   analytics.export                 settings.webhooks                       │ │

│  │   analytics.detailed                                                       │ │

│  │                                  Support:                                  │ │

│  │ Captions:                          support.view\_tickets                    │ │

│  │   captions.view                    support.create\_tickets                  │ │

│  │   captions.create                  support.manage\_tickets                  │ │

│  │   captions.update                                                          │ │

│  │   captions.delete                                                          │ │

│  └────────────────────────────────────────────────────────────────────────────┘ │

│                                                                                  │

│  Role Definition (Stored per Tenant):                                           │

│  ┌────────────────────────────────────────────────────────────────────────────┐ │

│  │ {                                                                          │ │

│  │   "role\_id": "role\_123",                                                   │ │

│  │   "tenant\_id": "tenant\_xyz",                                               │ │

│  │   "name": "Content Manager",                                               │ │

│  │   "description": "Can manage videos and collections",                      │ │

│  │   "is\_system": false,  // false \= custom role, true \= cannot delete        │ │

│  │   "permissions": \[                                                         │ │

│  │     "videos.view", "videos.create", "videos.update",                       │ │

│  │     "collections.\*",  // Wildcard for all collection perms                 │ │

│  │     "analytics.view"                                                       │ │

│  │   \]                                                                        │ │

│  │ }                                                                          │ │

│  └────────────────────────────────────────────────────────────────────────────┘ │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         2FA EMAIL VERIFICATION FLOW                              │

│                                                                                  │

│  ┌──────────┐         ┌──────────┐         ┌──────────┐         ┌──────────┐   │

│  │   User   │         │ Frontend │         │  Backend │         │  Email   │   │

│  └────┬─────┘         └────┬─────┘         └────┬─────┘         └────┬─────┘   │

│       │                    │                    │                    │          │

│       │ 1\. Login with email/password           │                    │          │

│       │────────────────────────────────────────►                    │          │

│       │                    │                    │                    │          │

│       │                    │ 2\. Credentials valid, 2FA required     │          │

│       │                    │◄────────────────────                   │          │

│       │                    │                    │                    │          │

│       │                    │                    │ 3\. Generate 6-digit code     │

│       │                    │                    │    (valid 10 minutes)        │

│       │                    │                    │────────────────────►          │

│       │                    │                    │                    │          │

│       │                    │                    │ 4\. Send email with code       │

│       │                    │                    │                    │          │

│       │ 5\. Show 2FA input screen               │                    │          │

│       │◄────────────────────                   │                    │          │

│       │                    │                    │                    │          │

│       │ 6\. Enter 6-digit code                  │                    │          │

│       │────────────────────────────────────────►                    │          │

│       │                    │                    │                    │          │

│       │ 7\. Validate code, issue JWT            │                    │          │

│       │◄────────────────────────────────────────                    │          │

│       │                    │                    │                    │          │

└───────┴────────────────────┴────────────────────┴────────────────────┴──────────┘

---

# PART 2: DATABASE SCHEMA DESIGN

## 2.1 Complete Entity Relationship Diagram

┌─────────────────────────────────────────────────────────────────────────────────┐

│                    DATABASE SCHEMA \- MASTER DATABASE                             │

│                         (PostgreSQL)                                             │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           SYSTEM LEVEL TABLES                                     ║

║                    (No TenantId \- Platform Wide)                                  ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

┌─────────────────────────────────────────────────────────────────────────────────┐

│ TENANTS                                                                          │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ name                : VARCHAR(255) NOT NULL                                      │

│ slug                : VARCHAR(100) UNIQUE NOT NULL  \-- for subdomain             │

│ status              : ENUM('active','suspended','trial','cancelled')             │

│ database\_type       : ENUM('shared','dedicated') DEFAULT 'shared'               │

│ dedicated\_db\_conn   : TEXT (encrypted, nullable)   \-- for dedicated tenants      │

│ bunny\_library\_id    : VARCHAR(50) (encrypted)      \-- Bunny.net library ID       │

│ bunny\_api\_key       : TEXT (encrypted)             \-- Per-library API key        │

│ bunny\_pull\_zone\_id  : VARCHAR(50)                  \-- Pull zone ID               │

│ bunny\_cdn\_hostname  : VARCHAR(255)                 \-- CDN hostname               │

│ trial\_ends\_at       : TIMESTAMP                                                  │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

│ settings            : JSONB                        \-- Tenant-specific settings   │

└─────────────────────────────────────────────────────────────────────────────────┘

        │

        │ 1:N

        ▼

┌─────────────────────────────────────────────────────────────────────────────────┐

│ TENANT\_BRANDING                                                                  │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) UNIQUE                               │

│ logo\_url            : VARCHAR(500)                                               │

│ favicon\_url         : VARCHAR(500)                                               │

│ primary\_color       : VARCHAR(7)  \-- \#RRGGBB                                     │

│ secondary\_color     : VARCHAR(7)                                                 │

│ accent\_color        : VARCHAR(7)                                                 │

│ player\_logo\_url     : VARCHAR(500)                                               │

│ player\_logo\_position: ENUM('top-left','top-right','bottom-left','bottom-right') │

│ custom\_css          : TEXT                                                       │

│ email\_footer\_text   : TEXT                                                       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ SUBSCRIPTION\_PLANS                                                               │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ name                : VARCHAR(100) NOT NULL                                      │

│ slug                : VARCHAR(50) UNIQUE NOT NULL                               │

│ description         : TEXT                                                       │

│ price\_monthly       : DECIMAL(10,2)                                              │

│ price\_yearly        : DECIMAL(10,2)                                              │

│ stripe\_price\_id\_monthly : VARCHAR(100)                                           │

│ stripe\_price\_id\_yearly  : VARCHAR(100)                                           │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ is\_custom           : BOOLEAN DEFAULT false \-- for enterprise custom plans       │

│ features            : JSONB                  \-- feature flags                    │

│ limits              : JSONB                  \-- see structure below              │

│ sort\_order          : INTEGER DEFAULT 0                                          │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

│

│ limits JSONB Structure:

│ {

│   "storage\_gb": 100,

│   "bandwidth\_gb\_monthly": 500,

│   "max\_videos": 1000,

│   "max\_users": 10,

│   "max\_collections": 50,

│   "video\_quality\_max": "1080p",  // "720p", "1080p", "4k"

│   "api\_requests\_daily": 10000,

│   "retention\_days": 365,

│   "custom\_roles": true,

│   "api\_access": true,

│   "priority\_encoding": false,

│   "dedicated\_support": false,

│   "sla\_percent": 99.5

│ }

│

│ features JSONB Structure:

│ {

│   "watermarking": true,

│   "geo\_blocking": false,

│   "analytics\_advanced": false,

│   "white\_label\_player": false,

│   "custom\_domain": false,

│   "webhook\_notifications": true,

│   "bulk\_operations": true,

│   "video\_chapters": true,

│   "captions": true,

│   "viewer\_authentication": false

│ }

│

┌─────────────────────────────────────────────────────────────────────────────────┐

│ TENANT\_SUBSCRIPTIONS                                                             │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ plan\_id             : UUID (FK \-\> SubscriptionPlans) NOT NULL                   │

│ status              : ENUM('active','past\_due','cancelled','trialing','paused') │

│ billing\_cycle       : ENUM('monthly','yearly','custom')                         │

│ stripe\_subscription\_id : VARCHAR(100)                                            │

│ stripe\_customer\_id  : VARCHAR(100)                                               │

│ current\_period\_start: TIMESTAMP                                                  │

│ current\_period\_end  : TIMESTAMP                                                  │

│ cancel\_at\_period\_end: BOOLEAN DEFAULT false                                      │

│ cancelled\_at        : TIMESTAMP                                                  │

│ payment\_method      : ENUM('stripe','manual','free')                            │

│ custom\_limits       : JSONB  \-- override plan limits for custom deals           │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ OVERAGE\_RATES                                                                    │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ plan\_id             : UUID (FK \-\> SubscriptionPlans)                            │

│ metric\_type         : ENUM('storage','bandwidth','encoding','api\_calls')        │

│ unit\_amount         : DECIMAL(10,4) \-- price per unit                           │

│ unit\_size           : INTEGER       \-- e.g., 1 for per GB                       │

│ unit\_name           : VARCHAR(20)   \-- e.g., "GB"                               │

│ minimum\_charge      : DECIMAL(10,2)                                              │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ INVOICES                                                                         │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ subscription\_id     : UUID (FK \-\> TenantSubscriptions)                          │

│ invoice\_number      : VARCHAR(50) UNIQUE NOT NULL                               │

│ status              : ENUM('draft','pending','paid','failed','cancelled','refunded')│

│ subtotal            : DECIMAL(10,2)                                              │

│ tax\_amount          : DECIMAL(10,2) DEFAULT 0                                    │

│ total               : DECIMAL(10,2)                                              │

│ currency            : VARCHAR(3) DEFAULT 'USD'                                   │

│ stripe\_invoice\_id   : VARCHAR(100)                                               │

│ payment\_method      : ENUM('stripe','manual','bank\_transfer')                   │

│ paid\_at             : TIMESTAMP                                                  │

│ due\_date            : TIMESTAMP                                                  │

│ period\_start        : TIMESTAMP                                                  │

│ period\_end          : TIMESTAMP                                                  │

│ notes               : TEXT                                                       │

│ line\_items          : JSONB        \-- detailed breakdown                         │

│ metadata            : JSONB                                                      │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ MANUAL\_PAYMENTS                                                                  │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ invoice\_id          : UUID (FK \-\> Invoices)                                     │

│ amount              : DECIMAL(10,2) NOT NULL                                     │

│ currency            : VARCHAR(3) DEFAULT 'USD'                                   │

│ payment\_method      : VARCHAR(100) \-- "Bank Transfer", "Check", etc.            │

│ reference\_number    : VARCHAR(100)                                               │

│ payment\_date        : DATE NOT NULL                                              │

│ notes               : TEXT                                                       │

│ proof\_document\_url  : VARCHAR(500) \-- receipt/proof upload                      │

│ recorded\_by         : UUID (FK \-\> SystemUsers)                                  │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ SYSTEM\_USERS (Super Admins)                                                      │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ email               : VARCHAR(255) UNIQUE NOT NULL                              │

│ password\_hash       : VARCHAR(255) NOT NULL                                      │

│ first\_name          : VARCHAR(100)                                               │

│ last\_name           : VARCHAR(100)                                               │

│ role                : ENUM('super\_admin','admin','support','billing')           │

│ permissions         : JSONB        \-- fine-grained perms                        │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ two\_factor\_enabled  : BOOLEAN DEFAULT false                                      │

│ two\_factor\_secret   : VARCHAR(100)                                               │

│ last\_login\_at       : TIMESTAMP                                                  │

│ last\_login\_ip       : VARCHAR(45)                                                │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ PLATFORM\_SETTINGS                                                                │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ key                 : VARCHAR(100) UNIQUE NOT NULL                              │

│ value               : TEXT                                                       │

│ value\_type          : ENUM('string','number','boolean','json')                  │

│ category            : VARCHAR(50)  \-- grouping                                  │

│ description         : TEXT                                                       │

│ is\_secret           : BOOLEAN DEFAULT false                                      │

│ updated\_by          : UUID (FK \-\> SystemUsers)                                  │

│ updated\_at          : TIMESTAMP DEFAULT NOW()                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ USAGE\_MULTIPLIERS                                                                │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants, nullable) \-- NULL \= global default   │

│ metric\_type         : ENUM('storage','bandwidth','encoding\_minutes','views')    │

│ multiplier          : DECIMAL(5,2) NOT NULL DEFAULT 1.00                        │

│ effective\_from      : TIMESTAMP DEFAULT NOW()                                    │

│ effective\_until     : TIMESTAMP   \-- NULL \= indefinite                          │

│ created\_by          : UUID (FK \-\> SystemUsers)                                  │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ notes               : TEXT                                                       │

└─────────────────────────────────────────────────────────────────────────────────┘

\-- Key: If tenant\_id is NULL, it's a global multiplier. Tenant-specific overrides global.

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           TENANT LEVEL TABLES                                     ║

║                    (Has TenantId \- Row-Level Isolation)                           ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

┌─────────────────────────────────────────────────────────────────────────────────┐

│ USERS                                                                            │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ email               : VARCHAR(255) NOT NULL                                      │

│ password\_hash       : VARCHAR(255)                                               │

│ first\_name          : VARCHAR(100)                                               │

│ last\_name           : VARCHAR(100)                                               │

│ avatar\_url          : VARCHAR(500)                                               │

│ status              : ENUM('active','inactive','pending','suspended')           │

│ is\_owner            : BOOLEAN DEFAULT false   \-- tenant owner                   │

│ two\_factor\_enabled  : BOOLEAN DEFAULT false                                      │

│ email\_verified      : BOOLEAN DEFAULT false                                      │

│ email\_verified\_at   : TIMESTAMP                                                  │

│ last\_login\_at       : TIMESTAMP                                                  │

│ last\_login\_ip       : VARCHAR(45)                                                │

│ invited\_by          : UUID (FK \-\> Users)                                        │

│ invited\_at          : TIMESTAMP                                                  │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

│ deleted\_at          : TIMESTAMP                \-- soft delete                    │

│ UNIQUE(tenant\_id, email)                                                         │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ ROLES                                                                            │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ name                : VARCHAR(100) NOT NULL                                      │

│ slug                : VARCHAR(50) NOT NULL                                       │

│ description         : TEXT                                                       │

│ is\_system           : BOOLEAN DEFAULT false  \-- cannot be deleted               │

│ permissions         : JSONB NOT NULL         \-- array of permission strings      │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

│ UNIQUE(tenant\_id, slug)                                                          │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ USER\_ROLES                                                                       │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ user\_id             : UUID (FK \-\> Users) NOT NULL                               │

│ role\_id             : UUID (FK \-\> Roles) NOT NULL                               │

│ assigned\_by         : UUID (FK \-\> Users)                                        │

│ assigned\_at         : TIMESTAMP DEFAULT NOW()                                    │

│ UNIQUE(user\_id, role\_id)                                                         │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ USER\_SESSIONS                                                                    │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ user\_id             : UUID (FK \-\> Users) NOT NULL                               │

│ refresh\_token       : VARCHAR(500) NOT NULL                                      │

│ device\_info         : VARCHAR(500)                                               │

│ ip\_address          : VARCHAR(45)                                                │

│ user\_agent          : TEXT                                                       │

│ is\_valid            : BOOLEAN DEFAULT true                                       │

│ expires\_at          : TIMESTAMP NOT NULL                                         │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ revoked\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ USER\_INVITATIONS                                                                 │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ email               : VARCHAR(255) NOT NULL                                      │

│ role\_id             : UUID (FK \-\> Roles)                                        │

│ token               : VARCHAR(100) UNIQUE NOT NULL                              │

│ invited\_by          : UUID (FK \-\> Users) NOT NULL                               │

│ status              : ENUM('pending','accepted','expired','cancelled')          │

│ expires\_at          : TIMESTAMP NOT NULL                                         │

│ accepted\_at         : TIMESTAMP                                                  │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ COLLECTIONS (Video Folders)                                                      │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ parent\_id           : UUID (FK \-\> Collections) \-- for nested folders            │

│ name                : VARCHAR(255) NOT NULL                                      │

│ description         : TEXT                                                       │

│ bunny\_collection\_id : VARCHAR(50)              \-- Bunny.net collection GUID     │

│ thumbnail\_url       : VARCHAR(500)                                               │

│ sort\_order          : INTEGER DEFAULT 0                                          │

│ video\_count         : INTEGER DEFAULT 0        \-- denormalized for performance   │

│ created\_by          : UUID (FK \-\> Users)                                        │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ VIDEOS                                                                           │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ collection\_id       : UUID (FK \-\> Collections)                                  │

│ bunny\_video\_guid    : VARCHAR(50) NOT NULL     \-- Bunny.net video GUID          │

│ title               : VARCHAR(500) NOT NULL                                      │

│ description         : TEXT                                                       │

│ status              : ENUM('uploading','encoding','ready','failed','deleted')   │

│ encoding\_progress   : INTEGER DEFAULT 0        \-- 0-100                         │

│ duration\_seconds    : INTEGER                                                    │

│ width               : INTEGER                                                    │

│ height              : INTEGER                                                    │

│ framerate           : DECIMAL(5,2)                                               │

│ storage\_size        : BIGINT                   \-- bytes                         │

│ thumbnail\_url       : VARCHAR(500)                                               │

│ preview\_url         : VARCHAR(500)             \-- animated thumbnail             │

│ available\_resolutions: JSONB                   \-- \["360p","720p","1080p"\]        │

│ has\_mp4\_fallback    : BOOLEAN DEFAULT false                                      │

│ rotation            : INTEGER DEFAULT 0                                          │

│ visibility          : ENUM('public','private','unlisted','password')            │

│ password\_hash       : VARCHAR(255)             \-- for password-protected         │

│ require\_auth        : BOOLEAN DEFAULT false    \-- viewer authentication          │

│ geo\_blocking        : JSONB                    \-- {allow: \[\], block: \[\]}         │

│ allowed\_referrers   : JSONB                    \-- \["example.com"\]                │

│ views\_count         : BIGINT DEFAULT 0                                           │

│ unique\_viewers      : BIGINT DEFAULT 0                                           │

│ average\_watch\_time  : INTEGER DEFAULT 0        \-- seconds                        │

│ tags                : JSONB                    \-- \["tag1", "tag2"\]               │

│ custom\_metadata     : JSONB                    \-- user-defined fields            │

│ original\_filename   : VARCHAR(500)                                               │

│ source\_type         : ENUM('upload','url\_pull','migration') DEFAULT 'upload'    │

│ source\_url          : VARCHAR(1000)            \-- for URL pull                   │

│ uploaded\_by         : UUID (FK \-\> Users)                                        │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

│ deleted\_at          : TIMESTAMP                \-- soft delete                    │

│ INDEX(tenant\_id, status)                                                         │

│ INDEX(tenant\_id, collection\_id)                                                  │

│ INDEX(bunny\_video\_guid)                                                          │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ VIDEO\_CAPTIONS                                                                   │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ video\_id            : UUID (FK \-\> Videos) NOT NULL                              │

│ language\_code       : VARCHAR(10) NOT NULL     \-- ISO 639-1 (en, es, fr)        │

│ label               : VARCHAR(100)             \-- e.g., "English", "Spanish"    │

│ srclang             : VARCHAR(10)                                                │

│ bunny\_caption\_id    : VARCHAR(50)                                                │

│ is\_auto\_generated   : BOOLEAN DEFAULT false                                      │

│ is\_default          : BOOLEAN DEFAULT false                                      │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ UNIQUE(video\_id, language\_code)                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ VIDEO\_CHAPTERS                                                                   │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ video\_id            : UUID (FK \-\> Videos) NOT NULL                              │

│ title               : VARCHAR(255) NOT NULL                                      │

│ start\_time          : INTEGER NOT NULL         \-- seconds                        │

│ end\_time            : INTEGER                  \-- seconds                        │

│ thumbnail\_url       : VARCHAR(500)                                               │

│ sort\_order          : INTEGER DEFAULT 0                                          │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ INDEX(video\_id, start\_time)                                                      │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ VIDEO\_THUMBNAILS                                                                 │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ video\_id            : UUID (FK \-\> Videos) NOT NULL                              │

│ thumbnail\_type      : ENUM('auto','custom','frame')                             │

│ url                 : VARCHAR(500) NOT NULL                                      │

│ timestamp\_seconds   : INTEGER                  \-- for frame thumbnails           │

│ is\_primary          : BOOLEAN DEFAULT false                                      │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ VIDEO\_WATERMARKS                                                                 │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ name                : VARCHAR(100) NOT NULL                                      │

│ type                : ENUM('image','text')                                       │

│ image\_url           : VARCHAR(500)                                               │

│ text\_content        : VARCHAR(255)                                               │

│ position            : ENUM('top-left','top-right','bottom-left','bottom-right','center')│

│ opacity             : INTEGER DEFAULT 100      \-- 0-100                          │

│ size\_percent        : INTEGER DEFAULT 10       \-- % of video width              │

│ is\_default          : BOOLEAN DEFAULT false                                      │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ VIDEO\_ANALYTICS\_DAILY                                                            │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ video\_id            : UUID (FK \-\> Videos) NOT NULL                              │

│ date                : DATE NOT NULL                                              │

│ views               : INTEGER DEFAULT 0                                          │

│ unique\_viewers      : INTEGER DEFAULT 0                                          │

│ watch\_time\_seconds  : BIGINT DEFAULT 0                                           │

│ bandwidth\_bytes     : BIGINT DEFAULT 0                                           │

│ engagement\_score    : DECIMAL(5,2)             \-- calculated metric             │

│ avg\_view\_duration   : INTEGER                  \-- seconds                        │

│ completion\_rate     : DECIMAL(5,2)             \-- percentage                     │

│ device\_breakdown    : JSONB                    \-- {desktop: 60, mobile: 40}     │

│ country\_breakdown   : JSONB                    \-- {US: 50, UK: 30, ...}         │

│ referrer\_breakdown  : JSONB                                                      │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ UNIQUE(video\_id, date)                                                           │

│ INDEX(tenant\_id, date)                                                           │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ TENANT\_USAGE\_DAILY (Actual Bunny.net Usage \- Internal)                          │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ date                : DATE NOT NULL                                              │

│ storage\_bytes\_actual: BIGINT DEFAULT 0         \-- actual from Bunny.net          │

│ bandwidth\_bytes\_actual: BIGINT DEFAULT 0       \-- actual from Bunny.net          │

│ encoding\_minutes\_actual: INTEGER DEFAULT 0                                       │

│ views\_actual        : BIGINT DEFAULT 0                                           │

│ storage\_bytes\_display: BIGINT DEFAULT 0        \-- after multiplier              │

│ bandwidth\_bytes\_display: BIGINT DEFAULT 0      \-- after multiplier              │

│ encoding\_minutes\_display: INTEGER DEFAULT 0    \-- after multiplier              │

│ views\_display       : BIGINT DEFAULT 0         \-- after multiplier              │

│ multipliers\_applied : JSONB                    \-- record of multipliers used     │

│ synced\_at           : TIMESTAMP                                                  │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ UNIQUE(tenant\_id, date)                                                          │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ API\_KEYS                                                                         │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ name                : VARCHAR(100) NOT NULL                                      │

│ key\_prefix          : VARCHAR(20) NOT NULL     \-- first 8 chars for display     │

│ key\_hash            : VARCHAR(255) NOT NULL    \-- hashed key                    │

│ permissions         : JSONB                    \-- scoped permissions             │

│ rate\_limit\_per\_hour : INTEGER DEFAULT 1000                                       │

│ allowed\_ips         : JSONB                    \-- IP whitelist                   │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ last\_used\_at        : TIMESTAMP                                                  │

│ expires\_at          : TIMESTAMP                                                  │

│ created\_by          : UUID (FK \-\> Users)                                        │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ revoked\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ WEBHOOK\_ENDPOINTS                                                                │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ url                 : VARCHAR(1000) NOT NULL                                     │

│ secret              : VARCHAR(255)             \-- for signature verification     │

│ events              : JSONB NOT NULL           \-- \["video.encoded", "video.deleted"\]│

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ failure\_count       : INTEGER DEFAULT 0                                          │

│ last\_triggered\_at   : TIMESTAMP                                                  │

│ last\_success\_at     : TIMESTAMP                                                  │

│ last\_failure\_at     : TIMESTAMP                                                  │

│ last\_failure\_reason : TEXT                                                       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ WEBHOOK\_DELIVERIES                                                               │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ webhook\_endpoint\_id : UUID (FK \-\> WebhookEndpoints) NOT NULL                    │

│ event\_type          : VARCHAR(50) NOT NULL                                       │

│ payload             : JSONB NOT NULL                                             │

│ status              : ENUM('pending','delivered','failed') DEFAULT 'pending'    │

│ attempts            : INTEGER DEFAULT 0                                          │

│ response\_code       : INTEGER                                                    │

│ response\_body       : TEXT                                                       │

│ delivered\_at        : TIMESTAMP                                                  │

│ next\_retry\_at       : TIMESTAMP                                                  │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ EMBED\_DOMAINS                                                                    │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ domain              : VARCHAR(255) NOT NULL    \-- e.g., "example.com"           │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ UNIQUE(tenant\_id, domain)                                                        │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ VIDEO\_ACCESS\_TOKENS (For Viewer Authentication)                                  │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ video\_id            : UUID (FK \-\> Videos) NOT NULL                              │

│ token               : VARCHAR(255) UNIQUE NOT NULL                              │

│ viewer\_identifier   : VARCHAR(255)             \-- email, user ID, etc.          │

│ permissions         : JSONB                    \-- {can\_download: false}         │

│ max\_views           : INTEGER                  \-- NULL \= unlimited              │

│ views\_used          : INTEGER DEFAULT 0                                          │

│ valid\_from          : TIMESTAMP DEFAULT NOW()                                    │

│ valid\_until         : TIMESTAMP                                                  │

│ ip\_restriction      : VARCHAR(45)              \-- lock to IP                    │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ created\_by          : UUID (FK \-\> Users)                                        │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           SUPPORT SYSTEM TABLES                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

┌─────────────────────────────────────────────────────────────────────────────────┐

│ SUPPORT\_DEPARTMENTS                                                              │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ name                : VARCHAR(100) NOT NULL                                      │

│ slug                : VARCHAR(50) UNIQUE NOT NULL                               │

│ description         : TEXT                                                       │

│ email               : VARCHAR(255)             \-- department email              │

│ auto\_assign\_to      : UUID (FK \-\> SystemUsers) \-- default assignee              │

│ sla\_response\_hours  : INTEGER DEFAULT 24                                         │

│ sla\_resolution\_hours: INTEGER DEFAULT 72                                         │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ sort\_order          : INTEGER DEFAULT 0                                          │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ SUPPORT\_TICKETS                                                                  │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ ticket\_number       : VARCHAR(20) UNIQUE NOT NULL  \-- e.g., "TKT-2024-00001"   │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ user\_id             : UUID (FK \-\> Users) NOT NULL  \-- who created ticket        │

│ department\_id       : UUID (FK \-\> SupportDepartments)                           │

│ assigned\_to         : UUID (FK \-\> SystemUsers)     \-- support agent             │

│ subject             : VARCHAR(500) NOT NULL                                      │

│ description         : TEXT NOT NULL                                              │

│ status              : ENUM('open','pending','in\_progress','waiting\_customer',   │

│                           'resolved','closed','cancelled')                       │

│ priority            : ENUM('low','medium','high','urgent') DEFAULT 'medium'     │

│ category            : VARCHAR(100)                 \-- video issues, billing, etc│

│ tags                : JSONB                                                      │

│ source              : ENUM('web','email','api')                                 │

│ first\_response\_at   : TIMESTAMP                    \-- SLA tracking              │

│ resolved\_at         : TIMESTAMP                                                  │

│ closed\_at           : TIMESTAMP                                                  │

│ satisfaction\_rating : INTEGER                      \-- 1-5 stars                 │

│ satisfaction\_feedback: TEXT                                                      │

│ is\_escalated        : BOOLEAN DEFAULT false                                      │

│ escalated\_at        : TIMESTAMP                                                  │

│ escalated\_by        : UUID                                                       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

│ INDEX(tenant\_id, status)                                                         │

│ INDEX(assigned\_to, status)                                                       │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ TICKET\_MESSAGES                                                                  │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ ticket\_id           : UUID (FK \-\> SupportTickets) NOT NULL                      │

│ sender\_type         : ENUM('user','system\_user','system')                       │

│ sender\_id           : UUID                         \-- user\_id or system\_user\_id │

│ message             : TEXT NOT NULL                                              │

│ is\_internal         : BOOLEAN DEFAULT false        \-- internal notes            │

│ attachments         : JSONB                        \-- \[{name, url, size}\]       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ INDEX(ticket\_id, created\_at)                                                     │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ TICKET\_ACTIVITIES                                                                │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ ticket\_id           : UUID (FK \-\> SupportTickets) NOT NULL                      │

│ activity\_type       : ENUM('status\_changed','assigned','escalated',             │

│                           'priority\_changed','department\_changed','merged')     │

│ old\_value           : VARCHAR(255)                                               │

│ new\_value           : VARCHAR(255)                                               │

│ performed\_by        : UUID                                                       │

│ performed\_by\_type   : ENUM('user','system\_user','system')                       │

│ note                : TEXT                                                       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ CANNED\_RESPONSES                                                                 │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ department\_id       : UUID (FK \-\> SupportDepartments) \-- NULL \= global          │

│ title               : VARCHAR(255) NOT NULL                                      │

│ content             : TEXT NOT NULL                                              │

│ category            : VARCHAR(100)                                               │

│ shortcut            : VARCHAR(50)                  \-- e.g., "/welcome"          │

│ use\_count           : INTEGER DEFAULT 0                                          │

│ created\_by          : UUID (FK \-\> SystemUsers)                                  │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ KNOWLEDGE\_BASE\_CATEGORIES                                                        │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ parent\_id           : UUID (FK \-\> KnowledgeBaseCategories)                      │

│ name                : VARCHAR(255) NOT NULL                                      │

│ slug                : VARCHAR(100) UNIQUE NOT NULL                              │

│ description         : TEXT                                                       │

│ icon                : VARCHAR(50)                  \-- icon name                 │

│ sort\_order          : INTEGER DEFAULT 0                                          │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ articles\_count      : INTEGER DEFAULT 0                                          │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ KNOWLEDGE\_BASE\_ARTICLES                                                          │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ category\_id         : UUID (FK \-\> KnowledgeBaseCategories) NOT NULL             │

│ title               : VARCHAR(500) NOT NULL                                      │

│ slug                : VARCHAR(200) UNIQUE NOT NULL                              │

│ content             : TEXT NOT NULL                \-- markdown/HTML             │

│ excerpt             : VARCHAR(500)                                               │

│ status              : ENUM('draft','published','archived')                      │

│ visibility          : ENUM('public','logged\_in','internal')                     │

│ views\_count         : INTEGER DEFAULT 0                                          │

│ helpfulness\_yes     : INTEGER DEFAULT 0                                          │

│ helpfulness\_no      : INTEGER DEFAULT 0                                          │

│ tags                : JSONB                                                      │

│ related\_articles    : JSONB                        \-- array of article IDs      │

│ meta\_title          : VARCHAR(200)                 \-- SEO                       │

│ meta\_description    : VARCHAR(300)                                               │

│ author\_id           : UUID (FK \-\> SystemUsers)                                  │

│ published\_at        : TIMESTAMP                                                  │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

│ INDEX(category\_id, status)                                                       │

│ INDEX(slug)                                                                      │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ ESCALATION\_RULES                                                                 │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ name                : VARCHAR(100) NOT NULL                                      │

│ department\_id       : UUID (FK \-\> SupportDepartments)                           │

│ condition\_type      : ENUM('time\_without\_response','time\_without\_resolution',   │

│                           'priority','keyword','subscription\_tier')             │

│ condition\_value     : JSONB                        \-- condition parameters       │

│ action\_type         : ENUM('assign','notify','change\_priority','escalate')      │

│ action\_value        : JSONB                        \-- action parameters          │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ sort\_order          : INTEGER DEFAULT 0                                          │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           AUDIT & LOGGING TABLES                                  ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

┌─────────────────────────────────────────────────────────────────────────────────┐

│ AUDIT\_LOGS                                                                       │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : BIGSERIAL (PK)               \-- bigint for high volume    │

│ tenant\_id           : UUID                         \-- nullable for system events│

│ user\_type           : ENUM('user','system\_user','system','api\_key')             │

│ user\_id             : UUID                                                       │

│ impersonated\_by     : UUID                         \-- if action via impersonation│

│ action              : VARCHAR(100) NOT NULL        \-- 'video.created', etc.     │

│ entity\_type         : VARCHAR(50)                  \-- 'Video', 'User', etc.     │

│ entity\_id           : UUID                                                       │

│ old\_values          : JSONB                        \-- before change             │

│ new\_values          : JSONB                        \-- after change              │

│ metadata            : JSONB                        \-- additional context         │

│ ip\_address          : VARCHAR(45)                                                │

│ user\_agent          : TEXT                                                       │

│ request\_id          : UUID                         \-- correlation ID            │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ INDEX(tenant\_id, created\_at)                                                     │

│ INDEX(user\_id, created\_at)                                                       │

│ INDEX(entity\_type, entity\_id)                                                    │

│ INDEX(action)                                                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

\-- Note: Partition by month for performance: PARTITION BY RANGE (created\_at)

┌─────────────────────────────────────────────────────────────────────────────────┐

│ API\_USAGE\_LOGS                                                                   │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : BIGSERIAL (PK)                                             │

│ tenant\_id           : UUID NOT NULL                                              │

│ api\_key\_id          : UUID (FK \-\> ApiKeys)                                      │

│ endpoint            : VARCHAR(255) NOT NULL                                      │

│ method              : VARCHAR(10) NOT NULL                                       │

│ status\_code         : INTEGER                                                    │

│ response\_time\_ms    : INTEGER                                                    │

│ request\_size\_bytes  : INTEGER                                                    │

│ response\_size\_bytes : INTEGER                                                    │

│ ip\_address          : VARCHAR(45)                                                │

│ error\_message       : TEXT                                                       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ INDEX(tenant\_id, created\_at)                                                     │

│ INDEX(api\_key\_id, created\_at)                                                    │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           NOTIFICATION TABLES                                     ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

┌─────────────────────────────────────────────────────────────────────────────────┐

│ EMAIL\_TEMPLATES                                                                  │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants)         \-- NULL \= system template    │

│ name                : VARCHAR(100) NOT NULL                                      │

│ slug                : VARCHAR(50) NOT NULL         \-- 'welcome', 'invoice', etc.│

│ subject             : VARCHAR(500) NOT NULL                                      │

│ body\_html           : TEXT NOT NULL                                              │

│ body\_text           : TEXT                                                       │

│ variables           : JSONB                        \-- available placeholders     │

│ is\_system           : BOOLEAN DEFAULT false        \-- cannot delete if true     │

│ is\_active           : BOOLEAN DEFAULT true                                       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

│ UNIQUE(tenant\_id, slug)                                                          │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ EMAIL\_QUEUE                                                                      │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID                                                       │

│ template\_id         : UUID (FK \-\> EmailTemplates)                               │

│ to\_email            : VARCHAR(255) NOT NULL                                      │

│ to\_name             : VARCHAR(255)                                               │

│ subject             : VARCHAR(500) NOT NULL                                      │

│ body\_html           : TEXT NOT NULL                                              │

│ body\_text           : TEXT                                                       │

│ status              : ENUM('pending','sent','failed') DEFAULT 'pending'         │

│ priority            : INTEGER DEFAULT 5            \-- 1=highest, 10=lowest      │

│ attempts            : INTEGER DEFAULT 0                                          │

│ last\_attempt\_at     : TIMESTAMP                                                  │

│ sent\_at             : TIMESTAMP                                                  │

│ error\_message       : TEXT                                                       │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ scheduled\_at        : TIMESTAMP DEFAULT NOW()                                    │

│ INDEX(status, scheduled\_at, priority)                                            │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ NOTIFICATIONS (In-App)                                                           │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants)         \-- NULL for system users     │

│ user\_type           : ENUM('user','system\_user')                                │

│ user\_id             : UUID NOT NULL                                              │

│ title               : VARCHAR(255) NOT NULL                                      │

│ message             : TEXT                                                       │

│ type                : ENUM('info','success','warning','error')                  │

│ category            : VARCHAR(50)                  \-- 'billing', 'video', etc.  │

│ action\_url          : VARCHAR(500)                 \-- link destination           │

│ is\_read             : BOOLEAN DEFAULT false                                      │

│ read\_at             : TIMESTAMP                                                  │

│ metadata            : JSONB                                                      │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ expires\_at          : TIMESTAMP                                                  │

│ INDEX(user\_id, is\_read, created\_at)                                              │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           VIDEO MIGRATION TABLES                                  ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

┌─────────────────────────────────────────────────────────────────────────────────┐

│ MIGRATION\_JOBS                                                                   │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ tenant\_id           : UUID (FK \-\> Tenants) NOT NULL                             │

│ name                : VARCHAR(255)                                               │

│ source\_type         : ENUM('youtube','vimeo','url\_list','ftp','s3')             │

│ status              : ENUM('pending','validating','in\_progress','completed',    │

│                           'failed','cancelled','paused')                        │

│ total\_videos        : INTEGER DEFAULT 0                                          │

│ processed\_videos    : INTEGER DEFAULT 0                                          │

│ failed\_videos       : INTEGER DEFAULT 0                                          │

│ skipped\_videos      : INTEGER DEFAULT 0                                          │

│ source\_config       : JSONB                        \-- source-specific config     │

│ options             : JSONB                        \-- {preserve\_metadata: true} │

│ error\_log           : TEXT\[\]                       \-- array of errors           │

│ started\_at          : TIMESTAMP                                                  │

│ completed\_at        : TIMESTAMP                                                  │

│ created\_by          : UUID (FK \-\> Users)                                        │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ updated\_at          : TIMESTAMP                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐

│ MIGRATION\_ITEMS                                                                  │

├─────────────────────────────────────────────────────────────────────────────────┤

│ id                  : UUID (PK)                                                  │

│ job\_id              : UUID (FK \-\> MigrationJobs) NOT NULL                       │

│ source\_url          : VARCHAR(2000) NOT NULL                                     │

│ source\_id           : VARCHAR(255)                 \-- YouTube ID, Vimeo ID, etc.│

│ source\_metadata     : JSONB                        \-- original metadata          │

│ video\_id            : UUID (FK \-\> Videos)          \-- created video             │

│ status              : ENUM('pending','downloading','uploading','processing',    │

│                           'completed','failed','skipped')                       │

│ error\_message       : TEXT                                                       │

│ progress\_percent    : INTEGER DEFAULT 0                                          │

│ started\_at          : TIMESTAMP                                                  │

│ completed\_at        : TIMESTAMP                                                  │

│ created\_at          : TIMESTAMP DEFAULT NOW()                                    │

│ INDEX(job\_id, status)                                                            │

└─────────────────────────────────────────────────────────────────────────────────┘

## 2.2 Database Indexes Strategy

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         CRITICAL INDEXES                                         │

├─────────────────────────────────────────────────────────────────────────────────┤

│                                                                                  │

│  Multi-Tenancy Indexes (CRITICAL for performance):                              │

│  ────────────────────────────────────────────────                               │

│  • Every tenant\_id column MUST be first in composite indexes                    │

│  • Partial indexes for common filters (status \= 'active')                       │

│                                                                                  │

│  CREATE INDEX idx\_videos\_tenant\_status ON videos(tenant\_id, status)             │

│       WHERE deleted\_at IS NULL;                                                  │

│                                                                                  │

│  CREATE INDEX idx\_videos\_tenant\_collection ON videos(tenant\_id, collection\_id)  │

│       WHERE deleted\_at IS NULL;                                                  │

│                                                                                  │

│  CREATE INDEX idx\_users\_tenant\_email ON users(tenant\_id, email)                 │

│       WHERE deleted\_at IS NULL;                                                  │

│                                                                                  │

│  Full-Text Search Indexes:                                                      │

│  ─────────────────────────                                                      │

│  CREATE INDEX idx\_videos\_search ON videos                                       │

│       USING GIN(to\_tsvector('english', title || ' ' || coalesce(description,'')));│

│                                                                                  │

│  CREATE INDEX idx\_kb\_articles\_search ON knowledge\_base\_articles                 │

│       USING GIN(to\_tsvector('english', title || ' ' || content));               │

│                                                                                  │

│  Audit Log Partitioning:                                                        │

│  ───────────────────────                                                        │

│  • Partition audit\_logs by month                                                │

│  • Automatic partition creation via pg\_partman or manually                      │

│  • Retention policy: Keep 12 months online, archive older                       │

│                                                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

---

# PART 3: BACKEND API STRUCTURE

## 3.1 API Organization (ASP.NET Core)

┌─────────────────────────────────────────────────────────────────────────────────┐

│                    ASP.NET CORE PROJECT STRUCTURE                                │

│                                                                                  │

│  Solution: StreamVault.sln                                                       │

│                                                                                  │

│  ├── src/                                                                        │

│  │   ├── StreamVault.Api/                      \# Web API Project                │

│  │   │   ├── Controllers/                                                        │

│  │   │   │   ├── V1/                           \# API Version 1                  │

│  │   │   │   │   ├── AuthController.cs                                          │

│  │   │   │   │   ├── VideoController.cs                                         │

│  │   │   │   │   ├── CollectionController.cs                                    │

│  │   │   │   │   ├── AnalyticsController.cs                                     │

│  │   │   │   │   ├── UserController.cs                                          │

│  │   │   │   │   ├── RoleController.cs                                          │

│  │   │   │   │   ├── SettingsController.cs                                      │

│  │   │   │   │   ├── WebhookController.cs                                       │

│  │   │   │   │   ├── UploadController.cs                                        │

│  │   │   │   │   ├── PlayerController.cs                                        │

│  │   │   │   │   ├── MigrationController.cs                                     │

│  │   │   │   │   └── SupportController.cs                                       │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Admin/                        \# Super Admin Controllers        │

│  │   │   │   │   ├── TenantsController.cs                                       │

│  │   │   │   │   ├── SubscriptionPlansController.cs                             │

│  │   │   │   │   ├── BillingController.cs                                       │

│  │   │   │   │   ├── PlatformSettingsController.cs                              │

│  │   │   │   │   ├── UsageMultipliersController.cs                              │

│  │   │   │   │   ├── SupportManagementController.cs                             │

│  │   │   │   │   ├── AuditLogsController.cs                                     │

│  │   │   │   │   ├── PlatformAnalyticsController.cs                             │

│  │   │   │   │   ├── KnowledgeBaseController.cs                                 │

│  │   │   │   │   └── ImpersonationController.cs                                 │

│  │   │   │   │                                                                   │

│  │   │   │   └── Public/                       \# Public/Embed Controllers       │

│  │   │   │       ├── EmbedController.cs                                         │

│  │   │   │       └── ViewerAuthController.cs                                    │

│  │   │   │                                                                       │

│  │   │   ├── Middleware/                                                         │

│  │   │   │   ├── TenantResolutionMiddleware.cs                                  │

│  │   │   │   ├── ExceptionHandlingMiddleware.cs                                 │

│  │   │   │   ├── RequestLoggingMiddleware.cs                                    │

│  │   │   │   ├── RateLimitingMiddleware.cs                                      │

│  │   │   │   └── ApiKeyAuthenticationMiddleware.cs                              │

│  │   │   │                                                                       │

│  │   │   ├── Filters/                                                            │

│  │   │   │   ├── RequirePermissionAttribute.cs                                  │

│  │   │   │   ├── ValidateTenantSubscriptionFilter.cs                            │

│  │   │   │   └── AuditLogFilter.cs                                              │

│  │   │   │                                                                       │

│  │   │   ├── Validators/                       \# FluentValidation               │

│  │   │   │   ├── VideoValidators.cs                                             │

│  │   │   │   ├── UserValidators.cs                                              │

│  │   │   │   └── ...                                                             │

│  │   │   │                                                                       │

│  │   │   ├── Configuration/                                                      │

│  │   │   │   ├── SwaggerConfiguration.cs                                        │

│  │   │   │   ├── AuthenticationConfiguration.cs                                 │

│  │   │   │   ├── CorsConfiguration.cs                                           │

│  │   │   │   └── HangfireConfiguration.cs                                       │

│  │   │   │                                                                       │

│  │   │   ├── Program.cs                                                          │

│  │   │   └── appsettings.json                                                    │

│  │   │                                                                           │

│  │   ├── StreamVault.Application/              \# Business Logic                 │

│  │   │   ├── Services/                                                           │

│  │   │   │   ├── Videos/                                                         │

│  │   │   │   │   ├── IVideoService.cs                                           │

│  │   │   │   │   ├── VideoService.cs                                            │

│  │   │   │   │   ├── VideoUploadService.cs                                      │

│  │   │   │   │   └── VideoMigrationService.cs                                   │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Collections/                                                    │

│  │   │   │   │   ├── ICollectionService.cs                                      │

│  │   │   │   │   └── CollectionService.cs                                       │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Users/                                                          │

│  │   │   │   │   ├── IUserService.cs                                            │

│  │   │   │   │   ├── UserService.cs                                             │

│  │   │   │   │   ├── RoleService.cs                                             │

│  │   │   │   │   └── InvitationService.cs                                       │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Auth/                                                           │

│  │   │   │   │   ├── IAuthService.cs                                            │

│  │   │   │   │   ├── AuthService.cs                                             │

│  │   │   │   │   ├── TwoFactorService.cs                                        │

│  │   │   │   │   └── ImpersonationService.cs                                    │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Analytics/                                                      │

│  │   │   │   │   ├── IAnalyticsService.cs                                       │

│  │   │   │   │   ├── AnalyticsService.cs                                        │

│  │   │   │   │   ├── UsageCalculationService.cs  \# Applies multipliers          │

│  │   │   │   │   └── AnalyticsExportService.cs                                  │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Billing/                                                        │

│  │   │   │   │   ├── IBillingService.cs                                         │

│  │   │   │   │   ├── BillingService.cs                                          │

│  │   │   │   │   ├── StripeService.cs                                           │

│  │   │   │   │   ├── ManualPaymentService.cs                                    │

│  │   │   │   │   ├── OverageCalculationService.cs                               │

│  │   │   │   │   └── InvoiceGenerationService.cs                                │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Tenants/                                                        │

│  │   │   │   │   ├── ITenantService.cs                                          │

│  │   │   │   │   ├── TenantService.cs                                           │

│  │   │   │   │   └── TenantProvisioningService.cs                               │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Support/                                                        │

│  │   │   │   │   ├── ITicketService.cs                                          │

│  │   │   │   │   ├── TicketService.cs                                           │

│  │   │   │   │   ├── EscalationService.cs                                       │

│  │   │   │   │   └── KnowledgeBaseService.cs                                    │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Notifications/                                                  │

│  │   │   │   │   ├── IEmailService.cs                                           │

│  │   │   │   │   ├── EmailService.cs                                            │

│  │   │   │   │   └── NotificationService.cs                                     │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Webhooks/                                                       │

│  │   │   │   │   ├── IWebhookService.cs                                         │

│  │   │   │   │   ├── WebhookService.cs                                          │

│  │   │   │   │   └── WebhookDeliveryService.cs                                  │

│  │   │   │   │                                                                   │

│  │   │   │   └── Platform/                                                       │

│  │   │   │       ├── IPlatformSettingsService.cs                                │

│  │   │   │       └── PlatformSettingsService.cs                                 │

│  │   │   │                                                                       │

│  │   │   ├── DTOs/                              \# Data Transfer Objects         │

│  │   │   │   ├── Videos/                                                         │

│  │   │   │   │   ├── VideoDto.cs                                                │

│  │   │   │   │   ├── CreateVideoRequest.cs                                      │

│  │   │   │   │   ├── UpdateVideoRequest.cs                                      │

│  │   │   │   │   ├── VideoListResponse.cs                                       │

│  │   │   │   │   └── VideoAnalyticsDto.cs                                       │

│  │   │   │   ├── Users/                                                          │

│  │   │   │   ├── Auth/                                                           │

│  │   │   │   ├── Billing/                                                        │

│  │   │   │   ├── Support/                                                        │

│  │   │   │   └── Common/                                                         │

│  │   │   │       ├── PagedResponse.cs                                           │

│  │   │   │       ├── ApiResponse.cs                                             │

│  │   │   │       └── ErrorResponse.cs                                           │

│  │   │   │                                                                       │

│  │   │   ├── Mappings/                          \# AutoMapper Profiles           │

│  │   │   └── Interfaces/                                                         │

│  │   │                                                                           │

│  │   ├── StreamVault.Domain/                    \# Domain Models                 │

│  │   │   ├── Entities/                                                           │

│  │   │   │   ├── Tenant.cs                                                       │

│  │   │   │   ├── User.cs                                                         │

│  │   │   │   ├── Video.cs                                                        │

│  │   │   │   ├── Collection.cs                                                   │

│  │   │   │   ├── ...                                                             │

│  │   │   │   └── BaseEntity.cs                                                   │

│  │   │   │                                                                       │

│  │   │   ├── Enums/                                                              │

│  │   │   │   ├── VideoStatus.cs                                                  │

│  │   │   │   ├── SubscriptionStatus.cs                                          │

│  │   │   │   └── ...                                                             │

│  │   │   │                                                                       │

│  │   │   ├── ValueObjects/                                                       │

│  │   │   └── Events/                            \# Domain Events                 │

│  │   │                                                                           │

│  │   ├── StreamVault.Infrastructure/            \# External Dependencies         │

│  │   │   ├── Persistence/                                                        │

│  │   │   │   ├── AppDbContext.cs                                                │

│  │   │   │   ├── TenantDbContext.cs             \# For dedicated tenant DBs      │

│  │   │   │   ├── Configurations/                \# EF Core Configurations        │

│  │   │   │   ├── Migrations/                                                     │

│  │   │   │   └── Repositories/                                                   │

│  │   │   │                                                                       │

│  │   │   ├── ExternalServices/                                                   │

│  │   │   │   ├── BunnyNet/                      \# Bunny.net Integration         │

│  │   │   │   │   ├── IBunnyStreamClient.cs                                      │

│  │   │   │   │   ├── BunnyStreamClient.cs                                       │

│  │   │   │   │   ├── BunnyVideoMapper.cs        \# Maps Bunny response to ours   │

│  │   │   │   │   └── BunnyWebhookHandler.cs                                     │

│  │   │   │   │                                                                   │

│  │   │   │   ├── Stripe/                                                         │

│  │   │   │   │   ├── IStripeClient.cs                                           │

│  │   │   │   │   ├── StripeClient.cs                                            │

│  │   │   │   │   └── StripeWebhookHandler.cs                                    │

│  │   │   │   │                                                                   │

│  │   │   │   └── Email/                                                          │

│  │   │   │       ├── IEmailSender.cs                                            │

│  │   │   │       └── SmtpEmailSender.cs         \# Using MailKit                 │

│  │   │   │                                                                       │

│  │   │   ├── BackgroundJobs/                    \# Hangfire Jobs                 │

│  │   │   │   ├── UsageSyncJob.cs                \# Sync with Bunny.net           │

│  │   │   │   ├── AnalyticsAggregationJob.cs                                     │

│  │   │   │   ├── InvoiceGenerationJob.cs                                        │

│  │   │   │   ├── WebhookDeliveryJob.cs                                          │

│  │   │   │   ├── EmailQueueProcessorJob.cs                                      │

│  │   │   │   ├── MigrationProcessorJob.cs                                       │

│  │   │   │   ├── SubscriptionRenewalCheckJob.cs                                 │

│  │   │   │   └── CleanupJob.cs                  \# Delete old logs, etc.         │

│  │   │   │                                                                       │

│  │   │   ├── Caching/                                                            │

│  │   │   │   ├── ICacheService.cs                                               │

│  │   │   │   └── RedisCacheService.cs                                           │

│  │   │   │                                                                       │

│  │   │   └── Security/                                                           │

│  │   │       ├── JwtTokenGenerator.cs                                           │

│  │   │       ├── PasswordHasher.cs                                              │

│  │   │       └── EncryptionService.cs           \# For API keys, etc.            │

│  │   │                                                                           │

│  │   └── StreamVault.Shared/                    \# Shared Utilities              │

│  │       ├── Extensions/                                                         │

│  │       ├── Helpers/                                                            │

│  │       ├── Constants/                                                          │

│  │       └── Exceptions/                                                         │

│  │                                                                               │

│  └── tests/                                                                      │

│      ├── StreamVault.Api.Tests/                                                 │

│      ├── StreamVault.Application.Tests/                                         │

│      └── StreamVault.Infrastructure.Tests/                                      │

│                                                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

## 3.2 Complete API Endpoints Definition

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         API ENDPOINTS \- VERSION 1                                │

│                                                                                  │

│  Base URL: https://api.yourplatform.com/v1                                      │

│  Authentication: Bearer Token (JWT) or API Key                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           AUTHENTICATION ENDPOINTS                               ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  POST   /auth/login                  Login with email/password                   ║

║         Request: { email, password }                                             ║

║         Response: { requires\_2fa: bool, temp\_token? } or { access\_token, ... }  ║

║                                                                                   ║

║  POST   /auth/login/2fa              Verify 2FA code                             ║

║         Request: { temp\_token, code }                                            ║

║         Response: { access\_token, refresh\_token, expires\_in, user }             ║

║                                                                                   ║

║  POST   /auth/refresh                Refresh access token                        ║

║         Request: { refresh\_token }                                               ║

║         Response: { access\_token, refresh\_token, expires\_in }                   ║

║                                                                                   ║

║  POST   /auth/logout                 Invalidate refresh token                    ║

║         Request: { refresh\_token }                                               ║

║                                                                                   ║

║  POST   /auth/forgot-password        Request password reset                      ║

║         Request: { email }                                                       ║

║                                                                                   ║

║  POST   /auth/reset-password         Reset password with token                   ║

║         Request: { token, new\_password }                                         ║

║                                                                                   ║

║  POST   /auth/verify-email           Verify email address                        ║

║         Request: { token }                                                       ║

║                                                                                   ║

║  POST   /auth/resend-verification    Resend verification email                   ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           VIDEO ENDPOINTS                                         ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /videos                      List videos (paginated, filterable)         ║

║         Query: page, limit, status, collection\_id, search, sort\_by, order       ║

║         Response: { data: Video\[\], pagination: {...} }                          ║

║                                                                                   ║

# PART 3: BACKEND API STRUCTURE (Continued)

## 3.2 Complete API Endpoints Definition (Continued)

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           VIDEO ENDPOINTS (Continued)                             ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /videos/{id}                 Get video details                           ║

║         Response: { Video with full details including chapters, captions }       ║

║                                                                                   ║

║  POST   /videos                      Create video (URL pull or prepare upload)   ║

║         Request: { title, description?, collection\_id?, source\_url?,             ║

║                   visibility?, tags?, custom\_metadata? }                         ║

║         Response: { video\_id, upload\_url? (for direct upload) }                 ║

║                                                                                   ║

║  PUT    /videos/{id}                 Update video metadata                        ║

║         Request: { title?, description?, collection\_id?, visibility?,            ║

║                   tags?, custom\_metadata?, geo\_blocking?, allowed\_referrers? }  ║

║                                                                                   ║

║  DELETE /videos/{id}                 Soft delete video                            ║

║                                                                                   ║

║  POST   /videos/{id}/restore         Restore soft-deleted video                   ║

║                                                                                   ║

║  DELETE /videos/{id}/permanent       Permanently delete video                     ║

║                                                                                   ║

║  POST   /videos/bulk-delete          Bulk delete videos                           ║

║         Request: { video\_ids: string\[\] }                                         ║

║                                                                                   ║

║  POST   /videos/bulk-move            Bulk move to collection                      ║

║         Request: { video\_ids: string\[\], collection\_id: string }                 ║

║                                                                                   ║

║  POST   /videos/bulk-update          Bulk update visibility/settings              ║

║         Request: { video\_ids: string\[\], updates: {...} }                        ║

║                                                                                   ║

║  GET    /videos/{id}/embed-code      Get embed code for video                     ║

║         Query: autoplay?, muted?, loop?, start\_time?                            ║

║         Response: { iframe\_code, direct\_url, player\_url }                       ║

║                                                                                   ║

║  GET    /videos/{id}/playback-url    Get signed playback URL                      ║

║         Query: expires\_in?, viewer\_id?                                           ║

║         Response: { hls\_url, dash\_url?, mp4\_urls?, expires\_at }                 ║

║                                                                                   ║

║  POST   /videos/{id}/reencode        Request re-encoding                          ║

║                                                                                   ║

║  POST   /videos/{id}/duplicate       Duplicate video                              ║

║         Request: { new\_title?, collection\_id? }                                  ║

║                                                                                   ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                           VIDEO UPLOAD ENDPOINTS                                  ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  POST   /upload/create               Create upload session (TUS protocol)         ║

║         Request: { title, file\_size, file\_type, collection\_id? }                ║

║         Response: { video\_id, upload\_url, upload\_token, expires\_at }            ║

║                                                                                   ║

║  OPTIONS /upload/{video\_id}          TUS protocol \- get server capabilities       ║

║                                                                                   ║

║  HEAD   /upload/{video\_id}           TUS protocol \- get upload progress           ║

║                                                                                   ║

║  PATCH  /upload/{video\_id}           TUS protocol \- upload chunk                  ║

║         Headers: Upload-Offset, Content-Type, Tus-Resumable                      ║

║         Body: Binary chunk data                                                  ║

║                                                                                   ║

║  DELETE /upload/{video\_id}           Cancel upload                                ║

║                                                                                   ║

║  POST   /upload/from-url             Import video from URL                        ║

║         Request: { url, title?, description?, collection\_id? }                  ║

║         Response: { video\_id, status: "downloading" }                           ║

║                                                                                   ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                           VIDEO THUMBNAILS ENDPOINTS                              ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /videos/{id}/thumbnails      Get all thumbnails for video                 ║

║                                                                                   ║

║  POST   /videos/{id}/thumbnails      Upload custom thumbnail                      ║

║         Request: multipart/form-data with image file                            ║

║                                                                                   ║

║  POST   /videos/{id}/thumbnails/generate  Generate from timestamp                 ║

║         Request: { timestamp\_seconds }                                           ║

║                                                                                   ║

║  PUT    /videos/{id}/thumbnails/{thumb\_id}/set-primary  Set as primary           ║

║                                                                                   ║

║  DELETE /videos/{id}/thumbnails/{thumb\_id}  Delete thumbnail                      ║

║                                                                                   ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                           VIDEO CAPTIONS ENDPOINTS                                ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /videos/{id}/captions        List all captions for video                  ║

║                                                                                   ║

║  POST   /videos/{id}/captions        Add caption track                            ║

║         Request: multipart with { language\_code, label, file (SRT/VTT) }        ║

║                                                                                   ║

║  PUT    /videos/{id}/captions/{lang} Update caption track                         ║

║         Request: multipart with { label?, file? }                               ║

║                                                                                   ║

║  DELETE /videos/{id}/captions/{lang} Delete caption track                         ║

║                                                                                   ║

║  PUT    /videos/{id}/captions/{lang}/set-default  Set as default caption         ║

║                                                                                   ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                           VIDEO CHAPTERS ENDPOINTS                                ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /videos/{id}/chapters        List all chapters                            ║

║                                                                                   ║

║  POST   /videos/{id}/chapters        Add chapter                                  ║

║         Request: { title, start\_time, end\_time?, thumbnail\_url? }               ║

║                                                                                   ║

║  PUT    /videos/{id}/chapters/{chapter\_id}  Update chapter                        ║

║                                                                                   ║

║  DELETE /videos/{id}/chapters/{chapter\_id}  Delete chapter                        ║

║                                                                                   ║

║  PUT    /videos/{id}/chapters/reorder  Reorder chapters                           ║

║         Request: { chapter\_ids: string\[\] }                                       ║

║                                                                                   ║

║  POST   /videos/{id}/chapters/bulk   Bulk create/replace chapters                 ║

║         Request: { chapters: \[{title, start\_time, ...}\] }                       ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           COLLECTION ENDPOINTS                                    ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /collections                 List all collections (hierarchical)          ║

║         Query: parent\_id?, include\_video\_count?, flat?                           ║

║                                                                                   ║

║  GET    /collections/{id}            Get collection details                       ║

║         Query: include\_videos?, include\_children?                                ║

║                                                                                   ║

║  POST   /collections                 Create collection                            ║

║         Request: { name, description?, parent\_id?, thumbnail\_url? }             ║

║                                                                                   ║

║  PUT    /collections/{id}            Update collection                            ║

║         Request: { name?, description?, parent\_id?, thumbnail\_url? }            ║

║                                                                                   ║

║  DELETE /collections/{id}            Delete collection                            ║

║         Query: move\_videos\_to? (collection\_id to move videos, else delete)       ║

║                                                                                   ║

║  PUT    /collections/reorder         Reorder collections                          ║

║         Request: { collection\_ids: string\[\] }                                    ║

║                                                                                   ║

║  GET    /collections/{id}/videos     Get videos in collection (paginated)        ║

║         Query: page, limit, sort\_by, order                                       ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           ANALYTICS ENDPOINTS (Tenant)                            ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  NOTE: All analytics data returned has multipliers applied (tenant sees          ║

║        modified values based on super admin configuration)                        ║

║                                                                                   ║

║  GET    /analytics/overview          Dashboard overview stats                     ║

║         Query: period (7d, 30d, 90d, custom), start\_date?, end\_date?            ║

║         Response: {                                                              ║

║           total\_videos, total\_storage, total\_bandwidth,                          ║

║           total\_views, total\_watch\_time, period\_comparison {...}                 ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /analytics/usage             Usage statistics (for billing display)       ║

║         Query: period, breakdown (daily, weekly, monthly)                        ║

║         Response: {                                                              ║

║           storage\_used: { value, limit, percentage },                            ║

║           bandwidth\_used: { value, limit, percentage },                          ║

║           encoding\_minutes: { value, limit, percentage },                        ║

║           videos\_count: { value, limit, percentage },                            ║

║           daily\_breakdown: \[{ date, storage, bandwidth, encoding }\]              ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /analytics/videos            Video-level analytics (aggregated)           ║

║         Query: period, sort\_by (views, watch\_time, engagement), limit           ║

║         Response: { videos: \[{ video\_id, title, views, watch\_time, ... }\] }     ║

║                                                                                   ║

║  GET    /analytics/videos/{id}       Single video detailed analytics              ║

║         Query: period, granularity (hourly, daily)                               ║

║         Response: {                                                              ║

║           summary: { views, unique\_viewers, watch\_time, completion\_rate },       ║

║           timeline: \[{ timestamp, views, watch\_time }\],                          ║

║           geography: \[{ country, views, percentage }\],                           ║

║           devices: \[{ device\_type, views, percentage }\],                         ║

║           referrers: \[{ domain, views, percentage }\],                            ║

║           engagement: { avg\_view\_duration, drop\_off\_points: \[...\] }              ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /analytics/geography         Geographic breakdown                          ║

║         Query: period                                                            ║

║         Response: { countries: \[{ code, name, views, bandwidth }\] }             ║

║                                                                                   ║

║  GET    /analytics/devices           Device/browser breakdown                      ║

║         Query: period                                                            ║

║                                                                                   ║

║  GET    /analytics/export            Export analytics data                         ║

║         Query: format (csv, xlsx, json), period, type (usage, videos, all)      ║

║         Response: File download or { download\_url, expires\_at }                 ║

║                                                                                   ║

║  GET    /analytics/realtime          Real-time stats (last 30 minutes)            ║

║         Response: { active\_viewers, views\_last\_30m, top\_videos: \[...\] }         ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           USER MANAGEMENT ENDPOINTS                               ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /users                       List tenant users                            ║

║         Query: page, limit, status, role\_id?, search                            ║

║                                                                                   ║

║  GET    /users/{id}                  Get user details                             ║

║                                                                                   ║

║  POST   /users/invite                Invite new user                              ║

║         Request: { email, role\_id, first\_name?, last\_name? }                    ║

║                                                                                   ║

║  POST   /users/invite/bulk           Bulk invite users                            ║

║         Request: { invitations: \[{ email, role\_id }\] }                          ║

║                                                                                   ║

║  PUT    /users/{id}                  Update user                                  ║

║         Request: { first\_name?, last\_name?, status?, role\_ids? }                ║

║                                                                                   ║

║  DELETE /users/{id}                  Deactivate/remove user                       ║

║                                                                                   ║

║  POST   /users/{id}/resend-invite    Resend invitation email                      ║

║                                                                                   ║

║  PUT    /users/{id}/roles            Update user roles                            ║

║         Request: { role\_ids: string\[\] }                                          ║

║                                                                                   ║

║  POST   /users/{id}/reset-password   Force password reset                         ║

║                                                                                   ║

║  GET    /users/me                    Get current user profile                     ║

║                                                                                   ║

║  PUT    /users/me                    Update current user profile                  ║

║         Request: { first\_name?, last\_name?, avatar? }                           ║

║                                                                                   ║

║  PUT    /users/me/password           Change own password                          ║

║         Request: { current\_password, new\_password }                              ║

║                                                                                   ║

║  PUT    /users/me/2fa/enable         Enable 2FA                                   ║

║         Response: { setup\_required: true }                                       ║

║                                                                                   ║

║  PUT    /users/me/2fa/disable        Disable 2FA                                  ║

║         Request: { password }                                                    ║

║                                                                                   ║

║  GET    /users/me/sessions           List active sessions                         ║

║                                                                                   ║

║  DELETE /users/me/sessions/{id}      Revoke specific session                      ║

║                                                                                   ║

║  DELETE /users/me/sessions           Revoke all other sessions                    ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           ROLE MANAGEMENT ENDPOINTS                               ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /roles                       List all roles for tenant                    ║

║                                                                                   ║

║  GET    /roles/{id}                  Get role details with permissions            ║

║                                                                                   ║

║  POST   /roles                       Create custom role                           ║

║         Request: { name, description?, permissions: string\[\] }                  ║

║                                                                                   ║

║  PUT    /roles/{id}                  Update role                                  ║

║         Request: { name?, description?, permissions? }                          ║

║                                                                                   ║

║  DELETE /roles/{id}                  Delete role (if not system role)             ║

║         Query: transfer\_users\_to? (role\_id)                                     ║

║                                                                                   ║

║  GET    /roles/permissions           List all available permissions               ║

║         Response: { permissions: \[{ key, name, description, category }\] }       ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           TENANT SETTINGS ENDPOINTS                               ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /settings                    Get all tenant settings                      ║

║                                                                                   ║

║  PUT    /settings                    Update tenant settings                       ║

║         Request: { setting\_key: value, ... }                                    ║

║                                                                                   ║

║  GET    /settings/branding           Get branding settings                        ║

║                                                                                   ║

║  PUT    /settings/branding           Update branding                              ║

║         Request: { logo?, primary\_color?, secondary\_color?, ... }               ║

║                                                                                   ║

║  POST   /settings/branding/logo      Upload logo                                  ║

║         Request: multipart/form-data                                            ║

║                                                                                   ║

║  GET    /settings/player             Get player settings                          ║

║                                                                                   ║

║  PUT    /settings/player             Update player settings                       ║

║         Request: { logo\_url?, logo\_position?, default\_quality?, ... }           ║

║                                                                                   ║

║  GET    /settings/security           Get security settings                        ║

║                                                                                   ║

║  PUT    /settings/security           Update security settings                     ║

║         Request: { require\_2fa?, session\_timeout?, password\_policy?, ... }      ║

║                                                                                   ║

║  GET    /settings/defaults           Get default video settings                   ║

║                                                                                   ║

║  PUT    /settings/defaults           Update default video settings                ║

║         Request: { default\_visibility?, default\_geo\_blocking?, ... }            ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           API KEY ENDPOINTS                                       ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /api-keys                    List API keys                                ║

║                                                                                   ║

║  POST   /api-keys                    Create API key                               ║

║         Request: { name, permissions?, expires\_at?, allowed\_ips? }              ║

║         Response: { id, key (shown once only), prefix, ... }                    ║

║                                                                                   ║

║  GET    /api-keys/{id}               Get API key details (not the key itself)     ║

║                                                                                   ║

║  PUT    /api-keys/{id}               Update API key                               ║

║         Request: { name?, permissions?, allowed\_ips?, is\_active? }              ║

║                                                                                   ║

║  DELETE /api-keys/{id}               Revoke API key                               ║

║                                                                                   ║

║  POST   /api-keys/{id}/regenerate    Regenerate API key                           ║

║         Response: { key (new key, shown once only) }                            ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           WEBHOOK ENDPOINTS                                       ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /webhooks                    List webhook endpoints                       ║

║                                                                                   ║

║  POST   /webhooks                    Create webhook endpoint                      ║

║         Request: { url, events: string\[\], secret? }                             ║

║         Response: { id, signing\_secret (generated if not provided) }            ║

║                                                                                   ║

║  GET    /webhooks/{id}               Get webhook details                          ║

║                                                                                   ║

║  PUT    /webhooks/{id}               Update webhook                               ║

║         Request: { url?, events?, is\_active? }                                  ║

║                                                                                   ║

║  DELETE /webhooks/{id}               Delete webhook                               ║

║                                                                                   ║

║  POST   /webhooks/{id}/test          Send test webhook                            ║

║         Request: { event\_type }                                                  ║

║                                                                                   ║

║  GET    /webhooks/{id}/deliveries    Get delivery history                         ║

║         Query: page, limit, status                                              ║

║                                                                                   ║

║  POST   /webhooks/{id}/deliveries/{delivery\_id}/retry  Retry failed delivery     ║

║                                                                                   ║

║  GET    /webhooks/events             List available webhook events                ║

║         Response: {                                                              ║

║           events: \[                                                              ║

║             { type: "video.created", description: "..." },                       ║

║             { type: "video.encoded", description: "..." },                       ║

║             { type: "video.deleted", description: "..." },                       ║

║             { type: "video.failed", description: "..." },                        ║

║             ...                                                                  ║

║           \]                                                                      ║

║         }                                                                        ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           EMBED DOMAIN ENDPOINTS                                  ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /embed-domains               List allowed embed domains                   ║

║                                                                                   ║

║  POST   /embed-domains               Add embed domain                             ║

║         Request: { domain }                                                      ║

║                                                                                   ║

║  DELETE /embed-domains/{id}          Remove embed domain                          ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           WATERMARK ENDPOINTS                                     ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /watermarks                  List watermark presets                       ║

║                                                                                   ║

║  POST   /watermarks                  Create watermark preset                      ║

║         Request: { name, type, image\_url?, text?, position, opacity, size }     ║

║                                                                                   ║

║  PUT    /watermarks/{id}             Update watermark                             ║

║                                                                                   ║

║  DELETE /watermarks/{id}             Delete watermark                             ║

║                                                                                   ║

║  PUT    /watermarks/{id}/set-default Set as default watermark                     ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           VIEWER AUTHENTICATION ENDPOINTS                         ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /viewer-tokens               List viewer access tokens                    ║

║         Query: video\_id?, status                                                ║

║                                                                                   ║

║  POST   /viewer-tokens               Create viewer access token                   ║

║         Request: {                                                               ║

║           video\_id, viewer\_identifier?,                                         ║

║           max\_views?, valid\_until?, ip\_restriction?,                            ║

║           permissions?: { can\_download: bool }                                   ║

║         }                                                                        ║

║         Response: { token, playback\_url }                                       ║

║                                                                                   ║

║  POST   /viewer-tokens/bulk          Bulk create viewer tokens                    ║

║         Request: { video\_id, tokens: \[{viewer\_identifier, ...}\] }               ║

║                                                                                   ║

║  DELETE /viewer-tokens/{id}          Revoke viewer token                          ║

║                                                                                   ║

║  DELETE /viewer-tokens/bulk          Bulk revoke tokens                           ║

║         Request: { token\_ids: string\[\] }                                         ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           VIDEO MIGRATION ENDPOINTS                               ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /migrations                  List migration jobs                          ║

║         Query: status                                                            ║

║                                                                                   ║

║  POST   /migrations                  Create migration job                         ║

║         Request: {                                                               ║

║           name?,                                                                 ║

║           source\_type: "youtube" | "vimeo" | "url\_list",                        ║

║           source\_config: {                                                       ║

║             // For youtube: { playlist\_url } or { video\_urls: \[\] }              ║

║             // For vimeo: { showcase\_url } or { video\_urls: \[\] }                ║

║             // For url\_list: { urls: \[{ url, title? }\] }                        ║

║           },                                                                     ║

║           options?: {                                                            ║

║             collection\_id?, preserve\_metadata?, auto\_start?                      ║

║           }                                                                      ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /migrations/{id}             Get migration job details                    ║

║                                                                                   ║

║  POST   /migrations/{id}/start       Start migration job                          ║

║                                                                                   ║

║  POST   /migrations/{id}/pause       Pause migration job                          ║

║                                                                                   ║

║  POST   /migrations/{id}/resume      Resume migration job                         ║

║                                                                                   ║

║  POST   /migrations/{id}/cancel      Cancel migration job                         ║

║                                                                                   ║

║  GET    /migrations/{id}/items       Get migration items                          ║

║         Query: status, page, limit                                              ║

║                                                                                   ║

║  POST   /migrations/{id}/items/{item\_id}/retry  Retry failed item                ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           SUPPORT TICKET ENDPOINTS (Tenant)                       ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /support/tickets             List tickets for tenant                      ║

║         Query: status, page, limit                                              ║

║                                                                                   ║

║  POST   /support/tickets             Create new ticket                            ║

║         Request: { subject, description, category?, priority?, attachments? }   ║

║                                                                                   ║

║  GET    /support/tickets/{id}        Get ticket details                           ║

║                                                                                   ║

║  POST   /support/tickets/{id}/messages  Add message to ticket                     ║

║         Request: { message, attachments? }                                       ║

║                                                                                   ║

║  PUT    /support/tickets/{id}/close  Close ticket                                 ║

║                                                                                   ║

║  PUT    /support/tickets/{id}/reopen Reopen ticket                                ║

║                                                                                   ║

║  POST   /support/tickets/{id}/rate   Rate ticket resolution                       ║

║         Request: { rating: 1-5, feedback? }                                     ║

║                                                                                   ║

║  GET    /support/knowledge-base      Get knowledge base articles                  ║

║         Query: category\_id?, search                                             ║

║                                                                                   ║

║  GET    /support/knowledge-base/categories  Get categories                        ║

║                                                                                   ║

║  GET    /support/knowledge-base/{slug}  Get article by slug                       ║

║                                                                                   ║

║  POST   /support/knowledge-base/{id}/feedback  Submit article feedback            ║

║         Request: { helpful: boolean, comment? }                                  ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           BILLING ENDPOINTS (Tenant)                              ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /billing                     Get current billing overview                 ║

║         Response: {                                                              ║

║           subscription: {...},                                                   ║

║           current\_usage: {...},                                                  ║

║           estimated\_overage: {...},                                              ║

║           next\_invoice\_date                                                      ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /billing/subscription        Get subscription details                     ║

║                                                                                   ║

║  GET    /billing/plans               List available plans                         ║

║                                                                                   ║

║  POST   /billing/subscription/change Request plan change                          ║

║         Request: { plan\_id, billing\_cycle }                                     ║

║         Response: { checkout\_url } or immediate change if downgrade             ║

║                                                                                   ║

║  POST   /billing/subscription/cancel Request cancellation                         ║

║         Request: { reason?, feedback? }                                         ║

║                                                                                   ║

║  POST   /billing/subscription/reactivate  Reactivate cancelled subscription       ║

║                                                                                   ║

║  GET    /billing/invoices            List invoices                                ║

║         Query: status, page, limit                                              ║

║                                                                                   ║

║  GET    /billing/invoices/{id}       Get invoice details                          ║

║                                                                                   ║

║  GET    /billing/invoices/{id}/pdf   Download invoice PDF                         ║

║                                                                                   ║

║  GET    /billing/payment-methods     List payment methods                         ║

║                                                                                   ║

║  POST   /billing/payment-methods     Add payment method                           ║

║         Response: { setup\_intent\_client\_secret }                                ║

║                                                                                   ║

║  DELETE /billing/payment-methods/{id}  Remove payment method                      ║

║                                                                                   ║

║  PUT    /billing/payment-methods/{id}/set-default  Set default payment method    ║

║                                                                                   ║

║  GET    /billing/portal              Get Stripe customer portal URL               ║

║         Response: { portal\_url }                                                ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           PUBLIC/EMBED ENDPOINTS (No Auth)                        ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  GET    /embed/{video\_id}            Get embed player data                        ║

║         Query: token? (for protected videos)                                    ║

║         Headers: Referer (for domain validation)                                ║

║         Response: { player\_config, video\_info, playback\_urls }                  ║

║                                                                                   ║

║  POST   /embed/{video\_id}/auth       Authenticate viewer for protected video     ║

║         Request: { password } or { token }                                      ║

║         Response: { playback\_urls, session\_token }                              ║

║                                                                                   ║

║  POST   /embed/{video\_id}/analytics  Track view event                             ║

║         Request: { event\_type, timestamp, duration?, ... }                      ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           INTERNAL WEBHOOK RECEIVERS                              ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  POST   /webhooks/bunny              Receive Bunny.net webhooks                   ║

║         (Encoding status, etc.)                                                  ║

║                                                                                   ║

║  POST   /webhooks/stripe             Receive Stripe webhooks                      ║

║         (Payment success, subscription changes, etc.)                            ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

## 3.3 Super Admin API Endpoints

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           SUPER ADMIN ENDPOINTS                                   ║

║                    Base URL: /admin                                              ║

║                    Requires: Super Admin authentication                           ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           TENANT MANAGEMENT                                       ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/tenants               List all tenants                             ║

║         Query: status, plan\_id, search, page, limit, sort\_by                    ║

║                                                                                   ║

║  POST   /admin/tenants               Create tenant (manual onboarding)            ║

║         Request: {                                                               ║

║           name, slug, owner\_email, plan\_id,                                     ║

║           billing\_cycle?, payment\_method?, custom\_limits?                       ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /admin/tenants/{id}          Get tenant details (full)                    ║

║         Response: {                                                              ║

║           tenant\_info, subscription, usage (actual & displayed),                 ║

║           users\_count, videos\_count, billing\_history, audit\_summary             ║

║         }                                                                        ║

║                                                                                   ║

║  PUT    /admin/tenants/{id}          Update tenant                                ║

║         Request: { name?, status?, settings?, custom\_limits? }                  ║

║                                                                                   ║

║  DELETE /admin/tenants/{id}          Delete tenant (soft delete \+ archival)       ║

║                                                                                   ║

║  POST   /admin/tenants/{id}/suspend  Suspend tenant                               ║

║         Request: { reason, notify\_owner? }                                      ║

║                                                                                   ║

║  POST   /admin/tenants/{id}/unsuspend  Unsuspend tenant                           ║

║                                                                                   ║

║  POST   /admin/tenants/{id}/provision-bunny  Create/link Bunny.net library        ║

║         (Usually done automatically during tenant creation)                      ║

║                                                                                   ║

║  GET    /admin/tenants/{id}/users    List tenant users                            ║

║                                                                                   ║

║  GET    /admin/tenants/{id}/videos   List tenant videos                           ║

║                                                                                   ║

║  GET    /admin/tenants/{id}/usage    Get tenant usage (actual & multiplied)       ║

║         Query: period                                                            ║

║         Response: {                                                              ║

║           actual: { storage, bandwidth, encoding, views },                       ║

║           displayed: { storage, bandwidth, encoding, views },                    ║

║           multipliers\_applied: {...}                                             ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /admin/tenants/{id}/invoices List tenant invoices                         ║

║                                                                                   ║

║  GET    /admin/tenants/{id}/audit-logs  Get tenant audit logs                     ║

║         Query: action?, user\_id?, start\_date?, end\_date?, page, limit           ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           IMPERSONATION                                           ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  POST   /admin/impersonate           Start impersonation session                  ║

║         Request: { tenant\_id, user\_id }                                         ║

║         Response: { impersonation\_token, redirect\_url }                         ║

║                                                                                   ║

║  POST   /admin/impersonate/end       End impersonation session                    ║

║         Response: { original\_token, redirect\_url }                              ║

║                                                                                   ║

║  GET    /admin/impersonation-logs    Get impersonation history                    ║

║         Query: admin\_id?, tenant\_id?, page, limit                               ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           SUBSCRIPTION PLANS                                      ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/plans                 List all subscription plans                  ║

║                                                                                   ║

║  POST   /admin/plans                 Create subscription plan                     ║

║         Request: {                                                               ║

║           name, slug, description, price\_monthly, price\_yearly,                 ║

║           limits: {                                                              ║

║             storage\_gb, bandwidth\_gb\_monthly, max\_videos,                        ║

║             max\_users, max\_collections, video\_quality\_max,                       ║

║             api\_requests\_daily, custom\_roles, api\_access, ...                    ║

║           },                                                                     ║

║           features: {                                                            ║

║             watermarking, geo\_blocking, analytics\_advanced, ...                  ║

║           }                                                                      ║

║         }                                                                        ║

║                                                                                   ║

║  PUT    /admin/plans/{id}            Update plan                                  ║

║                                                                                   ║

║  DELETE /admin/plans/{id}            Deactivate plan                              ║

║         Query: migrate\_to\_plan\_id? (migrate existing subscribers)               ║

║                                                                                   ║

║  POST   /admin/plans/{id}/sync-stripe  Sync plan pricing with Stripe              ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           OVERAGE RATES                                           ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/overage-rates         List overage rates                           ║

║                                                                                   ║

║  POST   /admin/overage-rates         Create overage rate                          ║

║         Request: { plan\_id?, metric\_type, unit\_amount, unit\_size, unit\_name }   ║

║                                                                                   ║

║  PUT    /admin/overage-rates/{id}    Update overage rate                          ║

║                                                                                   ║

║  DELETE /admin/overage-rates/{id}    Delete overage rate                          ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           USAGE MULTIPLIERS                                       ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/usage-multipliers     List all multipliers                         ║

║         Query: tenant\_id? (specific tenant or global)                           ║

║                                                                                   ║

║  POST   /admin/usage-multipliers     Create multiplier                            ║

║         Request: {                                                               ║

║           tenant\_id?,              // null for global default                    ║

║           metric\_type,             // 'storage' | 'bandwidth' | etc.             ║

║           multiplier,              // e.g., 2.0 means double the value           ║

║           effective\_from?,                                                       ║

║           effective\_until?,        // null for indefinite                        ║

║           notes?                                                                 ║

║         }                                                                        ║

║                                                                                   ║

║  PUT    /admin/usage-multipliers/{id}  Update multiplier                          ║

║                                                                                   ║

║  DELETE /admin/usage-multipliers/{id}  Delete multiplier                          ║

║                                                                                   ║

║  POST   /admin/usage-multipliers/preview  Preview effect of multiplier           ║

║         Request: { tenant\_id, metric\_type, multiplier, period }                 ║

║         Response: {                                                              ║

║           current\_actual: 50GB,                                                  ║

║           current\_displayed: 50GB,                                               ║

║           new\_displayed: 100GB     // with new multiplier                        ║

║         }                                                                        ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           BILLING MANAGEMENT                                      ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/billing/invoices      List all invoices (all tenants)              ║

║         Query: tenant\_id?, status?, start\_date?, end\_date?, page, limit         ║

║                                                                                   ║

║  GET    /admin/billing/invoices/{id} Get invoice details                          ║

║                                                                                   ║

║  POST   /admin/billing/invoices      Create manual invoice                        ║

║         Request: { tenant\_id, line\_items, due\_date, notes? }                    ║

║                                                                                   ║

║  PUT    /admin/billing/invoices/{id}/status  Update invoice status                ║

║         Request: { status, notes? }                                             ║

║                                                                                   ║

║  POST   /admin/billing/manual-payment  Record manual payment                      ║

║         Request: {                                                               ║

║           tenant\_id, invoice\_id?, amount, currency,                              ║

║           payment\_method, reference\_number, payment\_date,                        ║

║           notes?, proof\_document?                                                ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /admin/billing/manual-payments  List manual payments                      ║

║         Query: tenant\_id?, page, limit                                          ║

║                                                                                   ║

║  POST   /admin/billing/credits       Issue credit to tenant                       ║

║         Request: { tenant\_id, amount, reason }                                  ║

║                                                                                   ║

║  POST   /admin/billing/subscription/{tenant\_id}/change  Change subscription       ║

║         Request: { plan\_id, billing\_cycle?, prorate?, notify\_tenant? }          ║

║                                                                                   ║

║  POST   /admin/billing/subscription/{tenant\_id}/extend  Extend subscription       ║

║         Request: { days, reason }                                               ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           PLATFORM ANALYTICS                                      ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/analytics/dashboard   Platform dashboard stats                     ║

║         Response: {                                                              ║

║           tenants: { total, active, trial, churned\_this\_month },                 ║

║           revenue: { mrr, arr, this\_month, growth\_percent },                     ║

║           usage: { total\_storage, total\_bandwidth, total\_videos },               ║

║           support: { open\_tickets, avg\_response\_time }                           ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /admin/analytics/revenue     Revenue analytics                            ║

║         Query: period, breakdown (daily, weekly, monthly)                        ║

║         Response: {                                                              ║

║           total\_revenue, subscription\_revenue, overage\_revenue,                  ║

║           timeline: \[{ date, amount }\],                                         ║

║           by\_plan: \[{ plan, revenue, subscribers }\],                            ║

║           churn\_rate, ltv\_average                                                ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /admin/analytics/usage       Platform usage analytics                     ║

║         Query: period                                                            ║

║         Response: {                                                              ║

║           total\_storage\_gb, total\_bandwidth\_gb, total\_videos,                    ║

║           total\_encoding\_minutes, active\_viewers,                                ║

║           by\_tenant: \[{ tenant, storage, bandwidth }\]  // top consumers         ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /admin/analytics/tenants     Tenant growth analytics                      ║

║         Query: period                                                            ║

║         Response: {                                                              ║

║           signups: \[{ date, count }\],                                           ║

║           churn: \[{ date, count }\],                                             ║

║           conversion\_rate, trial\_to\_paid\_rate                                    ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /admin/analytics/bunny       Bunny.net account usage                      ║

║         Response: {                                                              ║

║           total\_storage, total\_bandwidth, total\_encoding,                        ║

║           cost\_estimate, libraries\_count                                         ║

║         }                                                                        ║

║                                                                                   ║

║  GET    /admin/analytics/export      Export platform analytics                    ║

║         Query: type, format, period                                             ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           SUPPORT MANAGEMENT                                      ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/support/tickets       List all tickets                             ║

║         Query: status, priority, department\_id, assigned\_to,                    ║

║                tenant\_id?, page, limit, sort\_by                                  ║

║                                                                                   ║

║  GET    /admin/support/tickets/{id}  Get ticket details                           ║

║                                                                                   ║

║  PUT    /admin/support/tickets/{id}  Update ticket                                ║

║         Request: { status?, priority?, assigned\_to?, department\_id? }           ║

║                                                                                   ║

║  POST   /admin/support/tickets/{id}/assign  Assign ticket                         ║

║         Request: { assigned\_to }                                                ║

║                                                                                   ║

║  POST   /admin/support/tickets/{id}/escalate  Escalate ticket                     ║

║         Request: { reason, escalate\_to? }                                       ║

║                                                                                   ║

║  POST   /admin/support/tickets/{id}/messages  Add message (reply)                 ║

║         Request: { message, is\_internal?, attachments? }                        ║

║                                                                                   ║

║  POST   /admin/support/tickets/{id}/merge  Merge tickets                          ║

║         Request: { merge\_with\_ticket\_id }                                       ║

║                                                                                   ║

║  GET    /admin/support/departments   List departments                             ║

║                                                                                   ║

║  POST   /admin/support/departments   Create department                            ║

║                                                                                   ║

║  PUT    /admin/support/departments/{id}  Update department                        ║

║                                                                                   ║

║  DELETE /admin/support/departments/{id}  Delete department                        ║

║                                                                                   ║

║  GET    /admin/support/canned-responses  List canned responses                    ║

║                                                                                   ║

║  POST   /admin/support/canned-responses  Create canned response                   ║

║                                                                                   ║

║  PUT    /admin/support/canned-responses/{id}  Update canned response              ║

║                                                                                   ║

║  DELETE /admin/support/canned-responses/{id}  Delete canned response              ║

║                                                                                   ║

║  GET    /admin/support/escalation-rules  List escalation rules                    ║

║                                                                                   ║

║  POST   /admin/support/escalation-rules  Create escalation rule                   ║

║                                                                                   ║

║  PUT    /admin/support/escalation-rules/{id}  Update escalation rule              ║

║                                                                                   ║

║  DELETE /admin/support/escalation-rules/{id}  Delete escalation rule              ║

║                                                                                   ║

║  GET    /admin/support/analytics     Support analytics                            ║

║         Query: period                                                            ║

║         Response: {                                                              ║

║           total\_tickets, avg\_response\_time, avg\_resolution\_time,                 ║

║           satisfaction\_score, by\_department, by\_category,                        ║

║           agent\_performance: \[...\]                                               ║

║         }                                                                        ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           KNOWLEDGE BASE MANAGEMENT                               ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/knowledge-base/categories  List categories                         ║

║                                                                                   ║

║  POST   /admin/knowledge-base/categories  Create category                         ║

║                                                                                   ║

║  PUT    /admin/knowledge-base/categories/{id}  Update category                    ║

║                                                                                   ║

║  DELETE /admin/knowledge-base/categories/{id}  Delete category                    ║

║                                                                                   ║

║  GET    /admin/knowledge-base/articles  List articles                             ║

║         Query: category\_id?, status?, search                                    ║

║                                                                                   ║

║  POST   /admin/knowledge-base/articles  Create article                            ║

║                                                                                   ║

║  PUT    /admin/knowledge-base/articles/{id}  Update article                       ║

║                                                                                   ║

║  DELETE /admin/knowledge-base/articles/{id}  Delete article                       ║

║                                                                                   ║

║  POST   /admin/knowledge-base/articles/{id}/publish  Publish article              ║

║                                                                                   ║

║  POST   /admin/knowledge-base/articles/{id}/unpublish  Unpublish article          ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           PLATFORM SETTINGS                                       ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/settings              Get all platform settings                    ║

║                                                                                   ║

║  PUT    /admin/settings              Update settings                              ║

║         Request: { key: value, ... }                                            ║

║                                                                                   ║

║  GET    /admin/settings/bunny        Get Bunny.net configuration                  ║

║                                                                                   ║

║  PUT    /admin/settings/bunny        Update Bunny.net configuration               ║

║         Request: { api\_key, pull\_zone\_url, webhook\_secret, ... }                ║

║                                                                                   ║

║  POST   /admin/settings/bunny/test   Test Bunny.net connection                    ║

║                                                                                   ║

║  GET    /admin/settings/stripe       Get Stripe configuration                     ║

║                                                                                   ║

║  PUT    /admin/settings/stripe       Update Stripe configuration                  ║

║         Request: { api\_key, webhook\_secret, ... }                               ║

║                                                                                   ║

║  POST   /admin/settings/stripe/test  Test Stripe connection                       ║

║                                                                                   ║

║  GET    /admin/settings/email        Get email configuration                      ║

║                                                                                   ║

║  PUT    /admin/settings/email        Update email configuration                   ║

║         Request: { smtp\_host, smtp\_port, username, password, from\_email, ... }  ║

║                                                                                   ║

║  POST   /admin/settings/email/test   Send test email                              ║

║         Request: { to\_email }                                                    ║

║                                                                                   ║

║  GET    /admin/settings/branding     Get platform branding                        ║

║                                                                                   ║

║  PUT    /admin/settings/branding     Update platform branding                     ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           SYSTEM USERS (Admin Users)                              ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/system-users          List admin users                             ║

║                                                                                   ║

║  POST   /admin/system-users          Create admin user                            ║

║         Request: { email, first\_name, last\_name, role, permissions? }           ║

║                                                                                   ║

║  GET    /admin/system-users/{id}     Get admin user details                       ║

║                                                                                   ║

║  PUT    /admin/system-users/{id}     Update admin user                            ║

║                                                                                   ║

║  DELETE /admin/system-users/{id}     Deactivate admin user                        ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           AUDIT LOGS                                              ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/audit-logs            List all audit logs                          ║

║         Query: tenant\_id?, user\_id?, action?, entity\_type?,                     ║

║                start\_date?, end\_date?, page, limit                               ║

║                                                                                   ║

║  GET    /admin/audit-logs/export     Export audit logs                            ║

║         Query: format, filters...                                               ║

║                                                                                   ║

║  GET    /admin/audit-logs/stats      Audit log statistics                         ║

║         Query: period                                                            ║

║         Response: { actions\_by\_type, actions\_by\_user, timeline }                ║

║                                                                                   ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                           EMAIL TEMPLATES                                         ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  GET    /admin/email-templates       List email templates                         ║

║                                                                                   ║

║  GET    /admin/email-templates/{id}  Get template details                         ║

║                                                                                   ║

║  PUT    /admin/email-templates/{id}  Update template                              ║

║                                                                                   ║

║  POST   /admin/email-templates/{id}/preview  Preview template                     ║

║         Request: { sample\_data }                                                ║

║         Response: { rendered\_html, rendered\_text }                              ║

║                                                                                   ║

║  POST   /admin/email-templates/{id}/test  Send test email                         ║

║         Request: { to\_email, sample\_data }                                      ║

║                                                                                   ║

║  POST   /admin/email-templates/{id}/reset  Reset to default                       ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

---

# PART 4: FRONTEND STRUCTURE

## 4.1 Next.js Project Structure

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         NEXT.JS PROJECT STRUCTURE                                │

│                                                                                  │

│  /streamvault-frontend                                                          │

│  │                                                                               │

│  ├── /app                              \# App Router (Next.js 13+)               │

│  │   │                                                                           │

│  │   ├── layout.tsx                    \# Root layout                            │

│  │   ├── page.tsx                      \# Landing page (marketing)               │

│  │   ├── globals.css                   \# Global styles                          │

│  │   │                                                                           │

│  │   ├── /(auth)                       \# Auth group (no layout sidebar)         │

│  │   │   ├── layout.tsx                \# Clean auth layout                      │

│  │   │   ├── /login                                                              │

│  │   │   │   └── page.tsx                                                        │

│  │   │   ├── /register                                                           │

│  │   │   │   └── page.tsx                                                        │

│  │   │   ├── /forgot-password                                                    │

│  │   │   │   └── page.tsx                                                        │

│  │   │   ├── /reset-password                                                     │

│  │   │   │   └── page.tsx                                                        │

│  │   │   ├── /verify-email                                                       │

│  │   │   │   └── page.tsx                                                        │

│  │   │   ├── /accept-invite                                                      │

│  │   │   │   └── page.tsx                                                        │

│  │   │   └── /2fa                                                                │

│  │   │       └── page.tsx                                                        │

│  │   │                                                                           │

│  │   ├── /(marketing)                  \# Marketing/public pages                 │

│  │   │   ├── layout.tsx                                                          │

│  │   │   ├── /pricing                                                            │

│  │   │   │   └── page.tsx                                                        │

│  │   │   ├── /features                                                           │

│  │   │   │   └── page.tsx                                                        │

│  │   │   ├── /about                                                              │

│  │   │   │   └── page.tsx                                                        │

│  │   │   ├── /contact                                                            │

│  │   │   │   └── page.tsx                                                        │

│  │   │   └── /terms                                                              │

│  │   │       └── page.tsx                                                        │

│  │   │                                                                           │

│  │   ├── /(dashboard)                  \# Tenant dashboard                       │

│  │   │   ├── layout.tsx                \# Dashboard layout with sidebar          │

│  │   │   │                                                                       │

│  │   │   ├── /dashboard                \# Main dashboard                         │

│  │   │   │   └── page.tsx                                                        │

│  │   │   │                                                                       │

│  │   │   ├── /videos                   \# Video management                       │

│  │   │   │   ├── page.tsx              \# Video list                             │

│  │   │   │   ├── /upload                                                         │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   ├── /\[id\]                                                           │

│  │   │   │   │   ├── page.tsx          \# Video details                          │

│  │   │   │   │   ├── /edit                                                       │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   ├── /analytics                                                  │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   ├── /chapters                                                   │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   └── /captions                                                   │

│  │   │   │   │       └── page.tsx                                                │

│  │   │   │   └── /migrate                                                        │

│  │   │   │       └── page.tsx                                                    │

│  │   │   │                                                                       │

│  │   │   ├── /collections              \# Collection management                  │

│  │   │   │   ├── page.tsx                                                        │

│  │   │   │   └── /\[id\]                                                           │

│  │   │   │       └── page.tsx                                                    │

│  │   │   │                                                                       │

│  │   │   ├── /analytics                \# Analytics section                      │

│  │   │   │   ├── page.tsx              \# Overview                               │

│  │   │   │   ├── /usage                                                          │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   ├── /videos                                                         │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   ├── /geography                                                      │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   └── /export                                                         │

│  │   │   │       └── page.tsx                                                    │

│  │   │   │                                                                       │

│  │   │   ├── /team                     \# Team/user management                   │

│  │   │   │   ├── page.tsx              \# User list                              │

│  │   │   │   ├── /invite                                                         │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   ├── /\[id\]                                                           │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   └── /roles                                                          │

│  │   │   │       ├── page.tsx                                                    │

│  │   │   │       └── /\[id\]                                                       │

│  │   │   │           └── page.tsx                                                │

│  │   │   │                                                                       │

│  │   │   ├── /settings                 \# Tenant settings                        │

│  │   │   │   ├── page.tsx              \# General settings                       │

│  │   │   │   ├── /branding                                                       │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   ├── /player                                                         │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   ├── /security                                                       │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   ├── /api-keys                                                       │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   ├── /webhooks                                                       │

│  │   │   │   │   ├── page.tsx                                                    │

│  │   │   │   │   └── /\[id\]                                                       │

│  │   │   │   │       └── page.tsx                                                │

│  │   │   │   ├── /embed-domains                                                  │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   └── /watermarks                                                     │

│  │   │   │       └── page.tsx                                                    │

│  │   │   │                                                                       │

│  │   │   ├── /billing                  \# Billing section                        │

│  │   │   │   ├── page.tsx              \# Overview                               │

│  │   │   │   ├── /plans                                                          │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   ├── /invoices                                                       │

│  │   │   │   │   ├── page.tsx                                                    │

│  │   │   │   │   └── /\[id\]                                                       │

│  │   │   │   │       └── page.tsx                                                │

│  │   │   │   └── /payment-methods                                                │

│  │   │   │       └── page.tsx                                                    │

│  │   │   │                                                                       │

│  │   │   ├── /support                  \# Support section                        │

│  │   │   │   ├── page.tsx              \# Ticket list                            │

│  │   │   │   ├── /new                                                            │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   ├── /\[id\]                                                           │

│  │   │   │   │   └── page.tsx          \# Ticket details                         │

│  │   │   │   └── /knowledge-base                                                 │

│  │   │   │       ├── page.tsx                                                    │

│  │   │   │       └── /\[slug\]                                                     │

│  │   │   │           └── page.tsx                                                │

│  │   │   │                                                                       │

│  │   │   └── /profile                  \# User profile                           │

│  │   │       ├── page.tsx                                                        │

│  │   │       └── /security                                                       │

│  │   │           └── page.tsx                                                    │

│  │   │                                                                           │

│  │   ├── /(admin)                      \# Super Admin dashboard                  │

│  │   │   ├── layout.tsx                \# Admin layout (different sidebar)       │

│  │   │   │                                                                       │

│  │   │   ├── /admin                                                              │

│  │   │   │   ├── page.tsx              \# Admin dashboard                        │

│  │   │   │   │                                                                   │

│  │   │   │   ├── /tenants                                                        │

│  │   │   │   │   ├── page.tsx          \# Tenant list                            │

│  │   │   │   │   ├── /new                                                        │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   └── /\[id\]                                                       │

│  │   │   │   │       ├── page.tsx      \# Tenant details                         │

│  │   │   │   │       ├── /users                                                  │

│  │   │   │   │       │   └── page.tsx                                            │

│  │   │   │   │       ├── /videos                                                 │

│  │   │   │   │       │   └── page.tsx                                            │

│  │   │   │   │       ├── /usage                                                  │

│  │   │   │   │       │   └── page.tsx                                            │

│  │   │   │   │       ├── /billing                                                │

│  │   │   │   │       │   └── page.tsx                                            │

│  │   │   │   │       └── /audit-logs                                             │

│  │   │   │   │           └── page.tsx                                            │

│  │   │   │   │                                                                   │

│  │   │   │   ├── /plans                                                          │

│  │   │   │   │   ├── page.tsx                                                    │

│  │   │   │   │   ├── /new                                                        │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   └── /\[id\]                                                       │

│  │   │   │   │       └── page.tsx                                                │

│  │   │   │   │                                                                   │

│  │   │   │   ├── /usage-multipliers                                              │

│  │   │   │   │   └── page.tsx                                                    │

│  │   │   │   │                                                                   │

│  │   │   │   ├── /billing                                                        │

│  │   │   │   │   ├── page.tsx          \# All invoices                           │

│  │   │   │   │   ├── /manual-payments                                            │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   └── /overage-rates                                              │

│  │   │   │   │       └── page.tsx                                                │

│  │   │   │   │                                                                   │

│  │   │   │   ├── /analytics                                                      │

│  │   │   │   │   ├── page.tsx          \# Platform analytics                     │

│  │   │   │   │   ├── /revenue                                                    │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   ├── /usage                                                      │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   └── /tenants                                                    │

│  │   │   │   │       └── page.tsx                                                │

│  │   │   │   │                                                                   │

│  │   │   │   ├── /support                                                        │

│  │   │   │   │   ├── page.tsx          \# All tickets                            │

│  │   │   │   │   ├── /\[id\]                                                       │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   ├── /departments                                                │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   ├── /canned-responses                                           │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   ├── /escalation-rules                                           │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   └── /analytics                                                  │

│  │   │   │   │       └── page.tsx                                                │

│  │   │   │   │                                                                   │

│  │   │   │   ├── /knowledge-base                                                 │

│  │   │   │   │   ├── page.tsx                                                    │

│  │   │   │   │   ├── /categories                                                 │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   └── /articles                                                   │

│  │   │   │   │       ├── page.tsx                                                │

│  │   │   │   │       ├── /new                                                    │

│  │   │   │   │       │   └── page.tsx                                            │

│  │   │   │   │       └── /\[id\]                                                   │

│  │   │   │   │           └── page.tsx                                            │

│  │   │   │   │                                                                   │

│  │   │   │   ├── /settings                                                       │

│  │   │   │   │   ├── page.tsx          \# Platform settings                      │

│  │   │   │   │   ├── /bunny                                                      │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   ├── /stripe                                                     │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   ├── /email                                                      │

│  │   │   │   │   │   └── page.tsx                                                │

│  │   │   │   │   ├── /email-templates                                            │

│  │   │   │   │   │   ├── page.tsx                                                │

│  │   │   │   │   │   └── /\[id\]                                                   │

│  │   │   │   │   │       └── page.tsx                                            │

│  │   │   │   │   └── /branding                                                   │

│  │   │   │   │       └── page.tsx                                                │

│  │   │   │   │                                                                   │

│  │   │   │   ├── /system-users                                                   │

│  │   │   │   │   ├── page.tsx                                                    │

│  │   │   │   │   └── /\[id\]                                                       │

│  │   │   │   │       └── page.tsx                                                │

│  │   │   │   │                                                                   │

│  │   │   │   └── /audit-logs                                                     │

│  │   │   │       └── page.tsx                                                    │

│  │   │   │                                                                       │

│  │   │   └── /admin/login              \# Separate admin login                   │

│  │   │       └── page.tsx                                                        │

│  │   │                                                                           │

│  │   └── /embed                        \# Video embed player                     │

│  │       └── /\[videoId\]                                                          │

│  │           └── page.tsx              \# Embeddable player page                 │

│  │                                                                               │

│  ├── /components                       \# Reusable components                    │

│  │   ├── /ui                           \# Base UI components (shadcn/ui)         │

│  │   │   ├── button.tsx                                                          │

│  │   │   ├── input.tsx                                                           │

│  │   │   ├── card.tsx                                                            │

│  │   │   ├── dialog.tsx                                                          │

│  │   │   ├── dropdown-menu.tsx                                                   │

│  │   │   ├── table.tsx                                                           │

│  │   │   ├── tabs.tsx                                                            │

│  │   │   ├── toast.tsx                                                           │

│  │   │   ├── select.tsx                                                          │

│  │   │   ├── checkbox.tsx                                                        │

│  │   │   ├── badge.tsx                                                           │

│  │   │   ├── avatar.tsx                                                          │

│  │   │   ├── progress.tsx                                                        │

│  │   │   ├── skeleton.tsx                                                        │

│  │   │   └── ... (other shadcn components)                                       │

│  │   │                                                                           │

│  │   ├── /layout                       \# Layout components                      │

│  │   │   ├── Header.tsx                                                          │

│  │   │   ├── Sidebar.tsx                                                         │

│  │   │   ├── AdminSidebar.tsx                                                    │

│  │   │   ├── Footer.tsx                                                          │

│  │   │   ├── Breadcrumb.tsx                                                      │

│  │   │   ├── PageHeader.tsx                                                      │

│  │   │   └── MobileNav.tsx                                                       │

│  │   │                                                                           │

│  │   ├── /auth                         \# Auth components                        │

│  │   │   ├── LoginForm.tsx                                                       │

│  │   │   ├── RegisterForm.tsx                                                    │

│  │   │   ├── ForgotPasswordForm.tsx                                              │

│  │   │   ├── ResetPasswordForm.tsx                                               │

│  │   │   ├── TwoFactorForm.tsx                                                   │

│  │   │   └── SocialAuthButtons.tsx                                               │

│  │   │                                                                           │

│  │   ├── /videos                       \# Video-related components               │

│  │   │   ├── VideoCard.tsx                                                       │

│  │   │   ├── VideoGrid.tsx                                                       │

│  │   │   ├── VideoList.tsx                                                       │

│  │   │   ├── VideoUploader.tsx         \# TUS upload component                   │

│  │   │   ├── VideoPlayer.tsx           \# Custom video player                    │

│  │   │   ├── VideoDetails.tsx                                                    │

│  │   │   ├── VideoEditForm.tsx                                                   │

│  │   │   ├── VideoFilters.tsx                                                    │

│  │   │   ├── VideoStatusBadge.tsx                                                │

│  │   │   ├── ChapterEditor.tsx                                                   │

│  │   │   ├── CaptionManager.tsx                                                  │

│  │   │   ├── ThumbnailSelector.tsx                                               │

│  │   │   ├── EmbedCodeGenerator.tsx                                              │

│  │   │   ├── BulkActionsToolbar.tsx                                              │

│  │   │   └── MigrationWizard.tsx                                                 │

│  │   │                                                                           │

│  │   ├── /collections                  \# Collection components                  │

│  │   │   ├── CollectionTree.tsx                                                  │

│  │   │   ├── CollectionCard.tsx                                                  │

│  │   │   ├── CollectionForm.tsx                                                  │

│  │   │   └── CollectionSelector.tsx                                              │

│  │   │                                                                           │

│  │   ├── /analytics                    \# Analytics components                   │

│  │   │   ├── StatsCard.tsx                                                       │

│  │   │   ├── UsageChart.tsx                                                      │

│  │   │   ├── ViewsChart.tsx                                                      │

│  │   │   ├── GeographyMap.tsx                                                    │

│  │   │   ├── DeviceBreakdown.tsx                                                 │

│  │   │   ├── TopVideosTable.tsx                                                  │

│  │   │   ├── EngagementChart.tsx                                                 │

│  │   │   ├── DateRangePicker.tsx                                                 │

│  │   │   └── ExportButton.tsx                                                    │

│  │   │                                                                           │

│  │   ├── /billing                      \# Billing components                     │

│  │   │   ├── PlanCard.tsx                                                        │

│  │   │   ├── PlanComparison.tsx                                                  │

│  │   │   ├── SubscriptionStatus.tsx                                              │

│  │   │   ├── UsageMeter.tsx                                                      │

│  │   │   ├── InvoiceList.tsx                                                     │

│  │   │   ├── InvoiceDetails.tsx                                                  │

│  │   │   ├── PaymentMethodCard.tsx                                               │

│  │   │   └── StripeElements.tsx                                                  │

│  │   │                                                                           │

│  │   ├── /support                      \# Support components                     │

│  │   │   ├── TicketList.tsx                                                      │

│  │   │   ├── TicketCard.tsx                                                      │

│  │   │   ├── TicketForm.tsx                                                      │

│  │   │   ├── TicketConversation.tsx                                              │

│  │   │   ├── TicketStatusBadge.tsx                                               │

│  │   │   ├── KnowledgeBaseSearch.tsx                                             │

│  │   │   ├── ArticleCard.tsx                                                     │

│  │   │   └── ArticleContent.tsx                                                  │

│  │   │                                                                           │

│  │   ├── /users                        \# User management components             │

│  │   │   ├── UserList.tsx                                                        │

│  │   │   ├── UserCard.tsx                                                        │

│  │   │   ├── UserInviteForm.tsx                                                  │

│  │   │   ├── RoleSelector.tsx                                                    │

│  │   │   ├── RolePermissionMatrix.tsx                                            │

│  │   │   └── SessionList.tsx                                                     │

│  │   │                                                                           │

│  │   ├── /settings                     \# Settings components                    │

│  │   │   ├── BrandingForm.tsx                                                    │

│  │   │   ├── PlayerSettingsForm.tsx                                              │

│  │   │   ├── SecuritySettingsForm.tsx                                            │

│  │   │   ├── ApiKeyManager.tsx                                                   │

│  │   │   ├── WebhookManager.tsx                                                  │

│  │   │   ├── EmbedDomainManager.tsx                                              │

│  │   │   └── WatermarkManager.tsx                                                │

│  │   │                                                                           │

│  │   ├── /admin                        \# Admin-specific components              │

│  │   │   ├── TenantList.tsx                                                      │

│  │   │   ├── TenantDetails.tsx                                                   │

│  │   │   ├── TenantForm.tsx                                                      │

│  │   │   ├── UsageMultiplierForm.tsx                                             │

│  │   │   ├── ManualPaymentForm.tsx                                               │

│  │   │   ├── PlanForm.tsx                                                        │

│  │   │   ├── ImpersonationBanner.tsx                                             │

│  │   │   ├── PlatformStats.tsx                                                   │

│  │   │   ├── RevenueChart.tsx                                                    │

│  │   │   ├── TenantGrowthChart.tsx                                               │

│  │   │   ├── SystemUserManager.tsx                                               │

│  │   │   ├── AuditLogViewer.tsx                                                  │

│  │   │   ├── EmailTemplateEditor.tsx                                             │

│  │   │   └── EscalationRuleForm.tsx                                              │

│  │   │                                                                           │

│  │   └── /common                       \# Common components                      │

│  │       ├── DataTable.tsx             \# Generic data table with sorting/filter │

│  │       ├── Pagination.tsx                                                      │

│  │       ├── SearchInput.tsx                                                     │

│  │       ├── FileUpload.tsx                                                      │

│  │       ├── ImageUpload.tsx                                                     │

│  │       ├── ConfirmDialog.tsx                                                   │

│  │       ├── EmptyState.tsx                                                      │

│  │       ├── LoadingSpinner.tsx                                                  │

│  │       ├── ErrorBoundary.tsx                                                   │

│  │       ├── NotificationBell.tsx                                                │

│  │       ├── UserMenu.tsx                                                        │

│  │       ├── ThemeToggle.tsx                                                     │

│  │       ├── CopyButton.tsx                                                      │

│  │       ├── JsonViewer.tsx                                                      │

│  │       └── MarkdownRenderer.tsx                                                │

│  │                                                                               │

│  ├── /lib                              \# Utility libraries                      │

│  │   ├── /api                          \# API client                             │

│  │   │   ├── client.ts                 \# Axios/fetch wrapper                    │

│  │   │   ├── auth.ts                   \# Auth API calls                         │

│  │   │   ├── videos.ts                 \# Video API calls                        │

│  │   │   ├── collections.ts                                                      │

│  │   │   ├── analytics.ts                                                        │

│  │   │   ├── users.ts                                                            │

│  │   │   ├── billing.ts                                                          │

│  │   │   ├── support.ts                                                          │

│  │   │   ├── settings.ts                                                         │

│  │   │   ├── webhooks.ts                                                         │

│  │   │   ├── admin/                    \# Admin API calls                        │

│  │   │   │   ├── tenants.ts                                                      │

│  │   │   │   ├── plans.ts                                                        │

│  │   │   │   ├── multipliers.ts                                                  │

│  │   │   │   └── ...                                                             │

│  │   │   └── types.ts                  \# API types                              │

│  │   │                                                                           │

│  │   ├── utils.ts                      \# General utilities                      │

│  │   ├── date.ts                       \# Date formatting                        │

│  │   ├── format.ts                     \# Number/byte formatting                 │

│  │   ├── validation.ts                 \# Form validation schemas (Zod)          │

│  │   └── constants.ts                  \# App constants                          │

│  │                                                                               │

│  ├── /hooks                            \# Custom React hooks                     │

│  │   ├── useAuth.ts                    \# Authentication hook                    │

│  │   ├── useUser.ts                    \# Current user hook                      │

│  │   ├── useTenant.ts                  \# Current tenant hook                    │

│  │   ├── usePermissions.ts             \# RBAC permission hook                   │

│  │   ├── useVideos.ts                  \# Videos data hook                       │

│  │   ├── useCollections.ts                                                       │

│  │   ├── useAnalytics.ts                                                         │

│  │   ├── useBilling.ts                                                           │

│  │   ├── useSupport.ts                                                           │

│  │   ├── useUpload.ts                  \# TUS upload hook                        │

│  │   ├── useDebounce.ts                                                          │

│  │   ├── useInfiniteScroll.ts                                                    │

│  │   ├── useLocalStorage.ts                                                      │

│  │   ├── useMediaQuery.ts                                                        │

│  │   └── useToast.ts                                                             │

│  │                                                                               │

│  ├── /store                            \# State management (Zustand)             │

│  │   ├── authStore.ts                  \# Auth state                             │

│  │   ├── uiStore.ts                    \# UI state (sidebar, theme)              │

│  │   ├── videoStore.ts                 \# Video list state                       │

│  │   ├── uploadStore.ts                \# Upload queue state                     │

│  │   └── notificationStore.ts          \# Notifications state                    │

│  │                                                                               │

│  ├── /providers                        \# React context providers                │

│  │   ├── AuthProvider.tsx                                                        │

│  │   ├── TenantProvider.tsx                                                      │

│  │   ├── ThemeProvider.tsx                                                       │

│  │   ├── ToastProvider.tsx                                                       │

│  │   └── QueryProvider.tsx             \# React Query provider                   │

│  │                                                                               │

│  ├── /styles                           \# Stylesheets                            │

│  │   ├── globals.css                                                             │

│  │   └── themes/                                                                 │

│  │       ├── default.css                                                         │

│  │       └── admin.css                                                           │

│  │                                                                               │

│  ├── /types                            \# TypeScript types                       │

│  │   ├── auth.ts                                                                 │

│  │   ├── video.ts                                                                │

│  │   ├── user.ts                                                                 │

│  │   ├── tenant.ts                                                               │

│  │   ├── billing.ts                                                              │

│  │   ├── analytics.ts                                                            │

│  │   ├── support.ts                                                              │

│  │   └── api.ts                        \# API response types                     │

│  │                                                                               │

│  ├── /config                           \# Configuration                          │

│  │   ├── site.ts                       \# Site metadata                          │

│  │   ├── navigation.ts                 \# Navigation config                      │

│  │   └── permissions.ts                \# Permission definitions                 │

│  │                                                                               │

│  ├── middleware.ts                     \# Next.js middleware (auth check)        │

│  ├── next.config.js                                                              │

│  ├── tailwind.config.ts                                                          │

│  ├── tsconfig.json                                                               │

│  ├── package.json                                                                │

│  └── .env.local                                                                  │

│                                                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

## 4.2 UI Component Library

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         RECOMMENDED UI STACK (ALL OPEN SOURCE)                   │

├─────────────────────────────────────────────────────────────────────────────────┤

│                                                                                  │

│  Base Components:                                                               │

│  ────────────────                                                               │

│  • shadcn/ui (https://ui.shadcn.com/) \- Component library (copy-paste)          │

│  • Radix UI \- Unstyled accessible components (used by shadcn)                   │

│  • Tailwind CSS \- Utility-first CSS framework                                   │

│                                                                                  │

│  Charts & Visualization:                                                        │

│  ───────────────────────                                                        │

│  • Recharts \- React charting library (MIT license)                              │

│  • Or: Chart.js with react-chartjs-2                                            │

│                                                                                  │

│  Data Tables:                                                                   │

│  ────────────                                                                   │

│  • TanStack Table (React Table v8) \- Headless table library                    │

│                                                                                  │

│  Forms:                                                                         │

│  ──────                                                                         │

│  • React Hook Form \- Form management                                            │

│  • Zod \- Schema validation                                                      │

│                                                                                  │

│  Video Player:                                                                  │

│  ─────────────                                                                  │

│  • Video.js \- Open source HTML5 player                                          │

│  • Or: Plyr \- Simple, accessible player                                         │

│  • HLS.js \- HLS playback support                                                │

│                                                                                  │

│  File Upload:                                                                   │

│  ────────────                                                                   │

│  • tus-js-client \- Resumable uploads                                            │

│  • react-dropzone \- Drag & drop file input                                      │

│                                                                                  │

│  Rich Text Editor (for KB articles):                                            │

│  ────────────────────────────────────                                           │

│  • TipTap \- Headless editor (based on ProseMirror)                             │

│  • Or: Lexical (by Meta)                                                        │

│                                                                                  │

│  State Management:                                                              │

│  ─────────────────                                                              │

│  • Zustand \- Simple state management                                            │

│  • TanStack Query (React Query) \- Server state management                       │

│                                                                                  │

│  Utilities:                                                                     │

│  ──────────                                                                     │

│  • date-fns \- Date manipulation                                                 │

│  • clsx / tailwind-merge \- Class merging                                        │

│  • Lucide React \- Icon library                                                   │

│                                                                                  │

│  Maps (for geography analytics):                                                │

│  ───────────────────────────────                                                │

│  • react-simple-maps \- SVG map components                                       │

│                                                                                  │

│  Markdown:                                                                      │

│  ─────────                                                                      │

│  • react-markdown \- Markdown renderer                                           │

│  • rehype-highlight \- Code syntax highlighting                                  │

│                                                                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

## 4.3 Key Page Wireframe Descriptions

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         PAGE WIREFRAME DESCRIPTIONS                              │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           TENANT DASHBOARD                                        ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  ┌─────────────────────────────────────────────────────────────────────────────┐ ║

║  │ HEADER                                                                      │ ║

║  │ ┌──────┐ ┌──────────────────────────────────┐ ┌────┐ ┌────┐ ┌─────────────┐│ ║

║  │ │ Logo │ │ Search videos...                  │ │Bell│ │Help│ │ User Avatar ││ ║

║  │ └──────┘ └──────────────────────────────────┘ └────┘ └────┘ └─────────────┘│ ║

║  └─────────────────────────────────────────────────────────────────────────────┘ ║

║                                                                                   ║

║  ┌─────────────┐ ┌───────────────────────────────────────────────────────────┐   ║

║  │ SIDEBAR     │ │ MAIN CONTENT AREA                                        │   ║

║  │             │ │                                                            │   ║

║  │ Dashboard   │ │ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌────────┐│   ║

║  │ Videos    ▼ │ │ │ Total      │ │ Storage    │ │ Bandwidth  │ │ Views  ││   ║

║  │ Collections │ │ │ Videos     │ │ Used       │ │ Used       │ │ Today  ││   ║

║  │ Analytics ▼ │ │ │ 1,234      │ │ 45/100 GB  │ │ 200/500 GB │ │ 12.5K  ││   ║

║  │ Team        │ │ └─────────────┘ └─────────────┘ └─────────────┘ └────────┘│   ║

║  │ Settings  ▼ │ │                                                            │   ║

║  │ Billing     │ │ ┌────────────────────────────────────────────────────────┐│   ║

║  │ Support     │ │ │ Recent Activity / Views Chart                         ││   ║

║  │             │ │ │ \[Line chart showing views over time\]                   ││   ║

║  │             │ │ │                                                         ││   ║

║  │             │ │ └────────────────────────────────────────────────────────┘│   ║

║  │             │ │                                                            │   ║

║  │             │ │ ┌──────────────────────┐ ┌──────────────────────────────┐ │   ║

║  │             │ │ │ Top Videos           │ │ Recent Uploads               │ │   ║

║  │             │ │ │ 1\. Video A \- 5K views│ │ \[Thumbnail\] Video X         │ │   ║

║  │             │ │ │ 2\. Video B \- 3K views│ │ \[Thumbnail\] Video Y         │ │   ║

║  │             │ │ │ 3\. Video C \- 2K views│ │ \[Thumbnail\] Video Z         │ │   ║

║  │             │ │ └──────────────────────┘ └──────────────────────────────┘ │   ║

║  │             │ │                                                            │   ║

║  └─────────────┘ └───────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           VIDEO LIST PAGE                                         ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Page Header: Videos               \[+ Upload Video\] \[Import Videos\]              ║

║  ──────────────────────────────────────────────────────────────────────────────  ║

║                                                                                   ║

║  Filters Bar:                                                                    ║

║  ┌──────────────────────────────────────────────────────────────────────────┐    ║

║  │ \[Search...        \] \[Collection ▼\] \[Status ▼\] \[Date ▼\] \[🔲 Grid\] \[≡ List\]│    ║

║  └──────────────────────────────────────────────────────────────────────────┘    ║

║                                                                                   ║

║  Bulk Actions (shown when items selected):                                       ║

║  ┌──────────────────────────────────────────────────────────────────────────┐    ║

║  │ ☑ 3 selected    \[Move to...\] \[Delete\] \[Change Visibility\] \[Cancel\]      │    ║

║  └──────────────────────────────────────────────────────────────────────────┘    ║

║                                                                                   ║

║  Grid View:                                                                      ║

║  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐ ┌────────────────┐ ║

║  │ \[  Thumbnail   \]│ │ \[  Thumbnail   \]│ │ \[  Thumbnail   \]│ │ \[ Thumbnail  \] │ ║

║  │ \[  with play   \]│ │ \[  with play   \]│ │ \[  ⏳ Encoding \]│ │ \[ with play  \] │ ║

║  │ ☐ Video Title 1 │ │ ☐ Video Title 2 │ │ ☐ Video Title 3 │ │ ☐ Video Title 4│ ║

║  │ 1.5K views • 2d │ │ 800 views • 5d  │ │ Encoding 45%    │ │ 300 views • 1w │ ║

║  │ \[🌐 Public\]  \[⋮\]│ │ \[🔒 Private\] \[⋮\]│ │ \[⏳ Processing\] │ │ \[🌐 Public\] \[⋮\]│ ║

║  └─────────────────┘ └─────────────────┘ └─────────────────┘ └────────────────┘ ║

║                                                                                   ║

║  Pagination:                                                                     ║

║  ┌──────────────────────────────────────────────────────────────────────────┐    ║

║  │ Showing 1-20 of 234 videos        \[\< Prev\] \[1\] \[2\] \[3\] ... \[12\] \[Next \>\] │    ║

║  └──────────────────────────────────────────────────────────────────────────┘    ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           VIDEO UPLOAD PAGE                                       ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Page Header: Upload Videos                                                       ║

║  ──────────────────────────────────────────────────────────────────────────────  ║

║                                                                                   ║

║  Upload Methods (Tabs):                                                          ║

║  ┌───────────────────┬─────────────────┬─────────────────┐                       ║

║  │   📁 File Upload  │  🔗 From URL    │  🔄 Bulk Import │                       ║

║  └───────────────────┴─────────────────┴─────────────────┘                       ║

║                                                                                   ║

║  File Upload Tab:                                                                ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │                                                                           │   ║

║  │    ┌─────────────────────────────────────────────────────────────────┐   │   ║

║  │    │                                                                 │   │   ║

║  │    │         📁                                                      │   │   ║

║  │    │                                                                 │   │   ║

║  │    │    Drag and drop video files here                              │   │   ║

║  │    │    or click to browse                                          │   │   ║

║  │    │                                                                 │   │   ║

║  │    │    Supported: MP4, MOV, AVI, MKV, WebM (up to 10GB)            │   │   ║

║  │    │                                                                 │   │   ║

║  │    └─────────────────────────────────────────────────────────────────┘   │   ║

║  │                                                                           │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  Upload Queue (shown when files added):                                          ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ ┌─────────────────────────────────────────────────────────────────────┐  │   ║

║  │ │ 📹 video\_file\_1.mp4                          45% ████████░░░░░ \[✕\] │  │   ║

║  │ │    Uploading... 23 MB / 50 MB                                      │  │   ║

║  │ └─────────────────────────────────────────────────────────────────────┘  │   ║

║  │ ┌─────────────────────────────────────────────────────────────────────┐  │   ║

║  │ │ 📹 video\_file\_2.mp4                          Waiting...       \[✕\]  │  │   ║

║  │ └─────────────────────────────────────────────────────────────────────┘  │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  Video Details Form (for each video in queue):                                   ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ Title: \[video\_file\_1                                                   \]  │   ║

║  │ Description: \[                                                         \]  │   ║

║  │ Collection: \[▼ Select collection or create new...                      \]  │   ║

║  │ Visibility: (●) Public  ( ) Private  ( ) Unlisted  ( ) Password        │   ║

║  │ Tags: \[tag1, tag2, \+ Add                                               \]  │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           SUPER ADMIN DASHBOARD                                   ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  ADMIN HEADER (Different from tenant header \- red/different color accent)        ║

║  ┌─────────────────────────────────────────────────────────────────────────────┐ ║

║  │ \[Admin Logo\]  SUPER ADMIN PANEL                     \[🔔\] \[Admin Avatar ▼\] │ ║

║  └─────────────────────────────────────────────────────────────────────────────┘ ║

║                                                                                   ║

║  ┌─────────────┐ ┌───────────────────────────────────────────────────────────┐   ║

║  │ ADMIN       │ │                                                            │   ║

║  │ SIDEBAR     │ │  Platform Overview                                         │   ║

║  │             │ │                                                            │   ║

║  │ Dashboard   │ │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐      │   ║

║  │ Tenants     │ │  │ Active   │ │ MRR      │ │ Total    │ │ Open     │      │   ║

║  │ Plans       │ │  │ Tenants  │ │ Revenue  │ │ Videos   │ │ Tickets  │      │   ║

║  │ Multipliers │ │  │ 156      │ │ $12,450  │ │ 45,678   │ │ 23       │      │   ║

║  │ Billing   ▼ │ │  │ ↑12%     │ │ ↑8%      │ │ ↑25%     │ │ ↓5%      │      │   ║

║  │ Analytics ▼ │ │  └──────────┘ └──────────┘ └──────────┘ └──────────┘      │   ║

║  │ Support   ▼ │ │                                                            │   ║

║  │ Knowledge   │ │  ┌────────────────────────────────────────────────────┐   │   ║

║  │   Base      │ │  │ Revenue Trend (Last 30 Days)                      │   │   ║

║  │ Settings  ▼ │ │  │ \[Area chart showing MRR growth\]                    │   │   ║

║  │ System Users│ │  │                                                     │   │   ║

║  │ Audit Logs  │ │  └────────────────────────────────────────────────────┘   │   ║

║  │             │ │                                                            │   ║

║  │ ─────────── │ │  ┌────────────────────┐ ┌────────────────────────────┐    │   ║

║  │ Bunny.net   │ │  │ New Signups Today │ │ Usage Overview             │    │   ║

║  │ Status: 🟢  │ │  │                    │ │                            │    │   ║

║  │             │ │  │ 1\. Acme Corp       │ │ Storage: 2.3 TB / 5 TB    │    │   ║

║  │             │ │  │ 2\. Beta Inc        │ │ Bandwidth: 12 TB / 20 TB  │    │   ║

║  │             │ │  │ 3\. Gamma LLC       │ │ Encoding: 450 / 1000 min  │    │   ║

║  │             │ │  └────────────────────┘ └────────────────────────────┘    │   ║

║  │             │ │                                                            │   ║

║  └─────────────┘ └───────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           TENANT DETAIL PAGE (Admin)                              ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Breadcrumb: Admin \> Tenants \> Acme Corporation                                  ║

║                                                                                   ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ ┌──────┐  Acme Corporation                    \[Suspend\] \[Login As ▼\]     │   ║

║  │ │ Logo │  acme.yourplatform.com                                          │   ║

║  │ └──────┘  Status: 🟢 Active    Plan: Professional    Since: Jan 2024     │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  Tabs: \[Overview\] \[Users\] \[Videos\] \[Usage\] \[Billing\] \[Audit Logs\]               ║

║                                                                                   ║

║  Overview Tab:                                                                   ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ Subscription Details                          Usage (Actual vs Displayed)│   ║

║  │ ┌─────────────────────────────┐              ┌─────────────────────────┐ │   ║

║  │ │ Plan: Professional          │              │ Metric    Actual  Display│ │   ║

║  │ │ Price: $49/month            │              │ Storage   25 GB   50 GB  │ │   ║

║  │ │ Billing: Monthly            │              │ Bandwidth 100 GB  200 GB │ │   ║

║  │ │ Next Invoice: Jan 15, 2025  │              │ Views     10K     10K    │ │   ║

║  │ │ Payment: Stripe             │              │                          │ │   ║

║  │ │ \[Change Plan\] \[Add Credit\]  │              │ Multiplier: 2x (custom)  │ │   ║

║  │ └─────────────────────────────┘              └─────────────────────────┘ │   ║

║  │                                                                           │   ║

║  │ Quick Stats                                                              │   ║

║  │ ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐              │   ║

║  │ │ Users: 8   │ │ Videos: 234│ │ Collections│ │ API Keys: 3│              │   ║

║  │ │            │ │            │ │ 12         │ │            │              │   ║

║  │ └────────────┘ └────────────┘ └────────────┘ └────────────┘              │   ║

║  │                                                                           │   ║

║  │ Bunny.net Details (Internal)                                             │   ║

║  │ ┌─────────────────────────────────────────────────────────────────────┐  │   ║

║  │ │ Library ID: abc123        Pull Zone: xyz789        CDN: cdn.bunny   │  │   ║

║  │ └─────────────────────────────────────────────────────────────────────┘  │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           USAGE MULTIPLIERS PAGE (Admin)                          ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Page Header: Usage Multipliers                      \[+ Add Multiplier\]          ║

║  ──────────────────────────────────────────────────────────────────────────────  ║

║                                                                                   ║

║  Info Box:                                                                       ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ ℹ️ Usage multipliers adjust the values shown to tenants. A multiplier of  │   ║

║  │    2.0 means tenants will see double the actual usage from Bunny.net.     │   ║

║  │    This allows you to add margins to your reseller pricing.               │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  Global Defaults:                                                                ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ Metric          Current Multiplier    Actions                             │   ║

║  │ ─────────────────────────────────────────────────────────────────────────│   ║

║  │ Storage         1.5x                  \[Edit\]                              │   ║

║  │ Bandwidth       2.0x                  \[Edit\]                              │   ║

║  │ Encoding Mins   1.0x                  \[Edit\]                              │   ║

║  │ Views           1.0x                  \[Edit\]                              │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  Tenant-Specific Overrides:                                                      ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ Tenant              Metric       Multiplier   Effective      Actions      │   ║

║  │ ─────────────────────────────────────────────────────────────────────────│   ║

║  │ Acme Corporation   Storage       2.5x         Jan 1 \- Dec 31 \[Edit\] \[Del\]│   ║

║  │ Acme Corporation   Bandwidth     3.0x         Jan 1 \- Dec 31 \[Edit\] \[Del\]│   ║

║  │ Beta Inc           All           1.0x         Indefinite     \[Edit\] \[Del\]│   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  Add/Edit Multiplier Modal:                                                      ║

║  ┌───────────────────────────────────────────────────────────

# PART 4: FRONTEND STRUCTURE (Continued)

## 4.3 Key Page Wireframe Descriptions (Continued)

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           USAGE MULTIPLIERS PAGE (Admin) \- Continued              ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Add/Edit Multiplier Modal:                                                      ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │  ╔═══════════════════════════════════════════════════════════════════╗   │   ║

║  │  ║  Add Usage Multiplier                                       \[✕\]  ║   │   ║

║  │  ╠═══════════════════════════════════════════════════════════════════╣   │   ║

║  │  ║                                                                   ║   │   ║

║  │  ║  Scope:                                                          ║   │   ║

║  │  ║  (●) Global Default  ( ) Specific Tenant                         ║   │   ║

║  │  ║                                                                   ║   │   ║

║  │  ║  Tenant: \[▼ Select tenant...                               \]     ║   │   ║

║  │  ║          (shown only when Specific Tenant selected)              ║   │   ║

║  │  ║                                                                   ║   │   ║

║  │  ║  Metric Type: \[▼ Storage                                   \]     ║   │   ║

║  │  ║               Options: Storage, Bandwidth, Encoding, Views       ║   │   ║

║  │  ║                                                                   ║   │   ║

║  │  ║  Multiplier Value: \[2.0    \]                                     ║   │   ║

║  │  ║  (e.g., 2.0 \= doubles the displayed value)                       ║   │   ║

║  │  ║                                                                   ║   │   ║

║  │  ║  Preview:                                                        ║   │   ║

║  │  ║  ┌─────────────────────────────────────────────────────────┐    ║   │   ║

║  │  ║  │ If actual usage is 50 GB:                               │    ║   │   ║

║  │  ║  │ Tenant will see: 100 GB                                  │    ║   │   ║

║  │  ║  └─────────────────────────────────────────────────────────┘    ║   │   ║

║  │  ║                                                                   ║   │   ║

║  │  ║  Effective Period:                                               ║   │   ║

║  │  ║  From: \[📅 Jan 1, 2025    \]  To: \[📅 Dec 31, 2025  \] ☐ Indefinite║   │   ║

║  │  ║                                                                   ║   │   ║

║  │  ║  Notes (internal): \[                                        \]    ║   │   ║

║  │  ║                                                                   ║   │   ║

║  │  ║                              \[Cancel\]  \[Save Multiplier\]         ║   │   ║

║  │  ╚═══════════════════════════════════════════════════════════════════╝   │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           SUPPORT TICKET PAGE (Admin View)                        ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Breadcrumb: Admin \> Support \> Ticket \#TKT-2024-00145                            ║

║                                                                                   ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ Video encoding fails for MP4 files                     Priority: 🔴 High  │   ║

║  │ Tenant: Acme Corp • Created: 2 hours ago • Status: 🟡 In Progress        │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  ┌─────────────────────────────────────────────┐ ┌─────────────────────────────┐ ║

║  │ Conversation                                │ │ Ticket Details              │ ║

║  │                                             │ │                             │ ║

║  │ ┌─────────────────────────────────────────┐│ │ Assigned To:                │ ║

║  │ │ 👤 John (Acme Corp)         2 hours ago ││ │ \[▼ Sarah (Support)     \]    │ ║

║  │ │                                         ││ │                             │ ║

║  │ │ We're having issues uploading MP4       ││ │ Department:                 │ ║

║  │ │ files. They seem to get stuck at 95%    ││ │ \[▼ Technical Support   \]    │ ║

║  │ │ and never complete encoding.            ││ │                             │ ║

║  │ │                                         ││ │ Priority:                   │ ║

║  │ │ Attached: error\_screenshot.png          ││ │ \[▼ High                \]    │ ║

║  │ └─────────────────────────────────────────┘│ │                             │ ║

║  │                                             │ │ Status:                     │ ║

║  │ ┌─────────────────────────────────────────┐│ │ \[▼ In Progress         \]    │ ║

║  │ │ 👩‍💼 Sarah (Support)        1 hour ago   ││ │                             │ ║

║  │ │                                         ││ │ Category:                   │ ║

║  │ │ Hi John, thank you for reaching out.    ││ │ \[▼ Encoding Issues     \]    │ ║

║  │ │ I'm looking into this now. Could you    ││ │                             │ ║

║  │ │ share the video ID that's affected?     ││ │ Tags:                       │ ║

║  │ └─────────────────────────────────────────┘│ │ \[encoding\] \[mp4\] \[+\]        │ ║

║  │                                             │ │                             │ ║

║  │ ┌─────────────────────────────────────────┐│ │ ─────────────────────────── │ ║

║  │ │ 🔒 Internal Note         45 mins ago    ││ │                             │ ║

║  │ │ (Only visible to staff)                 ││ │ SLA Status:                 │ ║

║  │ │                                         ││ │ First Response: ✅ Met       │ ║

║  │ │ Checked Bunny.net \- their encoding      ││ │ Resolution: ⏳ 5h remaining  │ ║

║  │ │ queue looks backed up. May be temp.     ││ │                             │ ║

║  │ └─────────────────────────────────────────┘│ │ ─────────────────────────── │ ║

║  │                                             │ │                             │ ║

║  │ Reply:                                     │ │ Quick Actions:              │ ║

║  │ ┌─────────────────────────────────────────┐│ │ \[🔼 Escalate\]               │ ║

║  │ │                                         ││ │ \[👤 Login as User\]          │ ║

║  │ │ Type your reply here...                 ││ │ \[🔗 Merge Ticket\]           │ ║

║  │ │                                         ││ │                             │ ║

║  │ └─────────────────────────────────────────┘│ │ ─────────────────────────── │ ║

║  │ ☐ Internal note (not visible to customer) │ │                             │ ║

║  │                                             │ │ Activity Log:              │ ║

║  │ \[📎 Attach\] \[📝 Canned ▼\]    \[Send Reply\] │ │ • Status changed (1h ago)  │ ║

║  │                                             │ │ • Assigned to Sarah (2h)   │ ║

║  └─────────────────────────────────────────────┘ │ • Ticket created (2h ago)  │ ║

║                                                   └─────────────────────────────┘ ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           VIDEO PLAYER (Embed Page)                               ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Full-screen responsive player:                                                  ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │                                                                           │   ║

║  │                                                                           │   ║

║  │                                                                           │   ║

║  │                         \[Tenant Logo Watermark\]                           │   ║

║  │                              (top-right)                                  │   ║

║  │                                                                           │   ║

║  │                                                                           │   ║

║  │                               advancement                                       │   ║

║  │                             advancement                                         │   ║

║  │                                                                           │   ║

║  │                                                                           │   ║

║  │                                                                           │   ║

║  │ ┌───────────────────────────────────────────────────────────────────────┐│   ║

║  │ │ advancement \[▶\] advancement advancement advancement \[advancement\] advancement advancement \[CC\] \[⚙\] \[⛶\]          ││   ║

║  │ │ 00:00 ═══════════●═══════════════════════════════════════════ 10:34 ││   ║

║  │ └───────────────────────────────────────────────────────────────────────┘│   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  Player Controls:                                                                ║

║  • Play/Pause button                                                            ║

║  • Progress bar with preview thumbnails on hover                                ║

║  • Volume control                                                               ║

║  • Quality selector (auto, 360p, 720p, 1080p based on encoding)                ║

║  • Playback speed (0.5x, 0.75x, 1x, 1.25x, 1.5x, 2x)                           ║

║  • Captions toggle (if available)                                               ║

║  • Chapters menu (if chapters defined)                                          ║

║  • Settings gear (quality, speed, captions)                                     ║

║  • Fullscreen toggle                                                            ║

║  • Picture-in-picture (if supported)                                            ║

║                                                                                   ║

║  Customizable by Tenant Branding:                                               ║

║  • Progress bar color (primary\_color from branding)                             ║

║  • Logo watermark (player\_logo\_url, player\_logo\_position)                       ║

║  • Button colors                                                                ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           BILLING PAGE (Tenant)                                   ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Page Header: Billing & Subscription                                             ║

║  ──────────────────────────────────────────────────────────────────────────────  ║

║                                                                                   ║

║  Current Plan:                                                                   ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │  ┌─────────────────────────────────┐   ┌────────────────────────────────┐│   ║

║  │  │  PROFESSIONAL PLAN              │   │  Current Period Usage          ││   ║

║  │  │                                 │   │                                 ││   ║

║  │  │  $49/month                      │   │  Storage                        ││   ║

║  │  │  Billed monthly                 │   │  ████████████░░░░ 75/100 GB    ││   ║

║  │  │                                 │   │                                 ││   ║

║  │  │  Next billing: Jan 15, 2025     │   │  Bandwidth                      ││   ║

║  │  │  Amount: $49.00                 │   │  ██████░░░░░░░░░░ 180/500 GB   ││   ║

║  │  │                                 │   │                                 ││   ║

║  │  │  \[Change Plan\]  \[Cancel\]        │   │  Videos                         ││   ║

║  │  │                                 │   │  ████░░░░░░░░░░░░ 234/1000     ││   ║

║  │  └─────────────────────────────────┘   └────────────────────────────────┘│   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  Estimated Charges This Period:                                                  ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ Base subscription                                              $49.00    │   ║

║  │ Storage overage (estimated: 0 GB over limit)                    $0.00    │   ║

║  │ Bandwidth overage (estimated: 0 GB over limit)                  $0.00    │   ║

║  │ ─────────────────────────────────────────────────────────────────────────│   ║

║  │ Estimated Total                                                $49.00    │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  Payment Method:                                                                 ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ 💳 Visa ending in 4242                              \[Default\]            │   ║

║  │    Expires 12/2025                                                        │   ║

║  │                                                \[Add New\]  \[Manage\]       │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

║  Billing History:                                                                ║

║  ┌───────────────────────────────────────────────────────────────────────────┐   ║

║  │ Invoice \#       Date           Amount      Status        Actions         │   ║

║  │ ─────────────────────────────────────────────────────────────────────────│   ║

║  │ INV-2024-0145   Dec 15, 2024   $49.00      ✅ Paid       \[View\] \[PDF\]   │   ║

║  │ INV-2024-0132   Nov 15, 2024   $52.50      ✅ Paid       \[View\] \[PDF\]   │   ║

║  │ INV-2024-0118   Oct 15, 2024   $49.00      ✅ Paid       \[View\] \[PDF\]   │   ║

║  └───────────────────────────────────────────────────────────────────────────┘   ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

---

# PART 5: BACKGROUND JOBS & SCHEDULED TASKS

## 5.1 Hangfire Job Definitions

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         BACKGROUND JOBS (HANGFIRE)                               │

│                                                                                  │

│  All jobs run on the same VPS. Hangfire dashboard accessible to super admins.  │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           RECURRING JOBS (SCHEDULED)                             ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Job Name                    Schedule           Description                       ║

║  ═══════════════════════════════════════════════════════════════════════════════ ║

║                                                                                   ║

║  UsageSyncJob                Every 1 hour       Syncs usage data from Bunny.net  ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Fetches storage, bandwidth, views from Bunny.net API for each tenant          ║

║  • Stores actual values in TenantUsageDaily table                                ║

║  • Applies multipliers and stores display values                                 ║

║  • Updates tenant's current usage cache in Redis                                 ║

║  • Checks for limit thresholds (80%, 90%, 100%) and sends alerts                ║

║                                                                                   ║

║  AnalyticsAggregationJob     Every 6 hours      Aggregates video analytics       ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Fetches detailed video stats from Bunny.net                                   ║

║  • Aggregates daily views, watch time, geography, device data                    ║

║  • Stores in VideoAnalyticsDaily table                                          ║

║  • Calculates engagement scores and completion rates                            ║

║                                                                                   ║

║  OverageCalculationJob       Daily at 00:00 UTC Calculates bandwidth overage     ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Runs at end of each day                                                       ║

║  • Calculates if any tenant exceeded their plan limits                           ║

║  • Records overage amounts for billing                                          ║

║  • Sends warning emails if approaching limits                                    ║

║                                                                                   ║

║  InvoiceGenerationJob        Daily at 02:00 UTC Generates invoices               ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Checks for subscriptions due for billing                                      ║

║  • Calculates base subscription \+ any overage charges                            ║

║  • Creates draft invoice records                                                 ║

║  • For Stripe: triggers Stripe invoice creation                                 ║

║  • For Manual: sends invoice email to tenant                                    ║

║                                                                                   ║

║  SubscriptionRenewalJob      Daily at 03:00 UTC Handles subscription renewals    ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Checks for subscriptions ending today                                         ║

║  • For auto-renew: confirms renewal, creates invoice                            ║

║  • For cancelled: marks tenant as inactive, sends final email                   ║

║  • Updates subscription periods                                                  ║

║                                                                                   ║

║  TrialExpirationJob          Daily at 06:00 UTC Handles trial expirations        ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Finds tenants with trials ending in 3 days, 1 day, today                     ║

║  • Sends reminder emails                                                        ║

║  • On expiration: suspends tenant if no payment method                          ║

║  • Converts to paid if payment method exists                                    ║

║                                                                                   ║

║  EmailQueueProcessorJob      Every 1 minute     Processes email queue            ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Fetches pending emails from EmailQueue table                                 ║

║  • Sends via SMTP (batches of 10\)                                               ║

║  • Updates status (sent/failed)                                                 ║

║  • Retries failed emails (max 3 attempts)                                       ║

║                                                                                   ║

║  WebhookDeliveryRetryJob     Every 5 minutes    Retries failed webhooks          ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Fetches failed webhook deliveries due for retry                              ║

║  • Attempts redelivery with exponential backoff                                 ║

║  • Max 5 retries, then marks as permanently failed                              ║

║  • Notifies tenant if webhook endpoint consistently fails                       ║

║                                                                                   ║

║  EscalationCheckJob          Every 15 minutes   Checks ticket escalations        ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Checks all escalation rules                                                  ║

║  • Finds tickets matching rule conditions                                       ║

║  • Applies escalation actions (reassign, notify, change priority)              ║

║  • Logs escalation events                                                       ║

║                                                                                   ║

║  CleanupJob                  Daily at 04:00 UTC Cleans up old data               ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Deletes expired sessions                                                     ║

║  • Cleans up expired invitations                                                ║

║  • Archives old audit logs (\>12 months)                                         ║

║  • Deletes failed upload records (\>7 days)                                      ║

║  • Cleans up expired viewer tokens                                              ║

║  • Removes soft-deleted videos (\>30 days) from Bunny.net                       ║

║                                                                                   ║

║  HealthCheckJob              Every 5 minutes    System health monitoring         ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  • Pings Bunny.net API                                                          ║

║  • Checks database connectivity                                                 ║

║  • Checks Redis connectivity                                                    ║

║  • Checks SMTP connectivity                                                     ║

║  • Updates platform health status                                               ║

║  • Sends alert email if any service is down                                     ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           ON-DEMAND JOBS (TRIGGERED)                             ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  VideoEncodingStatusJob                                                          ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Triggered: When Bunny.net webhook received OR manually                          ║

║  • Updates video status from Bunny.net                                          ║

║  • Fetches encoding progress percentage                                          ║

║  • Updates video metadata (duration, resolution, etc.)                          ║

║  • Triggers tenant webhook (video.encoded event)                                ║

║  • Sends email notification if configured                                       ║

║                                                                                   ║

║  VideoMigrationProcessorJob                                                      ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Triggered: When migration job started                                           ║

║  • Processes migration items one by one                                          ║

║  • Downloads from source (YouTube, Vimeo, URL)                                  ║

║  • Uploads to Bunny.net via TUS                                                 ║

║  • Updates migration item status                                                ║

║  • Preserves metadata if configured                                             ║

║  • Handles rate limiting for external APIs                                      ║

║                                                                                   ║

║  BulkVideoOperationJob                                                           ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Triggered: When bulk operation requested                                        ║

║  • Processes bulk delete, move, or update operations                            ║

║  • Updates each video sequentially                                              ║

║  • Handles Bunny.net API rate limits                                            ║

║  • Reports progress via WebSocket/polling                                       ║

║  • Sends completion notification                                                ║

║                                                                                   ║

║  AnalyticsExportJob                                                              ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Triggered: When export requested                                                ║

║  • Generates CSV/XLSX export file                                               ║

║  • Stores temporarily in file storage                                           ║

║  • Sends download link via email                                                ║

║  • Cleans up file after 24 hours                                                ║

║                                                                                   ║

║  TenantProvisioningJob                                                           ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Triggered: When new tenant registered                                           ║

║  • Creates Bunny.net video library                                              ║

║  • Configures pull zone                                                         ║

║  • Sets up token authentication                                                 ║

║  • Creates default roles for tenant                                             ║

║  • Sends welcome email with setup guide                                         ║

║                                                                                   ║

║  TenantDeletionJob                                                               ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Triggered: When tenant deletion requested (after grace period)                  ║

║  • Exports tenant data if requested                                             ║

║  • Deletes all videos from Bunny.net                                            ║

║  • Deletes Bunny.net library                                                    ║

║  • Cancels Stripe subscription                                                  ║

║  • Soft-deletes all tenant data                                                 ║

║  • Archives for compliance period                                               ║

║                                                                                   ║

║  WebhookDeliveryJob                                                              ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Triggered: When tenant webhook event occurs                                     ║

║  • Prepares webhook payload                                                     ║

║  • Signs payload with webhook secret                                            ║

║  • Delivers to tenant's webhook URL                                             ║

║  • Records delivery attempt                                                     ║

║  • Schedules retry if failed                                                    ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

---

# PART 6: DEPLOYMENT ARCHITECTURE

## 6.1 VPS Deployment Setup

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         VPS DEPLOYMENT ARCHITECTURE                              │

│                         (Contabo or Similar Linux VPS)                           │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           RECOMMENDED VPS SPECIFICATIONS                          ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Initial Setup (MVP \- up to 50 tenants):                                         ║

║  ────────────────────────────────────────                                        ║

║  • CPU: 6 vCPU cores                                                             ║

║  • RAM: 16 GB                                                                    ║

║  • Storage: 200 GB NVMe SSD                                                      ║

║  • Bandwidth: Unmetered (or high limit)                                          ║

║  • OS: Ubuntu 22.04 LTS                                                          ║

║                                                                                   ║

║  Growth Setup (50-200 tenants):                                                  ║

║  ──────────────────────────────                                                  ║

║  • CPU: 8-10 vCPU cores                                                          ║

║  • RAM: 32 GB                                                                    ║

║  • Storage: 400 GB NVMe SSD                                                      ║

║  • Consider: Separate database VPS                                               ║

║                                                                                   ║

║  Scale Setup (200+ tenants):                                                     ║

║  ───────────────────────────                                                     ║

║  • Multiple VPS instances                                                        ║

║  • Load balancer (nginx or HAProxy)                                              ║

║  • Dedicated database server                                                     ║

║  • Dedicated Redis server                                                        ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           SERVER SOFTWARE STACK                                   ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  ┌─────────────────────────────────────────────────────────────────────────────┐ ║

║  │                           SINGLE VPS ARCHITECTURE                            │ ║

║  │                                                                               │ ║

║  │  ┌─────────────────────────────────────────────────────────────────────────┐│ ║

║  │  │                    NGINX (Reverse Proxy \+ SSL)                          ││ ║

║  │  │                    Port 80, 443                                         ││ ║

║  │  │  • SSL termination (Let's Encrypt/Certbot)                             ││ ║

║  │  │  • Gzip compression                                                     ││ ║

║  │  │  • Rate limiting                                                        ││ ║

║  │  │  • Static file serving for Next.js                                      ││ ║

║  │  │  • Proxy to backend services                                            ││ ║

║  │  └─────────────────────────────────────────────────────────────────────────┘│ ║

║  │                              │                                               │ ║

║  │              ┌───────────────┼───────────────┐                              │ ║

║  │              │               │               │                              │ ║

║  │              ▼               ▼               ▼                              │ ║

║  │  ┌───────────────────┐ ┌───────────────────┐ ┌───────────────────┐        │ ║

║  │  │  Next.js App      │ │  ASP.NET Core API │ │  Static Assets    │        │ ║

║  │  │  (PM2/Node)       │ │  (Kestrel/systemd)│ │  (nginx direct)   │        │ ║

║  │  │  Port: 3000       │ │  Port: 5000       │ │                   │        │ ║

║  │  └───────────────────┘ └───────────────────┘ └───────────────────┘        │ ║

║  │              │               │                                              │ ║

║  │              │               ▼                                              │ ║

║  │              │   ┌───────────────────────────────────────────────┐        │ ║

║  │              │   │              DATA STORES                       │        │ ║

║  │              │   │                                                │        │ ║

║  │              │   │  ┌─────────────┐  ┌─────────────┐             │        │ ║

║  │              │   │  │ PostgreSQL  │  │    Redis    │             │        │ ║

║  │              │   │  │ Port: 5432  │  │  Port: 6379 │             │        │ ║

║  │              │   │  │             │  │             │             │        │ ║

║  │              │   │  │ • Main DB   │  │ • Sessions  │             │        │ ║

║  │              │   │  │ • Tenant DBs│  │ • Cache     │             │        │ ║

║  │              │   │  │             │  │ • Job Queue │             │        │ ║

║  │              │   │  └─────────────┘  └─────────────┘             │        │ ║

║  │              │   │                                                │        │ ║

║  │              │   └───────────────────────────────────────────────┘        │ ║

║  │              │                                                             │ ║

║  │              ▼                                                             │ ║

║  │  ┌───────────────────────────────────────────────────────────────────────┐│ ║

║  │  │                      FILE STORAGE (/var/www/storage)                  ││ ║

║  │  │  • /uploads (temp upload chunks)                                      ││ ║

║  │  │  • /exports (analytics exports)                                       ││ ║

║  │  │  • /attachments (support ticket attachments)                          ││ ║

║  │  │  • /logos (tenant logos/branding)                                     ││ ║

║  │  └───────────────────────────────────────────────────────────────────────┘│ ║

║  │                                                                             │ ║

║  └─────────────────────────────────────────────────────────────────────────────┘ ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           DOCKER COMPOSE SETUP (RECOMMENDED)                      ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Directory Structure:                                                            ║

║  /opt/streamvault/                                                               ║

║  ├── docker-compose.yml                                                          ║

║  ├── docker-compose.override.yml (local overrides)                              ║

║  ├── .env                                                                        ║

║  ├── nginx/                                                                      ║

║  │   ├── nginx.conf                                                              ║

║  │   ├── sites-enabled/                                                          ║

║  │   │   └── streamvault.conf                                                    ║

║  │   └── ssl/                                                                    ║

║  ├── backend/                                                                    ║

║  │   └── Dockerfile                                                              ║

║  ├── frontend/                                                                   ║

║  │   └── Dockerfile                                                              ║

║  ├── postgres/                                                                   ║

║  │   ├── init/                                                                   ║

║  │   │   └── 01-init.sql                                                         ║

║  │   └── data/ (volume mount)                                                   ║

║  ├── redis/                                                                      ║

║  │   └── redis.conf                                                              ║

║  ├── certbot/                                                                    ║

║  │   └── conf/ (volume mount)                                                   ║

║  └── storage/                                                                    ║

║      ├── uploads/                                                                ║

║      ├── exports/                                                                ║

║      ├── attachments/                                                            ║

║      └── logs/                                                                   ║

║                                                                                   ║

║  Services in docker-compose.yml:                                                 ║

║  ─────────────────────────────────                                               ║

║  • nginx          (nginx:alpine)                                                 ║

║  • api            (custom ASP.NET Core image)                                    ║

║  • frontend       (custom Next.js image)                                         ║

║  • postgres       (postgres:15-alpine)                                           ║

║  • redis          (redis:7-alpine)                                               ║

║  • hangfire       (same as api, different entry point)                           ║

║  • certbot        (certbot/certbot)                                              ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

## 6.2 Environment Configuration

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         ENVIRONMENT VARIABLES                                    │

│                         (.env file \- NEVER commit to git)                       │

└─────────────────────────────────────────────────────────────────────────────────┘

\# ═══════════════════════════════════════════════════════════════════════════════

\# APPLICATION

\# ═══════════════════════════════════════════════════════════════════════════════

APP\_NAME=StreamVault

APP\_ENV=production

APP\_URL=https://yourplatform.com

API\_URL=https://api.yourplatform.com

ADMIN\_URL=https://admin.yourplatform.com

\# ═══════════════════════════════════════════════════════════════════════════════

\# DATABASE (PostgreSQL)

\# ═══════════════════════════════════════════════════════════════════════════════

DB\_HOST=postgres

DB\_PORT=5432

DB\_NAME=streamvault

DB\_USER=streamvault\_user

DB\_PASSWORD=\<strong-generated-password\>

DATABASE\_URL=Host=${DB\_HOST};Port=${DB\_PORT};Database=${DB\_NAME};Username=${DB\_USER};Password=${DB\_PASSWORD}

\# ═══════════════════════════════════════════════════════════════════════════════

\# REDIS

\# ═══════════════════════════════════════════════════════════════════════════════

REDIS\_HOST=redis

REDIS\_PORT=6379

REDIS\_PASSWORD=\<strong-generated-password\>

REDIS\_URL=redis://:${REDIS\_PASSWORD}@${REDIS\_HOST}:${REDIS\_PORT}

\# ═══════════════════════════════════════════════════════════════════════════════

\# JWT AUTHENTICATION

\# ═══════════════════════════════════════════════════════════════════════════════

JWT\_SECRET=\<64-character-random-string\>

JWT\_ISSUER=yourplatform.com

JWT\_AUDIENCE=yourplatform.com

JWT\_ACCESS\_TOKEN\_EXPIRY\_MINUTES=15

JWT\_REFRESH\_TOKEN\_EXPIRY\_DAYS=7

\# ═══════════════════════════════════════════════════════════════════════════════

\# ENCRYPTION (for API keys, Bunny credentials, etc.)

\# ═══════════════════════════════════════════════════════════════════════════════

ENCRYPTION\_KEY=\<32-byte-base64-encoded-key\>

\# ═══════════════════════════════════════════════════════════════════════════════

\# BUNNY.NET CONFIGURATION (Your reseller account)

\# ═══════════════════════════════════════════════════════════════════════════════

BUNNY\_API\_KEY=\<your-bunny-api-key\>

BUNNY\_STREAM\_API\_KEY=\<your-bunny-stream-api-key\>

BUNNY\_PULL\_ZONE\_URL=https://stream.yourplatform.com

BUNNY\_STORAGE\_ZONE=streamvault-storage

BUNNY\_WEBHOOK\_SECRET=\<bunny-webhook-secret\>

\# Default library for new tenants (or create dynamically)

BUNNY\_DEFAULT\_REGION=EU  \# EU, US, SG, etc.

\# ═══════════════════════════════════════════════════════════════════════════════

\# STRIPE CONFIGURATION

\# ═══════════════════════════════════════════════════════════════════════════════

STRIPE\_SECRET\_KEY=sk\_live\_\<your-stripe-secret-key\>

STRIPE\_PUBLISHABLE\_KEY=pk\_live\_\<your-stripe-publishable-key\>

STRIPE\_WEBHOOK\_SECRET=whsec\_\<your-stripe-webhook-secret\>

\# ═══════════════════════════════════════════════════════════════════════════════

\# EMAIL (SMTP)

\# ═══════════════════════════════════════════════════════════════════════════════

SMTP\_HOST=smtp.mailgun.org

SMTP\_PORT=587

SMTP\_USER=postmaster@mail.yourplatform.com

SMTP\_PASSWORD=\<smtp-password\>

SMTP\_FROM\_EMAIL=noreply@yourplatform.com

SMTP\_FROM\_NAME=StreamVault

SMTP\_ENCRYPTION=tls

\# ═══════════════════════════════════════════════════════════════════════════════

\# RATE LIMITING

\# ═══════════════════════════════════════════════════════════════════════════════

RATE\_LIMIT\_ENABLED=true

RATE\_LIMIT\_REQUESTS\_PER\_MINUTE=60

RATE\_LIMIT\_REQUESTS\_PER\_HOUR=1000

\# ═══════════════════════════════════════════════════════════════════════════════

\# HANGFIRE (Background Jobs)

\# ═══════════════════════════════════════════════════════════════════════════════

HANGFIRE\_DASHBOARD\_USERNAME=admin

HANGFIRE\_DASHBOARD\_PASSWORD=\<strong-password\>

\# ═══════════════════════════════════════════════════════════════════════════════

\# LOGGING

\# ═══════════════════════════════════════════════════════════════════════════════

LOG\_LEVEL=Information

LOG\_PATH=/var/log/streamvault/

\# ═══════════════════════════════════════════════════════════════════════════════

\# CORS

\# ═══════════════════════════════════════════════════════════════════════════════

CORS\_ALLOWED\_ORIGINS=https://yourplatform.com,https://admin.yourplatform.com

## 6.3 Nginx Configuration

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         NGINX CONFIGURATION                                      │

│                         /etc/nginx/sites-enabled/streamvault.conf               │

└─────────────────────────────────────────────────────────────────────────────────┘

\# Rate limiting zones

limit\_req\_zone $binary\_remote\_addr zone=api\_limit:10m rate=10r/s;

limit\_req\_zone $binary\_remote\_addr zone=upload\_limit:10m rate=1r/s;

\# Upstream definitions

upstream api\_backend {

    server 127.0.0.1:5000;

    keepalive 32;

}

upstream frontend {

    server 127.0.0.1:3000;

    keepalive 32;

}

\# Redirect HTTP to HTTPS

server {

    listen 80;

    server\_name yourplatform.com api.yourplatform.com admin.yourplatform.com;

    

    location /.well-known/acme-challenge/ {

        root /var/www/certbot;

    }

    

    location / {

        return 301 https://$host$request\_uri;

    }

}

\# Main Application (Tenant Dashboard)

server {

    listen 443 ssl http2;

    server\_name yourplatform.com;

    

    ssl\_certificate /etc/letsencrypt/live/yourplatform.com/fullchain.pem;

    ssl\_certificate\_key /etc/letsencrypt/live/yourplatform.com/privkey.pem;

    

    \# SSL configuration

    ssl\_protocols TLSv1.2 TLSv1.3;

    ssl\_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256;

    ssl\_prefer\_server\_ciphers off;

    ssl\_session\_cache shared:SSL:10m;

    ssl\_session\_timeout 1d;

    

    \# Security headers

    add\_header X-Frame-Options "SAMEORIGIN" always;

    add\_header X-Content-Type-Options "nosniff" always;

    add\_header X-XSS-Protection "1; mode=block" always;

    add\_header Referrer-Policy "strict-origin-when-cross-origin" always;

    

    \# Gzip

    gzip on;

    gzip\_vary on;

    gzip\_proxied any;

    gzip\_comp\_level 6;

    gzip\_types text/plain text/css text/xml application/json application/javascript 

               application/xml application/rss+xml image/svg+xml;

    

    \# Frontend (Next.js)

    location / {

        proxy\_pass http://frontend;

        proxy\_http\_version 1.1;

        proxy\_set\_header Upgrade $http\_upgrade;

        proxy\_set\_header Connection 'upgrade';

        proxy\_set\_header Host $host;

        proxy\_set\_header X-Real-IP $remote\_addr;

        proxy\_set\_header X-Forwarded-For $proxy\_add\_x\_forwarded\_for;

        proxy\_set\_header X-Forwarded-Proto $scheme;

        proxy\_cache\_bypass $http\_upgrade;

    }

    

    \# Static files (Next.js)

    location /\_next/static {

        proxy\_pass http://frontend;

        proxy\_cache\_valid 200 365d;

        add\_header Cache-Control "public, max-age=31536000, immutable";

    }

    

    \# Uploaded files (logos, etc.)

    location /storage {

        alias /opt/streamvault/storage;

        expires 7d;

        add\_header Cache-Control "public";

    }

}

\# API Server

server {

    listen 443 ssl http2;

    server\_name api.yourplatform.com;

    

    ssl\_certificate /etc/letsencrypt/live/yourplatform.com/fullchain.pem;

    ssl\_certificate\_key /etc/letsencrypt/live/yourplatform.com/privkey.pem;

    

    \# SSL configuration (same as above)

    ssl\_protocols TLSv1.2 TLSv1.3;

    ssl\_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256;

    

    \# Security headers

    add\_header X-Frame-Options "DENY" always;

    add\_header X-Content-Type-Options "nosniff" always;

    

    \# CORS headers (handled by API, but can add here too)

    

    \# Client max body size (for uploads \- though uploads go to Bunny)

    client\_max\_body\_size 100M;

    

    \# API endpoints

    location /v1 {

        limit\_req zone=api\_limit burst=20 nodelay;

        

        proxy\_pass http://api\_backend;

        proxy\_http\_version 1.1;

        proxy\_set\_header Host $host;

        proxy\_set\_header X-Real-IP $remote\_addr;

        proxy\_set\_header X-Forwarded-For $proxy\_add\_x\_forwarded\_for;

        proxy\_set\_header X-Forwarded-Proto $scheme;

        

        \# Timeout for long operations

        proxy\_read\_timeout 300s;

        proxy\_connect\_timeout 75s;

    }

    

    \# Webhook endpoints (from Bunny.net, Stripe)

    location /webhooks {

        proxy\_pass http://api\_backend;

        proxy\_http\_version 1.1;

        proxy\_set\_header Host $host;

        proxy\_set\_header X-Real-IP $remote\_addr;

        proxy\_set\_header X-Forwarded-For $proxy\_add\_x\_forwarded\_for;

        proxy\_set\_header X-Forwarded-Proto $scheme;

    }

    

    \# Upload endpoint (TUS protocol)

    location /upload {

        limit\_req zone=upload\_limit burst=5 nodelay;

        

        proxy\_pass http://api\_backend;

        proxy\_http\_version 1.1;

        proxy\_set\_header Host $host;

        proxy\_set\_header X-Real-IP $remote\_addr;

        proxy\_set\_header X-Forwarded-For $proxy\_add\_x\_forwarded\_for;

        proxy\_set\_header X-Forwarded-Proto $scheme;

        

        \# TUS needs these

        proxy\_request\_buffering off;

        proxy\_buffering off;

        client\_max\_body\_size 0;  \# Unlimited for chunked uploads

    }

    

    \# Health check

    location /health {

        proxy\_pass http://api\_backend;

        access\_log off;

    }

    

    \# Hangfire dashboard (protected)

    location /hangfire {

        auth\_basic "Hangfire Dashboard";

        auth\_basic\_user\_file /etc/nginx/.htpasswd;

        

        proxy\_pass http://api\_backend;

        proxy\_http\_version 1.1;

        proxy\_set\_header Host $host;

        proxy\_set\_header X-Real-IP $remote\_addr;

    }

}

\# Admin Panel (Super Admin)

server {

    listen 443 ssl http2;

    server\_name admin.yourplatform.com;

    

    ssl\_certificate /etc/letsencrypt/live/yourplatform.com/fullchain.pem;

    ssl\_certificate\_key /etc/letsencrypt/live/yourplatform.com/privkey.pem;

    

    \# More restrictive security for admin

    

    \# IP whitelist (optional, for extra security)

    \# allow 1.2.3.4;  \# Your IP

    \# deny all;

    

    location / {

        proxy\_pass http://frontend;

        proxy\_http\_version 1.1;

        proxy\_set\_header Upgrade $http\_upgrade;

        proxy\_set\_header Connection 'upgrade';

        proxy\_set\_header Host $host;

        proxy\_set\_header X-Real-IP $remote\_addr;

        proxy\_set\_header X-Forwarded-For $proxy\_add\_x\_forwarded\_for;

        proxy\_set\_header X-Forwarded-Proto $scheme;

        proxy\_cache\_bypass $http\_upgrade;

    }

}

\# Video Embed Domain

server {

    listen 443 ssl http2;

    server\_name embed.yourplatform.com;

    

    ssl\_certificate /etc/letsencrypt/live/yourplatform.com/fullchain.pem;

    ssl\_certificate\_key /etc/letsencrypt/live/yourplatform.com/privkey.pem;

    

    \# Allow embedding in iframes

    add\_header X-Frame-Options "ALLOWALL";

    add\_header Content-Security-Policy "frame-ancestors \*";

    

    location / {

        proxy\_pass http://frontend;

        proxy\_http\_version 1.1;

        proxy\_set\_header Host $host;

        proxy\_set\_header X-Real-IP $remote\_addr;

        proxy\_set\_header X-Forwarded-For $proxy\_add\_x\_forwarded\_for;

        proxy\_set\_header X-Forwarded-Proto $scheme;

    }

}

---

# PART 7: DEVELOPMENT ROADMAP

## 7.1 Phase-wise Development Plan

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         DEVELOPMENT ROADMAP                                      │

│                                                                                  │

│  Total Estimated Duration: 16-20 weeks (4-5 months)                             │

│  Team Size Recommendation: 2 Backend \+ 2 Frontend \+ 1 DevOps/QA                 │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║  PHASE 1: FOUNDATION (Weeks 1-4)                                                 ║

║  Focus: Core infrastructure, authentication, basic tenant management             ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  WEEK 1: Project Setup & Architecture                                            ║

║  ─────────────────────────────────────                                           ║

║  Backend:                                                                         ║

║  □ Set up ASP.NET Core solution structure                                        ║

║  □ Configure Entity Framework Core with PostgreSQL                               ║

║  □ Set up project layers (API, Application, Domain, Infrastructure)             ║

║  □ Configure dependency injection                                                ║

║  □ Set up logging (Serilog)                                                      ║

║  □ Create base entity classes and repository patterns                            ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Initialize Next.js project with TypeScript                                    ║

║  □ Set up Tailwind CSS and shadcn/ui                                            ║

║  □ Configure ESLint, Prettier                                                    ║

║  □ Create base layout components                                                 ║

║  □ Set up API client (Axios wrapper)                                             ║

║  □ Configure Zustand for state management                                        ║

║                                                                                   ║

║  DevOps:                                                                          ║

║  □ Set up Git repository with branching strategy                                 ║

║  □ Create Docker development environment                                         ║

║  □ Set up PostgreSQL and Redis containers                                        ║

║  □ Configure local HTTPS with mkcert                                             ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 2: Database Schema & Authentication                                        ║

║  ─────────────────────────────────────────                                       ║

║  Backend:                                                                         ║

║  □ Create all database migrations (system tables)                                ║

║  □ Implement tenant resolution middleware                                        ║

║  □ Build JWT authentication service                                              ║

║  □ Implement refresh token mechanism                                             ║

║  □ Create user registration/login endpoints                                      ║

║  □ Implement password hashing (BCrypt)                                           ║

║  □ Build email verification flow                                                 ║

║  □ Implement 2FA (email-based) service                                           ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build login page and form                                                     ║

║  □ Build registration page                                                       ║

║  □ Implement 2FA verification page                                               ║

║  □ Build forgot/reset password pages                                             ║

║  □ Create auth context and hooks                                                 ║

║  □ Implement protected route middleware                                          ║

║  □ Build email verification page                                                 ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 3: Multi-Tenancy & Super Admin Foundation                                  ║

║  ───────────────────────────────────────────────                                 ║

║  Backend:                                                                         ║

║  □ Implement multi-tenant data filtering                                         ║

║  □ Build tenant CRUD operations                                                  ║

║  □ Create system user (super admin) model                                        ║

║  □ Implement super admin authentication                                          ║

║  □ Build impersonation service                                                   ║

║  □ Create tenant provisioning service (basic)                                   ║

║  □ Implement row-level security helpers                                          ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build super admin login page                                                  ║

║  □ Create super admin layout (different sidebar)                                ║

║  □ Build tenant list page                                                        ║

║  □ Build tenant creation form                                                    ║

║  □ Build basic tenant detail page                                                ║

║  □ Implement impersonation banner and flow                                       ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 4: RBAC & User Management                                                  ║

║  ───────────────────────────────────                                             ║

║  Backend:                                                                         ║

║  □ Implement full RBAC system                                                    ║

║  □ Create permission checking middleware                                         ║

║  □ Build role CRUD endpoints                                                     ║

║  □ Implement user invitation system                                              ║

║  □ Build user management endpoints                                               ║

║  □ Create default roles seeder                                                   ║

║  □ Implement permission resolution logic                                         ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build team/user list page                                                     ║

║  □ Build user invitation form                                                    ║

║  □ Build role management page                                                    ║

║  □ Create permission matrix component                                            ║

║  □ Build accept invitation page                                                  ║

║  □ Implement permission-based UI hiding                                          ║

║                                                                                   ║

║  Deliverables:                                                                    ║

║  ✓ Working authentication system                                                ║

║  ✓ Multi-tenant database with row-level isolation                               ║

║  ✓ Super admin can create and manage tenants                                    ║

║  ✓ Super admin can impersonate tenant users                                     ║

║  ✓ Full RBAC system operational                                                 ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║  PHASE 2: BUNNY.NET INTEGRATION (Weeks 5-8)                                      ║

║  Focus: Video management, upload, playback \- all Bunny.net features              ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  WEEK 5: Bunny.net Client & Video Library                                        ║

║  ──────────────────────────────────────────                                      ║

║  Backend:                                                                         ║

║  □ Create Bunny.net API client wrapper                                           ║

║  □ Implement video library creation for tenants                                  ║

║  □ Build video list endpoint (with pagination, filtering)                        ║

║  □ Implement video CRUD operations                                               ║

║  □ Create response transformation (hide Bunny.net references)                   ║

║  □ Build URL rewriting for CDN links                                             ║

║  □ Implement signed URL generation                                               ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build video list page with grid/list view                                    ║

║  □ Create video card component                                                   ║

║  □ Build video filters and search                                                ║

║  □ Create video detail page                                                      ║

║  □ Build video edit form                                                         ║

║  □ Implement video deletion with confirmation                                    ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 6: Video Upload System                                                     ║

║  ─────────────────────────────                                                   ║

║  Backend:                                                                         ║

║  □ Implement TUS protocol endpoints                                              ║

║  □ Create upload session management                                              ║

║  □ Build direct upload to Bunny.net                                             ║

║  □ Implement URL pull (import from URL)                                          ║

║  □ Set up Bunny.net webhook receiver                                             ║

║  □ Build encoding status update handler                                          ║

║  □ Create video metadata extraction on completion                                ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build upload page with drag-and-drop                                         ║

║  □ Implement TUS upload client (tus-js-client)                                  ║

║  □ Create upload progress tracking                                               ║

║  □ Build upload queue management                                                 ║

║  □ Implement resume interrupted uploads                                          ║

║  □ Build URL import form                                                         ║

║  □ Create real-time encoding status display                                      ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 7: Collections, Captions, Chapters                                         ║

║  ───────────────────────────────────────                                         ║

║  Backend:                                                                         ║

║  □ Implement collection CRUD endpoints                                           ║

║  □ Build hierarchical collection structure                                       ║

║  □ Sync collections with Bunny.net                                               ║

║  □ Implement caption upload/management                                           ║

║  □ Build chapter CRUD endpoints                                                  ║

║  □ Create thumbnail management endpoints                                         ║

║  □ Implement custom thumbnail upload                                             ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build collection tree view                                                    ║

║  □ Create collection management page                                             ║

║  □ Build move videos to collection UI                                            ║

║  □ Create caption manager component                                              ║

║  □ Build chapter editor with timeline                                            ║

║  □ Create thumbnail selector/uploader                                            ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 8: Video Player & Embedding                                                ║

║  ──────────────────────────────────                                              ║

║  Backend:                                                                         ║

║  □ Build embed endpoint with signed URLs                                         ║

║  □ Implement referer validation                                                  ║

║  □ Create viewer authentication system                                           ║

║  □ Build embed code generator                                                    ║

║  □ Implement password protection for videos                                      ║

║  □ Create view tracking endpoint                                                 ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build custom video player (Video.js \+ HLS.js)                                ║

║  □ Implement player customization (colors from branding)                        ║

║  □ Create chapter navigation in player                                           ║

║  □ Build caption display                                                         ║

║  □ Create embed page (standalone player)                                         ║

║  □ Build embed code generator UI                                                 ║

║  □ Implement embed domain management                                             ║

║                                                                                   ║

║  Deliverables:                                                                    ║

║  ✓ Full video CRUD with Bunny.net (abstracted)                                  ║

║  ✓ Working upload system (direct \+ URL pull)                                    ║

║  ✓ Collections with hierarchy                                                    ║

║  ✓ Captions and chapters management                                              ║

║  ✓ Custom-branded video player                                                   ║

║  ✓ Embeddable player with domain protection                                     ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║  PHASE 3: BILLING & SUBSCRIPTIONS (Weeks 9-11)                                   ║

║  Focus: Stripe integration, subscription management, usage billing               ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  WEEK 9: Stripe Integration & Plans                                              ║

║  ────────────────────────────────────                                            ║

║  Backend:                                                                         ║

║  □ Set up Stripe SDK integration                                                 ║

║  □ Create subscription plan management                                           ║

║  □ Build Stripe product/price sync                                               ║

║  □ Implement checkout session creation                                           ║

║  □ Build Stripe webhook handler                                                  ║

║  □ Create subscription lifecycle management                                      ║

║  □ Implement plan change (upgrade/downgrade)                                     ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build pricing page (public)                                                   ║

║  □ Create plan comparison component                                              ║

║  □ Build subscription management page                                            ║

║  □ Implement Stripe checkout redirect                                            ║

║  □ Build plan change confirmation                                                ║

║                                                                                   ║

║  Admin:                                                                           ║

║  □ Build plan management page                                                    ║

║  □ Create plan creation/edit form                                                ║

║  □ Build feature and limit configuration                                         ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 10: Usage Tracking & Multipliers                                           ║

║  ───────────────────────────────────────                                         ║

║  Backend:                                                                         ║

║  □ Build usage sync job (from Bunny.net)                                        ║

║  □ Implement usage multiplier system                                             ║

║  □ Create usage calculation service                                              ║

║  □ Build usage limit checking                                                    ║

║  □ Implement overage calculation                                                 ║

║  □ Create usage alert notifications                                              ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build usage dashboard for tenants                                             ║

║  □ Create usage meters/progress bars                                             ║

║  □ Implement usage alerts display                                                ║

║                                                                                   ║

║  Admin:                                                                           ║

║  □ Build usage multiplier management page                                        ║

║  □ Create multiplier preview tool                                                ║

║  □ Build tenant usage overview (actual vs displayed)                            ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 11: Invoicing & Manual Payments                                            ║

║  ──────────────────────────────────────                                          ║

║  Backend:                                                                         ║

║  □ Build invoice generation service                                              ║

║  □ Implement automatic invoice creation                                          ║

║  □ Create manual payment recording                                               ║

║  □ Build invoice PDF generation                                                  ║

║  □ Implement credit system                                                       ║

║  □ Create overage billing                                                        ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build invoice list page                                                       ║

║  □ Create invoice detail view                                                    ║

║  □ Implement invoice PDF download                                                ║

║  □ Build payment method management                                               ║

║                                                                                   ║

║  Admin:                                                                           ║

║  □ Build invoice management (all tenants)                                       ║

║  □ Create manual payment form                                                    ║

║  □ Build credit issuing form                                                     ║

║  □ Implement subscription override                                               ║

║                                                                                   ║

║  Deliverables:                                                                    ║

║  ✓ Full Stripe subscription integration                                          ║

║  ✓ Subscription plan management                                                  ║

║  ✓ Usage tracking with multipliers                                               ║

║  ✓ Automatic invoicing with overage                                              ║

║  ✓ Manual payment support                                                        ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║  PHASE 4: ANALYTICS & REPORTING (Weeks 12-13)                                    ║

║  Focus: Video analytics, platform analytics, exports                             ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  WEEK 12: Tenant Analytics                                                       ║

║  ───────────────────────────                                                     ║

║  Backend:                                                                         ║

║  □ Build analytics aggregation job                                               ║

║  □ Create video analytics endpoints                                              ║

║  □ Implement usage analytics endpoints                                           ║

║  □ Build geography/device breakdown                                              ║

║  □ Create date range filtering                                                   ║

║  □ Implement analytics caching                                                   ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build analytics dashboard                                                     ║

║  □ Create views/watch time charts (Recharts)                                    ║

║  □ Build usage charts                                                            ║

║  □ Create geography map component                                                ║

║  □ Build device breakdown chart                                                  ║

║  □ Implement date range picker                                                   ║

║  □ Create top videos table                                                       ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 13: Platform Analytics & Exports                                           ║

║  ───────────────────────────────────────                                         ║

║  Backend:                                                                         ║

║  □ Build platform-level analytics                                                ║

║  □ Create revenue analytics                                                      ║

║  □ Implement tenant growth metrics                                               ║

║  □ Build analytics export job                                                    ║

║  □ Create CSV/Excel export service                                               ║

║  □ Implement scheduled reports                                                   ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build analytics export UI                                                     ║

║  □ Implement export format selector                                              ║

║                                                                                   ║

║  Admin:                                                                           ║

║  □ Build platform analytics dashboard                                            ║

║  □ Create revenue charts                                                         ║

║  □ Build tenant growth charts                                                    ║

║  □ Implement platform usage overview                                             ║

║                                                                                   ║

║  Deliverables:                                                                    ║

║  ✓ Full tenant analytics dashboard                                               ║

║  ✓ Video-level analytics                                                         ║

║  ✓ Platform-wide analytics for super admin                                       ║

║  ✓ Export functionality                                                          ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║  PHASE 5: SUPPORT SYSTEM (Weeks 14-15)                                           ║

║  Focus: Ticket system, knowledge base, escalations                               ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  WEEK 14: Ticket System                                                          ║

║  ────────────────────────                                                        ║

║  Backend:                                                                         ║

║  □ Build ticket CRUD endpoints                                                   ║

║  □ Implement department management                                               ║

║  □ Create ticket assignment logic                                                ║

║  □ Build message/conversation system                                             ║

║  □ Implement ticket status workflow                                              ║

║  □ Create canned responses                                                       ║

║  □ Build internal notes feature                                                  ║

║                                                                                   ║

║  Frontend (Tenant):                                                               ║

║  □ Build ticket list page                                                        ║

║  □ Create ticket submission form                                                 ║

║  □ Build ticket conversation view                                                ║

║  □ Implement file attachments                                                    ║

║                                                                                   ║

║  Admin:                                                                           ║

║  □ Build ticket management dashboard                                             ║

║  □ Create ticket detail with admin actions                                      ║

║  □ Build department management                                                   ║

║  □ Implement canned response manager                                             ║

║  □ Create ticket assignment UI                                                   ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 15: Knowledge Base & Escalations                                           ║

║  ───────────────────────────────────────                                         ║

║  Backend:                                                                         ║

║  □ Build knowledge base CRUD                                                     ║

║  □ Implement category management                                                 ║

║  □ Create article search                                                         ║

║  □ Build escalation rule engine                                                  ║

║  □ Implement SLA tracking                                                        ║

║  □ Create support analytics                                                      ║

║                                                                                   ║

║  Frontend (Tenant):                                                               ║

║  □ Build knowledge base browser                                                  ║

║  □ Create article view page                                                      ║

║  □ Implement search functionality                                                ║

║  □ Build article feedback                                                        ║

║                                                                                   ║

║  Admin:                                                                           ║

║  □ Build KB article editor (TipTap)                                             ║

║  □ Create category manager                                                       ║

║  □ Build escalation rule configuration                                           ║

║  □ Create support analytics dashboard                                            ║

║                                                                                   ║

║  Deliverables:                                                                    ║

║  ✓ Full ticketing system                                                         ║

║  ✓ Knowledge base with search                                                    ║

║  ✓ Escalation rules                                                              ║

║  ✓ Support analytics                                                             ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║  PHASE 6: ADDITIONAL FEATURES & POLISH (Weeks 16-18)                             ║

║  Focus: Webhooks, migrations, settings, audit logs, notifications                ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  WEEK 16: Webhooks & API Keys                                                    ║

║  ──────────────────────────────                                                  ║

║  Backend:                                                                         ║

║  □ Build webhook endpoint management                                             ║

║  □ Implement webhook delivery system                                             ║

║  □ Create webhook signature verification                                         ║

║  □ Build API key generation and management                                       ║

║  □ Implement API key authentication                                              ║

║  □ Create API rate limiting per key                                              ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build webhook management page                                                 ║

║  □ Create webhook testing UI                                                     ║

║  □ Build delivery history view                                                   ║

║  □ Create API key management page                                                ║

║  □ Implement key permission scoping UI                                           ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 17: Video Migration & Settings                                             ║

║  ─────────────────────────────────────                                           ║

║  Backend:                                                                         ║

║  □ Build migration job processor                                                 ║

║  □ Implement YouTube URL parser                                                  ║

║  □ Implement Vimeo URL parser                                                    ║

║  □ Create bulk URL import                                                        ║

║  □ Build branding settings endpoints                                             ║

║  □ Create watermark management                                                   ║

║  □ Implement player settings                                                     ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build migration wizard                                                        ║

║  □ Create migration progress tracking                                            ║

║  □ Build branding settings page                                                  ║

║  □ Create watermark manager                                                      ║

║  □ Build player customization preview                                            ║

║                                                                                   ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║                                                                                   ║

║  WEEK 18: Audit Logs, Notifications & Polish                                     ║

║  ─────────────────────────────────────────                                       ║

║  Backend:                                                                         ║

║  □ Implement comprehensive audit logging                                         ║

║  □ Build audit log query endpoints                                               ║

║  □ Complete email notification system                                            ║

║  □ Create all email templates                                                    ║

║  □ Implement in-app notifications                                                ║

║  □ Final API documentation (Swagger)                                             ║

║                                                                                   ║

║  Frontend:                                                                        ║

║  □ Build audit log viewer                                                        ║

║  □ Complete notification bell                                                    ║

║  □ UI polish and consistency check                                               ║

║  □ Responsive design fixes                                                       ║

║  □ Loading states and error handling                                             ║

║  □ Build empty states                                                            ║

║                                                                                   ║

║  Admin:                                                                           ║

║  □ Build email template editor                                                   ║

║  □ Create platform settings page                                                 ║

║  □ Build system user management                                                  ║

║                                                                                   ║

║  Deliverables:                                                                    ║

║  ✓ Webhook system for tenants                                                    ║

║  ✓ API key management                                                            ║

║  ✓ Video migration from YouTube/Vimeo                                           ║

║  ✓ Full branding customization                                                   ║

║  ✓ Comprehensive audit logs                                                      ║

║  ✓ Email notification system                                                     ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║  PHASE 7: TESTING & DEPLOYMENT (Weeks 19-20)                                     ║

║  Focus: QA, security testing, production deployment                              ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  WEEK 19: Testing & Security                                                     ║

║  ─────────────────────────────                                                   ║

║  □ Unit testing for critical services                                            ║

║  □ Integration testing for API endpoints                                         ║

║  □ End-to-end testing for critical flows                                        ║

║  □ Security audit (OWASP top 10\)                                                ║

║  □ SQL injection testing                                                         ║

║  □ XSS testing                                                                   ║

║  □ Authentication/authorization testing                                          ║

║  □ Performance testing                                                           ║

║  □ Load testing                                                                  ║

║  □ Bug fixing                                                                    ║

║                                                                                   ║

║  WEEK 20: Production Deployment                                                  ║

║  ────────────────────────────────                                                ║

║  □ Set up production VPS                                                         ║

║  □ Configure Docker production environment                                       ║

║  □ Set up SSL certificates                                                       ║

║  □ Configure backups (automated)                                                 ║

║  □ Set up monitoring (UptimeRobot or similar)                                   ║

║  □ Configure log aggregation                                                     ║

║  □ Final security hardening                                                      ║

║  □ DNS configuration                                                             ║

║  □ Stripe live mode setup                                                        ║

║  □ Bunny.net production configuration                                            ║

║  □ Final testing in production                                                   ║

║  □ Documentation completion                                                      ║

║  □ Launch\! 🚀                                                                    ║

║                                                                                   ║

║  Deliverables:                                                                    ║

║  ✓ Fully tested application                                                      ║

║  ✓ Production-ready deployment                                                   ║

║  ✓ Monitoring and alerting                                                       ║

║  ✓ Backup system                                                                 ║

║  ✓ Complete documentation                                                        ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

---

# PART 8: SECURITY GUIDELINES

## 8.1 Security Best Practices

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         SECURITY IMPLEMENTATION GUIDE                            │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           AUTHENTICATION SECURITY                                ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Password Security:                                                              ║

║  ──────────────────                                                              ║

║  • Use BCrypt with cost factor 12 minimum                                        ║

║  • Minimum password length: 8 characters                                         ║

║  • Require mix of uppercase, lowercase, numbers                                  ║

║  • Check against common password lists (optional)                                ║

║  • Implement account lockout after 5 failed attempts (15 min lockout)           ║

║                                                                                   ║

║  JWT Security:                                                                   ║

║  ─────────────                                                                   ║

║  • Use RS256 or HS256 with 256-bit secret minimum                               ║

║  • Short access token expiry (15 minutes)                                        ║

║  • Refresh tokens stored in database (revocable)                                ║

║  • Include tenant\_id in token to prevent cross-tenant access                    ║

║  • Validate token signature on every request                                     ║

║  • Implement token refresh rotation                                              ║

║                                                                                   ║

║  Session Security:                                                               ║

║  ─────────────────                                                               ║

║  • Store session data in Redis (not in JWT)                                     ║

║  • Implement device fingerprinting                                               ║

║  • Allow users to view and revoke sessions                                       ║

║  • Auto-expire sessions after 7 days of inactivity                              ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           DATA SECURITY                                           ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Multi-Tenancy Security:                                                         ║

║  ───────────────────────                                                         ║

║  • ALWAYS filter by tenant\_id in queries (global query filter in EF)            ║

║  • Validate tenant\_id from JWT matches request context                          ║

║  • Never trust tenant\_id from client \- always from token                        ║

║  • Audit log all cross-tenant access (impersonation)                            ║

║  • Use separate schemas/databases for sensitive enterprise tenants              ║

║                                                                                   ║

║  Encryption:                                                                     ║

║  ───────────                                                                     ║

║  • Encrypt sensitive fields at rest (API keys, Bunny credentials)               ║

║  • Use AES-256-GCM for field-level encryption                                   ║

║  • Store encryption keys in environment variables (not in code/DB)              ║

║  • All traffic over HTTPS (TLS 1.2+)                                            ║

║  • Database connections over SSL                                                 ║

║                                                                                   ║

║  API Keys:                                                                       ║

║  ─────────                                                                       ║

║  • Generate with cryptographically secure random generator                      ║

║  • Store only hash in database (like passwords)                                 ║

║  • Show full key only once at creation                                          ║

║  • Include prefix for identification (sv\_live\_, sv\_test\_)                       ║

║  • Implement key rotation capability                                             ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           INPUT VALIDATION                                        ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  General Rules:                                                                  ║

║  ──────────────                                                                  ║

║  • Validate ALL input on the server (never trust client validation)             ║

║  • Use FluentValidation for complex validation rules                            ║

║  • Sanitize HTML input (if any rich text allowed)                               ║

║  • Validate file uploads (type, size, content)                                  ║

║  • Use parameterized queries (EF Core does this automatically)                  ║

║                                                                                   ║

║  SQL Injection Prevention:                                                       ║

║  ─────────────────────────                                                       ║

║  • Use Entity Framework Core (parameterized by default)                         ║

║  • Never concatenate user input in SQL                                          ║

║  • Avoid raw SQL queries; if needed, use parameterized                          ║

║                                                                                   ║

║  XSS Prevention:                                                                 ║

║  ───────────────                                                                 ║

║  • React/Next.js escapes by default                                             ║

║  • Be careful with dangerouslySetInnerHTML                                      ║

║  • Sanitize any user-generated HTML (knowledge base articles)                   ║

║  • Set Content-Security-Policy headers                                          ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           API SECURITY                                            ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Rate Limiting:                                                                  ║

║  ──────────────                                                                  ║

║  • Implement per-tenant rate limits                                              ║

║  • Implement per-API-key rate limits                                             ║

║  • Use sliding window algorithm                                                  ║

║  • Return 429 with Retry-After header                                           ║

║  • Different limits for different endpoints (upload vs. read)                   ║

║                                                                                   ║

║  CORS:                                                                           ║

║  ─────                                                                           ║

║  • Whitelist specific origins (not \*)                                           ║

║  • Include tenant subdomains dynamically                                         ║

║  • Restrict methods and headers                                                  ║

║                                                                                   ║

║  Webhook Security:                                                               ║

║  ─────────────────                                                               ║

║  • Sign all outgoing webhooks with HMAC-SHA256                                  ║

║  • Include timestamp to prevent replay attacks                                   ║

║  • Verify Bunny.net and Stripe webhook signatures                               ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

---

# PART 9: TESTING STRATEGY

## 9.1 Testing Approach

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         TESTING STRATEGY                                         │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           UNIT TESTING                                            ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Backend (xUnit \+ Moq):                                                          ║

║  ──────────────────────                                                          ║

║  • Test all service layer methods                                                ║

║  • Mock external dependencies (Bunny.net, Stripe, SMTP)                         ║

║  • Test RBAC permission checking                                                 ║

║  • Test usage multiplier calculations                                            ║

║  • Test billing calculations                                                     ║

║  • Test JWT token generation/validation                                          ║

║                                                                                   ║

║  Coverage Target: 80%+ for critical services                                     ║

║                                                                                   ║

║  Frontend (Jest \+ React Testing Library):                                        ║

║  ─────────────────────────────────────────                                       ║

║  • Test custom hooks                                                             ║

║  • Test utility functions                                                        ║

║  • Test form validation                                                          ║

║  • Test component rendering                                                      ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           INTEGRATION TESTING                                     ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  API Integration Tests:                                                          ║

║  ──────────────────────                                                          ║

║  • Test complete API endpoints                                                   ║

║  • Use test database (in-memory or test PostgreSQL)                             ║

║  • Test authentication flows                                                     ║

║  • Test multi-tenant data isolation                                              ║

║  • Test webhook receiving                                                        ║

║                                                                                   ║

║  Key Test Scenarios:                                                             ║

║  ────────────────────                                                            ║

║  □ Tenant A cannot access Tenant B's videos                                     ║

║  □ User without permission cannot perform action                                ║

║  □ Subscription limits are enforced                                             ║

║  □ Usage multipliers are applied correctly                                      ║

║  □ Impersonation creates proper audit trail                                     ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

╔═══════════════════════════════════════════════════════════════════════════════════╗

║                           END-TO-END TESTING                                      ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  Tools: Playwright or Cypress                                                    ║

║                                                                                   ║

║  Critical Flows to Test:                                                         ║

║  ────────────────────────                                                        ║

║  □ New tenant registration and onboarding                                       ║

║  □ Video upload complete flow                                                   ║

║  □ Video playback from embed                                                    ║

║  □ Subscription purchase via Stripe                                             ║

║  □ Support ticket creation and response                                         ║

║  □ Super admin impersonation                                                    ║

║  □ User invitation acceptance                                                   ║

║  □ Password reset flow                                                          ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

---

# PART 10: OPEN SOURCE TOOLS SUMMARY

## 10.1 Complete Technology Stack

┌─────────────────────────────────────────────────────────────────────────────────┐

│                    COMPLETE OPEN SOURCE TECHNOLOGY STACK                         │

└─────────────────────────────────────────────────────────────────────────────────┘

╔═══════════════════════════════════════════════════════════════════════════════════╗

║  CATEGORY              TOOL                    LICENSE        PURPOSE            ║

╠═══════════════════════════════════════════════════════════════════════════════════╣

║                                                                                   ║

║  BACKEND FRAMEWORK                                                               ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Runtime              .NET 8                   MIT            Core runtime       ║

║  Framework            ASP.NET Core 8           MIT            Web API           ║

║  ORM                  Entity Framework Core    MIT            Database ORM      ║

║  Validation           FluentValidation         Apache 2.0     Request validation║

║  Mapping              AutoMapper               MIT            Object mapping    ║

║  Background Jobs      Hangfire                 LGPL           Job scheduling    ║

║  Logging              Serilog                  Apache 2.0     Structured logs   ║

║  HTTP Client          Refit                    MIT            API client gen    ║

║  Password Hashing     BCrypt.Net               MIT            Password security ║

║  JWT                  System.IdentityModel     MIT            Token handling    ║

║                                                                                   ║

║  FRONTEND FRAMEWORK                                                              ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Framework            Next.js 14               MIT            React framework   ║

║  UI Library           React 18                 MIT            UI components     ║

║  Styling              Tailwind CSS             MIT            Utility CSS       ║

║  Components           shadcn/ui                MIT            UI components     ║

║  State (Client)       Zustand                  MIT            State management  ║

║  State (Server)       TanStack Query           MIT            Server state      ║

║  Forms                React Hook Form          MIT            Form handling     ║

║  Validation           Zod                      MIT            Schema validation ║

║  Charts               Recharts                 MIT            Charts/graphs     ║

║  Tables               TanStack Table           MIT            Data tables       ║

║  Icons                Lucide React             ISC            Icon library      ║

║  Date Utils           date-fns                 MIT            Date manipulation ║

║  Markdown             react-markdown           MIT            MD rendering      ║

║  Rich Text            TipTap                   MIT            WYSIWYG editor    ║

║                                                                                   ║

║  VIDEO PLAYER                                                                    ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Player               Video.js                 Apache 2.0     Video player      ║

║  HLS Support          HLS.js                   Apache 2.0     HLS streaming     ║

║  Upload               tus-js-client            MIT            Resumable uploads ║

║  Dropzone             react-dropzone           MIT            Drag & drop       ║

║                                                                                   ║

║  DATABASE & CACHE                                                                ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Database             PostgreSQL 15            PostgreSQL     Primary database  ║

║  Cache                Redis 7                  BSD            Caching & queues  ║

║                                                                                   ║

║  WEB SERVER                                                                      ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Reverse Proxy        Nginx                    BSD            Load balancing    ║

║  SSL                  Let's Encrypt            Free           SSL certificates  ║

║  Certbot              Certbot                  Apache 2.0     Cert automation   ║

║                                                                                   ║

║  CONTAINERIZATION                                                                ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Container Runtime    Docker                   Apache 2.0     Containers        ║

║  Orchestration        Docker Compose           Apache 2.0     Multi-container   ║

║                                                                                   ║

║  DEVELOPMENT TOOLS                                                               ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  API Docs             Swagger/OpenAPI          Apache 2.0     API documentation ║

║  Testing (Backend)    xUnit                    Apache 2.0     Unit testing      ║

║  Mocking              Moq                      BSD            Mock objects      ║

║  Testing (Frontend)   Jest                     MIT            Unit testing      ║

║  E2E Testing          Playwright               Apache 2.0     Browser testing   ║

║                                                                                   ║

║  EMAIL                                                                           ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  SMTP Client          MailKit                  MIT            Email sending     ║

║  Templates            Scriban                  BSD            Email templates   ║

║                                                                                   ║

║  EXTERNAL SERVICES (Paid, but required)                                          ║

║  ─────────────────────────────────────────────────────────────────────────────── ║

║  Video CDN            Bunny.net Stream         Paid           Video hosting     ║

║  Payments             Stripe                   Paid           Payment processing║

║  SMTP                 Mailgun/Sendgrid         Paid\*          Email delivery    ║

║                       or Self-hosted                                            ║

║                                                                                   ║

║  \* Can use free tier or self-hosted SMTP                                        ║

║                                                                                   ║

╚═══════════════════════════════════════════════════════════════════════════════════╝

---

# FINAL NOTES

## Key Success Factors

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         KEY SUCCESS FACTORS                                      │

└─────────────────────────────────────────────────────────────────────────────────┘

1\. BUNNY.NET ABSTRACTION IS CRITICAL

   ─────────────────────────────────

   • Never expose Bunny.net URLs, IDs, or terminology to tenants

   • All API responses must be transformed

   • Player must not reveal Bunny branding

   • Error messages must be sanitized

2\. MULTI-TENANCY SECURITY

   ───────────────────────

   • Test data isolation extensively

   • Always filter by tenant\_id from authenticated token

   • Audit logs for all sensitive operations

3\. USAGE MULTIPLIER TESTING

   ─────────────────────────

   • Ensure multipliers apply correctly everywhere

   • Test billing calculations with multipliers

   • Provide admin preview tools

4\. SCALABILITY CONSIDERATIONS

   ───────────────────────────

   • Start with single VPS, but design for scale

   • Use connection pooling for database

   • Cache aggressively (Redis)

   • Consider CDN for static assets

5\. MONITORING & ALERTING

   ──────────────────────

   • Set up UptimeRobot or similar for uptime

   • Monitor Bunny.net status

   • Alert on failed jobs, payment failures

   • Log aggregation for debugging

## Documentation Checklist

┌─────────────────────────────────────────────────────────────────────────────────┐

│                         DOCUMENTATION CHECKLIST                                  │

└─────────────────────────────────────────────────────────────────────────────────┘

□ API Documentation (Swagger/OpenAPI)

□ Database Schema Documentation

□ Environment Setup Guide

□ Deployment Guide

□ Admin User Manual

□ Tenant User Manual

□ API Integration Guide (for tenants)

□ Webhook Events Documentation

□ Troubleshooting Guide

□ Security Best Practices

□ Backup & Recovery Procedures

---

**This completes the comprehensive development plan for your SaaS Multi-Tenant DRM Video Hosting Platform.**

The plan covers:

- ✅ Complete system architecture  
- ✅ Database schema with all tables  
- ✅ All API endpoints (tenant \+ admin)  
- ✅ Frontend structure and components  
- ✅ Background job definitions  
- ✅ Deployment architecture for VPS  
- ✅ 20-week development roadmap  
- ✅ Security guidelines  
- ✅ Testing strategy  
- ✅ Complete open-source tech stack

**Total Estimated Development Time:** 16-20 weeks with a team of 4-5 developers


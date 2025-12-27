# StreamVault Backend (ASP.NET Core)

## Goals
- ASP.NET Core 8 Web API (multi-tenant, JWT + refresh, Bunny proxy layer).
- EF Core with Npgsql for master DB; Redis for cache/rate limits; Hangfire for jobs.

## Quick start (local, via docker-compose)
1. Copy `env.example` to `.env` (or use `dotnet user-secrets`).
2. From repo root: `docker-compose up --build`
3. API exposed at `http://localhost:5000` (Kestrel behind docker).

## Manual dev (outside Docker)
1. `dotnet new webapi` in this folder (if not yet scaffolded).
2. Add packages: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.Extensions.Caching.StackExchangeRedis`, `Hangfire`, `Hangfire.AspNetCore`, `FluentValidation.AspNetCore`, `Swashbuckle.AspNetCore`.
3. Add EF Core context for master DB and run first migration: `dotnet ef migrations add InitialMasterSchema` then `dotnet ef database update` (pointing to Postgres in docker-compose).
4. Run: `dotnet run --project StreamVault.Api.csproj`.

## Project layout (planned)
- `StreamVault.Api/` – controllers (V1, Admin, Public), middleware (tenant resolution, exception, logging, rate limiting, API key), filters (RequirePermission, ValidateTenantSubscription, AuditLog), program setup.
- `StreamVault.Application/` – services (Videos/Uploads/Collections/Users/Auth/Analytics/Billing/Tenants/Support/Notifications/Webhooks/Migration).
- `StreamVault.Domain/` – entities, value objects, permissions, enums.
- `StreamVault.Infrastructure/` – EF Core, repositories, Bunny client abstraction, Stripe client, SMTP email sender, Redis cache, Hangfire jobs.

## Environment variables (see env.example)
- `POSTGRES_CONNECTION_STRING`, `REDIS_CONNECTION_STRING`
- `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_SIGNING_KEY`
- `SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_PASS`, `SMTP_FROM`
- `STRIPE_SECRET_KEY`, `STRIPE_WEBHOOK_SECRET`
- `BUNNY_API_KEY`, `BUNNY_LIBRARY_ID`, `BUNNY_PULL_ZONE_ID`, `BUNNY_CDN_HOSTNAME`
- `STORAGE_PROVIDER`, `MINIO_ENDPOINT`, `MINIO_KEY`, `MINIO_SECRET`, `MINIO_BUCKET`
- `FRONTEND_BASE_URL`

## Next implementation steps
- Scaffold solution + projects as above.
- Add TenantResolution middleware and JWT auth skeleton.
- Add health endpoint hitting Postgres/Redis to validate docker-compose wiring.
- Wire Hangfire with dashboard protected for super admins.

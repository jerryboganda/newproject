# StreamVault Monorepo

## Stack
- Backend: ASP.NET Core 8 (StreamVault.Api) with Application/Infrastructure/Domain projects
- Frontend: Next.js 13 (shadcn admin template planned)
- Infra (local): Postgres, Redis, MinIO via docker-compose

## Quick start
1) Copy env samples
   - Backend: `cd streamvault-backend` → copy `env.example` to `.env` (or use dotnet user-secrets).
   - Frontend: `cd streamvault-frontend` → copy `env.example` to `.env.local`.
2) Install deps
   - Backend: `dotnet restore`
   - Frontend: `npm install`
3) Run locally with Docker (after deps exist):
   - From repo root: `docker-compose up --build`
   - API: http://localhost:5000 (Swagger in Development)
   - Frontend: http://localhost:3000

## Current state
- Backend solution scaffolded with Program.cs exposing /health and Swagger.
- Frontend scaffolded with minimal Next.js app shell; replace with shadcn admin template.

## Next steps (Phase 0 → Phase 1)
- Backend: add DbContext, initial EF Core migration for master schema; add TenantResolution middleware and JWT auth skeleton.
- Frontend: import shadcn admin template, hook NEXT_PUBLIC_API_BASE_URL, and build basic auth/dashboard pages.
- CI: add GitHub Actions for backend (build/test) and frontend (lint/build); add docker build jobs if desired.

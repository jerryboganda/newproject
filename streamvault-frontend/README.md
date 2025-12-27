# StreamVault Frontend (Next.js 13 App Router + shadcn admin template)

## Goals
- Use Vercel shadcn UI admin dashboard template as base (Next.js 13 App Router).
- Integrate with backend API at `NEXT_PUBLIC_API_BASE_URL`.

## Quick start (local, via docker-compose)
1. Copy `env.example` to `.env.local` and set values.
2. From repo root: `docker-compose up --build`.
3. Frontend served at `http://localhost:3000`.

## Manual dev (outside Docker)
1. Install deps: `npm install`.
2. Run dev server: `npm run dev`.
3. Set `.env.local` with `NEXT_PUBLIC_API_BASE_URL` and any CDN host.

## Environment variables (see env.example)
- `NEXT_PUBLIC_API_BASE_URL` (e.g., http://localhost:5000)
- `NEXT_PUBLIC_CDN_HOST` (optional)
- `NEXTAUTH_SECRET` (if/when NextAuth used)

## Next implementation steps
- Import/clone the shadcn admin template into this folder.
- Add API client wrapper pointing to `NEXT_PUBLIC_API_BASE_URL`.
- Wire auth screens (login/register/2FA) and dashboard nav per StreamVault structure.

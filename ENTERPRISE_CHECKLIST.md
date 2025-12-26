# Enterprise Deployment Checklist (Free/Open-Source Stack)

Single source of truth for readiness to ship on Netlify (frontend) + Hetzner CPX11 (backend) with Node.js/Next.js, Postgres, Redis, MinIO, FFmpeg, Nginx, PM2.

## How to use
- Check items with `[x]` as you complete them.
- Each item has a brief acceptance note (Definition of Done).
- Keep this file in sync with runbooks and `FREE_DEPLOYMENT_GUIDE.md`.

---

1) Governance & Access
- [ ] Roles and least-privilege defined across GitHub/Netlify/Hetzner/DB/MinIO; 2FA enforced; SSH key-only on VPS; default passwords rotated.
  - Acceptance: Role matrix stored in repo; password SSH disabled; credentials rotated and recorded.

2) Architecture & Environments
- [ ] Environment layout (local, optional staging, prod on CPX11) and data flows documented.
  - Acceptance: Diagram/checklist in repo shows app/API, Postgres, Redis, MinIO, Nginx/PM2, external endpoints.

3) Secrets & Config
- [ ] .env templates committed; production secrets stored securely on VPS with tight perms.
  - Acceptance: `.env` in .gitignore; unique secrets per env; no secrets in logs or VCS.

4) Local Developer Environment
- [ ] Node 20, pnpm, Postgres, Redis, FFmpeg, MinIO installed (Docker OK); backend/frontend run locally with sample data.
  - Acceptance: `pnpm dev` works for both; healthcheck passes; sample video processed and stored.

5) Backend App Readiness
- [ ] Lint/format/typecheck/tests passing; health endpoint `/api/v1/health` covers DB/Redis.
  - Acceptance: CI green; health returns 200 with dependency checks; flakiness addressed.

6) Frontend Readiness
- [ ] Env-based API URLs; auth/login and upload UI working against target env.
  - Acceptance: Netlify preview builds succeed; CORS/mixed-content clean; API calls succeed.

7) Data Layer (Postgres)
- [ ] Prod DB/user created with least privilege; migrations applied.
  - Acceptance: Migrations run cleanly; strong password; backup command tested once.

8) Cache (Redis)
- [ ] Redis running and not publicly exposed; requirepass if exposure possible; memory cap set if needed.
  - Acceptance: `redis-cli ping` ok; sessions/queues (if used) verified; bind to localhost or firewall.

9) Object Storage (MinIO)
- [ ] MinIO running (9000/9001); bucket `videos` created; keys configured; default creds changed.
  - Acceptance: Upload/list via backend succeeds; console reachable via TLS; bucket private by default.

10) Video Processing (FFmpeg)
- [ ] ffmpeg/ffprobe paths set; sample transcode/thumbnail flow validated end-to-end.
  - Acceptance: Sample upload → processed artifact stored in MinIO; resource use acceptable.

11) Process Management (PM2)
- [ ] PM2 start script for API; startup on reboot; logs accessible; reload strategy defined.
  - Acceptance: `pm2 status` clean; `pm2 save` done; restart works without downtime.

12) Reverse Proxy & TLS (Nginx + Certbot)
- [ ] Nginx site proxies 80/443 to API 4000 with secure headers; Certbot SSL for `api.yourdomain.com`.
  - Acceptance: `nginx -t` passes; HTTP→HTTPS redirect; HSTS set; certificate renews.

13) CI/CD
- [ ] GitHub Actions (or similar) for lint/test on PR; optional deploy gates.
  - Acceptance: CI required on main; secrets protected; failures actionable.

14) Deployment Steps (Prod)
- [ ] Backend deploy runbook: pull, install, build, env, PM2 reload; Frontend: Netlify build `npm run build`, publish `.next`, envs set.
  - Acceptance: Runbook executed successfully once; rollback path documented.

15) Observability & Logs
- [ ] Centralized API/Nginx logs with rotation; basic CPU/RAM/disk metrics and uptime checks.
  - Acceptance: Health monitored; alert for downtime/disk-full; logs free of secrets.

16) Security Hardening
- [ ] SSH hardened (key-only), ufw firewall (80/443/22), root password login disabled; defaults changed.
  - Acceptance: Ports restricted; TLS valid; admin panels not exposed publicly.

17) Performance & Load Basics
- [ ] Smoke and light load tests on login/upload/playback/transcode.
  - Acceptance: Baseline latency/error rate recorded; timeouts sensible; bottlenecks noted.

18) Data Protection & Backups
- [ ] Postgres backup/restore tested; MinIO backup/sync policy decided if required.
  - Acceptance: Restore test succeeds; retention documented; backups encrypted at rest if possible.

19) Compliance & Privacy (Right-Sized)
- [ ] Minimal PII stored; cookie/security settings reviewed; CORS restricted.
  - Acceptance: Privacy note in docs; tokens protected (HttpOnly/SameSite where applicable); over-collection avoided.

20) Runbooks & Documentation
- [ ] Runbooks for deploy, rollback, rotate secrets, renew TLS, restore DB, restart services kept with this file.
  - Acceptance: A new engineer can execute runbooks without tribal knowledge.

21) Release / Go-Live Checklist
- [ ] CI green; health green; SSL, firewall, backups verified; alerts configured; end-to-end upload→transcode→store→playback tested.
  - Acceptance: Sign-off recorded with dates/versions/owner.

22) Post-Go-Live Monitoring
- [ ] 24–48h heightened monitoring; error budget tracked; scaling thresholds defined.
  - Acceptance: Issues triaged; scaling criteria documented (CPU/RAM/disk).

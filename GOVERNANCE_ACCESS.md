# Governance & Access Plan (Local Windows VPS)

Scope: This Windows machine is the production VPS (backend + storage + database + cache + reverse proxy). Frontend stays on Netlify.

## Roles
- Owner/Operator: You (local Admin account on this machine).
- Principle of Least Privilege: Only the Owner has admin; no additional shared accounts.

## Services & Access Controls
- GitHub: Enforce 2FA; restrict repo access to Owner.
- Netlify: Enforce 2FA; deploy access limited to Owner.
- Local VPS (this machine):
  - SSH service: plan to install Windows OpenSSH Server, key-based only, password login disabled.
  - Firewall: allow only 22 (SSH), 80/443 (HTTP/S), 5432 (Postgres) and 6379 (Redis) localhost-only, 9000/9001 (MinIO) ideally localhost-only and proxied via Nginx when exposed.

## Secrets & Config
- Store production secrets in backend `.env` with NTFS permissions restricted to Owner.
- Keep `.env` out of Git; rotate JWT secrets, DB passwords, MinIO keys.
- Back up secrets securely (offline or password manager).

## SSH Key Management (Planned)
- Target authorized keys file: `C:\ProgramData\ssh\administrators_authorized_keys` (for admin-only access).
- Require public key(s) for access; disallow password auth in `sshd_config`.
- Rotate keys periodically; remove unused keys.

## Logging & Monitoring
- OpenSSH logs via Windows Event Log; Nginx/PM2 logs rotated.
- Track access attempts; alert on repeated failures.

## Runbook (to execute with admin rights)
1) Install OpenSSH Server capability.
2) Start and set `sshd` to auto-start.
3) Configure firewall rule for SSH (port 22).
4) Add public key(s) to `administrators_authorized_keys` with correct ACLs.
5) Harden `sshd_config`: disable password auth, disable administrator password login, set logging.
6) Restart `sshd`.

## Outstanding Inputs Needed
- A public SSH key to authorize (or permission to generate a new keypair here and store the private key for you to retrieve securely).

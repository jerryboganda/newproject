# StreamVault - Professional Podman Container Deployment Guide

## Overview
This guide provides professional container-based deployment for StreamVault using Podman with proper networking, health checks, and orchestration.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                  streamvault-network (bridge)              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────┐  ┌──────────────┐  ┌─────────────┐   │
│  │  Frontend        │  │  Backend     │  │  Postgres   │   │
│  │  :3000           │  │  :8080       │  │  :5432      │   │
│  │  Next.js         │  │  ASP.NET     │  │  Database   │   │
│  └─────────┬────────┘  └──────┬───────┘  └──────┬──────┘   │
│            │                   │                  │          │
│            └───────────────────┼──────────────────┘          │
│                                │                             │
│                        ┌────────▼────────┐                  │
│                        │  Redis          │                  │
│                        │  :6379          │                  │
│                        │  Cache          │                  │
│                        └─────────────────┘                  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Prerequisites

1. **Podman Installed**: v5.7.1+
   ```powershell
   C:\Program Files\RedHat\Podman\podman.exe --version
   ```

2. **Podman Machine Running**
   ```powershell
   podman machine list
   podman machine start  # if needed
   ```

3. **Sufficient Disk Space**: ~10GB for images and containers

## Quick Start (One-Step Container Launch)

### Step 1: Create Global Podman Alias

Create `$PROFILE\podman-alias.ps1`:
```powershell
$global:PODMAN = "C:\Program Files\RedHat\Podman\podman.exe"
function podman { & $PODMAN @args }
Export-ModuleMember -Function podman
```

Load it:
```powershell
. "$PROFILE\podman-alias.ps1"
```

### Step 2: Launch All Containers

```powershell
# Navigate to project
cd "c:\Users\Admin\Desktop\newproject"

# Create network
podman network create streamvault-network 2>&1 | Where-Object { $_ -notmatch "already" }

# Start PostgreSQL (if not running)
podman run -d `
  --name streamvault-postgres `
  --network streamvault-network `
  -p 5432:5432 `
  -e POSTGRES_USER=streamvault `
  -e POSTGRES_PASSWORD=StreamVault123! `
  -e POSTGRES_DB=streamvault_db `
  -v postgres_data:/var/lib/postgresql/data `
  --restart unless-stopped `
  postgres:15-alpine

# Start Redis (if not running)
podman run -d `
  --name streamvault-redis `
  --network streamvault-network `
  -p 6379:6379 `
  -v redis_data:/data `
  --restart unless-stopped `
  redis:7-alpine redis-server --requirepass "StreamVault123!"

# Build and start backend
cd streamvault-backend
podman build -t streamvault-backend:latest -f Dockerfile .
cd ..

podman run -d `
  --name streamvault-backend `
  --network streamvault-network `
  -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e ASPNETCORE_URLS=http://+:8080 `
  -e "ConnectionStrings__DefaultConnection=Host=streamvault-postgres;Port=5432;Database=streamvault_db;Username=streamvault;Password=StreamVault123!" `
  -e "Redis__ConnectionString=streamvault-redis:6379,password=StreamVault123!" `
  --restart unless-stopped `
  streamvault-backend:latest

# Build and start frontend
cd streamvault-admin-dashboard
podman build -t streamvault-frontend:latest -f Dockerfile .
cd ..

podman run -d `
  --name streamvault-frontend `
  --network streamvault-network `
  -p 3000:3000 `
  -e NODE_ENV=production `
  -e NEXT_PUBLIC_API_URL=http://streamvault-backend:8080 `
  -e PORT=3000 `
  --restart unless-stopped `
  streamvault-frontend:latest

# Verify all containers are running
podman ps -a
```

## Professional Commands Reference

###  Network Management

```powershell
# Create network
podman network create streamvault-network

# List networks
podman network ls

# Inspect network
podman network inspect streamvault-network

# Connect container to network
podman network connect streamvault-network container-name

# Disconnect from network
podman network disconnect streamvault-network container-name
```

### Image Management

```powershell
# Build image
podman build -t streamvault-backend:latest -f Dockerfile .

# List images
podman images

# Remove image
podman rmi streamvault-backend:latest

# Tag image
podman tag streamvault-backend:latest streamvault-backend:v1.0

# Push to registry (future: Docker Hub, Quay.io)
podman push streamvault-backend:latest docker://docker.io/yourorg/streamvault-backend:latest
```

### Container Lifecycle

```powershell
# Run container
podman run -d --name myapp --network streamvault-network -p 3000:3000 myimage:latest

# List containers
podman ps -a

# Stop container
podman stop container-name

# Start container
podman start container-name

# Restart container
podman restart container-name

# Remove container
podman rm container-name

# Inspect container
podman inspect container-name

# View container logs
podman logs container-name

# Stream logs (follow)
podman logs -f container-name

# Get container IP
podman inspect container-name --format="{{.NetworkSettings.IPAddress}}"
```

### Container Execution

```powershell
# Execute command in running container
podman exec -it container-name bash

# Get container stats
podman stats

# Get container processes
podman top container-name
```

### Health Checks

```powershell
# Check container health
podman inspect container-name --format="{{.State.Health.Status}}"

# View health logs
podman inspect container-name --format="{{.State.Health.Log}}"
```

## Environmental Variables

### Backend (ASP.NET Core)
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=Host=streamvault-postgres;Port=5432;Database=streamvault_db;Username=streamvault;Password=StreamVault123!
Redis__ConnectionString=streamvault-redis:6379,password=StreamVault123!
```

### Frontend (Next.js)
```
NODE_ENV=production
NEXT_PUBLIC_API_URL=http://streamvault-backend:8080
PORT=3000
HOSTNAME=0.0.0.0
```

### Database
```
POSTGRES_USER=streamvault
POSTGRES_PASSWORD=StreamVault123!
POSTGRES_DB=streamvault_db
POSTGRES_INITDB_ARGS=--encoding=UTF8 --locale=en_US.UTF-8
```

### Redis
```
REDIS_PASSWORD=StreamVault123!
```

## Service Endpoints (When Containerized)

| Service | URL | Container DNS |
|---------|-----|---|
| Frontend | http://localhost:3000 | http://streamvault-frontend:3000 |
| Backend API | http://localhost:8080 | http://streamvault-backend:8080 |
| PostgreSQL | localhost:5432 | streamvault-postgres:5432 |
| Redis | localhost:6379 | streamvault-redis:6379 |

## Volume Management

```powershell
# List volumes
podman volume ls

# Create volume
podman volume create postgres_data

# Inspect volume
podman volume inspect postgres_data

# Remove volume
podman volume rm postgres_data

# Mount volume to container
podman run -v postgres_data:/var/lib/postgresql/data ...

# Backup volume
podman run --rm -v postgres_data:/data -v C:\backup:/backup postgres:15-alpine tar czf /backup/postgres.tar.gz -C /data .

# Restore volume
podman run --rm -v postgres_data:/data -v C:\backup:/backup postgres:15-alpine tar xzf /backup/postgres.tar.gz -C /data
```

## Troubleshooting

### Container Won't Start

```powershell
# Check logs
podman logs container-name

# Check container status
podman inspect container-name --format="{{.State}}"

# Try running interactively to see errors
podman run -it image:tag /bin/bash
```

### Network Issues

```powershell
# Test connectivity between containers
podman exec frontend ping streamvault-backend

# Check network configuration
podman network inspect streamvault-network

# Check DNS resolution
podman exec frontend nslookup streamvault-backend
```

### Resource Issues

```powershell
# Check container resource usage
podman stats

# Check Podman machine resources
podman machine inspect

# Increase Podman machine resources
podman machine set --memory 8192 --cpus 4
```

### Image Build Failures

```powershell
# Build with verbose output
podman build -t image:tag --progress=plain .

# Build without cache
podman build -t image:tag --no-cache .

# Check Dockerfile syntax
podman build --dry-run .

# Use buildah for more control
buildah build-using-dockerfile -f Dockerfile -t image:tag .
```

## Performance Optimization

### Build Performance

```dockerfile
# Use .dockerignore to exclude unnecessary files
# Use multi-stage builds
# Layer caching: put frequently changing files last
```

### Runtime Performance

```powershell
# Run with resource limits
podman run -d `
  --memory=512m `
  --cpus=1.0 `
  --pids-limit=500 `
  myimage:latest

# Monitor resource usage
podman stats
```

## Security Best Practices

```powershell
# Run as non-root user (Dockerfiles already configure this)
USER nextjs  # in frontend
USER streamvault  # for backend

# Use secrets for sensitive data
podman secret create db_password db_password.txt
podman run -d --secret db_password myimage:latest

# Scan images for vulnerabilities
podman run --rm -v /var/run/docker.sock:/var/run/docker.sock aquasec/trivy image myimage:latest

# Use read-only filesystems where possible
podman run -d --read-only myimage:latest
```

## Production Deployment Checklist

- [ ] All containers have healthchecks configured
- [ ] All containers have restart policies set
- [ ] Volumes are properly backed up
- [ ] Environmental variables are securely managed
- [ ] Images are signed and verified
- [ ] Container registry is private
- [ ] Logs are aggregated and monitored
- [ ] Resource limits are defined
- [ ] Network policies restrict inter-container traffic
- [ ] Containers run as non-root users

## Cleanup

```powershell
# Remove all containers
podman rm -f $(podman ps -a -q)

# Remove all images
podman rmi -f $(podman images -q)

# Remove all volumes
podman volume rm $(podman volume ls -q)

# Remove all networks
podman network rm $(podman network ls -q)

# Prune everything
podman system prune -a --volumes
```

## Next Steps: Container Orchestration

When ready for production, consider:

1. **Kubernetes**: `podman-kube` for orchestration
2. **Podman Compose**: Multiple environment support
3. **Systemd Integration**: Run containers as systemd services
4. **Monitoring**: Prometheus + Grafana for container metrics
5. **Logging**: ELK Stack or Loki for centralized logging

## References

- [Podman Documentation](https://docs.podman.io)
- [Dockerfile Best Practices](https://docs.docker.com/develop/develop-images/dockerfile_best-practices/)
- [Container Security](https://cheatsheetseries.owasp.org/cheatsheets/Docker_Security_Cheat_Sheet.html)

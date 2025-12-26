# FREE BUNNYSTREAM - QUICK START DEPLOYMENT GUIDE

## 🎯 GOAL: $0-3/month completely free & open-source setup

---

## 📋 STEP 1: LOCAL DEVELOPMENT (Your Computer)

### Install Free Tools:
```bash
# Download these (all FREE):
1. Node.js 20 LTS - https://nodejs.org/
2. PostgreSQL - https://www.postgresql.org/download/
3. Redis - https://redis.io/download
4. VS Code - https://code.visualstudio.com/
5. Git - https://git-scm.com/
6. Docker (optional) - https://docker.com/products/docker-desktop
```

### Setup Project:
```bash
# Clone your repo
git clone https://github.com/YOUR-USERNAME/bunnystream.git
cd bunnystream

# Install dependencies
pnpm install

# OR if you don't have pnpm:
npm install -g pnpm
pnpm install
```

### Start Local Services (Docker - FREE):
```bash
# Start PostgreSQL
docker run -d \
  --name postgres \
  -e POSTGRES_PASSWORD=password \
  -p 5432:5432 \
  postgres:16

# Start Redis
docker run -d \
  --name redis \
  -p 6379:6379 \
  redis:7

# Start MinIO (object storage)
docker run -d \
  --name minio \
  -e MINIO_ROOT_USER=minioadmin \
  -e MINIO_ROOT_PASSWORD=minioadmin \
  -p 9000:9000 \
  -p 9001:9001 \
  minio/minio server /minio/data --console-address ":9001"
```

### Create .env Files:

**backend/.env:**
```env
NODE_ENV=development
PORT=4000
API_PREFIX=api/v1

# Database
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_USER=postgres
DATABASE_PASSWORD=password
DATABASE_NAME=bunnystream
DATABASE_URL=postgresql://postgres:password@localhost:5432/bunnystream

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=
REDIS_URL=redis://localhost:6379

# JWT
JWT_SECRET=your-super-secret-jwt-key-change-this-in-production
JWT_EXPIRATION=7d
JWT_REFRESH_SECRET=your-refresh-secret
JWT_REFRESH_EXPIRATION=30d

# MinIO (instead of Bunny.net)
MINIO_ENDPOINT=localhost
MINIO_PORT=9000
MINIO_ACCESS_KEY=minioadmin
MINIO_SECRET_KEY=minioadmin
MINIO_BUCKET=videos

# CORS
ALLOWED_ORIGINS=http://localhost:3000

# FFmpeg (for video processing)
FFMPEG_PATH=/usr/bin/ffmpeg
FFPROBE_PATH=/usr/bin/ffprobe
```

**frontend/.env.local:**
```env
NEXT_PUBLIC_API_URL=http://localhost:4000/api/v1
NEXTAUTH_SECRET=your-super-secret-nextauth-key-change-this
NEXTAUTH_URL=http://localhost:3000
```

### Run Development Servers:

**Terminal 1 - Backend:**
```bash
cd backend
pnpm install
pnpm dev
# Backend running at http://localhost:4000
```

**Terminal 2 - Frontend:**
```bash
cd frontend
pnpm install
pnpm dev
# Frontend running at http://localhost:3000
```

---

## 🚀 STEP 2: DEPLOY FRONTEND (Netlify - FREE)

### Create Netlify Account:
```
1. Go to https://www.netlify.com
2. Sign up with GitHub (FREE)
3. Authorize GitHub access
```

### Deploy Frontend:
```bash
# In your frontend directory:
cd frontend

# Push to GitHub
git add .
git commit -m "Initial commit"
git push origin main

# Go to Netlify dashboard
# Click "New site from Git"
# Select your GitHub repo
# Build command: npm run build
# Publish directory: .next
# Add env variables:
#   NEXT_PUBLIC_API_URL=https://your-api.domain.com/api/v1
#   NEXTAUTH_SECRET=<generate a random string>
#   NEXTAUTH_URL=https://your-frontend-domain.netlify.app

# Click Deploy!
# Your site is live at: https://yourproject.netlify.app
```

✅ **FRONTEND DEPLOYED - Cost: $0/month**

---

## 🖥️ STEP 3: DEPLOY BACKEND (Hetzner VPS - €2.49/month)

### Create Hetzner Account:
```
1. Go to https://www.hetzner.com/cloud
2. Create account (FREE to start)
3. Add billing method
4. Create server
```

### Create VPS:
```
- Location: Any closest to you
- OS: Ubuntu 22.04
- Server type: CPX11 (€2.49/month)
  - 1 vCore
  - 2 GB RAM
  - 40 GB NVMe SSD
  - 20 Gbps network
- Backups: Disabled (to save cost)
- Click "Create Server"
```

### SSH into Your Server:
```bash
# You'll get an IP address like: 123.45.67.89
ssh root@123.45.67.89

# First time login, change root password
passwd
```

### Install Dependencies (on VPS):
```bash
# Update system
apt-get update
apt-get upgrade -y

# Install Node.js
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
apt-get install -y nodejs

# Install PostgreSQL
apt-get install -y postgresql postgresql-contrib

# Install Redis
apt-get install -y redis-server

# Install FFmpeg
apt-get install -y ffmpeg

# Install Nginx (reverse proxy)
apt-get install -y nginx

# Install PM2 (process manager)
npm install -g pm2

# Install pnpm
npm install -g pnpm
```

### Clone Your Backend Repo:
```bash
# On the VPS server:
cd /opt
git clone https://github.com/YOUR-USERNAME/bunnystream-backend.git
cd bunnystream-backend

# Install dependencies
pnpm install

# Build
pnpm build
```

### Create Backend .env (on VPS):
```bash
# Edit environment file
nano .env

# Paste this (modify as needed):
NODE_ENV=production
PORT=4000
API_PREFIX=api/v1

DATABASE_URL=postgresql://postgres:your-secure-password@localhost:5432/bunnystream
REDIS_URL=redis://localhost:6379

JWT_SECRET=generate-a-random-string-here
JWT_EXPIRATION=7d

MINIO_ENDPOINT=localhost
MINIO_PORT=9000
MINIO_ACCESS_KEY=minioadmin
MINIO_SECRET_KEY=minioadmin
MINIO_BUCKET=videos

ALLOWED_ORIGINS=https://yourfrontenddomain.netlify.app

FFMPEG_PATH=/usr/bin/ffmpeg
FFPROBE_PATH=/usr/bin/ffprobe

# Save: Ctrl+X, then Y, then Enter
```

### Setup PostgreSQL (on VPS):
```bash
# Login to PostgreSQL
sudo -u postgres psql

# Create database
CREATE DATABASE bunnystream;

# Create user
CREATE USER bunnystream WITH PASSWORD 'your-secure-password';

# Grant privileges
GRANT ALL PRIVILEGES ON DATABASE bunnystream TO bunnystream;

# Exit
\q
```

### Start Backend with PM2:
```bash
# On the VPS, in your backend directory:
cd /opt/bunnystream-backend

# Start with PM2
pm2 start dist/main.js --name "bunnystream-api"

# Make it start on system reboot
pm2 startup
pm2 save

# Check status
pm2 status
```

### Setup Nginx (Reverse Proxy):
```bash
# Create Nginx config
sudo nano /etc/nginx/sites-available/bunnystream

# Paste this:
server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://localhost:4000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}

# Save: Ctrl+X, Y, Enter

# Enable the site
sudo ln -s /etc/nginx/sites-available/bunnystream /etc/nginx/sites-enabled/

# Test Nginx
sudo nginx -t

# Start Nginx
sudo systemctl start nginx
sudo systemctl enable nginx
```

### Get Free SSL Certificate (Let's Encrypt):
```bash
# Install Certbot
apt-get install -y certbot python3-certbot-nginx

# Get certificate
sudo certbot --nginx -d api.yourdomain.com

# Auto-renew
sudo systemctl enable certbot.timer
```

✅ **BACKEND DEPLOYED - Cost: €2.49/month**

---

## 📊 FINAL SETUP CHECK

### Test Everything:
```bash
# Test Frontend
https://yourproject.netlify.app

# Test Backend
curl https://api.yourdomain.com/api/v1/health

# Test Database
ssh root@your-vps-ip
psql -U postgres -d bunnystream -c "SELECT 1"

# Test Redis
redis-cli ping
# Should return: PONG

# Test MinIO
# Visit: https://api.yourdomain.com:9001
# Login with: minioadmin / minioadmin
```

---

## 💰 TOTAL MONTHLY COSTS

```
Frontend (Netlify):        €0.00  ($0)
Backend (Hetzner VPS):     €2.49  ($2.70)
Database (PostgreSQL):     €0.00  ($0)
Cache (Redis):             €0.00  ($0)
Storage (MinIO):           €0.00  ($0)
Video Processing (FFmpeg): €0.00  ($0)
─────────────────────────────────────
TOTAL:                     €2.49/month ($2.70/month)
ANNUAL:                    €29.88/year ($32.40/year)
```

### Compare to Paid Services:
- Paid Setup: $110/month = $1,320/year
- Free Setup: $2.70/month = $32.40/year
- **SAVINGS: $1,287.60/year** 🎉

---

## 🔒 SECURITY CHECKLIST

- [ ] Change all default passwords
- [ ] Enable SSH key authentication (disable password)
- [ ] Setup firewall on VPS
- [ ] Setup SSL/TLS certificates
- [ ] Enable 2FA on GitHub
- [ ] Enable 2FA on Hetzner
- [ ] Regularly backup PostgreSQL
- [ ] Monitor VPS resource usage

---

## 📈 SCALING TIPS (When You Grow)

If you need more resources:

```
Current: €2.49/month (1 core, 2 GB RAM)

Upgrade Options:
- CPX21: €4.49/month (2 cores, 4 GB RAM)
- CPX31: €8.49/month (4 cores, 8 GB RAM)
- CPX41: €15.49/month (8 cores, 16 GB RAM)

Or add separate services:
- Add storage server: €2.49/month
- Add database server: €2.49/month
- Add Redis server: €2.49/month
```

---

## 🎓 RECOMMENDED NEXT STEPS

1. ✅ Setup everything locally first (no cost)
2. ✅ Test thoroughly on localhost
3. ✅ Deploy frontend to Netlify (free)
4. ✅ Deploy backend to Hetzner VPS (€2.49/mo)
5. ✅ Setup custom domain
6. ✅ Enable SSL/TLS
7. ✅ Setup monitoring & backups
8. ✅ Document everything

---

**You now have a PROFESSIONAL, PRODUCTION-READY system for just €2.49/month!**

No vendor lock-in. Full control. Everything open source. 🚀

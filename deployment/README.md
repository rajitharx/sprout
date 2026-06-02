# Sprout Deployment Package

Source-code deployment for Docker containerization on Linux homelab.

## What's Included

- `Dockerfile` — Multi-stage build (frontend + backend)
- `docker-compose.yml` — Container orchestration with persistent storage
- `.dockerignore` — Optimizes build context
- `deploy.sh` — Helper script for Linux server

## Pre-Deployment Checklist

✅ `backend/Sprout.Api/` — .NET 10 source  
✅ `frontend/sprout-web/` — React 18 + Vite source  
✅ `Dockerfile` — Build instructions  
✅ `docker-compose.yml` — Container config  
✅ `.dockerignore` — Build optimization  

## Deployment Steps

### 1. Copy to Linux Server

From your development machine, copy the **entire project** to your homelab:

```bash
scp -r /Users/rajitha/Development/sprout/ user@<homelab-ip>:/home/user/
```

**Or with rsync** (skips node_modules/bin/obj):

```bash
rsync -av --exclude=node_modules --exclude=bin --exclude=obj --exclude=.git \
  /Users/rajitha/Development/sprout/ user@<homelab-ip>:/home/user/sprout/
```

### 2. SSH into the Server

```bash
ssh user@<homelab-ip>
cd /home/user/sprout
```

### 3. Start the Application

```bash
docker-compose up -d
```

**Wait ~3-5 minutes for the first build to complete.**

### 4. Verify Deployment

```bash
# Check container status
docker-compose ps

# View logs
docker-compose logs -f sprout

# Test the app
curl http://localhost:5000
```

Then open your browser to: `http://<homelab-ip>:5000`

---

## Directory Structure on Server

```
/home/user/sprout/
├── backend/Sprout.Api/          ← .NET source
├── frontend/sprout-web/         ← React source
├── deployment/                  ← This folder
│   ├── Dockerfile
│   ├── docker-compose.yml
│   ├── .dockerignore
│   └── README.md
├── Dockerfile                   ← Symlink or copy from deployment/
├── docker-compose.yml           ← Symlink or copy from deployment/
└── .dockerignore                ← Symlink or copy from deployment/
```

**Copy Docker files to root:**
```bash
cp deployment/Dockerfile .
cp deployment/docker-compose.yml .
cp deployment/.dockerignore .
```

---

## Common Commands on Server

```bash
# Start
docker-compose up -d

# Stop
docker-compose down

# Restart
docker-compose restart sprout

# View logs
docker-compose logs -f sprout

# SSH into container
docker-compose exec sprout sh

# Rebuild (if you update source)
docker-compose up -d --build
```

---

## Persistent Storage

Task data and profiles are stored in `sprout-storage` Docker volume:

```bash
# View volume location
docker inspect sprout-storage

# Backup data
docker run --rm -v sprout-storage:/data -v $(pwd):/backup \
  alpine tar czf /backup/sprout-backup.tar.gz -C /data .

# List stored files
docker exec sprout-app ls -la /app/Storage
```

---

## Troubleshooting

**Build fails:**
```bash
docker-compose build --no-cache
docker-compose up -d
```

**Container exits immediately:**
```bash
docker-compose logs sprout  # Check error
```

**Frontend not loading:**
```bash
# Check if wwwroot has files
docker exec sprout-app ls -la wwwroot/

# Rebuild if needed
docker-compose up -d --build
```

---

## Next Steps

- Set up **nginx reverse proxy** for SSL/TLS and custom domain
- Configure **backups** of the `sprout-storage` volume
- Monitor with **Portainer** or similar for easy management

---

**Built with:** .NET 10 | React 18 + Vite | Docker | Alpine

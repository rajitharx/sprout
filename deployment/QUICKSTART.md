# Quick Start - Source Deployment

## TL;DR

```bash
# On your Mac (development machine)
rsync -av --exclude=node_modules --exclude=bin --exclude=obj --exclude=.git \
  /Users/rajitha/Development/sprout/ user@homelab-ip:/home/user/sprout/

# On your Linux homelab
ssh user@homelab-ip
cd /home/user/sprout
bash deployment/deploy.sh
```

## Then Access

Open browser: **`http://homelab-ip:5000`**

---

## What Gets Copied

When you sync the source code to the server, this deployment folder contains:
- ✅ `Dockerfile` — Builds frontend + backend from source
- ✅ `docker-compose.yml` — Runs the container with persistent storage
- ✅ `.dockerignore` — Optimizes build
- ✅ `deploy.sh` — Automated setup script
- ✅ `README.md` — Full documentation

The script automatically copies Docker files to the root and starts the app.

---

## Files NOT Copied (Saves Space)

- `node_modules/` — Docker installs fresh
- `bin/`, `obj/` — Docker rebuilds
- `.git/` — Not needed for runtime

Use `rsync --exclude` to skip them automatically.

---

## On the Linux Server

```bash
# Auto-setup (recommended)
bash deployment/deploy.sh

# Or manual setup
cp deployment/Dockerfile .
cp deployment/docker-compose.yml .
cp deployment/.dockerignore .
docker-compose up -d
```

---

## Monitor & Logs

```bash
docker-compose ps                    # Status
docker-compose logs -f sprout        # Live logs
curl http://localhost:5000           # Health check
```

---

## Full Documentation

See `deployment/README.md` for detailed info on storage, backups, troubleshooting, and more.

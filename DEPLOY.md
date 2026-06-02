# Sprout Deployment Guide

## Single Container Deployment

This setup builds and packages both the frontend (React) and backend (.NET) into a single Docker container.

### Prerequisites

- Docker & Docker Compose installed on your homelab machine
- Access to the Sprout repository

### Quick Start

1. **Clone or push your code to the homelab machine:**
   ```bash
   git clone <your-repo-url> /path/to/sprout
   cd /path/to/sprout
   ```

2. **Build and run with Docker Compose:**
   ```bash
   docker-compose up -d
   ```

3. **Access the app:**
   - Open your browser to `http://<homelab-ip>:5000`
   - The backend API is at `http://<homelab-ip>:5000/api/...`
   - The frontend is served as static files from the backend

### Manual Docker Build (without Compose)

If you prefer not to use Docker Compose:

```bash
# Build the image
docker build -t sprout:latest .

# Run the container
docker run -d \
  --name sprout-app \
  -p 5000:5000 \
  -v sprout-storage:/app/Storage \
  --restart unless-stopped \
  sprout:latest
```

### Persistent Storage

Task progress and profile data are stored in the `Storage/` directory within the container:
- Mounted to the `sprout-storage` Docker volume
- Persists even when the container restarts
- Location on host: `/var/lib/docker/volumes/sprout-storage/_data`

### View Logs

```bash
# With Compose
docker-compose logs -f sprout

# Manual container
docker logs -f sprout-app
```

### Stop/Restart

```bash
# Stop
docker-compose down

# Restart
docker-compose up -d
```

### Environment Configuration

To customize the backend (if needed), add environment variables in `docker-compose.yml`:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - LOG_LEVEL=Information
```

### Troubleshooting

**Container won't start:**
```bash
docker-compose logs sprout  # Check error messages
```

**Storage not persisting:**
- Verify the volume exists: `docker volume ls | grep sprout`
- Check permissions in `/var/lib/docker/volumes/sprout-storage/_data`

**Frontend not loading:**
- Ensure the build completed successfully: `docker-compose build --no-cache`
- Check that `wwwroot/` has files: `docker exec sprout-app ls -la wwwroot/`

### Security Notes

- Currently exposes the app on port 5000 to your network
- For external access, use a reverse proxy (nginx) with SSL/TLS
- No authentication enabled in the base config (parent unlocks via UI)

---

**Built with:** .NET 10 | React 18 + Vite | Alpine Linux

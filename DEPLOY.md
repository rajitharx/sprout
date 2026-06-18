# Sprout Deployment Guide

## Single Container Deployment

This setup builds and packages both the frontend (React) and backend (.NET) into a single Docker container.

### Prerequisites

- Docker & Docker Compose installed on your homelab machine
- PostgreSQL 13+ running (locally or remote)
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

# Run the container with PostgreSQL connection
docker run -d \
  --name sprout-app \
  -p 5000:5000 \
  -e ConnectionStrings__DefaultConnection="Host=postgres-server;Database=Sprout;Username=sprout_user;Password=secure_password" \
  --restart unless-stopped \
  sprout:latest
```

### Persistent Storage

Data is now persisted in PostgreSQL database instead of JSON files:

**Database Configuration:**
- Point the container to your PostgreSQL server via environment variable
- Example: `ConnectionStrings__DefaultConnection=Host=postgres-server;Database=Sprout;Username=sprout_user;Password=secure_password`

**JSON Migration:**
- On first run, JSON files in `Storage/data/` are automatically migrated to PostgreSQL (if present)
- JSON files are left untouched as a backup after migration

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

**Database connection errors:**
- Verify PostgreSQL is accessible from the container
- Check connection string in environment variables
- Ensure database exists: `psql -h postgres-server -U sprout_user -c "SELECT * FROM pg_database WHERE datname='Sprout';"`

**Frontend not loading:**
- Ensure the build completed successfully: `docker-compose build --no-cache`
- Check that `wwwroot/` has files: `docker exec sprout-app ls -la wwwroot/`

### Security Notes

- Currently exposes the app on port 5000 to your network
- For external access, use a reverse proxy (nginx) with SSL/TLS
- No authentication enabled in the base config (parent unlocks via UI)

---

**Built with:** .NET 10 | React 18 + Vite | Alpine Linux

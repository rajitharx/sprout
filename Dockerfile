# Stage 1: Build frontend
FROM node:20-alpine AS frontend-builder

WORKDIR /app/frontend

# Copy frontend files
COPY frontend/sprout-web/package*.json ./
RUN npm ci

COPY frontend/sprout-web/ .
RUN npm run build

# Stage 2: Build and run backend with frontend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-builder

WORKDIR /app

# Copy backend files
COPY backend/Sprout.Api/ ./Sprout.Api/
COPY backend/Sprout.Api.Tests/ ./Sprout.Api.Tests/

# Copy frontend dist from Stage 1
COPY --from=frontend-builder /app/frontend/dist ./Sprout.Api/wwwroot

WORKDIR /app/Sprout.Api

# Restore and publish
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

WORKDIR /app

# Copy published app
COPY --from=backend-builder /app/publish .

# Create storage directory for JSON persistence
RUN mkdir -p Storage

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5000/health || exit 1

# Set environment
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Sprout.Api.dll"]

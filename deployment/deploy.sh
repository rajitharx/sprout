#!/bin/bash

# Sprout Deployment Script
# Run this on your Linux homelab after copying the project

set -e

echo "🌱 Sprout Deployment Script"
echo "============================"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker not found. Please install Docker first."
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose not found. Please install Docker Compose first."
    exit 1
fi

echo "✅ Docker and Docker Compose found"
echo ""

# Check directory structure
if [ ! -d "backend/Sprout.Api" ]; then
    echo "❌ backend/Sprout.Api not found. Are you in the sprout root directory?"
    exit 1
fi

if [ ! -d "frontend/sprout-web" ]; then
    echo "❌ frontend/sprout-web not found. Are you in the sprout root directory?"
    exit 1
fi

echo "✅ Project structure verified"
echo ""

# Copy Docker files to root if not already there
echo "📋 Checking Docker configuration files..."
if [ ! -f "Dockerfile" ]; then
    cp deployment/Dockerfile .
    echo "✅ Copied Dockerfile"
fi

if [ ! -f "docker-compose.yml" ]; then
    cp deployment/docker-compose.yml .
    echo "✅ Copied docker-compose.yml"
fi

if [ ! -f ".dockerignore" ]; then
    cp deployment/.dockerignore .
    echo "✅ Copied .dockerignore"
fi

echo ""
echo "🔨 Building Docker image..."
echo "This may take 3-5 minutes on first run..."
echo ""

docker-compose build

echo ""
echo "🚀 Starting Sprout application..."
docker-compose up -d

echo ""
echo "⏳ Waiting for container to be healthy..."
sleep 5

# Check container status
if docker-compose ps | grep -q "sprout"; then
    echo ""
    echo "✅ Sprout is running!"
    echo ""
    echo "📍 Access the app at:"
    echo "   http://localhost:5000"
    echo "   or http://$(hostname -I | awk '{print $1}'):5000"
    echo ""
    echo "📋 View logs:"
    echo "   docker-compose logs -f sprout"
    echo ""
    echo "🛑 Stop:"
    echo "   docker-compose down"
    echo ""
else
    echo ""
    echo "⚠️  Container may not have started. Checking logs..."
    docker-compose logs sprout
    exit 1
fi

#!/usr/bin/env bash
set -e

# Start backend in background
(cd backend/Sprout.Api && dotnet run) &
BACKEND_PID=$!

# Start frontend dev server
(cd frontend/sprout-web && npm run dev) &
FRONTEND_PID=$!

trap "kill $BACKEND_PID $FRONTEND_PID 2>/dev/null" EXIT INT TERM

echo "Backend:  http://localhost:5000"
echo "Frontend: http://localhost:5173"
echo "Press Ctrl+C to stop both."

wait

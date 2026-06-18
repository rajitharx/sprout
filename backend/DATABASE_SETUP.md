# Database Setup Guide

This guide explains how to set up the PostgreSQL database for the Sprout application.

## Prerequisites

- PostgreSQL 12+ installed and running
- `psql` command-line tool available

## Steps to Set Up

### 1. Create the Database

Run the following SQL commands to create the database and user:

```bash
# Connect to PostgreSQL as the default superuser
psql -U postgres

# Create the Sprout database
CREATE DATABASE "Sprout";

# Create a user for the application (optional but recommended)
CREATE USER sprout_user WITH PASSWORD 'sp!23kkJ';

# Grant privileges
GRANT ALL PRIVILEGES ON DATABASE "Sprout" TO sprout_user;

# Connect to the Sprout database and grant schema privileges
\c Sprout
GRANT ALL PRIVILEGES ON SCHEMA public TO sprout_user;

# Exit psql
\q
```

### 2. Update Connection String

Update the `appsettings.json` file with your PostgreSQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=Sprout;Username=postgres;Password=your_password;Port=5432"
  }
}
```

**Default credentials (development):**
- Username: `postgres`
- Password: `postgres`
- Host: `localhost`
- Port: `5432`

### 3. Run Database Migrations

When you start the application, it will automatically:
1. Create all required tables
2. Migrate existing data from JSON files (`tasks.json`, `progress.json`, `profile.json`)

```bash
cd backend/Sprout.Api
dotnet run
```

The application will log:
```
🗄️ Running database migrations...
✅ Database migrations completed
📦 Migrating data from JSON files to database...
✅ Data migration completed
```

## Database Schema

The application creates the following tables:

### HabitTasks
- `Id` (string, PK)
- `Label` (string)
- `Emoji` (string)
- `SortOrder` (int)
- `IsActive` (bool)

### DailyProgress
- `Id` (int, PK, auto-increment)
- `Date` (string, unique)
- `LastUpdated` (datetime)

### CompletedTasks
- `Id` (int, PK, auto-increment)
- `DailyProgressId` (int, FK)
- `TaskId` (string, FK)
- Unique constraint on `(DailyProgressId, TaskId)`

### ChildProfiles
- `Id` (int, PK)
- `Name` (string)
- `Avatar` (string)

## Manual Database Creation (if migrations fail)

If automatic migrations fail, create the database manually:

```sql
-- Connect to the Sprout database
\c Sprout

-- Create HabitTasks table
CREATE TABLE "HabitTasks" (
    "Id" TEXT PRIMARY KEY,
    "Label" TEXT NOT NULL,
    "Emoji" TEXT NOT NULL,
    "SortOrder" INTEGER NOT NULL,
    "IsActive" BOOLEAN NOT NULL
);

-- Create DailyProgress table
CREATE TABLE "DailyProgress" (
    "Id" SERIAL PRIMARY KEY,
    "Date" TEXT NOT NULL UNIQUE,
    "LastUpdated" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);

-- Create CompletedTasks table
CREATE TABLE "CompletedTasks" (
    "Id" SERIAL PRIMARY KEY,
    "DailyProgressId" INTEGER NOT NULL REFERENCES "DailyProgress"("Id") ON DELETE CASCADE,
    "TaskId" TEXT NOT NULL,
    UNIQUE("DailyProgressId", "TaskId")
);

-- Create ChildProfiles table
CREATE TABLE "ChildProfiles" (
    "Id" INTEGER PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Avatar" TEXT NOT NULL
);

-- Create index for DailyProgress.Date
CREATE INDEX "IX_DailyProgress_Date" ON "DailyProgress"("Date");
```

## Troubleshooting

### Database Connection Issues

**Error:** `FATAL: role "postgres" does not exist`
- Start PostgreSQL service: `brew services start postgresql`
- Or `pg_ctl -D /usr/local/var/postgres start`

**Error:** `FATAL: database "Sprout" does not exist`
- Create the database using the steps above

### Port Already in Use

If port 5432 is already in use:
1. Find the process: `lsof -i :5432`
2. Kill it: `kill -9 <PID>`
3. Or change the port in the connection string

## Data Migration from JSON

The application automatically migrates data from JSON files to the database on first run. The JSON files are read from:

- `backend/Sprout.Api/Storage/data/tasks.json`
- `backend/Sprout.Api/Storage/data/progress.json`
- `backend/Sprout.Api/Storage/data/profile.json`

The migration only happens if:
1. The database tables are empty
2. The JSON files exist and contain data

## Reverting to JSON Storage

To revert to JSON-based storage, you can use the `JsonTaskService`, `JsonProgressService`, and `JsonChildProfileService` classes. Update `Program.cs`:

```csharp
// Instead of:
builder.Services.AddScoped<ITaskService, DbTaskService>();
builder.Services.AddScoped<IProgressService, DbProgressService>();
builder.Services.AddScoped<IChildProfileService, DbChildProfileService>();

// Use:
builder.Services.AddSingleton<ITaskService, JsonTaskService>();
builder.Services.AddSingleton<IProgressService, JsonProgressService>();
builder.Services.AddSingleton<IChildProfileService, JsonChildProfileService>();
```

## Testing

Run the test suite to verify the database setup:

```bash
cd backend/Sprout.Api.Tests
dotnet test
```

All database-related tests use an in-memory SQLite database and don't require PostgreSQL.

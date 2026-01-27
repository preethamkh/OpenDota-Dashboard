# OpenDota Dashboard - Production grade

## Project Overview (draft)

A Dota 2 match analytics dashboard that:
- **Ingests data** from OpenDota API
- **Displays KPIs** and charts
- **Processes jobs** via RabbitMQ background workers
- **Deploys free** on Render + Supabase

## Tech Stack

- **Backend:** ASP.NET Core 8 (C#) with Razor Pages
- **Database:** PostgreSQL (Docker local, Supabase production)
- **Message Queue:** RabbitMQ (Docker local, CloudAMQP production)
- **Charts:** Chart.js
- **Testing:** xUnit + Moq
- **Deployment:** Render (free tier)

## Why PostgreSQL from Day 1?

✅ Same database locally and in production
✅ No migration headaches on deployment day
✅ Better concurrent write support for background workers
✅ Native support on all cloud platforms (Render, Railway, Fly.io)
✅ Supabase free tier ready

## Quick Start (15 Minutes)

### Prerequisites
- Visual Studio 2022
- .NET 8.0 SDK
- Docker Desktop (running)

### Step 1: Start Docker Services
```bash
docker-compose up -d
```

### Step 2: Create Project
1. Open Visual Studio 2022
2. Create new "ASP.NET Core Web App" (Razor Pages)
3. Name: `OpenDotaDashboard`
4. Framework: .NET 8.0

### Step 3: Install NuGet Packages
```powershell
Install-Package Microsoft.EntityFrameworkCore -Version 8.0.0
Install-Package Npgsql.EntityFrameworkCore.PostgreSQL -Version 8.0.0
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 8.0.0
Install-Package RabbitMQ.Client -Version 6.8.1
Install-Package Newtonsoft.Json -Version 13.0.3
```

### Step 4: Add Project Files
Copy all the code from the artifacts into your project structure

### Step 5: Create Database
```powershell
Add-Migration InitialCreate
Update-Database
```

### Step 6: Run & Test
- Press F5
- Navigate to `/TestIngestion`
- Click "Ingest Heroes" then "Ingest Matches"

## Project Structure (currently planned...)

```
OpenDotaDashboard/
├── Models/
│   ├── Entities/          # Database entities
│   └── DTOs/              # API response models
├── Data/
│   └── ApplicationDbContext.cs
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Pages/
│   ├── Dashboard/         # 
│   ├── Jobs/              # 
│   └── TestIngestion.cshtml
└── Workers/               # 
```

## Running in Docker

| Service | Port | Credentials |
|---------|------|------------|
| PostgreSQL | 5432 | postgres/postgres123 |
| RabbitMQ | 5672, 15672 | admin/admin123 |
| pgAdmin | 5050 | admin@opendota.com/admin123 |

## Project Roadmap

### (In progress...)
- Project setup with PostgreSQL
- Database schema and models
- OpenDota API client
- Basic data ingestion
- Test page

### Later ...
- RabbitMQ integration
- Background job workers
- Dashboard with KPIs
- Chart.js visualizations
- Recent matches table

### Then ...
- Jobs management page
- Job retry logic
- Unit & integration tests
- Deploy to Render + Supabase
- Final polish

## Connection Strings

### Local Development (Docker)
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=opendota;Username=postgres;Password=postgres123"
```

### Production (Supabase)
```json
"SupabaseConnection": "Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require"
```

## Useful Commands

### Docker
```bash
docker-compose up -d        # Start services
docker-compose down         # Stop services
docker-compose down -v      # Stop and delete data
docker-compose logs -f      # View logs
```

### Migrations
```powershell
Add-Migration MigrationName
Update-Database
Remove-Migration
```

## Access Points

- **App:** https://localhost:7xxx
- **pgAdmin:** http://localhost:5050
- **RabbitMQ:** http://localhost:15672
- **OpenDota API:** https://api.opendota.com/api

## Database Schema

### Tables
- **Matches** - Match data (ID, time, duration, winner)
- **Players** - Player profiles with aggregate stats
- **Heroes** - Hero data with pick/win rates
- **MatchPlayers** - Junction table with K/D/A stats
- **Jobs** - Background job tracking

### Relationships
- One Match → Many MatchPlayers
- One Player → Many MatchPlayers  
- One Hero → Many MatchPlayers

## Features

### Current (in progress...)
✅ PostgreSQL database with EF Core
✅ OpenDota API integration
✅ Hero and match ingestion
✅ Automatic aggregate stats
✅ Rate limiting (60 calls/min)

### Coming ...
🔄 Background job processing
📊 Dashboard with charts
📈 KPI cards
🎨 Hero images and avatars

### Next ...
📋 Job management interface
🧪 Unit & integration tests
🚀 Cloud deployment
✨ Final polish

## Deployment (TBD)

### Supabase (Database)
1. Create project at supabase.com
2. Get connection string
3. Update appsettings.Production.json

### CloudAMQP (RabbitMQ)
1. Create account at cloudamqp.com
2. Create instance (free tier)
3. Get AMQP URL

### Render (Hosting)
1. Connect GitHub repo
2. Add environment variables
3. Deploy!

## Next Steps

After completing Intiial/Day 1 setup:
1. Verify Docker containers are running
2. Check database has data in pgAdmin
3. Test hero and match ingestion
4. Ready for Day 2!

---

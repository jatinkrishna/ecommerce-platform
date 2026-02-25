# 🐳 Docker & SQL Server Setup Guide

## 📋 Quick Reference

| Method | SQL Server | Connection String | Command |
|--------|-----------|-------------------|---------|
| Docker (Recommended) | Docker Container | `Server=sqlserver;...` | `docker-compose up -d` |
| Local SQL Server | Your Machine | `Server=(localdb)\\mssqllocaldb;...` | `dotnet run` |

---

## 🚀 Option 1: Full Docker Setup (Easiest)

### Step 1: Start Docker Desktop

Make sure Docker Desktop is running (check system tray).

### Step 2: Start Services

```powershell
# Navigate to project root
cd E:\Project\ecommerce-platform\backend\ecommerce-platform

# Start both SQL Server + API
docker-compose up -d
```

**First time:** Downloads SQL Server image (~1.5 GB)  
**Time:** 2-3 minutes first run, 30 seconds after

### Step 3: Check Status

```powershell
docker-compose ps

# Should show:
# NAME                      STATUS
# ecommerce-sqlserver       Up (healthy)
# ecommerce-identity-api    Up
```

### Step 4: View Logs (Optional)

```powershell
# Watch API logs
docker-compose logs -f identity-api

# Press Ctrl+C to exit
```

### Step 5: Test API

Open browser: **http://localhost:5001/swagger**

Or run tests:
```powershell
.\test-api.ps1
```

---

## 💻 Option 2: Local SQL Server

### Step 1: Update Connection String

Edit `Ecommerce.Identity.API/appsettings.Development.json`:

**For SQL Server LocalDB:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceIdentityDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**For SQL Server Express:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EcommerceIdentityDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Step 2: Run the API

```powershell
cd Ecommerce.Identity.API
dotnet run
```

**Database is created automatically!**

### Step 3: Test

Open: **https://localhost:7001/swagger**

---

## 🔌 Connecting to SQL Server

### SQL Server Management Studio (SSMS)

#### Docker SQL Server:
- **Server:** `localhost,1433`
- **Authentication:** SQL Server Authentication
- **Login:** `sa`
- **Password:** `YourStrong@Passw0rd`
- **Database:** `EcommerceIdentityDb`

#### Local SQL Server:
- **Server:** `(localdb)\mssqllocaldb`
- **Authentication:** Windows Authentication
- **Database:** `EcommerceIdentityDb`

### Command Line (sqlcmd)

**Docker:**
```powershell
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd" -d EcommerceIdentityDb
```

**Local:**
```powershell
sqlcmd -S (localdb)\mssqllocaldb -d EcommerceIdentityDb
```

### Docker Exec (Direct to Container)

```powershell
docker exec -it ecommerce-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd"
```

Then run SQL:
```sql
SELECT name FROM sys.databases;
GO

USE EcommerceIdentityDb;
GO

SELECT * FROM Users;
GO
```

---

## 🎯 Docker Commands Cheat Sheet

```powershell
# Start services
docker-compose up -d

# Stop services
docker-compose down

# Restart services
docker-compose restart

# View logs (follow mode)
docker-compose logs -f

# View logs for specific service
docker-compose logs -f identity-api
docker-compose logs -f sqlserver

# Check status
docker-compose ps

# Rebuild and start
docker-compose up -d --build

# Stop and remove data (fresh start)
docker-compose down -v

# List running containers
docker ps

# Connect to API container
docker exec -it ecommerce-identity-api /bin/bash

# Connect to SQL Server container
docker exec -it ecommerce-sqlserver bash

# View container resource usage
docker stats
```

---

## 🛠️ Troubleshooting

### Issue: Port 1433 Already in Use

**Cause:** Local SQL Server using port 1433

**Solution 1 - Stop Local SQL Server:**
```powershell
net stop MSSQLSERVER
```

**Solution 2 - Change Docker Port:**

Edit `docker-compose.yml`:
```yaml
sqlserver:
  ports:
    - "1434:1433"  # Use different port
```

Update connection string in docker-compose.yml:
```yaml
- ConnectionStrings__DefaultConnection=Server=sqlserver,1434;...
```

### Issue: SQL Server Container Not Starting

**Check logs:**
```powershell
docker-compose logs sqlserver
```

**Common fixes:**
```powershell
# Clean restart
docker-compose down -v
docker-compose up -d

# Check Docker resources (Settings -> Resources)
# SQL Server needs at least 2 GB RAM
```

### Issue: API Can't Connect to Database

**Check SQL Server is healthy:**
```powershell
docker-compose ps

# Should show (healthy) for sqlserver
```

**Wait longer:**
SQL Server takes 20-30 seconds to be ready.

**Check connection string:**
```powershell
docker-compose logs identity-api | Select-String "connection"
```

### Issue: Database Not Created

**The database is auto-created!**

Check API logs:
```powershell
docker-compose logs identity-api | Select-String "migration"
```

**Manual creation (if needed):**
```powershell
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd" -i "Ecommerce.Identity.API\Database\setup.sql"
```

### Issue: Docker Build Fails

**Clean rebuild:**
```powershell
# Remove all containers and images
docker-compose down
docker system prune -a

# Rebuild
docker-compose up -d --build
```

**Check Dockerfile issues:**
```powershell
# Build manually to see errors
docker build -t identity-api .
```

### Issue: "Cannot find Shared.Common"

**Cause:** Project reference issue

**Fix:**
```powershell
# Make sure both projects are in the same directory level
# Rebuild
docker-compose down
docker-compose up -d --build
```

---

## 📊 Verifying the Setup

### 1. Check Containers Running

```powershell
docker ps
```

Should show 2 containers running.

### 2. Check API Health

```powershell
curl http://localhost:5001/health

# Or in browser
start http://localhost:5001/swagger
```

### 3. Check Database

```powershell
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd" -Q "SELECT name FROM sys.databases" -d master
```

### 4. Check Users Table

```powershell
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd" -Q "SELECT Email, FirstName, LastName FROM Users" -d EcommerceIdentityDb
```

### 5. Run Full Test

```powershell
.\test-api.ps1
```

---

## 🎯 Recommended Workflow

### Daily Development (Docker)

```powershell
# Morning: Start services
docker-compose up -d

# Check logs if needed
docker-compose logs -f identity-api

# Code changes? Restart API
docker-compose restart identity-api

# View updated logs
docker-compose logs -f identity-api

# Evening: Stop services
docker-compose down

# Need fresh start?
docker-compose down -v && docker-compose up -d
```

### Daily Development (Local)

```powershell
# Start API
cd Ecommerce.Identity.API
dotnet run

# Make changes
# Stop (Ctrl+C)
# Run again
dotnet run
```

---

## 🔍 Understanding docker-compose.yml

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "1433:1433"  # Host:Container
    environment:
      - SA_PASSWORD=YourStrong@Passw0rd  # SQL SA password
    volumes:
      - sqlserver_data:/var/opt/mssql  # Persist data
    healthcheck:
      # Wait until SQL is ready
      test: sqlcmd -S localhost -U sa -P "..." -Q "SELECT 1"

  identity-api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5001:8080"  # HTTP
      - "7001:8081"  # HTTPS
    depends_on:
      sqlserver:
        condition: service_healthy  # Wait for SQL
    environment:
      # Override appsettings.json
      - ConnectionStrings__DefaultConnection=Server=sqlserver;...
      - Jwt__Secret=...
```

---

## 📞 Quick Help

**Services won't start?**
```powershell
docker-compose down -v
docker-compose up -d
docker-compose logs -f
```

**Need clean slate?**
```powershell
docker-compose down -v
docker system prune -a
docker-compose up -d
```

**Check what's using ports?**
```powershell
netstat -ano | findstr :1433
netstat -ano | findstr :5001
```

**Stop process using port?**
```powershell
# Find PID first, then:
taskkill /PID <process_id> /F
```

---

## 🎉 Success Checklist

- [ ] Docker Desktop is running
- [ ] `docker-compose ps` shows 2 containers UP
- [ ] `http://localhost:5001/swagger` opens
- [ ] `.\test-api.ps1` runs successfully
- [ ] Can connect to SQL Server
- [ ] Can register/login a user

---

## 📚 Next Steps

1. ✅ Run `docker-compose up -d`
2. ✅ Test with `.\test-api.ps1`
3. ✅ Explore Swagger UI
4. ✅ Connect with SSMS to view data
5. ⏳ Build next microservice!

---

**Need more help?** Check:
- [QUICK-START.md](./QUICK-START.md) - Fastest way to run
- [DEPLOYMENT.md](./DEPLOYMENT.md) - Production deployment
- [README.md](./README.md) - Full platform overview

# Ecommerce Identity API - Quick Start Guide

## 🎯 Choose Your Deployment Method

### ⚡ Option 1: Docker Compose (Fastest - 2 minutes)

```bash
# Start everything (SQL Server + API)
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f identity-api

# Test the API
curl http://localhost:5001/health
```

**Access:** http://localhost:5001/swagger

---

### 💻 Option 2: Run Locally (5 minutes)

#### Step 1: Update Connection String

Edit `Ecommerce.Identity.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceIdentityDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

#### Step 2: Run the API

The database will be created automatically on first run!

```bash
cd Ecommerce.Identity.API
dotnet run
```

**Access:** https://localhost:7001/swagger

---

## 🧪 Test the API

### 1. Register a User

Go to Swagger UI and try:

```json
POST /api/auth/register
{
  "email": "john@example.com",
  "password": "Test@123",
  "confirmPassword": "Test@123",
  "firstName": "John",
  "lastName": "Doe"
}
```

### 2. Login

```json
POST /api/auth/login
{
  "email": "john@example.com",
  "password": "Test@123"
}
```

Copy the `accessToken` from the response.

### 3. Get Profile

Click "Authorize" button in Swagger, paste the token, then:

```json
GET /api/auth/profile
```

### 4. Refresh Token

```json
POST /api/auth/refresh-token
{
  "accessToken": "your_old_token",
  "refreshToken": "your_refresh_token"
}
```

---

## 🔐 Test Credentials (Pre-loaded)

If you run the SQL setup script, these accounts are available:

| Email | Password | Role |
|-------|----------|------|
| admin@ecommerce.com | Admin@123 | Admin, Manager |
| customer@example.com | Customer@123 | Customer |

---

## 🐛 Troubleshooting

### "Cannot connect to database"

**Docker:**
```bash
# Check if SQL Server is running
docker-compose ps
docker-compose logs sqlserver
```

**Local:**
```bash
# Check if SQL Server LocalDB is installed
sqllocaldb info

# Start LocalDB
sqllocaldb start mssqllocaldb
```

### "Port already in use"

Change ports in `docker-compose.yml` or `launchSettings.json`:

```json
"applicationUrl": "https://localhost:7002;http://localhost:5002"
```

### "Database migrations failed"

The app automatically applies migrations on startup. Check logs:

```bash
# Docker
docker-compose logs identity-api

# Local
# Check console output when running dotnet run
```

---

## 📚 API Documentation

Once running, visit:
- **Swagger UI:** https://localhost:7001/swagger
- **OpenAPI JSON:** https://localhost:7001/swagger/v1/swagger.json

---

## 🎯 Next Steps

1. ✅ Test all endpoints in Swagger
2. ✅ Create test users
3. ✅ Test authentication flow
4. ⏳ Build Product Service
5. ⏳ Build Order Service
6. ⏳ Build Payment Service
7. ⏳ Build Notification Service
8. ⏳ Setup API Gateway

---

## 🔥 Quick Commands Reference

```bash
# Docker
docker-compose up -d              # Start services
docker-compose down               # Stop services
docker-compose logs -f            # View logs
docker-compose restart            # Restart services
docker-compose down -v            # Clean everything

# Local Development
dotnet run                        # Run the API
dotnet build                      # Build project
dotnet test                       # Run tests (if any)

# Database
# Migrations are auto-applied on startup!
# No manual migration commands needed
```

---

## 📞 Need Help?

1. Check logs first
2. Verify database connection
3. Check firewall/ports
4. Review `DEPLOYMENT.md` for detailed guides

# Ecommerce Platform - Deployment Guide

## 🚀 Quick Start Options

### Option 1: Docker Compose (Recommended for Local Development)

This is the easiest way to get everything running locally with SQL Server.

```bash
# 1. Build and start all services
docker-compose up -d

# 2. Wait for services to be healthy (about 30 seconds)
docker-compose ps

# 3. Access the API
# Swagger UI: http://localhost:5001/swagger
# HTTPS: https://localhost:7001/swagger
```

**Default Test Accounts:**
- Admin: `admin@ecommerce.com` / `Admin@123`
- Customer: `customer@example.com` / `Customer@123`

To stop:
```bash
docker-compose down
```

To stop and remove volumes (clean database):
```bash
docker-compose down -v
```

---

### Option 2: Visual Studio / VS Code (Local SQL Server)

#### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code with C# extension

#### Step 1: Setup SQL Server

**Option A: Use SQL Server LocalDB (Windows only)**
```bash
# Check if LocalDB is installed
sqllocaldb info

# Create instance if needed
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

**Option B: Use SQL Server Express or Full SQL Server**
- Ensure SQL Server is running
- Update connection string in `appsettings.Development.json`

#### Step 2: Update Connection String

Edit `Ecommerce.Identity.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceIdentityDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Or for SQL Server Express/Full:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EcommerceIdentityDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

#### Step 3: Apply Database Migrations

The migrations are already created in the `Migrations` folder. You have two options:

**Option A: Automatic Migration on Startup (Recommended)**

Add this to `Program.cs` (before `app.Run()`):

```csharp
// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    db.Database.Migrate();
}
```

**Option B: Manual SQL Script**

Run the SQL script in `Ecommerce.Identity.API/Database/setup.sql` using:
- SQL Server Management Studio (SSMS)
- Azure Data Studio
- sqlcmd command line

```bash
sqlcmd -S (localdb)\mssqllocaldb -i "Ecommerce.Identity.API/Database/setup.sql"
```

#### Step 4: Run the Application

**Visual Studio:**
1. Open the solution
2. Set `Ecommerce.Identity.API` as startup project
3. Press F5 or click Run

**VS Code:**
```bash
cd Ecommerce.Identity.API
dotnet run
```

**Command Line:**
```bash
dotnet run --project Ecommerce.Identity.API/Ecommerce.Identity.API.csproj
```

#### Step 5: Test the API

Navigate to: `https://localhost:7001/swagger`

---

### Option 3: Azure Deployment

#### Prerequisites
- Azure subscription
- Azure CLI installed
- Azure SQL Database created

#### Step 1: Create Azure Resources

```bash
# Login to Azure
az login

# Create resource group
az group create --name rg-ecommerce --location eastus

# Create Azure SQL Server
az sql server create \
  --name sql-ecommerce \
  --resource-group rg-ecommerce \
  --location eastus \
  --admin-user sqladmin \
  --admin-password YourStrong@Passw0rd123

# Create database
az sql db create \
  --resource-group rg-ecommerce \
  --server sql-ecommerce \
  --name EcommerceIdentityDb \
  --service-objective S0

# Configure firewall (allow Azure services)
az sql server firewall-rule create \
  --resource-group rg-ecommerce \
  --server sql-ecommerce \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create App Service Plan
az appservice plan create \
  --name plan-ecommerce \
  --resource-group rg-ecommerce \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group rg-ecommerce \
  --plan plan-ecommerce \
  --name api-ecommerce-identity \
  --runtime "DOTNETCORE:8.0"
```

#### Step 2: Configure Application Settings

```bash
# Get SQL connection string
CONNECTION_STRING=$(az sql db show-connection-string \
  --server sql-ecommerce \
  --name EcommerceIdentityDb \
  --client ado.net \
  --output tsv)

# Set app settings
az webapp config appsettings set \
  --resource-group rg-ecommerce \
  --name api-ecommerce-identity \
  --settings \
    ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
    Jwt__Secret="YOUR_PRODUCTION_SECRET_KEY_32_CHARS" \
    Jwt__Issuer="EcommerceIdentityAPI" \
    Jwt__Audience="EcommerceClients" \
    Jwt__ExpirationInMinutes="60" \
    Jwt__RefreshTokenExpirationInDays="7"
```

#### Step 3: Deploy Application

**Option A: GitHub Actions (Recommended)**

Create `.github/workflows/deploy-identity-api.yml`:

```yaml
name: Deploy Identity API

on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Publish
      run: dotnet publish Ecommerce.Identity.API/Ecommerce.Identity.API.csproj -c Release -o ./publish
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'api-ecommerce-identity'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

**Option B: Azure CLI Deployment**

```bash
# Build and publish
dotnet publish Ecommerce.Identity.API/Ecommerce.Identity.API.csproj -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy
az webapp deployment source config-zip \
  --resource-group rg-ecommerce \
  --name api-ecommerce-identity \
  --src deploy.zip
```

#### Step 4: Apply Database Migrations

Connect to Azure SQL and run the setup script from `Ecommerce.Identity.API/Database/setup.sql`

---

## 🧪 Testing the API

### 1. Register a New User

```bash
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@123",
    "confirmPassword": "Test@123",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### 2. Login

```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@123"
  }'
```

Response:
```json
{
  "accessToken": "eyJhbG...",
  "refreshToken": "base64...",
  "expiresIn": 3600,
  "user": {
    "id": "guid",
    "email": "test@example.com",
    "firstName": "Test",
    "lastName": "User",
    "roles": ["Customer"],
    "isActive": true
  }
}
```

### 3. Get Profile (Authenticated)

```bash
curl -X GET https://localhost:7001/api/auth/profile \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 4. Refresh Token

```bash
curl -X POST https://localhost:7001/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "accessToken": "expired_token",
    "refreshToken": "valid_refresh_token"
  }'
```

---

## 📊 Health Check

Add health checks to verify the service is running:

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>();

app.MapHealthChecks("/health");
```

Test:
```bash
curl https://localhost:7001/health
```

---

## 🔒 Security Checklist for Production

- [ ] Change JWT secret in production
- [ ] Use Azure Key Vault for secrets
- [ ] Enable HTTPS only
- [ ] Configure CORS properly
- [ ] Use strong database passwords
- [ ] Enable Azure SQL firewall rules
- [ ] Implement rate limiting
- [ ] Add API versioning
- [ ] Enable logging and monitoring
- [ ] Configure proper authentication policies
- [ ] Use Azure Application Insights

---

## 📈 Monitoring

### Application Insights (Azure)

Add to `Program.cs`:
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

Add to `appsettings.json`:
```json
{
  "ApplicationInsights": {
    "ConnectionString": "YOUR_CONNECTION_STRING"
  }
}
```

### Logs

View logs locally:
```bash
dotnet run --project Ecommerce.Identity.API/Ecommerce.Identity.API.csproj
```

View Azure logs:
```bash
az webapp log tail --name api-ecommerce-identity --resource-group rg-ecommerce
```

---

## 🐛 Troubleshooting

### Database Connection Issues

```bash
# Test SQL connection
sqlcmd -S your-server -U your-user -P your-password -Q "SELECT 1"
```

### Docker Issues

```bash
# Check container logs
docker-compose logs identity-api
docker-compose logs sqlserver

# Restart services
docker-compose restart

# Clean rebuild
docker-compose down -v
docker-compose up --build -d
```

### Build Issues

```bash
# Clean and rebuild
dotnet clean
dotnet build --no-incremental
```

---

## 📦 Next Steps

1. ✅ Identity Service (Deployed)
2. ⏳ Deploy Product Service
3. ⏳ Deploy Order Service
4. ⏳ Deploy Payment Service
5. ⏳ Deploy Notification Service
6. ⏳ Deploy API Gateway

---

## 📞 Support

For issues or questions:
- Check the logs first
- Review the API documentation at `/swagger`
- Verify database connectivity
- Check firewall and network rules

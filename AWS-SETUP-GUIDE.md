# ☁️ AWS SQL Server Setup Guide for Ecommerce Platform

## 📋 Table of Contents
1. [AWS RDS SQL Server Setup](#aws-rds-sql-server-setup)
2. [Getting Your Connection String](#getting-your-connection-string)
3. [Configuring Your Application](#configuring-your-application)
4. [Security Best Practices](#security-best-practices)
5. [Testing the Connection](#testing-the-connection)
6. [Troubleshooting](#troubleshooting)
7. [Cost Optimization](#cost-optimization)

---

## 🚀 AWS RDS SQL Server Setup

### Step 1: Login to AWS Console

1. Go to: https://console.aws.amazon.com
2. Login with your AWS account
3. Select your preferred region (e.g., **us-east-1**, **us-west-2**, **eu-west-1**)

### Step 2: Navigate to RDS

1. Click **Services** → **Database** → **RDS**
2. Or search for "RDS" in the search bar

### Step 3: Create Database

Click **"Create database"** button

### Step 4: Choose Database Configuration

#### **Engine Options**

- **Engine type:** Microsoft SQL Server
- **Edition:** 
  - 🆓 **SQL Server Express Edition** (Free tier eligible - up to 20 GB)
  - 💰 **SQL Server Standard Edition** (Production)
  - 💰 **SQL Server Enterprise Edition** (Large scale)
- **Version:** SQL Server 2022 (latest) or 2019

#### **Templates**

Choose based on your needs:

- 🆓 **Free tier** (For learning/testing)
  - db.t3.micro (1 vCPU, 1 GB RAM)
  - 20 GB storage
  - Single-AZ
  
- 🏢 **Production** (For real applications)
  - Multi-AZ deployment
  - Automatic backups
  - Enhanced monitoring

- 💻 **Dev/Test** (Development environment)
  - Single-AZ
  - Cost-optimized

#### **Settings**

```
DB instance identifier: ecommerce-identity-db
Master username: admin
Master password: **************** (Create a strong password!)
Confirm password: ****************
```

**Password Requirements:**
- At least 8 characters
- Contains uppercase, lowercase, numbers, and symbols
- Example: `Ecommerce@2024!Secure`

💡 **Save your password securely!** You'll need it for the connection string.

#### **Instance Configuration**

**For Free Tier:**
- **DB instance class:** db.t3.micro
  - 1 vCPU
  - 1 GB RAM
  - Low to moderate network performance

**For Production:**
- **DB instance class:** db.t3.small or larger
  - db.t3.small: 2 vCPU, 2 GB RAM
  - db.t3.medium: 2 vCPU, 4 GB RAM
  - db.m5.large: 2 vCPU, 8 GB RAM

#### **Storage**

```
Storage type: General Purpose SSD (gp3)
Allocated storage: 20 GB (minimum)
Storage autoscaling: Enable
Maximum storage threshold: 100 GB
```

**Storage Options:**
- **gp3** - General Purpose SSD (Recommended) - 3,000 IOPS, 125 MB/s
- **io1** - Provisioned IOPS SSD (High performance) - Up to 64,000 IOPS
- **gp2** - General Purpose SSD (Legacy) - 100-16,000 IOPS

#### **Availability & Durability**

- **Multi-AZ deployment:** 
  - ❌ No (Free tier / Dev)
  - ✅ Yes (Production - highly available)

#### **Connectivity**

```
Virtual private cloud (VPC): Default VPC
Subnet group: default
Public access: Yes (for testing) / No (for production)
VPC security group: Create new
  - Name: ecommerce-identity-sg
  - Port: 1433
  - Source: Your IP / VPC range
Availability Zone: No preference
```

**Public Access Options:**

| Option | When to Use | Security |
|--------|-------------|----------|
| **Yes** | Testing, development from your machine | Less secure - needs IP whitelist |
| **No** | Production with VPN/VPC peering | More secure |

#### **Database Authentication**

- Authentication options: **Password authentication**
- (Optional) Enable IAM database authentication for enhanced security

#### **Additional Configuration**

```
Initial database name: EcommerceIdentityDb
DB parameter group: default.sqlserver-ee-16.0
Option group: default:sqlserver-ee-16-0
Backup:
  - Backup retention period: 7 days
  - Backup window: No preference
  - Copy tags to snapshots: Yes
Encryption:
  - Enable encryption: Yes
  - Master key: (default) aws/rds
Monitoring:
  - Enable Enhanced Monitoring: Yes (optional)
  - Granularity: 60 seconds
Maintenance:
  - Enable auto minor version upgrade: Yes
  - Maintenance window: No preference
Deletion protection: Enable (for production)
```

### Step 5: Review and Create

1. Review all settings
2. **Estimated monthly costs** will be shown
3. Click **"Create database"**

⏱️ **Wait Time:** 10-15 minutes for database creation

---

## 🔌 Getting Your Connection String

### Step 1: Find Your RDS Endpoint

1. Go to **RDS Console** → **Databases**
2. Click on your database: `ecommerce-identity-db`
3. Look for **"Endpoint & port"** section

**You'll see:**
```
Endpoint: ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com
Port: 1433
```

### Step 2: Build Your Connection String

**Template:**
```
Server={ENDPOINT},{PORT};Database={DATABASE_NAME};User Id={USERNAME};Password={PASSWORD};Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;
```

**Your Connection String:**
```
Server=ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com,1433;Database=EcommerceIdentityDb;User Id=admin;Password=Ecommerce@2024!Secure;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;
```

### Connection String Parameters Explained

| Parameter | Description | Value |
|-----------|-------------|-------|
| Server | RDS endpoint and port | `{endpoint},1433` |
| Database | Database name | `EcommerceIdentityDb` |
| User Id | Master username | `admin` |
| Password | Master password | Your password |
| Encrypt | Enable SSL/TLS | `True` |
| TrustServerCertificate | Trust server certificate | `False` (for AWS) |
| MultipleActiveResultSets | Enable MARS | `true` |

---

## 🔒 Configure Security Group

**CRITICAL:** Without this, you can't connect!

### Step 1: Find Security Group

1. RDS Console → Databases → Click your database
2. Under **"Connectivity & security"** tab
3. Click on the **VPC security group** link (e.g., `sg-xxxxxxxxx`)

### Step 2: Add Inbound Rule

1. Click **"Edit inbound rules"**
2. Click **"Add rule"**

**Rule Configuration:**

```
Type: MS SQL
Protocol: TCP
Port range: 1433
Source: [Choose one below]
```

**Source Options:**

| Source Type | Value | When to Use |
|-------------|-------|-------------|
| **My IP** | Automatic | Testing from your machine |
| **Custom** | Your IP/32 | Specific IP address |
| **Custom** | 0.0.0.0/0 | ⚠️ Allow from anywhere (NOT RECOMMENDED) |
| **Custom** | VPC CIDR | Production - allow from VPC only |

**Example for Testing:**
```
Type: MS SQL
Port: 1433
Source: My IP (74.125.224.72/32)
Description: Allow from my development machine
```

**Example for Production (EC2 in same VPC):**
```
Type: MS SQL
Port: 1433
Source: sg-webapp-security-group
Description: Allow from web application servers
```

3. Click **"Save rules"**

---

## 📝 Configuring Your Application

### Option 1: appsettings.json (Development Only)

**⚠️ WARNING:** Never commit passwords to source control!

Edit `Ecommerce.Identity.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com,1433;Database=EcommerceIdentityDb;User Id=admin;Password=YOUR_PASSWORD_HERE;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;"
  }
}
```

### Option 2: appsettings.Development.json (Better)

Create/Edit `Ecommerce.Identity.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com,1433;Database=EcommerceIdentityDb;User Id=admin;Password=YOUR_PASSWORD_HERE;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;"
  }
}
```

Add to `.gitignore`:
```
**/appsettings.Development.json
**/appsettings.*.json
```

### Option 3: Environment Variables (Recommended for Production)

**Windows:**
```powershell
$env:ConnectionStrings__DefaultConnection="Server=ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com,1433;Database=EcommerceIdentityDb;User Id=admin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;"
```

**Linux/Mac:**
```bash
export ConnectionStrings__DefaultConnection="Server=ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com,1433;Database=EcommerceIdentityDb;User Id=admin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;"
```

### Option 4: AWS Secrets Manager (Best Practice)

Store connection string in AWS Secrets Manager and retrieve at runtime.

**Store Secret:**
```bash
aws secretsmanager create-secret \
  --name ecommerce/identity/connectionstring \
  --secret-string "Server=ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com,1433;Database=EcommerceIdentityDb;User Id=admin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;"
```

Then retrieve in your application startup.

---

## 🧪 Testing the Connection

### Method 1: From Your Machine (sqlcmd)

Install SQL Server command-line tools, then:

```powershell
sqlcmd -S ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com,1433 -U admin -P "YOUR_PASSWORD" -d EcommerceIdentityDb -Q "SELECT @@VERSION"
```

### Method 2: SQL Server Management Studio (SSMS)

1. Open SSMS
2. Connect to Server:
   ```
   Server: ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com,1433
   Authentication: SQL Server Authentication
   Login: admin
   Password: YOUR_PASSWORD
   ```
3. Click **Connect**

### Method 3: Azure Data Studio

1. New Connection
2. Server: `ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com,1433`
3. Authentication: SQL Login
4. User name: `admin`
5. Password: `YOUR_PASSWORD`
6. Database: `EcommerceIdentityDb`

### Method 4: Run Your Application

```powershell
cd Ecommerce.Identity.API
dotnet run
```

Check logs for:
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (123ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT 1
```

If you see this, connection is successful!

---

## 🔒 Security Best Practices

### 1. Use AWS Secrets Manager

**Never hardcode passwords!**

```bash
# Store connection string
aws secretsmanager create-secret \
  --name prod/ecommerce/identity/db \
  --description "Identity API Database Connection" \
  --secret-string "Server=...;Password=...;"
```

### 2. Enable SSL/TLS

Always use `Encrypt=True` in your connection string.

### 3. Rotate Passwords Regularly

1. RDS Console → Databases → Modify
2. Change Master password
3. Update Secrets Manager
4. Restart application

### 4. Use IAM Database Authentication (Advanced)

Enable IAM authentication for password-less connections.

### 5. Network Security

- Use **private subnets** for production
- Use **VPN** or **AWS PrivateLink** to connect
- **Never expose** RDS to 0.0.0.0/0

### 6. Enable Encryption at Rest

AWS RDS supports encryption using AWS KMS.

### 7. Monitor Access

Enable **CloudWatch Logs** and **RDS Enhanced Monitoring**.

---

## 🛠️ Troubleshooting

### Issue: Cannot Connect to Database

**Check 1: Security Group**
```bash
# Verify your IP is allowed
aws ec2 describe-security-groups --group-ids sg-xxxxxxxxx
```

**Check 2: Public Access**
- RDS Console → Connectivity → Public accessibility should be "Yes"

**Check 3: Test Connection**
```powershell
Test-NetConnection -ComputerName ecommerce-identity-db.c9akkkkkkkkk.us-east-1.rds.amazonaws.com -Port 1433
```

### Issue: Login Failed for User 'admin'

**Check 1: Verify Password**
- Make sure password is correct
- Check for special characters that need escaping

**Check 2: Database Exists**
- Initial database name must be set during creation
- Or create manually after connection

### Issue: Database Not Created

**Solution:**

Connect with SSMS and run:
```sql
CREATE DATABASE EcommerceIdentityDb;
GO
```

Or run the setup script:
```powershell
sqlcmd -S YOUR_ENDPOINT,1433 -U admin -P "YOUR_PASSWORD" -i "Ecommerce.Identity.API\Database\setup.sql"
```

### Issue: SSL/TLS Connection Error

**Update connection string:**
```
TrustServerCertificate=True
```

Or install AWS RDS SSL certificate.

---

## 💰 Cost Optimization

### Free Tier (First 12 Months)

- **750 hours** per month of db.t3.micro instance usage
- **20 GB** of General Purpose SSD storage
- **20 GB** of backup storage

### Cost Estimate (After Free Tier)

**db.t3.micro (1 vCPU, 1 GB RAM):**
- Instance: ~$15-20/month
- Storage (20 GB): ~$2.30/month
- Backups (20 GB): ~$0.95/month
- **Total: ~$18-23/month**

**db.t3.small (2 vCPU, 2 GB RAM):**
- Instance: ~$30-40/month
- Storage (50 GB): ~$5.75/month
- **Total: ~$36-46/month**

### Reduce Costs

1. **Stop database when not in use** (Dev/Test only)
   - RDS Console → Stop temporarily (up to 7 days)
   
2. **Use Reserved Instances** (1 or 3 year commitment)
   - Save up to 60%

3. **Delete unnecessary snapshots**
   - RDS Console → Snapshots → Delete old ones

4. **Optimize storage**
   - Monitor usage and reduce allocated storage if not needed

---

## 📊 Quick Reference

### Connection String Template

```
Server={RDS_ENDPOINT},1433;Database=EcommerceIdentityDb;User Id=admin;Password={PASSWORD};Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;
```

### Security Group Rule

```
Type: MS SQL
Port: 1433
Source: My IP
```

### Test Connection

```powershell
sqlcmd -S {ENDPOINT},1433 -U admin -P "{PASSWORD}" -Q "SELECT @@VERSION"
```

### Run Application

```powershell
cd Ecommerce.Identity.API
dotnet run
```

### Access Swagger

```
http://localhost:5001/swagger
```

---

## 🎯 Complete Setup Checklist

- [ ] AWS account created
- [ ] RDS SQL Server instance created
- [ ] Security group configured (port 1433 open)
- [ ] Connection string obtained
- [ ] `appsettings.json` updated with connection string
- [ ] Test connection with sqlcmd or SSMS
- [ ] Run application: `dotnet run`
- [ ] Database and tables created automatically
- [ ] Test API endpoints in Swagger
- [ ] Run `.\test-api.ps1` successfully

---

## 🚀 Next Steps

1. ✅ Create RDS instance
2. ✅ Configure security group
3. ✅ Update connection string
4. ✅ Test connection
5. ✅ Run application
6. ✅ Test APIs
7. ⏳ Deploy application to AWS (EC2, ECS, or Elastic Beanstalk)

---

## 📞 Need Help?

**AWS RDS Documentation:**
https://docs.aws.amazon.com/rds/

**AWS Support:**
- Basic: 24/7 customer service
- Developer: $29/month
- Business: $100/month

**Common AWS Regions:**
- `us-east-1` - N. Virginia (most common)
- `us-west-2` - Oregon
- `eu-west-1` - Ireland
- `ap-southeast-1` - Singapore

---

## 🎉 You're Ready!

Once your RDS instance is running and connection string is configured, your application will:

1. ✅ Connect to AWS RDS automatically
2. ✅ Create database if not exists
3. ✅ Apply migrations automatically
4. ✅ Be ready to handle requests

**Your application is now cloud-ready!** 🚀

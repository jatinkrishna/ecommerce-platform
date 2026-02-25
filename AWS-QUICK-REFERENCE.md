# ⚡ AWS RDS SQL Server - Quick Reference Card

## 🎯 Step-by-Step Setup (5 Minutes)

### 1️⃣ Create RDS Instance
```
AWS Console → RDS → Create database
  Engine: Microsoft SQL Server
  Edition: SQL Server Express (Free tier)
  Template: Free tier
  DB identifier: ecommerce-identity-db
  Master username: admin
  Master password: [CREATE STRONG PASSWORD]
  Instance: db.t3.micro
  Storage: 20 GB
  Public access: Yes
  Initial database: EcommerceIdentityDb
  → Create database (wait 10-15 min)
```

### 2️⃣ Configure Security
```
RDS Console → Your database → VPC security group → Edit inbound rules
  → Add rule:
     Type: MS SQL
     Port: 1433
     Source: My IP
  → Save rules
```

### 3️⃣ Get Connection Details
```
RDS Console → Your database → Connectivity & security
  Copy: Endpoint (example: ecommerce-identity-db.xxx.us-east-1.rds.amazonaws.com)
  Port: 1433
```

### 4️⃣ Build Connection String
```
Server=[YOUR_ENDPOINT],1433;Database=EcommerceIdentityDb;User Id=admin;Password=[YOUR_PASSWORD];Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;
```

### 5️⃣ Update Your Application
```
Edit: Ecommerce.Identity.API/appsettings.json

Replace this line:
"DefaultConnection": "Server=YOUR_RDS_ENDPOINT.rds.amazonaws.com,1433;Database=EcommerceIdentityDb;User Id=admin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;"
```

### 6️⃣ Test Connection
```powershell
# From command line
sqlcmd -S [ENDPOINT],1433 -U admin -P "[PASSWORD]" -Q "SELECT @@VERSION"

# Or run your app
cd Ecommerce.Identity.API
dotnet run

# Open Swagger
start https://localhost:7001/swagger
```

---

## 📋 Connection String Template

```
Server=YOUR_ENDPOINT_HERE.rds.amazonaws.com,1433;Database=EcommerceIdentityDb;User Id=admin;Password=YOUR_PASSWORD_HERE;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;
```

**Replace:**
- `YOUR_ENDPOINT_HERE` with your RDS endpoint
- `YOUR_PASSWORD_HERE` with your master password

---

## 🔒 Security Group Rule

```
Type:     MS SQL
Protocol: TCP
Port:     1433
Source:   My IP (or your specific IP)
```

---

## 💰 Cost Estimate

**Free Tier (12 months):**
- ✅ 750 hours/month db.t3.micro
- ✅ 20 GB storage
- ✅ 20 GB backups

**After Free Tier:**
- db.t3.micro: ~$15-20/month
- db.t3.small: ~$30-40/month

---

## 🧪 Test Commands

**Test Network Connection:**
```powershell
Test-NetConnection -ComputerName YOUR_ENDPOINT.rds.amazonaws.com -Port 1433
```

**Test SQL Connection:**
```powershell
sqlcmd -S YOUR_ENDPOINT.rds.amazonaws.com,1433 -U admin -P "YOUR_PASSWORD" -Q "SELECT @@VERSION"
```

**Test from Application:**
```powershell
cd Ecommerce.Identity.API
dotnet run
# Check logs for successful connection
```

---

## 🛠️ Troubleshooting

| Problem | Solution |
|---------|----------|
| Can't connect | Check security group allows your IP on port 1433 |
| Login failed | Verify username and password are correct |
| Timeout | Check "Public access" is set to "Yes" |
| SSL error | Add `TrustServerCertificate=True` to connection string |

---

## 📞 Quick Links

- **AWS Console:** https://console.aws.amazon.com
- **RDS Dashboard:** AWS Console → Services → RDS
- **Full Guide:** See [AWS-SETUP-GUIDE.md](./AWS-SETUP-GUIDE.md)

---

## ✅ Verification Checklist

- [ ] RDS instance shows "Available" status
- [ ] Security group has port 1433 open to your IP
- [ ] Connection string copied to appsettings.json
- [ ] Test connection with sqlcmd succeeds
- [ ] Application runs without errors
- [ ] Swagger UI opens and works
- [ ] Can register and login users

---

## 🚀 You're All Set!

**Your app is now connected to AWS RDS SQL Server!**

Database migrations run automatically when you start the app.

Test it: `.\test-api.ps1` 🎉

# 🔧 Phase 3 - Day 1: Troubleshooting Guide

## ❌ **Issue: RabbitMQ Connection Not Establishing**

### **Symptoms:**
- ✗ No "RabbitMQ connection established" log in console
- ✗ No "ecommerce.events" exchange in RabbitMQ UI
- ✗ No connections shown in RabbitMQ UI

### **Root Cause:**
The `RabbitMQEventPublisher` was registered as a singleton but was **lazy-loaded**. It won't initialize until something requests it. Since nothing was using the publisher yet, the connection never happened.

---

## ✅ **Solution Applied**

I've updated `Program.cs` to **force initialization** at startup. The publisher is now resolved during the startup process, which triggers the connection.

---

## 🚀 **Testing the Fix**

### **Step 1: Stop Current API**
If the API is running, stop it (Ctrl+C in terminal or Stop in Visual Studio)

### **Step 2: Make Sure RabbitMQ is Running**

```powershell
# Check if RabbitMQ is running
docker ps | findstr rabbitmq

# If not running, start it
docker-compose up -d rabbitmq

# Wait 10 seconds for it to fully start
Start-Sleep -Seconds 10

# Check logs
docker logs ecommerce-rabbitmq
```

**Expected in logs:**
```
Server startup complete; 0 plugins started.
```

### **Step 3: Restart the API**

```powershell
cd Ecommerce.Identity.API
dotnet run
```

**Expected Console Output (in order):**
```
[10:30:40 INF] Starting Ecommerce Identity API
[10:30:41 INF] Applying database migrations...
[10:30:42 INF] Database migrations applied successfully
[10:30:42 INF] Initializing RabbitMQ connection...
[10:30:42 INF] RabbitMQ connection established. Exchange: ecommerce.events, Type: topic
[10:30:42 INF] RabbitMQ event publisher initialized successfully
[10:30:42 INF] Ecommerce Identity API started successfully
[10:30:42 INF] Now listening on: https://localhost:7010
```

### **Step 4: Verify in RabbitMQ UI**

1. **Open:** http://localhost:15672
2. **Login:** admin / admin123

#### **Check Exchanges:**
1. Click **"Exchanges"** tab
2. Look for **`ecommerce.events`**
3. Should show:
   - Type: **topic**
   - Features: **D** (Durable)
   - Message rate: **0** (nothing published yet)

#### **Check Connections:**
1. Click **"Connections"** tab
2. Should see:
   - 1 connection
   - From: **127.0.0.1** (or your IP)
   - User: **admin**
   - State: **running**
   - Channels: **1**

---

## 🧪 **Quick Verification Script**

Run this to verify everything:

```powershell
Write-Host "=== Phase 3 - Day 1 Verification ===" -ForegroundColor Cyan

# 1. Check RabbitMQ is running
Write-Host "`n1. Checking RabbitMQ..." -ForegroundColor Yellow
$rabbitmq = docker ps | Select-String "ecommerce-rabbitmq"
if ($rabbitmq) {
    Write-Host "✓ RabbitMQ is running" -ForegroundColor Green
} else {
    Write-Host "✗ RabbitMQ is NOT running" -ForegroundColor Red
    Write-Host "Run: docker-compose up -d rabbitmq" -ForegroundColor Yellow
    exit
}

# 2. Check RabbitMQ API
Write-Host "`n2. Checking RabbitMQ API..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:15672/api/overview" `
        -Headers @{Authorization=("Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123")))}
    Write-Host "✓ RabbitMQ Management API is accessible" -ForegroundColor Green
} catch {
    Write-Host "✗ Cannot access RabbitMQ Management API" -ForegroundColor Red
    exit
}

# 3. Check for exchange
Write-Host "`n3. Checking for ecommerce.events exchange..." -ForegroundColor Yellow
try {
    $exchanges = Invoke-RestMethod -Uri "http://localhost:15672/api/exchanges" `
        -Headers @{Authorization=("Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123")))}
    
    $ourExchange = $exchanges | Where-Object { $_.name -eq "ecommerce.events" }
    
    if ($ourExchange) {
        Write-Host "✓ ecommerce.events exchange exists" -ForegroundColor Green
        Write-Host "  Type: $($ourExchange.type)" -ForegroundColor Gray
        Write-Host "  Durable: $($ourExchange.durable)" -ForegroundColor Gray
    } else {
        Write-Host "✗ ecommerce.events exchange NOT found" -ForegroundColor Red
        Write-Host "API may not have started yet. Run the API and try again." -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Error checking exchanges" -ForegroundColor Red
}

# 4. Check for connections
Write-Host "`n4. Checking RabbitMQ connections..." -ForegroundColor Yellow
try {
    $connections = Invoke-RestMethod -Uri "http://localhost:15672/api/connections" `
        -Headers @{Authorization=("Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123")))}
    
    if ($connections.Count -gt 0) {
        Write-Host "✓ $($connections.Count) connection(s) found" -ForegroundColor Green
        foreach ($conn in $connections) {
            Write-Host "  Connection from: $($conn.peer_host):$($conn.peer_port)" -ForegroundColor Gray
            Write-Host "  State: $($conn.state)" -ForegroundColor Gray
        }
    } else {
        Write-Host "⚠ No connections found" -ForegroundColor Yellow
        Write-Host "API may not be running. Start the API and try again." -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Error checking connections" -ForegroundColor Red
}

Write-Host "`n=== Verification Complete ===" -ForegroundColor Cyan
```

---

## 🔍 **Common Issues & Solutions**

### **Issue 1: "Failed to initialize RabbitMQ connection"**

**Cause:** RabbitMQ is not running or not accessible

**Solution:**
```powershell
# Start RabbitMQ
docker-compose up -d rabbitmq

# Wait for it to start
Start-Sleep -Seconds 10

# Check logs
docker logs ecommerce-rabbitmq

# Restart API
cd Ecommerce.Identity.API
dotnet run
```

---

### **Issue 2: "Connection refused" in logs**

**Cause:** Wrong host/port in appsettings.json

**Solution:**
Check `appsettings.json`:
```json
{
  "RabbitMQ": {
    "Host": "localhost",  // Should be "localhost" for local dev
    "Port": 5672,         // Should be 5672
    "Username": "admin",
    "Password": "admin123"
  }
}
```

---

### **Issue 3: Exchange exists but API shows error**

**Cause:** Exchange already exists with different settings

**Solution:**
```powershell
# Delete the exchange
docker exec -it ecommerce-rabbitmq rabbitmqctl delete_exchange ecommerce.events

# Restart API
cd Ecommerce.Identity.API
dotnet run
```

---

### **Issue 4: "Authentication failed"**

**Cause:** Wrong username/password

**Solution:**
Check docker-compose.yml:
```yaml
rabbitmq:
  environment:
    - RABBITMQ_DEFAULT_USER=admin
    - RABBITMQ_DEFAULT_PASS=admin123
```

Must match appsettings.json:
```json
{
  "RabbitMQ": {
    "Username": "admin",
    "Password": "admin123"
  }
}
```

---

## 📊 **What Should You See Now**

### **In Console:**
```
[INF] Starting Ecommerce Identity API
[INF] Applying database migrations...
[INF] Database migrations applied successfully
[INF] Initializing RabbitMQ connection...        ← NEW
[INF] RabbitMQ connection established...         ← NEW
[INF] RabbitMQ event publisher initialized...    ← NEW
[INF] Ecommerce Identity API started successfully
[INF] Now listening on: https://localhost:7010
```

### **In RabbitMQ UI - Exchanges Tab:**
```
Name                Type    Features    Message rate
ecommerce.events   topic    D           0 in, 0 out
```

### **In RabbitMQ UI - Connections Tab:**
```
Name                           From        State    Channels
127.0.0.1:12345 -> 127.0.0.1  127.0.0.1   running  1
```

---

## ✅ **Success Criteria**

Phase 3 - Day 1 is properly working when:
- [x] RabbitMQ container is running
- [x] Console shows "RabbitMQ connection established"
- [x] Exchange "ecommerce.events" exists in UI
- [x] Connection shown in RabbitMQ UI
- [x] API starts without errors

---

## 🎯 **Next Steps**

Once everything is working:
1. ✅ RabbitMQ connection confirmed
2. ✅ Exchange created
3. ✅ Ready for Day 2: Event Publishing

**Tomorrow (Day 2):** We'll inject the event publisher into AuthService and actually publish events!

---

## 📞 **Still Having Issues?**

Run this diagnostic:

```powershell
# Full diagnostic
Write-Host "=== RabbitMQ Diagnostic ===" -ForegroundColor Cyan

Write-Host "`n1. Docker Status:" -ForegroundColor Yellow
docker ps | Select-String "rabbitmq"

Write-Host "`n2. RabbitMQ Logs (last 20 lines):" -ForegroundColor Yellow
docker logs --tail 20 ecommerce-rabbitmq

Write-Host "`n3. Network Connectivity:" -ForegroundColor Yellow
Test-NetConnection -ComputerName localhost -Port 5672

Write-Host "`n4. Management UI:" -ForegroundColor Yellow
Test-NetConnection -ComputerName localhost -Port 15672

Write-Host "`n5. Processes Listening:" -ForegroundColor Yellow
netstat -ano | Select-String ":5672"
netstat -ano | Select-String ":15672"
```

---

**Share the output if you're still stuck!**

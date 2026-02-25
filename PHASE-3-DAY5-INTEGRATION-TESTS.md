# 🧪 Phase 3 - Day 5: Integration Testing Guide

## 🎯 **Complete System Test**

This guide tests the entire event-driven flow from end to end.

---

## ✅ **Prerequisites**

Before testing, ensure all services are running:

```powershell
# Check Docker
docker ps

# Should see:
# - ecommerce-rabbitmq
# - ecommerce-redis

# Start if needed
docker-compose up -d
```

---

## 🧪 **Test 1: Complete Registration Flow**

### **Goal:** Test user registration → event publishing → event consumption → email notification

### **Steps:**

1. **Start all services:**

```powershell
# Terminal 1: Identity API
cd Ecommerce.Identity.API
dotnet run

# Terminal 2: Notification API
cd Ecommerce.Notification.API
dotnet run
```

2. **Register a user via Swagger:**
   - Open: http://localhost:5010/swagger
   - POST /api/auth/register
   - Use test data

3. **Verify in Identity API logs:**
```
✅ [INF] User registered successfully
✅ [INF] Published UserRegistered event
```

4. **Verify in Notification API logs:**
```
✅ [INF] Received event with routing key: userregisteredevent
✅ [INF] Processing UserRegistered event
✅ [INF] Email service is disabled (or email sent if enabled)
```

5. **Verify in RabbitMQ UI:**
   - Open: http://localhost:15672
   - Check: notification.service.queue shows messages consumed

**Expected:** All logs present, no errors ✅

---

## 🧪 **Test 2: Login Event Flow**

### **Goal:** Test login → event publishing → event consumption

### **Steps:**

1. **Login via Swagger:**
   - POST /api/auth/login
   - Use registered user credentials

2. **Verify Identity API logs:**
```
✅ [INF] User logged in successfully
✅ [INF] Published UserLoggedIn event
```

3. **Verify Notification API logs:**
```
✅ [INF] Received event with routing key: userlogginedevent
✅ [INF] Processing UserLoggedIn event
```

**Expected:** Login event received and logged ✅

---

## 🧪 **Test 3: Error Handling**

### **Goal:** Test system resilience to errors

### **Test 3.1: Invalid Registration**

```powershell
# Weak password
POST /api/auth/register
{
  "email": "test@test.com",
  "password": "weak"
}

# Expected: 400 Bad Request with validation errors
```

### **Test 3.2: Duplicate User**

```powershell
# Register same email twice
# Expected: 409 Conflict
```

### **Test 3.3: Invalid Login**

```powershell
# Wrong password
POST /api/auth/login
{
  "email": "test@test.com",
  "password": "WrongPassword123!"
}

# Expected: 401 Unauthorized
```

**Expected:** All errors handled gracefully, no crashes ✅

---

## 🧪 **Test 4: Performance Test**

### **Goal:** Test system under load

```powershell
# Register 10 users quickly
for ($i=1; $i -le 10; $i++) {
    Invoke-RestMethod -Uri "http://localhost:5010/api/auth/register" `
        -Method POST `
        -ContentType "application/json" `
        -Body "{\"email\":\"user$i@test.com\",\"password\":\"Test@123456\",\"confirmPassword\":\"Test@123456\",\"firstName\":\"User\",\"lastName\":\"$i\"}"
}
```

**Expected:**
- All registrations succeed
- All events processed
- No crashes
- Acceptable response times

---

## 🧪 **Test 5: Health Checks**

```powershell
# Identity API health
Invoke-RestMethod -Uri "http://localhost:5010/api/health"

# Notification API health
Invoke-RestMethod -Uri "http://localhost:7020/api/health"
```

**Expected:** Both return status: "Healthy" ✅

---

## 🧪 **Test 6: RabbitMQ Resilience**

### **Goal:** Test recovery from RabbitMQ downtime

1. **Stop RabbitMQ:**
```powershell
docker-compose stop rabbitmq
```

2. **Try to register a user**
   - Expected: Registration succeeds but event fails gracefully

3. **Restart RabbitMQ:**
```powershell
docker-compose start rabbitmq
```

4. **Notification API should reconnect automatically**

5. **Register another user**
   - Expected: Event processed successfully ✅

---

## 🧪 **Test 7: Email Service Test**

### **If Email is Enabled (Optional):**

1. **Configure Mailtrap/Gmail in appsettings.json**

2. **Set "Enabled": true**

3. **Restart Notification API**

4. **Register a user**

5. **Check inbox:**
   - ✅ Welcome email received
   - ✅ HTML template rendered correctly
   - ✅ User name personalized

---

## 📊 **Success Criteria**

### **All tests should show:**

- [x] ✅ Registration works
- [x] ✅ Login works
- [x] ✅ Events are published
- [x] ✅ Events are consumed
- [x] ✅ Emails are triggered
- [x] ✅ Error handling works
- [x] ✅ Health checks respond
- [x] ✅ System recovers from failures
- [x] ✅ No memory leaks
- [x] ✅ Good performance

---

## 🎊 **If All Tests Pass:**

**🏆 YOUR SYSTEM IS PRODUCTION READY! 🏆**

You have:
- ✅ Robust authentication
- ✅ Event-driven architecture
- ✅ Fault tolerance
- ✅ Comprehensive error handling
- ✅ Monitoring capabilities
- ✅ Email notifications

---

## 📚 **Next Steps**

1. **Deployment preparation**
2. **Set up monitoring (Application Insights, etc.)**
3. **Configure production SMTP**
4. **Set up CI/CD pipeline**
5. **Load testing**

---

**Run these tests and confirm everything works!** 🚀

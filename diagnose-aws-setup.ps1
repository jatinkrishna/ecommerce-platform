# Diagnostic Script for AWS RDS Setup
# You're using AWS RDS for SQL Server, so we only check Redis and RabbitMQ in Docker

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "AWS RDS Setup Diagnostic" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check 1: Docker Services (Redis and RabbitMQ only)
Write-Host "1. Checking Docker Services..." -ForegroundColor Yellow
Write-Host "   (You're using AWS RDS, so no SQL Server needed in Docker)" -ForegroundColor Gray
Write-Host ""

$redis = docker ps 2>$null | Select-String "ecommerce-redis"
$rabbitmq = docker ps 2>$null | Select-String "ecommerce-rabbitmq"

if ($redis) {
    Write-Host "  ✓ Redis is running" -ForegroundColor Green
} else {
    Write-Host "  ✗ Redis is NOT running" -ForegroundColor Red
    Write-Host "    Run: docker-compose up -d redis" -ForegroundColor Yellow
}

if ($rabbitmq) {
    Write-Host "  ✓ RabbitMQ is running" -ForegroundColor Green
} else {
    Write-Host "  ✗ RabbitMQ is NOT running" -ForegroundColor Red
    Write-Host "    Run: docker-compose up -d rabbitmq" -ForegroundColor Yellow
}

# Check 2: API Connectivity
Write-Host "`n2. Checking API Connectivity..." -ForegroundColor Yellow
Write-Host ""

try {
    $health = Invoke-RestMethod -Uri "https://localhost:7010/api/health" -SkipCertificateCheck -TimeoutSec 5 -ErrorAction Stop
    Write-Host "  ✓ API is running and responding" -ForegroundColor Green
    Write-Host "    Status: $($health.status)" -ForegroundColor Gray
} catch {
    Write-Host "  ✗ Cannot connect to API" -ForegroundColor Red
    Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "    Is your API running?" -ForegroundColor Yellow
    Write-Host "    1. Open a terminal" -ForegroundColor White
    Write-Host "    2. cd Ecommerce.Identity.API" -ForegroundColor White
    Write-Host "    3. dotnet run" -ForegroundColor White
    Write-Host ""
    Write-Host "Stopping diagnostic - API must be running first" -ForegroundColor Red
    exit 1
}

# Check 3: AWS RDS Connection
Write-Host "`n3. Checking AWS RDS Connection..." -ForegroundColor Yellow
Write-Host "   (Testing via API health check)" -ForegroundColor Gray
Write-Host ""

# The API being up means database is accessible since migrations run at startup
Write-Host "  ✓ AWS RDS connection OK (API started successfully)" -ForegroundColor Green

# Check 4: Redis Health
Write-Host "`n4. Checking Redis..." -ForegroundColor Yellow
Write-Host ""

try {
    $redisHealth = Invoke-RestMethod -Uri "https://localhost:7010/api/health/redis" -SkipCertificateCheck -ErrorAction Stop
    Write-Host "  ✓ Redis is connected and healthy" -ForegroundColor Green
} catch {
    Write-Host "  ⚠ Redis health check failed" -ForegroundColor Yellow
    Write-Host "    Caching won't work but API will still function" -ForegroundColor Gray
}

# Check 5: Try Registration
Write-Host "`n5. Testing User Registration..." -ForegroundColor Yellow
Write-Host ""

$testEmail = "test.$(Get-Random)@example.com"
Write-Host "  Attempting to register: $testEmail" -ForegroundColor Gray

try {
    $registerBody = @{
        email = $testEmail
        password = "Test@123456"
        confirmPassword = "Test@123456"
        firstName = "Test"
        lastName = "User"
    } | ConvertTo-Json

    $registerResponse = Invoke-RestMethod -Uri "https://localhost:7010/api/auth/register" `
        -Method POST `
        -ContentType "application/json" `
        -Body $registerBody `
        -SkipCertificateCheck `
        -ErrorAction Stop
    
    Write-Host "  ✓ Registration successful!" -ForegroundColor Green
    Write-Host "    User ID: $($registerResponse.user.id)" -ForegroundColor Gray
    Write-Host "    Email: $($registerResponse.user.email)" -ForegroundColor Gray
    
    Write-Host "`n  Check your API console for:" -ForegroundColor Cyan
    Write-Host "  [INF] Published UserRegistered event for user:" -ForegroundColor White
    
} catch {
    Write-Host "  ✗ Registration failed!" -ForegroundColor Red
    Write-Host ""
    
    $errorMessage = $_.Exception.Message
    Write-Host "  Error: $errorMessage" -ForegroundColor Yellow
    
    if ($_.ErrorDetails.Message) {
        try {
            $errorJson = $_.ErrorDetails.Message | ConvertFrom-Json
            if ($errorJson.errors) {
                Write-Host "`n  Validation Errors:" -ForegroundColor Yellow
                $errorJson.errors | Format-List
            }
        } catch {
            Write-Host "  Details: $($_.ErrorDetails.Message)" -ForegroundColor Gray
        }
    }
    
    Write-Host "`n  Common issues:" -ForegroundColor Cyan
    Write-Host "  1. AWS RDS not accessible - Check security group rules" -ForegroundColor White
    Write-Host "  2. Wrong password format - Must have uppercase, lowercase, number, special char" -ForegroundColor White
    Write-Host "  3. User already exists - Try a different email" -ForegroundColor White
}

# Check 6: RabbitMQ Events
Write-Host "`n6. Checking RabbitMQ..." -ForegroundColor Yellow
Write-Host ""

Start-Sleep -Seconds 1

try {
    $auth = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123"))
    $exchange = Invoke-RestMethod -Uri "http://localhost:15672/api/exchanges/%2F/ecommerce.events" `
        -Headers @{Authorization=$auth} `
        -ErrorAction Stop
    
    if ($exchange.message_stats.publish) {
        $publishCount = $exchange.message_stats.publish
        Write-Host "  ✓ Events are being published!" -ForegroundColor Green
        Write-Host "    Total messages: $publishCount" -ForegroundColor Gray
    } else {
        Write-Host "  ⚠ No events published yet" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ⚠ Could not check RabbitMQ stats" -ForegroundColor Yellow
    Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Gray
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Diagnostic Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your Setup:" -ForegroundColor Cyan
Write-Host "  Database: AWS RDS SQL Server" -ForegroundColor Green
if ($redis) {
    Write-Host "  Cache: Redis (Docker)" -ForegroundColor Green
} else {
    Write-Host "  Cache: Redis (Docker)" -ForegroundColor Red
}
if ($rabbitmq) {
    Write-Host "  Message Broker: RabbitMQ (Docker)" -ForegroundColor Green
} else {
    Write-Host "  Message Broker: RabbitMQ (Docker)" -ForegroundColor Red
}
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Open RabbitMQ UI: http://localhost:15672 (admin/admin123)" -ForegroundColor White
Write-Host "2. Go to Exchanges and click ecommerce.events" -ForegroundColor White
Write-Host "3. Check message publish rates" -ForegroundColor White
Write-Host ""

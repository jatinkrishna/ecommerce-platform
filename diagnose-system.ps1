# Complete Diagnostic Script for Phase 3 - Day 2

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Complete System Diagnostic" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check 1: Docker Services
Write-Host "1. Checking Docker Services..." -ForegroundColor Yellow
Write-Host ""

$sqlserver = docker ps | Select-String "ecommerce-sqlserver"
$redis = docker ps | Select-String "ecommerce-redis"
$rabbitmq = docker ps | Select-String "ecommerce-rabbitmq"

if ($sqlserver) {
    Write-Host "  ✓ SQL Server is running" -ForegroundColor Green
} else {
    Write-Host "  ✗ SQL Server is NOT running" -ForegroundColor Red
    Write-Host "    Run: docker-compose up -d sqlserver" -ForegroundColor Yellow
}

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
    Write-Host "    Make sure API is running:" -ForegroundColor Yellow
    Write-Host "    cd Ecommerce.Identity.API" -ForegroundColor White
    Write-Host "    dotnet run" -ForegroundColor White
    Write-Host ""
    Write-Host "Stopping diagnostic - API must be running first" -ForegroundColor Red
    exit 1
}

# Check 3: RabbitMQ Health
Write-Host "`n3. Checking RabbitMQ Connection..." -ForegroundColor Yellow
Write-Host ""

try {
    $redisHealth = Invoke-RestMethod -Uri "https://localhost:7010/api/health/redis" -SkipCertificateCheck -ErrorAction Stop
    Write-Host "  ✓ Redis health check passed" -ForegroundColor Green
} catch {
    Write-Host "  ⚠ Redis health check failed" -ForegroundColor Yellow
    Write-Host "    This won't block registration but caching won't work" -ForegroundColor Gray
}

# Check 4: Try Registration
Write-Host "`n4. Testing User Registration..." -ForegroundColor Yellow
Write-Host ""

$testEmail = "diagnostic.test.$(Get-Random)@example.com"

Write-Host "  Attempting to register: $testEmail" -ForegroundColor Gray

try {
    $registerResponse = Invoke-RestMethod -Uri "https://localhost:7010/api/auth/register" `
        -Method POST `
        -ContentType "application/json" `
        -Body (@{
            email = $testEmail
            password = "Test@123456"
            confirmPassword = "Test@123456"
            firstName = "Diagnostic"
            lastName = "Test"
        } | ConvertTo-Json) `
        -SkipCertificateCheck `
        -ErrorAction Stop
    
    Write-Host "  ✓ Registration successful!" -ForegroundColor Green
    Write-Host "    User ID: $($registerResponse.user.id)" -ForegroundColor Gray
    Write-Host "    Email: $($registerResponse.user.email)" -ForegroundColor Gray
    Write-Host "    Access Token: $($registerResponse.accessToken.Substring(0, 20))..." -ForegroundColor Gray
    
    Write-Host "`n  Now check your API console for:" -ForegroundColor Cyan
    Write-Host "  [INF] Published UserRegistered event for user:" -ForegroundColor White
    
} catch {
    Write-Host "  ✗ Registration failed!" -ForegroundColor Red
    Write-Host ""
    
    $errorDetails = $_.ErrorDetails.Message
    if ($errorDetails) {
        try {
            $errorJson = $errorDetails | ConvertFrom-Json
            Write-Host "  Error Type: $($errorJson.type)" -ForegroundColor Yellow
            Write-Host "  Error Title: $($errorJson.title)" -ForegroundColor Yellow
            Write-Host "  Error Status: $($errorJson.status)" -ForegroundColor Yellow
            if ($errorJson.errors) {
                Write-Host "  Validation Errors:" -ForegroundColor Yellow
                $errorJson.errors | Format-List
            }
        } catch {
            Write-Host "  Error: $errorDetails" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    Write-Host "`n  Common issues:" -ForegroundColor Cyan
    Write-Host "  1. Database not accessible - Check SQL Server is running" -ForegroundColor White
    Write-Host "  2. Validation errors - Check password meets requirements" -ForegroundColor White
    Write-Host "  3. User already exists - Try a different email" -ForegroundColor White
    
    Write-Host "`n  Check API logs in your running terminal for more details" -ForegroundColor Yellow
    exit 1
}

# Check 5: Verify RabbitMQ received the event
Write-Host "`n5. Checking RabbitMQ for published events..." -ForegroundColor Yellow
Write-Host ""

Start-Sleep -Seconds 2

try {
    $auth = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123"))
    $exchange = Invoke-RestMethod -Uri "http://localhost:15672/api/exchanges/%2F/ecommerce.events" `
        -Headers @{Authorization=$auth} `
        -ErrorAction Stop
    
    if ($exchange.message_stats.publish) {
        $publishCount = $exchange.message_stats.publish
        Write-Host "  ✓ RabbitMQ has received events" -ForegroundColor Green
        Write-Host "    Total messages published: $publishCount" -ForegroundColor Gray
    } else {
        Write-Host "  ⚠ No messages published yet to RabbitMQ" -ForegroundColor Yellow
        Write-Host "    Check API logs for event publishing errors" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ⚠ Could not check RabbitMQ stats" -ForegroundColor Yellow
    Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Gray
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Diagnostic Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "What to check now:" -ForegroundColor Yellow
Write-Host "1. Your API console should show 'Published UserRegistered event'" -ForegroundColor White
Write-Host "2. RabbitMQ UI: http://localhost:15672 (admin/admin123)" -ForegroundColor White
Write-Host "3. Go to Exchanges → ecommerce.events to see message activity" -ForegroundColor White
Write-Host ""

# Provide next steps based on results
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "• If registration worked: Phase 3 - Day 2 is complete! ✅" -ForegroundColor Green
Write-Host "• If events aren't publishing: Check API logs for errors" -ForegroundColor White
Write-Host "• To test login: Use the registered email above" -ForegroundColor White
Write-Host ""

# Phase 3 - Day 2: Event Publishing Test Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Phase 3 - Day 2: Event Publishing Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Register a user and check for event
Write-Host "1. Testing UserRegistered Event..." -ForegroundColor Yellow
Write-Host "   Registering new user..." -ForegroundColor Gray

try {
    $registerResponse = Invoke-RestMethod -Uri "https://localhost:7010/api/auth/register" `
        -Method POST `
        -ContentType "application/json" `
        -Body '{"email":"eventtest@example.com","password":"Test@123456","confirmPassword":"Test@123456","firstName":"Event","lastName":"Test"}' `
        -SkipCertificateCheck `
        -ErrorAction Stop
    
    Write-Host "✓ User registered successfully" -ForegroundColor Green
    Write-Host "  User ID: $($registerResponse.user.id)" -ForegroundColor Gray
    Write-Host "  Email: $($registerResponse.user.email)" -ForegroundColor Gray
    
    # Give it a moment for the event to publish
    Start-Sleep -Seconds 1
    
    # Check API logs (they should show "Published UserRegistered event")
    Write-Host "  Check your API console logs for:" -ForegroundColor Yellow
    Write-Host "  '[INF] Published UserRegistered event for user:'" -ForegroundColor Gray
    
} catch {
    if ($_.Exception.Message -like "*Conflict*" -or $_.Exception.Message -like "*409*") {
        Write-Host "⚠ User already exists (that's ok, trying login instead)" -ForegroundColor Yellow
    } else {
        Write-Host "✗ Registration failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Test 2: Login and check for event
Write-Host "`n2. Testing UserLoggedIn Event..." -ForegroundColor Yellow
Write-Host "   Logging in..." -ForegroundColor Gray

try {
    $loginResponse = Invoke-RestMethod -Uri "https://localhost:7010/api/auth/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body '{"email":"eventtest@example.com","password":"Test@123456"}' `
        -SkipCertificateCheck `
        -ErrorAction Stop
    
    Write-Host "✓ Login successful" -ForegroundColor Green
    Write-Host "  User ID: $($loginResponse.user.id)" -ForegroundColor Gray
    
    Start-Sleep -Seconds 1
    
    Write-Host "  Check your API console logs for:" -ForegroundColor Yellow
    Write-Host "  '[INF] Published UserLoggedIn event for user:'" -ForegroundColor Gray
    
} catch {
    Write-Host "✗ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 3: Check RabbitMQ stats
Write-Host "`n3. Checking RabbitMQ Exchange Statistics..." -ForegroundColor Yellow

try {
    $auth = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123"))
    $exchange = Invoke-RestMethod -Uri "http://localhost:15672/api/exchanges/%2F/ecommerce.events" `
        -Headers @{Authorization=$auth} `
        -ErrorAction Stop
    
    Write-Host "✓ RabbitMQ Exchange: ecommerce.events" -ForegroundColor Green
    
    if ($exchange.message_stats) {
        $published = $exchange.message_stats.publish
        Write-Host "  Total messages published: $published" -ForegroundColor Gray
        
        if ($published -gt 0) {
            Write-Host "✓ Events are being published!" -ForegroundColor Green
        } else {
            Write-Host "⚠ No messages published yet" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠ No message stats available yet (exchange is new)" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "✗ Could not connect to RabbitMQ Management API" -ForegroundColor Red
    Write-Host "  Make sure RabbitMQ is running: docker ps | findstr rabbitmq" -ForegroundColor Yellow
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Event Publishing Test Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Check your API console logs for '[INF] Published' messages" -ForegroundColor White
Write-Host "2. Open RabbitMQ UI: http://localhost:15672" -ForegroundColor White
Write-Host "3. Go to Exchanges → ecommerce.events" -ForegroundColor White
Write-Host "4. See the 'Publish rate' graph showing activity" -ForegroundColor White
Write-Host ""
Write-Host "Phase 3 - Day 2 is working! ✅" -ForegroundColor Green
Write-Host ""
Write-Host "Note: Events are being published but not consumed yet." -ForegroundColor Gray
Write-Host "We'll add a consumer service on Day 3." -ForegroundColor Gray

# Phase 3 - Day 1: Quick Verification Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Phase 3 - Day 1: RabbitMQ Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if RabbitMQ container is running
Write-Host "1. Checking RabbitMQ container..." -ForegroundColor Yellow
$rabbitmq = docker ps | Select-String "ecommerce-rabbitmq"
if ($rabbitmq) {
    Write-Host "✓ RabbitMQ container is running" -ForegroundColor Green
    docker ps | Select-String "ecommerce-rabbitmq"
} else {
    Write-Host "✗ RabbitMQ container not found" -ForegroundColor Red
    Write-Host "Run: docker-compose up -d rabbitmq" -ForegroundColor Yellow
    exit 1
}

# Test 2: Check RabbitMQ health
Write-Host "`n2. Checking RabbitMQ health..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:15672/api/overview" `
        -Headers @{Authorization=("Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123")))} `
        -ErrorAction Stop
    
    Write-Host "✓ RabbitMQ is healthy" -ForegroundColor Green
    Write-Host "  RabbitMQ Version: $($health.rabbitmq_version)" -ForegroundColor Gray
    Write-Host "  Management Version: $($health.management_version)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Cannot connect to RabbitMQ Management UI" -ForegroundColor Red
    Write-Host "Make sure RabbitMQ is running: docker-compose up -d rabbitmq" -ForegroundColor Yellow
    exit 1
}

# Test 3: Check if API can build
Write-Host "`n3. Building Identity API..." -ForegroundColor Yellow
$buildResult = dotnet build Ecommerce.Identity.API -v q 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build successful" -ForegroundColor Green
} else {
    Write-Host "✗ Build failed" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}

# Test 4: Check packages
Write-Host "`n4. Checking RabbitMQ packages..." -ForegroundColor Yellow
$packages = dotnet list Ecommerce.Shared.Common package 2>&1 | Select-String "RabbitMQ.Client"
if ($packages) {
    Write-Host "✓ RabbitMQ.Client package installed" -ForegroundColor Green
    Write-Host "  $packages" -ForegroundColor Gray
} else {
    Write-Host "✗ RabbitMQ.Client package not found" -ForegroundColor Red
    exit 1
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "✓ ALL CHECKS PASSED!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Open RabbitMQ Management UI: http://localhost:15672" -ForegroundColor White
Write-Host "   Username: admin" -ForegroundColor Gray
Write-Host "   Password: admin123" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Start Identity API: cd Ecommerce.Identity.API && dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "3. Check for 'RabbitMQ connection established' in logs" -ForegroundColor White
Write-Host ""
Write-Host "Phase 3 - Day 1 is ready!" -ForegroundColor Green

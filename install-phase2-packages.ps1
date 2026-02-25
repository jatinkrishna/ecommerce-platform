# Phase 2 - Day 1: NuGet Package Installation Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Phase 2 - Day 1 Package Installation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to the Identity API project
$projectPath = "Ecommerce.Identity.API"

if (Test-Path $projectPath) {
    Write-Host "✓ Found project directory: $projectPath" -ForegroundColor Green
    Set-Location $projectPath
} else {
    Write-Host "✗ Project directory not found: $projectPath" -ForegroundColor Red
    Write-Host "Please run this script from the solution root directory" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Installing NuGet packages..." -ForegroundColor Yellow
Write-Host ""

# Install Redis packages
Write-Host "1. Installing StackExchange.Redis..." -ForegroundColor Cyan
dotnet add package StackExchange.Redis --version 2.7.10
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ StackExchange.Redis installed successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to install StackExchange.Redis" -ForegroundColor Red
}

Write-Host ""
Write-Host "2. Installing Microsoft.Extensions.Caching.StackExchangeRedis..." -ForegroundColor Cyan
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis --version 8.0.0
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Microsoft.Extensions.Caching.StackExchangeRedis installed successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to install Microsoft.Extensions.Caching.StackExchangeRedis" -ForegroundColor Red
}

Write-Host ""
Write-Host "3. Installing AspNetCoreRateLimit..." -ForegroundColor Cyan
dotnet add package AspNetCoreRateLimit --version 5.0.0
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ AspNetCoreRateLimit installed successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to install AspNetCoreRateLimit" -ForegroundColor Red
}

Write-Host ""
Write-Host "4. Restoring all packages..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to restore packages" -ForegroundColor Red
}

Write-Host ""
Write-Host "5. Building project..." -ForegroundColor Cyan
dotnet build
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Project built successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "✓ ALL PACKAGES INSTALLED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Start Redis: docker-compose up -d redis" -ForegroundColor White
    Write-Host "2. Run the API: dotnet run" -ForegroundColor White
    Write-Host "3. Follow PHASE-2-DAY1-VERIFICATION.md for testing" -ForegroundColor White
} else {
    Write-Host "✗ Build failed - please check error messages above" -ForegroundColor Red
}

Write-Host ""
Set-Location ..

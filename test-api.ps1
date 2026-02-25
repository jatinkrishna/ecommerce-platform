# Ecommerce Identity API - Test Script
# This script tests all endpoints of the Identity API

$baseUrl = "https://localhost:7001"
$headers = @{
    "Content-Type" = "application/json"
}

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Ecommerce Identity API - Test Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Ignore SSL certificate errors for local testing
if (-not ([System.Management.Automation.PSTypeName]'ServerCertificateValidationCallback').Type) {
    $certCallback = @"
        using System;
        using System.Net;
        using System.Net.Security;
        using System.Security.Cryptography.X509Certificates;
        public class ServerCertificateValidationCallback {
            public static void Ignore() {
                if(ServicePointManager.ServerCertificateValidationCallback ==null) {
                    ServicePointManager.ServerCertificateValidationCallback += 
                        delegate(
                            Object obj, 
                            X509Certificate certificate, 
                            X509Chain chain, 
                            SslPolicyErrors errors
                        ) {
                            return true;
                        };
                }
            }
        }
"@
    Add-Type $certCallback
}
[ServerCertificateValidationCallback]::Ignore()

# Variables to store tokens
$accessToken = ""
$refreshToken = ""
$testEmail = "testuser_$(Get-Random)@example.com"

# Test 1: Health Check
Write-Host "1. Testing Health Check..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/health" -Method Get
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✅ Health check passed" -ForegroundColor Green
    }
} catch {
    Write-Host "   ❌ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Make sure the API is running!" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Register New User
Write-Host "2. Testing User Registration..." -ForegroundColor Yellow
$registerBody = @{
    email = $testEmail
    password = "Test@123456"
    confirmPassword = "Test@123456"
    firstName = "Test"
    lastName = "User"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Headers $headers -Body $registerBody
    $accessToken = $response.accessToken
    $refreshToken = $response.refreshToken
    
    Write-Host "   ✅ Registration successful" -ForegroundColor Green
    Write-Host "   Email: $testEmail" -ForegroundColor Gray
    Write-Host "   User ID: $($response.user.id)" -ForegroundColor Gray
    Write-Host "   Roles: $($response.user.roles -join ', ')" -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Registration failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: Login
Write-Host "3. Testing User Login..." -ForegroundColor Yellow
$loginBody = @{
    email = $testEmail
    password = "Test@123456"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Headers $headers -Body $loginBody
    $accessToken = $response.accessToken
    $refreshToken = $response.refreshToken
    
    Write-Host "   ✅ Login successful" -ForegroundColor Green
    Write-Host "   Token expires in: $($response.expiresIn) seconds" -ForegroundColor Gray
    Write-Host "   Access Token (first 50 chars): $($accessToken.Substring(0, [Math]::Min(50, $accessToken.Length)))..." -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 4: Get User Profile (Authenticated)
Write-Host "4. Testing Get User Profile (Authenticated)..." -ForegroundColor Yellow
$authHeaders = @{
    "Content-Type" = "application/json"
    "Authorization" = "Bearer $accessToken"
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/profile" -Method Get -Headers $authHeaders
    
    Write-Host "   ✅ Profile retrieval successful" -ForegroundColor Green
    Write-Host "   Name: $($response.firstName) $($response.lastName)" -ForegroundColor Gray
    Write-Host "   Email: $($response.email)" -ForegroundColor Gray
    Write-Host "   Active: $($response.isActive)" -ForegroundColor Gray
    Write-Host "   Created: $($response.createdAt)" -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Profile retrieval failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 5: Refresh Token
Write-Host "5. Testing Token Refresh..." -ForegroundColor Yellow
$refreshBody = @{
    accessToken = $accessToken
    refreshToken = $refreshToken
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/refresh-token" -Method Post -Headers $headers -Body $refreshBody
    $newAccessToken = $response.accessToken
    $newRefreshToken = $response.refreshToken
    
    Write-Host "   ✅ Token refresh successful" -ForegroundColor Green
    Write-Host "   New access token received" -ForegroundColor Gray
    Write-Host "   New refresh token received" -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Token refresh failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 6: Invalid Login
Write-Host "6. Testing Invalid Login (Security)..." -ForegroundColor Yellow
$invalidLoginBody = @{
    email = $testEmail
    password = "WrongPassword"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Headers $headers -Body $invalidLoginBody
    Write-Host "   ❌ Security issue: Invalid login should fail!" -ForegroundColor Red
} catch {
    Write-Host "   ✅ Invalid login correctly rejected" -ForegroundColor Green
}

Write-Host ""

# Test 7: Unauthenticated Profile Access
Write-Host "7. Testing Unauthenticated Access (Security)..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/profile" -Method Get -Headers $headers
    Write-Host "   ❌ Security issue: Should require authentication!" -ForegroundColor Red
} catch {
    Write-Host "   ✅ Unauthenticated access correctly blocked" -ForegroundColor Green
}

Write-Host ""

# Test 8: Duplicate Registration
Write-Host "8. Testing Duplicate Registration (Validation)..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Headers $headers -Body $registerBody
    Write-Host "   ❌ Validation issue: Duplicate email should be rejected!" -ForegroundColor Red
} catch {
    Write-Host "   ✅ Duplicate registration correctly rejected" -ForegroundColor Green
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "All Tests Completed!" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test User Created:" -ForegroundColor Yellow
Write-Host "Email: $testEmail" -ForegroundColor Gray
Write-Host "Password: Test@123456" -ForegroundColor Gray
Write-Host ""
Write-Host "You can now test these credentials in Swagger UI:" -ForegroundColor Yellow
Write-Host "$baseUrl/swagger" -ForegroundColor Cyan

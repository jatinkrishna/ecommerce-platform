# Ecommerce Identity API

Authentication and user management microservice for the E-Commerce platform.

## Features

- ✅ User Registration
- ✅ User Login with JWT tokens
- ✅ Refresh Token mechanism
- ✅ User Profile retrieval
- ✅ Password hashing with BCrypt
- ✅ Role-based authorization
- ✅ Global exception handling
- ✅ Swagger documentation
- ✅ Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ Repository pattern
- ✅ Service pattern

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication
- BCrypt.Net for password hashing
- Swagger/OpenAPI

## Architecture

```
Ecommerce.Identity.API/
├── Controllers/           # API endpoints
├── Application/
│   ├── Interfaces/       # Service interfaces
│   └── Services/         # Business logic
├── Domain/
│   └── Repositories/     # Repository interfaces
├── Infrastructure/
│   ├── Data/            # DbContext
│   └── Repositories/    # Repository implementations
└── Middleware/          # Custom middleware (exception handling)
```

## Setup Instructions

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full SQL Server)
- Visual Studio 2022 or VS Code

### 1. Update Connection String

Edit `appsettings.json` or `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=EcommerceIdentityDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 2. Update JWT Configuration

⚠️ **IMPORTANT**: Change the JWT secret in production!

```json
"Jwt": {
  "Secret": "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS",
  "Issuer": "EcommerceIdentityAPI",
  "Audience": "EcommerceClients",
  "ExpirationInMinutes": "60",
  "RefreshTokenExpirationInDays": "7"
}
```

### 3. Create Database Migration

```bash
cd Ecommerce.Identity.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5001`
- HTTPS: `https://localhost:7001`
- Swagger: `https://localhost:7001/swagger`

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login user | No |
| POST | `/api/auth/refresh-token` | Refresh access token | No |
| GET | `/api/auth/profile` | Get user profile | Yes |

## Example Requests

### Register User

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

### Response

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedtoken...",
  "expiresIn": 3600,
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Customer"],
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

### Refresh Token

```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "accessToken": "expired_token",
  "refreshToken": "valid_refresh_token"
}
```

### Get Profile

```http
GET /api/auth/profile
Authorization: Bearer your_access_token
```

## Default Roles

- `Admin` - Full system access
- `Customer` - Default role for new users
- `Seller` - For vendors
- `Support` - Customer support
- `Manager` - Management role

## Security Features

- ✅ Password hashing with BCrypt (salt rounds: 11)
- ✅ JWT token with configurable expiration
- ✅ Refresh token mechanism
- ✅ Token validation
- ✅ Role-based authorization
- ✅ Account activation status check
- ✅ Secure password requirements

## Database Schema

### Users Table

| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | Primary key |
| Email | nvarchar(256) | Unique email address |
| PasswordHash | nvarchar(max) | BCrypt hashed password |
| FirstName | nvarchar(50) | User's first name |
| LastName | nvarchar(50) | User's last name |
| Roles | nvarchar(max) | JSON array of roles |
| IsActive | bit | Account status |
| RefreshToken | nvarchar(max) | Current refresh token |
| RefreshTokenExpiryTime | datetime2 | Token expiration |
| CreatedAt | datetime2 | Account creation time |
| UpdatedAt | datetime2 | Last update time |
| LastLoginAt | datetime2 | Last login time |

## Next Steps

1. ✅ Identity Service (COMPLETED)
2. ⏳ Product Service
3. ⏳ Order Service
4. ⏳ Payment Service
5. ⏳ Notification Service
6. ⏳ API Gateway with YARP

## Contributing

This is a production-grade microservices platform following clean architecture and best practices.

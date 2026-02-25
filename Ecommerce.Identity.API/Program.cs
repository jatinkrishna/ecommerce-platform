using Ecommerce.Identity.API.Application.Interfaces;
using Ecommerce.Identity.API.Application.Services;
using Ecommerce.Identity.API.Configuration;
using Ecommerce.Identity.API.Domain.Repositories;
using Ecommerce.Identity.API.Infrastructure.Data;
using Ecommerce.Identity.API.Infrastructure.Repositories;
using Ecommerce.Identity.API.Infrastructure.Services;
using Ecommerce.Identity.API.Middleware;
using Ecommerce.Shared.Common.Messaging;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

// ========== PHASE 2 - DAY 3: Configure Serilog ==========
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

try
{
    Log.Information("Starting Ecommerce Identity API");

    var builder = WebApplication.CreateBuilder(args);

    // ========== PHASE 2 - DAY 3: Add Serilog ==========
    builder.Host.UseSerilog();
    // ========== END PHASE 2 - DAY 3 ==========

// Add services to the container
builder.Services.AddControllers();

// Configure Database
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== PHASE 2: Configure Redis ==========
var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled", true);
if (redisEnabled)
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName", "EcommerceIdentity:");
    });

    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
    builder.Logging.AddConsole();
}
else
{
    // Fallback to in-memory cache if Redis disabled
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}
// ========== END PHASE 2 ==========

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// ========== PHASE 2 - DAY 2: Token Blacklist Service ==========
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
// ========== END PHASE 2 - DAY 2 ==========

// ========== PHASE 3 - DAY 1: RabbitMQ Event Publisher ==========
var rabbitMQConfig = new RabbitMQConfiguration();
builder.Configuration.GetSection("RabbitMQ").Bind(rabbitMQConfig);
builder.Services.AddSingleton(rabbitMQConfig);
builder.Services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
// ========== END PHASE 3 - DAY 1 ==========

// ========== PHASE 2: Configure Rate Limiting ==========
builder.Services.AddRateLimiting(builder.Configuration);
// ========== END PHASE 2 ==========

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ecommerce Identity API",
        Version = "v1",
        Description = "Authentication and user management service for E-Commerce platform"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ========== PHASE 2: Middleware Pipeline (ORDER MATTERS!) ==========
// 1. Security Headers - FIRST
app.UseMiddleware<SecurityHeadersMiddleware>();

// 2. Rate Limiting - BEFORE routing
app.UseIpRateLimiting();

// 3. Request Logging - Log all requests (PHASE 2 - DAY 3)
app.UseMiddleware<RequestLoggingMiddleware>();
// ========== END PHASE 2 ==========

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce Identity API v1");
    });
}

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Auto-apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations");
        throw;
    }

    // ========== PHASE 3 - DAY 1: Initialize RabbitMQ Connection ==========
    try
    {
        Log.Information("Initializing RabbitMQ connection...");
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        Log.Information("RabbitMQ event publisher initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to initialize RabbitMQ connection. Event publishing will not work.");
        // Don't throw - allow app to start even if RabbitMQ is not available
    }
    // ========== END PHASE 3 - DAY 1 ==========
}

    Log.Information("Ecommerce Identity API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

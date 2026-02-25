using Ecommerce.Notification.API.Configuration;
using Ecommerce.Notification.API.Messaging;
using Ecommerce.Notification.API.Services;
using Ecommerce.Shared.Common.Messaging;
using Serilog;

// ========== PHASE 3 - DAY 3: Configure Serilog ==========
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting Ecommerce Notification API");

    var builder = WebApplication.CreateBuilder(args);

    // ========== PHASE 3 - DAY 3: Add Serilog ==========
    builder.Host.UseSerilog();
    // ========== END PHASE 3 - DAY 3 ==========

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ========== PHASE 3 - DAY 3: Configure RabbitMQ Consumer ==========
    var rabbitMQConfig = new RabbitMQConfiguration();
    builder.Configuration.GetSection("RabbitMQ").Bind(rabbitMQConfig);
    builder.Services.AddSingleton(rabbitMQConfig);

    // Register event consumer
    builder.Services.AddSingleton<IEventConsumer, RabbitMQEventConsumer>();

    // Register background service
    builder.Services.AddHostedService<EventConsumerHostedService>();
    // ========== END PHASE 3 - DAY 3 ==========

    // ========== PHASE 3 - DAY 4: Configure Email Service ==========
    var emailConfig = new EmailConfiguration();
    builder.Configuration.GetSection("Email").Bind(emailConfig);
    builder.Services.AddSingleton(emailConfig);

    // Register email service
    builder.Services.AddScoped<IEmailService, EmailService>();
    // ========== END PHASE 3 - DAY 4 ==========

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Ecommerce Notification API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly: {Message}", ex.Message);
    Console.WriteLine($"FATAL ERROR: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
}
finally
{
    Log.CloseAndFlush();
}

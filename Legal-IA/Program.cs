using Azure.Storage.Blobs;
using Legal_IA;
using Legal_IA.Data;
using Legal_IA.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configure Entity Framework with PostgreSQL
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection")
                       ?? "Host=localhost;Port=5433;Database=LegalIA;Username=postgres;Password=password";

builder.Services.AddDbContext<LegalIaDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// Configure Redis Cache
var redisConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:Redis")
                            ?? "localhost:6380";

// Configure Azure Blob Storage (Azurite)
var azuriteConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                              ??
                              "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

builder.Services.AddSingleton<BlobServiceClient>(serviceProvider => new BlobServiceClient(azuriteConnectionString));

// Register IConnectionMultiplexer for StackExchange.Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));

// Register repositories, services, validators, and external clients using Startup extensions
builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddExternalClients(redisConnectionString, azuriteConnectionString);

// Configure logging
builder.Services.AddLogging();

// Register JwtService
builder.Services.AddSingleton<JwtService>();

// Configure CORS using Startup extension
builder.Services.AddCorsConfiguration();

builder.Services.AddValidators();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LegalIaDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex,
            "Could not ensure database is created. This may be expected if database is not available during startup.");
    }
}

app.Run();
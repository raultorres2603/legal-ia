using Azure.Storage.Blobs;
using FluentValidation;
using Legal_IA.Data;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Repositories;
using Legal_IA.Interfaces.Services;
using Legal_IA.Repositories;
using Legal_IA.Services;
using Legal_IA.Validators;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configure Entity Framework with PostgreSQL
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                       ?? "Host=localhost;Port=5433;Database=LegalIA;Username=postgres;Password=password";

builder.Services.AddDbContext<LegalIADbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// Configure Redis Cache
var redisConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Redis")
                            ?? "localhost:6380";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "LegalIA";
});

// Configure Azure Blob Storage (Azurite)
var azuriteConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                              ??
                              "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

builder.Services.AddSingleton<BlobServiceClient>(serviceProvider =>
{
    return new BlobServiceClient(azuriteConnectionString);
});

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Register services
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAIDocumentGeneratorService, AiDocumentGeneratorService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Register validators
builder.Services.AddTransient<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddTransient<IValidator<CreateDocumentRequest>, CreateDocumentRequestValidator>();

// Configure logging
builder.Services.AddLogging();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LegalIADbContext>();
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
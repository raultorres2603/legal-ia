using AI_Agent;
using AI_Agent.Interfaces;
using AI_Agent.Services;
using Azure.Storage.Blobs;
using FluentValidation;
using Legal_IA.DTOs;
using Legal_IA.Shared.Repositories.Interfaces;
using Legal_IA.Interfaces.Services;
using Legal_IA.Shared.Repositories;
using Legal_IA.Services;
using Legal_IA.Validators;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Legal_IA;

public static class Startup
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<JwtService>();

        // Add AI Agent services
        services.AddAiAgentServices();
    }

    private static void AddAiAgentServices(this IServiceCollection services)
    {
        var openAiApiKey = Environment.GetEnvironmentVariable("OpenAI:ApiKey")
                           ?? throw new InvalidOperationException("OpenAI:ApiKey environment variable is required");

        // Register AI-Agent repository interfaces with their implementations
        services.AddScoped<IUserDataAggregatorService, UserDataAggregatorService>();

        // Register the agent with all dependencies
        services.AddScoped<ILegalAiAgent>(provider =>
            new LegalAiAgent(
                openAiApiKey,
                provider.GetRequiredService<AI_Agent.Services.IUserDataAggregatorService>()
            )
        );
    }

    public static void AddExternalClients(this IServiceCollection services, string redisConnectionString,
        string azuriteConnectionString)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "LegalIA";
        });
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<BlobServiceClient>(_ => new BlobServiceClient(azuriteConnectionString));
    }

    public static void AddCorsConfiguration(this IServiceCollection services)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                          throw new ArgumentNullException();
        var isLocal = environment is "Local" or "Development";

        services.AddCors(options =>
        {
            if (isLocal)
            {
                // Use CORS_ALLOWED_ORIGIN for local
                var localOrigin = Environment.GetEnvironmentVariable("LOCAL_DOMAIN") ??
                                  throw new ArgumentNullException();
                options.AddPolicy("LocalPolicy", policy =>
                {
                    policy.WithOrigins(localOrigin)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            }
            else
            {
                // Only allow https for production domain
                var prodDomain = Environment.GetEnvironmentVariable("PRODUCTION_DOMAIN") ??
                                 throw new ArgumentNullException();
                options.AddPolicy("ProductionPolicy", policy =>
                {
                    policy.WithOrigins(prodDomain)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            }

            // Default policy
            options.AddDefaultPolicy(policy =>
            {
                if (isLocal)
                {
                    var localOrigin = Environment.GetEnvironmentVariable("LOCAL_DOMAIN") ??
                                      throw new ArgumentNullException();
                    policy.WithOrigins(localOrigin)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
                else
                {
                    var prodDomain = Environment.GetEnvironmentVariable("PRODUCTION_DOMAIN") ??
                                     throw new ArgumentNullException();
                    policy.WithOrigins(prodDomain)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            });
        });
    }

    public static void AddValidators(this IServiceCollection services)
    {
        // Register validators manually for Azure Functions
        services.AddTransient<IValidator<BatchUpdateInvoiceItemRequest>, BatchUpdateInvoiceItemRequestValidator>();
        services.AddTransient<IValidator<UpdateInvoiceItemRequest>, UpdateInvoiceItemRequestValidator>();
    }
}
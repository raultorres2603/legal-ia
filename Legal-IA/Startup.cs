using Azure.Storage.Blobs;
using FluentValidation;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Repositories;
using Legal_IA.Interfaces.Services;
using Legal_IA.Repositories;
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
    }

    public static void AddValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
    }

    public static void AddExternalClients(this IServiceCollection services, string redisConnectionString,
        string azuriteConnectionString)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "LegalIA";
        });
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<BlobServiceClient>(serviceProvider => new BlobServiceClient(azuriteConnectionString));
    }

    public static void AddCorsConfiguration(this IServiceCollection services)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? throw new ArgumentNullException();
        bool isLocal = environment is "Local" or "Development";

        services.AddCors(options =>
        {
            if (isLocal)
            {
                // Use CORS_ALLOWED_ORIGIN for local
                var localOrigin = Environment.GetEnvironmentVariable("LOCAL_DOMAIN") ?? throw new ArgumentNullException();
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
                var prodDomain = Environment.GetEnvironmentVariable("PRODUCTION_DOMAIN") ?? throw new ArgumentNullException();
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
                    var localOrigin = Environment.GetEnvironmentVariable("LOCAL_DOMAIN") ?? throw new ArgumentNullException();
                    policy.WithOrigins(localOrigin)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                else
                {
                    var prodDomain = Environment.GetEnvironmentVariable("PRODUCTION_DOMAIN") ?? throw new ArgumentNullException();
                    policy.WithOrigins(prodDomain)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            });
        });
    }
}
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
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<JwtService>();
        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        return services;
    }

    public static IServiceCollection AddExternalClients(this IServiceCollection services, string redisConnectionString,
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
        return services;
    }
}
using App.Data;
using App.Services;
using Microsoft.EntityFrameworkCore;

namespace App.StaticTools.Extensions;

public static class ServiceCollectionExtensions
{
    // Adds database context service for user-selected provider.
    public static IServiceCollection AddAppDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useSqlite)
    {
        if (useSqlite)
        {
            const string connStrKey = "Sqlite";
            string connectionString = configuration.GetConnectionString(connStrKey) ??
                throw new InvalidOperationException($"Connection string '{connStrKey}' not found.");
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        }
        else
        {
            const string connStrKey = "SqlServerEnvVarKey";
            string envVarKey = configuration.GetConnectionString(connStrKey) ??
                throw new InvalidOperationException($"Connection string '{connStrKey}' not found.");
            string connectionString = configuration[envVarKey] ??
                throw new InvalidOperationException($"Environment variable '{envVarKey}' not found.");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        }

        return services;
    }

    // Adds messaging service as singleton, either as effective or stub object.
    public static async Task<IServiceCollection> AddMessagingService(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useMessaging)
    {
        IMessagingService messagingService;

        if (useMessaging)
        {
            messagingService = await MessagingServiceFactory.Create(configuration["Messaging:Host"]!);
        }
        else
        {
            messagingService = MessagingServiceFactory.CreateEmpty();
        }

        services.AddSingleton(messagingService);
        return services;
    }
}

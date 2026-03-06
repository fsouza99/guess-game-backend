using App.Applications;
using App.Data;
using App.Services;
using Microsoft.EntityFrameworkCore;

namespace App.StaticTools;

public static class ServiceCollectionExtensions
{
    // Add database context service for user-selected provider.
    public static IServiceCollection AddAppDbContext(
        this IServiceCollection services, IConfiguration configuration, bool useDbServer)
    {
        if (useDbServer)
        {
            const string connStrKey = "SqlServerEnvVarKey";
            string envVarKey = configuration.GetConnectionString(connStrKey) ??
                throw new InvalidOperationException(
                    $"Connection string '{connStrKey}' not found.");
            string connectionString = configuration[envVarKey] ??
                throw new InvalidOperationException(
                    $"Environment variable '{envVarKey}' not found.");
            services.AddDbContext<AppDbContext>(
                options => options.UseSqlServer(connectionString));
        }
        else
        {
            const string connStrKey = "Sqlite";
            string connectionString = configuration.GetConnectionString(connStrKey) ??
                throw new InvalidOperationException(
                    $"Connection string '{connStrKey}' not found.");
            services.AddDbContext<AppDbContext>(
                options => options.UseSqlite(connectionString));
        }

        return services;
    }

    // Add messaging service as singleton, either as effective or stub object.
    public static async Task<IServiceCollection> AddMessagingService(
        this IServiceCollection services, IConfiguration configuration, bool useMsgServer)
    {
        IMessagingService messagingService;

        if (useMsgServer)
        {
            messagingService = await MessagingServiceFactory.Create(
                configuration["Messaging:Host"]!);
        }
        else
        {
            messagingService = MessagingServiceFactory.CreateEmpty();
        }

        services.AddSingleton(messagingService);
        return services;
    }

    // Add aplication services.
    public static IServiceCollection AddApplications(this IServiceCollection services)
    {
        services.AddScoped<FormulaApp>();
        services.AddScoped<CompetitionApp>();
        services.AddScoped<GameApp>();
        services.AddScoped<GuessApp>();

        return services;
    }
}

using App.Globals;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure;

public static class InfrastructureServicesRegistration
{
    public static IServiceCollection AddDbContext(
        this IServiceCollection services, IConfiguration configuration, bool useDbServer)
    {
        if (useDbServer)
        {
            const string connStrKey = "SqlServerEnvVarKey";
            string envVarKey = configuration.GetConnectionString(connStrKey) ??
                throw new InvalidOperationException($"Connection string '{connStrKey}' not found.");
            string connectionString = configuration[envVarKey] ??
                throw new InvalidOperationException(
                    $"Environment variable '{envVarKey}' not found.");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        }
        else
        {
            const string connStrKey = "Sqlite";
            string connectionString = configuration.GetConnectionString(connStrKey) ??
                throw new InvalidOperationException($"Connection string '{connStrKey}' not found.");
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        }

        return services;
    }

    public static async Task<IServiceCollection> AddMessaging(
        this IServiceCollection services, IConfiguration configuration, bool useMsgServer)
    {
        IMessagingService messagingService = useMsgServer
            ? await MessagingServiceFactory.Create(configuration["Messaging:Host"]!)
            : MessagingServiceFactory.CreateEmpty();

        services.AddSingleton(messagingService);
        return services;
    }

    public static IServiceCollection AddEventDispatcher(this IServiceCollection services)
    {
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        return services;
    }

    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<GuessCreatedEvent>, GuessCreatedEventHandler>();
        return services;
    }

    public static IServiceCollection AddEmailMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IEmailAppMessager, EmailAppMessager>();
        return services;
    }

    public static IServiceCollection AddGameObservation(this IServiceCollection services)
    {
        services.AddScoped<IGameObserver, GameObserver>();
        return services;
    }

    public static IServiceCollection AddAuthorizationHandlers(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, GameOpAuthorizationHandler>();
        return services;
    }

    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .AddPolicy(PolicyReference.AccreditedOnly, policy =>
                policy.RequireRole(RoleReference.Admin, RoleReference.Staff))
            .AddPolicy(PolicyReference.AdminOnly, policy =>
                policy.RequireRole(RoleReference.Admin));

        services
            .AddIdentityApiEndpoints<AppUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.ConfigureIdentity();

        services.AddAuthorizationHandlers();

        return services;
    }

    public static async Task<IServiceCollection> AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useDbServer,
        bool useMsgServer)
    {
        services.AddAuthentication();

        await services.AddMessaging(configuration, useMsgServer);

        services
            .AddEmailMessaging()
            .AddGameObservation()
            .AddEventHandlers()
            .AddEventDispatcher()
            .AddDbContext(configuration, useDbServer)
            .AddAppAuthorization();

        return services;
    }

    private static IServiceCollection ConfigureIdentity(this IServiceCollection services)
    {
        services
            .Configure<IdentityOptions>(options =>
                {
                    // All the following do not equal their defaults.
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                })
            .ConfigureApplicationCookie(options => { options.Cookie.Name = "GuessGame"; });
        return services;
    }
}

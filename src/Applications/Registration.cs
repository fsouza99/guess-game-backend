namespace App.Applications;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplications(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<IApp>()
            .AddClasses(classes => classes.AssignableTo<IApp>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        return services;
    }
}

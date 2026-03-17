namespace App.Api;

public static class ApiServicesRegistration
{
    public static IServiceCollection AddApi(this IServiceCollection services, bool useSwagger)
    {
        services.AddControllers();
        if (useSwagger)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }
        return services;
    }
}

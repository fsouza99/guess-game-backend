using App.Api;
using App.Applications;
using App.Infrastructure;
using App.Models;

bool useDbServer = args.Contains("--dbserver");
bool useMsgServer = args.Contains("--msgserver");
bool useSwagger = args.Contains("--swagger");

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddInfrastructure(builder.Configuration, useDbServer, useMsgServer);

builder.Services
    .AddApi(useSwagger)
    .AddApplications();

var app = builder.Build();

if (useSwagger && app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.InitializeDatabaseAsync(useDbServer);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<AppUser>();
app.MapExtraIdentityEndpoints();
app.MapGetHealthCheckEndpoint();

app.Run();

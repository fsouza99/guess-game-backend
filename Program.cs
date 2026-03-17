using App.Api;
using App.Applications;
using App.Globals;
using App.Infrastructure;
using App.Models;

var builder = WebApplication.CreateBuilder(args);

// Retrieve user parameters.

bool useDbServer = args.Contains("--dbserver");
bool useMsgServer = args.Contains("--msgserver");
bool useSwagger = args.Contains("--swagger");

// Add services on database, auth operations and controllers.

builder.Services.AddDbContext(builder.Configuration, useDbServer);

builder.Services.AddAuthentication();

builder.Services.AddAppAuthorization();

builder.Services.AddControllers();

// Add related services of game observation, messaging and email.

await builder.Services.AddMessaging(builder.Configuration, useMsgServer);

builder.Services
    .AddEmailMessaging()
    .AddGameObservation();

// Add application and event-related services.

builder.Services
    .AddApplications()
    .AddEventHandlers()
    .AddEventDispatcher();

var app = builder.Build();

// Add Swagger if applicable.

if (app.Environment.IsDevelopment() && useSwagger)
{
    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Provide initial app data.

await app.InitializeDatabaseAsync(useDbServer);

// Add middleware, set mappings and run.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<AppUser>();
app.MapExtraIdentityEndpoints();

app.Run();

using App.Api;
using App.Applications;
using App.Globals;
using App.Infrastructure;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Retrieve user parameters.

bool useDbServer = args.Contains("--dbserver");
bool useMsgServer = args.Contains("--msgserver");
bool useSwagger = args.Contains("--swagger");

// Add services on auth operations, database, access and testing.

builder.Services.AddAuthentication();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(PolicyReference.AccreditedOnly, policy =>
        policy.RequireRole(RoleReference.Admin, RoleReference.Staff))
    .AddPolicy(PolicyReference.AdminOnly, policy =>
        policy.RequireRole(RoleReference.Admin));

builder.Services.AddControllers();

builder.Services.AddAppDbContext(builder.Configuration, useDbServer);

builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

if (useSwagger)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services.AddGameOpAuthorization();

// Add related services of game observation, messaging and email.

await builder.Services.AddMessagingService(builder.Configuration, useMsgServer);

builder.Services.AddEmailMessaging();

builder.Services.AddGameObservation();

// Add application and event-related services.

builder.Services.AddApplications();
builder.Services.AddEventHandlers();
builder.Services.AddEventDispatcher();

// Make user-related configurations.

builder.Services.Configure<IdentityOptions>(options =>
{
    // All the following do not equal their defaults.
    options.User.RequireUniqueEmail = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "GuessGame";
});

// Build app.

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment() && useSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Provide initial app data.

await app.InitializeDatabaseAsync(useDbServer);

// Settle middleware and mappings.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<AppUser>();
app.MapExtraIdentityEndpoints();

// Run app.

app.Run();

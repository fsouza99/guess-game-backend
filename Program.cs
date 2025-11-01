using App.Authorization.Handlers;
using App.Authorization.References;
using App.Controllers;
using App.Data;
using App.Identity.Data;
using App.Services;
using App.StaticTools.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Retrieve user parameters.

bool useMessaging = !args.Contains("--nomsg");
bool useSqlite = args.Contains("--sqlite");
bool useSwaggerUI = args.Contains("--swagger");

// Add services on auth operations, database, access and testing.

builder.Services.AddAuthentication();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(PolicyReference.AccreditedOnly, policy =>
        policy.RequireRole(RoleReference.Admin, RoleReference.Staff))
    .AddPolicy(PolicyReference.AdminOnly, policy =>
        policy.RequireRole(RoleReference.Admin));

builder.Services.AddControllers();

builder.Services.AddAppDbContext(builder.Configuration, useSqlite);

builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

if (useSwaggerUI)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services
    .AddSingleton<IAuthorizationHandler, GameOpAuthorizationHandler>();

// Add related services of game observation, messaging and email.

await builder.Services.AddMessagingService(builder.Configuration, useMessaging);

builder.Services.AddSingleton<IEmailAppMessager, EmailAppMessager>();

builder.Services.AddScoped<IGameObserver, GameObserver>();

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

if (app.Environment.IsDevelopment() && useSwaggerUI)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Provide initial app data.

await app.InitializeDatabaseAsync(useSqlite);

// Settle middleware and mappings.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<AppUser>();
app.MapExtraIdentityEndpoints();

// Run app.

app.Run();

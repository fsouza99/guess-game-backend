using App.Authorization.Handlers;
using App.Authorization.References;
using App.Controllers;
using App.Data;
using App.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();

builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(PolicyReference.AccreditedOnly, policy =>
            policy.RequireRole(RoleReference.Admin, RoleReference.Staff));
        options.AddPolicy(PolicyReference.AdminOnly, policy =>
            policy.RequireRole(RoleReference.Admin));
    });

builder.Services.AddSingleton<IAuthorizationHandler, GameOpAuthorizationHandler>();

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AppDbContext") ?? throw new InvalidOperationException("Connection string 'AppDbContext' not found.")));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddSwaggerGen();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "GuessGame";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    var dbinit = new DbInitializer(services, context);
    dbinit.Initialize();
}

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<AppUser>();

// Extra Identity endpoint, for a user to log out from his account.
app.MapPost(
    "/logout",
    async (
        SignInManager<AppUser> signInManager,
        [FromBody] object empty) =>
    {
        if (empty is null)
        {
            return Results.Unauthorized();
        }
        await signInManager.SignOutAsync();
        return Results.Ok();
    }
).WithOpenApi().RequireAuthorization();

// Extra Identity endpoint, for a user to delete his own account.
app.MapDelete(
    "/delete",
    async (
        ClaimsPrincipal claimsPrincipal,
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        [FromBody] object empty) =>
    {
        if (empty is null)
        {
            return Results.Unauthorized();
        }
        var user = await userManager.GetUserAsync(claimsPrincipal);
        if (user is null)
        {
            return Results.NotFound();
        }
        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            // Necessary to make any new logout requests fail.
            await signInManager.SignOutAsync();

            return Results.NoContent();
        }
        return Results.BadRequest();
    }
).WithOpenApi().RequireAuthorization();

app.Run();

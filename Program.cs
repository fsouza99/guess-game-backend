using App.Authorization.Handlers;
using App.Authorization.References;
using App.Controllers;
using App.Data;
using App.Identity.Data;
using App.Identity.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    // All the following do not equal their defaults.
    options.User.RequireUniqueEmail = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
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
    if (!context.Formula.Any())
    {
        var dbinit = new DbInitializer(services, context);
        dbinit.Initialize();
    }
}

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<AppUser>();

CustomEndpoints.MapExtraIdentityEndpoints(app);

app.Run();

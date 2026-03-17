using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;

namespace App.Infrastructure;

public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    // The password used by all added users.
    private const string StdPass = "Passw0rd";

    public DbInitializer(IServiceProvider serviceProvider, AppDbContext context)
    {
        _serviceProvider = serviceProvider;
        _context = context;
    }

    private void AddJsonData<T>(string filename, DbSet<T> dbSet) where T : class
    {
        string filepath = $"src\\Infrastructure\\Database\\Placeholders\\{filename}.json";
        string data = File.ReadAllText(filepath);
        List<T> objs = JsonSerializer.Deserialize<List<T>>(data)!;
        dbSet.AddRange(objs);
    }

    private async Task AddAppUsersAsync()
    {
        var manager = _serviceProvider.GetService<UserManager<AppUser>>()!;
        
        var user1 = new AppUser
        {
            Email = "ajquill@gmail.com",
            EmailConfirmed = false,
            Id = "admin-user",
            Nickname = "AJQuill",
            UserName = "ajquill@gmail.com"
        };
        await manager.CreateAsync(user1, StdPass);
        await manager.AddToRoleAsync(user1, RoleReference.Admin);

        var user2 = new AppUser
        {
            Email = "bwillow@gmail.com",
            EmailConfirmed = false,
            Id = "staff-user",
            Nickname = "BWillow",
            UserName = "bwillow@gmail.com"
        };
        await manager.CreateAsync(user2, StdPass);
        await manager.AddToRoleAsync(user2, RoleReference.Staff);

        var user3 = new AppUser
        {
            Email = "gfritz@hotmail.com",
            EmailConfirmed = false,
            Id = "regular-user-1",
            Nickname = "GFritz",
            UserName = "gfritz@hotmail.com"
        };
        await manager.CreateAsync(user3, StdPass);
    }

    private async Task AddRolesAsync()
    {
        var manager = _serviceProvider.GetService<RoleManager<IdentityRole>>()!;
        var roles = new string[] { RoleReference.Admin, RoleReference.Staff };
        foreach (var role in roles)
        {
            await manager.CreateAsync(new IdentityRole(role));
        }
    }

    public async Task AddUserDataToContext()
    {
        // Insert orderly to respect FK constraints.
        await AddRolesAsync();
        await AddAppUsersAsync();
    }

    public async Task AddBusinessDataToContext()
    {
        // Insert orderly to respect FK constraints.
        AddJsonData<Formula>("Formulas", _context.Formula);
        AddJsonData<Competition>("Competitions", _context.Competition);
        AddJsonData<Game>("Games", _context.Game);
        AddJsonData<Guess>("Guesses", _context.Guess);

        await _context.SaveChangesAsync();
    }

    public async Task AddUserAndBusinessDataToContext()
    {
        await AddUserDataToContext();
        await AddBusinessDataToContext();
    }
}

using App.Authorization.References;
using App.Identity.Data;
using App.Models;
using App.StaticTools;
using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Text.Json;

namespace App.Data;

public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    // The password used by all added users.
    private const string StdPass = "Passw0rd";

    // Reads JSON file and returns its content appropriately.
    private static JsonDocument ReadJsonFile(string file)
    {
        string filePath = $"Data\\Placeholders\\{file}";
        StreamReader reader = new StreamReader(filePath);
        string rawData = reader.ReadToEnd();
        reader.Dispose();
        return JsonDocument.Parse(rawData);
    }

    // Constructor
    public DbInitializer(IServiceProvider serviceProvider, AppDbContext context)
    {
        _serviceProvider = serviceProvider;
        _context = context;
    }

    private void AddCompetitions()
    {
        var objs = ReadJsonFile("Competitions.json").RootElement;
        foreach (var obj in objs.EnumerateArray())
        {
            var competition = new Competition
            {
                Active = obj.GetProperty("Active").GetBoolean(),
                ID = obj.GetProperty("ID").GetInt32(),
                FormulaID = obj.GetProperty("FormulaID").GetInt32(),
                Name = obj.GetProperty("Name").GetString()!,
                Description = obj.GetProperty("Description").GetString()!,
                Data = JsonSerializer.Serialize(obj.GetProperty("Data")),
                Creation = obj.GetProperty("Creation").GetDateTime(),
                Start = obj.GetProperty("Start").GetDateTime(),
                End = obj.GetProperty("End").GetDateTime()
            };
            _context.Competition.Add(competition);
        }
    }

    private void AddFormulas()
    {
        var objs = ReadJsonFile("Formulas.json").RootElement;
        foreach (var obj in objs.EnumerateArray())
        {
            var formula = new Formula
            {
                Creation = DateTime.Now,
                DataTemplate = JsonSerializer.Serialize(obj.GetProperty("DataTemplate")),
                Description = obj.GetProperty("Description").GetString()!,
                ID = obj.GetProperty("ID").GetInt32(),
                Name = obj.GetProperty("Name").GetString()!
            };
            _context.Formula.Add(formula);
        }
    }

    private void AddGames()
    {
        var objs = ReadJsonFile("Games.json").RootElement;
        foreach (var obj in objs.EnumerateArray())
        {
            var deadline = obj.GetProperty("SubsDeadline");
            var game = new Game
            {
                AppUserID = obj.GetProperty("AppUserID").GetString()!,
                CompetitionID = obj.GetProperty("CompetitionID").GetInt32(),
                Creation = obj.GetProperty("Creation").GetDateTime(),
                Description = obj.GetProperty("Description").GetString()!,
                ID = obj.GetProperty("ID").GetString()!,
                MaxScore = obj.GetProperty("MaxScore").GetInt32(),
                MaxGuessCount = obj.GetProperty("MaxGuessCount").GetInt32(),
                Name = obj.GetProperty("Name").GetString()!,
                NextGuessNumber = obj.GetProperty("NextGuessNumber").GetInt32(),
                Passcode = obj.GetProperty("Passcode").GetString(),
                ScoringRules = JsonSerializer.Serialize(obj.GetProperty("ScoringRules")),
                SubsDeadline = (deadline.ValueKind == JsonValueKind.Null) ? null : deadline.GetDateTime()
            };
            _context.Game.Add(game);
        }
    }

    private void AddGuesses()
    {
        var objs = ReadJsonFile("Guesses.json").RootElement;
        foreach (var obj in objs.EnumerateArray())
        {
            var guess = new Guess
            {
                Creation = DateTime.Now,
                Data = JsonSerializer.Serialize(obj.GetProperty("Data")),
                GameID = obj.GetProperty("GameID").GetString()!,
                Name = obj.GetProperty("Name").GetString()!,
                Number = obj.GetProperty("Number").GetInt32(),
                Score = obj.GetProperty("Score").GetInt32()
            };
            _context.Guess.Add(guess);
        }
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
        // Respect FK constraints when inserting.
        await AddRolesAsync();
        await AddAppUsersAsync();
    }

    public async Task AddBusinessDataToContext()
    {
        /* "Formula" and "Competition" have "auto IDs" on SQL Server. With that,
            no manual ID setting is allowed. Although SQL "SET" commands could solve
            this, we judge it more adequate to avoid insertion on these models
            directly from code. Instead, an associated procedure must be made
            available for execution on the database.

            Call this method only when inserting into SQLite.
        */
        // FK constraints must be respected for insertion.
        AddFormulas();
        AddCompetitions();
        AddGames();
        AddGuesses();

        await _context.SaveChangesAsync();
    }

    public async Task AddUserAndBusinessDataToContext()
    {
        await AddUserDataToContext();
        await AddBusinessDataToContext();
    }
}

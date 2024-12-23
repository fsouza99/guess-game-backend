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
    private const string StdPass = "W8$14#4u$wnuCX0suO7#a@82i2754k$2";

    // The regular user to be added.
    private const string RegularUserId = "regular-user-1";

    // Reads JSON file and returns its content appropriately.
    private JsonDocument ReadJsonFile(string file)
    {
        string filePath = $"Data\\Placeholders\\{file}";
        StreamReader reader = new StreamReader(filePath);
        string rawData = reader.ReadToEnd();
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
                Creation = DateTime.Now,
                Data = JsonSerializer.Serialize(obj.GetProperty("Data")),
                Description = obj.GetProperty("Description").GetString()!,
                FormulaID = obj.GetProperty("FormulaID").GetInt32(),
                ID = obj.GetProperty("ID").GetInt32(),
                Name = obj.GetProperty("Name").GetString()!
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
                Name = obj.GetProperty("Name").GetString()!,
                ScoringRulesTemplate = JsonSerializer.Serialize(obj.GetProperty("ScoringRulesTemplate"))
            };
            _context.Formula.Add(formula);
        }
    }

    private void AddGames()
    {
        var objs = ReadJsonFile("Games.json").RootElement;
        foreach (var obj in objs.EnumerateArray())
        {
            var game = new Game
            {
                AppUserID = RegularUserId,
                CompetitionID = obj.GetProperty("CompetitionID").GetInt32(),
                Creation = obj.GetProperty("Creation").GetDateTime(),
                Description = obj.GetProperty("Description").GetString()!,
                ID = obj.GetProperty("ID").GetInt32(),
                MaxGuessCount = obj.GetProperty("MaxGuessCount").GetInt32(),
                Name = obj.GetProperty("Name").GetString()!,
                Passcode = obj.GetProperty("Passcode").GetString(),
                ScoringRules = JsonSerializer.Serialize(obj.GetProperty("ScoringRules")),
                SubsDeadline = obj.GetProperty("SubsDeadline").GetDateTime()
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
                GameID = obj.GetProperty("GameID").GetInt32(),
                Name = obj.GetProperty("Name").GetString()!,
                Number = obj.GetProperty("Number").GetInt32(),
                Score = obj.GetProperty("Score").GetInt32()
            };
            _context.Guess.Add(guess);
        }
    }

    private async void AddAppUsers()
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
            Id = RegularUserId,
            Nickname = "GFritz",
            UserName = "gfritz@hotmail.com"
        };
        await manager.CreateAsync(user3, StdPass);
    }

    private async void AddRoles()
    {
        var manager = _serviceProvider.GetService<RoleManager<IdentityRole>>()!;
        var roles = new string[] { RoleReference.Admin, RoleReference.Staff };
        foreach (var role in roles)
        {
            await manager.CreateAsync(new IdentityRole(role));
        }
    }

    public void Initialize()
    {
        // Data must be added in some order that respects FK constraints.
        AddRoles();
        AddAppUsers();
        AddFormulas();
        AddCompetitions();
        AddGames();
        AddGuesses();

        _context.SaveChanges();
        return;
    }
}

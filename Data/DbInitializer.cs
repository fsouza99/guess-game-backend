using App.Authorization.References;
using App.Identity.Data;
using App.Models;
using App.StaticTools;
using Microsoft.AspNetCore.Identity;
using System.IO;

namespace App.Data;

public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    // The password used by all added users.
    private const string StdPass = "W8$14#4u$wnuCX0suO7#a@82i2754k$2";

    // The ID key of the regular user to be added.
    private const string RegularUserId = "regular-user-1";

    private string ReadJsonFileAsString(string file)
    {
        string filePath = $"Data\\Placeholders\\{file}";
        StreamReader reader = new StreamReader(filePath);
        return reader.ReadToEnd();
    }

    // Constructor
    public DbInitializer(IServiceProvider serviceProvider, AppDbContext context)
    {
        _serviceProvider = serviceProvider;
        _context = context;
    }

    private void AddCompetitions()
    {
        Console.Write("Adding \"Competition\" entity 1... ");

        string template = ReadJsonFileAsString("DataTemplate.json");
        string data = ReadJsonFileAsString("CompData.json");

        // if (!JsonDataChecker.DataOnTemplate(template, data))
        // {
        //     throw new Exception("Data does not match template.");
        // }

        var competition = new Competition
        {
            Active = true,
            Creation = DateTime.Now,
            Description = string.Concat(Enumerable.Repeat("description ", 85)),
            FormulaID = 1,
            ID = 1,
            Name = "Copa do Mundo da FIFA Catar 2022",
            Data = data
        };
        _context.Competition.Add(competition);
        Console.WriteLine("Done.");

        Console.Write("Saving changes... ");
        _context.SaveChanges();
        Console.WriteLine("Done.");
    }

    private void AddFormulas()
    {
        Console.Write("Adding \"Formula\" entity 1... ");

        string dataTemp = ReadJsonFileAsString("DataTemplate.json");
        string srulesTemp = ReadJsonFileAsString("ScoringRulesTemplate.json");

        // if (!JsonDataChecker.DataTemplate(dataTemp))
        // {
        //     throw new Exception("Invalid data template.");
        // }
        
        // if (!JsonDataChecker.ScoringRulesTemplate(srulesTemp))
        // {
        //     throw new Exception("Invalid scoring rules template.");
        // }

        var formula = new Formula
        {
            Creation = DateTime.Now,
            Description = string.Concat(Enumerable.Repeat("description ", 85)),
            ID = 1,
            Name = "1998-to-2022-world-cup",
            DataTemplate = dataTemp,
            ScoringRulesTemplate = srulesTemp
        };
        _context.Formula.Add(formula);
        Console.WriteLine("Done.");

        Console.Write("Saving changes... ");
        _context.SaveChanges();
        Console.WriteLine("Done.");
    }

    private void AddGames()
    {
        string template = ReadJsonFileAsString("ScoringRulesTemplate.json");
        string srules = ReadJsonFileAsString("GameScoringRules.json");

        // if (!JsonDataChecker.ScoringRulesTemplate(template))
        // {
        //     throw new Exception("Scoring rules template is invalid.");
        // }
        
        // if (!JsonDataChecker.ScoringRulesOnTemplate(template, srules))
        // {
        //     throw new Exception("Scoring rules do not match template.");
        // }

        Console.Write("Adding \"Game\" entity 1... ");

        var free_game = new Game
        {
            CompetitionID = 1,
            Creation = DateTime.Now,
            Description = string.Concat(Enumerable.Repeat("description ", 85)),
            ID = 1,
            MaxGuessCount = 100,
            Name = "Free Game - No restrictions",
            ScoringRules = srules,
            SubsDeadline = DateTime.Now.AddYears(1),
            AppUserID = RegularUserId
        };
        _context.Game.Add(free_game);
        Console.WriteLine("Done.");

        Console.Write("Adding \"Game\" entity 2... ");

        var old_game = new Game
        {
            CompetitionID = 1,
            Creation = DateTime.Now.AddYears(-2),
            Description = string.Concat(Enumerable.Repeat("description ", 85)),
            ID = 2,
            MaxGuessCount = 100,
            Name = "Old Game - Deadline Passed",
            ScoringRules = srules,
            SubsDeadline = DateTime.Now.AddYears(-1),
            AppUserID = RegularUserId
        };
        _context.Game.Add(old_game);
        Console.WriteLine("Done.");

        Console.Write("Adding \"Game\" entity 3... ");

        var single_player_game = new Game
        {
            CompetitionID = 1,
            Creation = DateTime.Now,
            Description = string.Concat(Enumerable.Repeat("description ", 85)),
            ID = 3,
            MaxGuessCount = 1,
            Name = "Single Player Game - 1 Guess",
            ScoringRules = srules,
            SubsDeadline = DateTime.Now.AddYears(1),
            AppUserID = RegularUserId
        };
        _context.Game.Add(single_player_game);
        Console.WriteLine("Done.");

        Console.Write("Adding \"Game\" entity 4... ");

        var private_game = new Game
        {
            CompetitionID = 1,
            Creation = DateTime.Now,
            Description = string.Concat(Enumerable.Repeat("description ", 85)),
            ID = 4,
            MaxGuessCount = 100,
            Name = "Private Game - Passcode Needed",
            Passcode = "12345",
            ScoringRules = srules,
            SubsDeadline = DateTime.Now.AddYears(1),
            AppUserID = RegularUserId
        };
        _context.Game.Add(private_game);
        Console.WriteLine("Done.");

        Console.Write("Saving changes... ");
        _context.SaveChanges();
        Console.WriteLine("Done.");
    }

    private void AddGuesses()
    {
        string template = ReadJsonFileAsString("DataTemplate.json");
        
        for (int i = 1; i <= 5; i++)
        {
            Console.Write($"Adding \"Guess\" entity {i}... ");
            string data = ReadJsonFileAsString($"Guess{i}.json");
            if (!JsonDataChecker.DataOnTemplate(template, data))
            {
                throw new Exception("Data does not match template.");
            }
            var guess = new Guess
            {
                AuthorName = $"Author Name {i}",
                Creation = DateTime.Now.AddHours(-i),
                Data = data,
                GameID = 1,
                Number = i,
                Score = 0
            };
            _context.Guess.Add(guess);
            Console.WriteLine("Done.");
        }

        Console.Write("Saving changes... ");
        _context.SaveChanges();
        Console.WriteLine("Done.");
    }

    private async void AddAppUsers()
    {
        var manager = _serviceProvider.GetService<UserManager<AppUser>>()!;
        
        Console.Write("Adding \"AppUser\" entity 1 (Admin)... ");
        var user1 = new AppUser
        {
            Email="ajquill@gmail.com",
            EmailConfirmed=true,
            UserName="ajquill@gmail.com",
            Id = "admin-user"
        };
        await manager.CreateAsync(user1, StdPass);
        await manager.AddToRoleAsync(user1, RoleReference.Admin);
        Console.WriteLine("Done.");

        Console.Write("Adding \"AppUser\" entity 2 (Staff)... ");
        var user2 = new AppUser
        {
            Email="bwillow@gmail.com",
            EmailConfirmed=true,
            UserName="bwillow@gmail.com",
            Id = "staff-user"
        };
        await manager.CreateAsync(user2, StdPass);
        await manager.AddToRoleAsync(user2, RoleReference.Staff);
        Console.WriteLine("Done.");

        Console.Write("Adding \"AppUser\" entity 3 (Regular)... ");
        var user3 = new AppUser
        {
            Email="gfritz@hotmail.com",
            EmailConfirmed=true,
            UserName="gfritz@hotmail.com",
            Id = RegularUserId
        };
        await manager.CreateAsync(user3, StdPass);
        Console.WriteLine("Done.");
    }

    private async void AddRoles()
    {
        var manager = _serviceProvider.GetService<RoleManager<IdentityRole>>()!;
        var roles = new string[] {RoleReference.Admin, RoleReference.Staff};
        foreach (var role in roles)
        {
            await manager.CreateAsync(new IdentityRole(role));
        }
    }

    // Methods
    public void Initialize()
    {
        // Data must be added in some order that respects FK constraints.

        if (_context.Competition.Any())
        {
            return;
        }
        AddRoles();
        AddAppUsers();
        AddFormulas();
        AddCompetitions();
        AddGames();
        AddGuesses();

        return;
    }
}

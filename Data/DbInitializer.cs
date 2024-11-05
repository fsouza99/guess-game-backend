using App.Authorization.References;
using App.Identity.Data;
using App.Models;
using Microsoft.AspNetCore.Identity;
using System.IO;

namespace App.Data;

public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    // The password used by all added users.
    private const string StdPass = "W8$14#4u$wnuCX0suO7#a@82i2754k$2";

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
        var competition = new Competition
        {
            Active = true,
            Creation = DateTime.Now,
            Description = string.Concat(Enumerable.Repeat("description ", 85)),
            FormulaID = 1,
            ID = 1,
            Name = "Copa do Mundo da FIFA Catar 2022",
            Data = ReadJsonFileAsString("CompData.json")
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
        var formula = new Formula
        {
            Creation = DateTime.Now,
            Description = string.Concat(Enumerable.Repeat("description ", 85)),
            ID = 1,
            Name = "1998-to-2022-world-cup",
            DataTemplate = ReadJsonFileAsString("DataTemplate.json"),
            ScoringRulesTemplate = ReadJsonFileAsString("ScoringRulesTemplate.json")
        };
        _context.Formula.Add(formula);
        Console.WriteLine("Done.");

        Console.Write("Saving changes... ");
        _context.SaveChanges();
        Console.WriteLine("Done.");
    }

    private void AddGames()
    {
        Console.Write("Adding \"Game\" entity 1... ");
        var game = new Game
        {
            CompetitionID = 1,
            Creation = DateTime.Now,
            Description = string.Concat(Enumerable.Repeat("description ", 85)),
            ID = 1,
            MaxGuessCount = 3,
            Name = "Lorem Ipsum Dolor",
            ScoringRules = ReadJsonFileAsString("GameScoringRules.json"),
            SubsDeadline = DateTime.Now.AddDays(7),
            AppUserID = "regular-user-1"
        };
        _context.Game.Add(game);
        Console.WriteLine("Done.");

        Console.Write("Saving changes... ");
        _context.SaveChanges();
        Console.WriteLine("Done.");
    }

    private void AddGuesses()
    {
        for (int i = 1; i <= 5; i++)
        {
            Console.Write($"Adding \"Guess\" entity {i}... ");
            var guess = new Guess
            {
                AuthorName = $"Author Name {i}",
                Creation = DateTime.Now.AddHours(-i),
                Data = ReadJsonFileAsString($"Guess{i}.json"),
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
            Id = "regular-user-1"
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

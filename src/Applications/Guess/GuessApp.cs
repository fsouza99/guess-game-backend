using App.Globals;
using App.Infrastructure;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace App.Applications;

public class GuessApp(AppDbContext context, IAuthorizationService authService) : IGuessApp
{
    public async Task<Result<int>> CountAsync(string? gameId, string? name)
    {
        var query = QueryRefiner.Guesses(context.Guess, gameId, name);
        return await query.CountAsync();
    }

    public async Task<Result<List<GuessView>>> ReadManyAsync(
        string? gameId, string? name, int? offset, int? limit)
    {
        var query = QueryRefiner.Guesses(context.Guess, gameId, name, offset, limit);
        var result = await query
            .Select(g => new GuessView(g))
            .ToListAsync();
        return result;
    }

    public async Task<Result<GuessView>> ReadOneAsync(string gameId, int number)
    {
        var guess = await context.Guess.FindAsync(gameId, number);
        if (guess is null)
        {
            return Result.Failure<GuessView>(GuessErrors.NotFound());
        }

        return new GuessView(guess);
    }

    public async Task<Result<GuessView>> CreateAsync(GuessDto dto)
    {
        // Check game.
        var game = await context.Game
            .Include(g => g.Competition)
            .ThenInclude(c => c.Formula)
            .FirstOrDefaultAsync(g => g.ID == dto.GameID);
        if (game is null)
        {
            return Result.Failure<GuessView>(GuessErrors.GameNotFound());
        }

        // Check passcode.
        if (game.Passcode is null ? false : dto.GamePasscode != game.Passcode)
        {
            return Result.Failure<GuessView>(GuessErrors.WrongPasscode());
        }

        // Check deadline.
        var dateTimeNow = DateTime.Now;
        if (game.SubsDeadline is not null && dateTimeNow > game.SubsDeadline)
        {
            return Result.Failure<GuessView>(GuessErrors.DeadlinePassed());
        }

        // Check guess count.
        int gameGuessCount = await context.Guess
            .Where(g => g.GameID == game.ID)
            .CountAsync();
        if (gameGuessCount >= game.MaxGuessCount)
        {
            return Result.Failure<GuessView>(GuessErrors.TooManyGuesses());
        }

        // Check conformance of "Data" with template.
        var dataTemp = JsonDocument.Parse(game.Competition.Formula.DataTemplate);
        if (!JsonDataChecker.DataOnTemplate(dto.Data, dataTemp))
        {
            return Result.Failure<GuessView>(GuessErrors.UnfitData());
        }

        var guess = new Guess
        {
            Creation = dateTimeNow,
            Data = JsonSerializer.Serialize(dto.Data),
            GameID = dto.GameID,
            Name = dto.Name,
            Number = game.NextGuessNumber++,
            Score = GuessScorer.Evaluate(
                dto.Data,
                JsonDocument.Parse(game.Competition.Data),
                JsonDocument.Parse(game.ScoringRules))
        };
        context.Guess.Add(guess);

        guess.Raise(new GuessCreatedEvent(game, guess));

        await context.SaveChangesAsync();

        return new GuessView(guess);
    }

    public async Task<Result> RemoveAsync(string gameId, int number, ClaimsPrincipal user)
    {
        var guess = await context.Guess
            .Where(g => g.GameID == gameId && g.Number == number)
            .FirstOrDefaultAsync();
        if (guess is null)
        {
            return Result.Failure(GuessErrors.NotFound());
        }

        if (!(await UserCanDelete(user, gameId)))
        {
            return Result.Failure(GuessErrors.CannotDelete());
        }

        context.Guess.Remove(guess);
        await context.SaveChangesAsync();

        return Result.Success();
    }

    private async Task<bool> UserCanDelete(ClaimsPrincipal user, string gameId)
    {
        var game = await context.Game.FindAsync(gameId);
        var authCheck = await authService.AuthorizeAsync(user, game, Operations.Delete);
        return authCheck.Succeeded;
    }
}


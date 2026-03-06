using App.Authorization;
using App.Data;
using App.Models;
using App.Services;
using App.StaticTools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace App.Applications;

public class GuessApp
{
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authService;
    private readonly IGameObserver _gameObserver;

    public GuessApp(
        AppDbContext context,
        IAuthorizationService authService,
        IGameObserver gameObserver)
    {
        _authService = authService;
        _context = context;
        _gameObserver = gameObserver;
    }

    public async Task<Result<int>> CountAsync(string? gameId, string? name)
    {
        var query = QueryRefiner.Guesses(_context.Guess, gameId, name);
        return await query.CountAsync();
    }

    public async Task<Result<List<GuessView>>> ReadManyAsync(
        string? gameId, string? name, int? offset, int? limit)
    {
        var query = QueryRefiner.Guesses(_context.Guess, gameId, name, offset, limit);
        var result = await query
            .Select(f => ViewFactory.Guess(f))
            .ToListAsync();
        return result;
    }

    public async Task<Result<GuessView>> ReadOneAsync(string gameId, int number)
    {
        var guess = await _context.Guess.FindAsync(gameId, number);
        if (guess is null)
        {
            return Result.Failure<GuessView>(GuessErrors.NotFound());
        }

        return ViewFactory.Guess(guess);
    }

    public async Task<Result<GuessView>> CreateAsync(GuessDto dto)
    {
        // Check game.
        var game = await _context.Game
            .Include(g => g.Competition)
            .ThenInclude(c => c.Formula)
            .FirstAsync(g => g.ID == dto.GameID);
        if (game is null)
        {
            return Result.Failure<GuessView>(GuessErrors.GameNotFound());
        }

        // Check passcode.
        if (dto.GamePasscode != game.Passcode)
        {
            return Result.Failure<GuessView>(GuessErrors.WrongPassword());
        }

        // Check deadline.
        var dateTimeNow = DateTime.Now;
        if (game.SubsDeadline is not null && dateTimeNow > game.SubsDeadline)
        {
            return Result.Failure<GuessView>(GuessErrors.DeadlinePassed());
        }

        // Check guess count.
        int gameGuessCount = await _context.Guess
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
        _context.Guess.Add(guess);

        // Save changes and notify game's owner of relevant events.
        await _context.SaveChangesAsync();
        await _gameObserver.WatchAsync(game);

        return ViewFactory.Guess(guess);
    }

    public async Task<Result> RemoveAsync(string gameId, int number, ClaimsPrincipal user)
    {
        var guess = await _context.Guess
            .Where(g => g.GameID == gameId && g.Number == number)
            .FirstOrDefaultAsync();
        if (guess is null)
        {
            return Result.Failure(GuessErrors.NotFound());
        }

        if (!(await UserCanDelete(user, gameId)))
        {
            return Result.Failure(GuessErrors.Forbidden());
        }

        _context.Guess.Remove(guess);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    private async Task<bool> UserCanDelete(ClaimsPrincipal user, string gameId)
    {
        var game = await _context.Game.FindAsync(gameId);
        var authCheck = await _authService.AuthorizeAsync(user, game, Operations.Delete);
        return authCheck.Succeeded;
    }
}


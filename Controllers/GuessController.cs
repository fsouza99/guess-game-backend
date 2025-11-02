using App.Authorization.Requirements;
using App.Controllers.ResponseMessages;
using App.Data;
using App.Models;
using App.Services;
using App.StaticTools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GuessController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authService;
    private readonly IGameObserver _gameObserver;

    public GuessController(
        AppDbContext context,
        IAuthorizationService authService,
        IGameObserver gameObserver)
    {
        _authService = authService;
        _context = context;
        _gameObserver = gameObserver;
    }

    // GET: api/Guess/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(
        string? gameId, string? name)
    {
        var query = QueryRefiner.Guesses(_context.Guess, gameId, name);
        var count = await query.CountAsync();
        return count;
    }

    // GET: api/Guess
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuessView>>> GetGuesses(
        string? gameId, string? name, int? offset, int? limit)
    {
        var query = QueryRefiner.Guesses(
            _context.Guess, gameId, name, offset, limit);
        var result = await query
            .Select(g => CreateGuessView(g))
            .ToListAsync();
        return result;
    }

    // GET: api/Guess/5/5
    [HttpGet("{gameId}/{number}")]
    public async Task<ActionResult<GuessView>> GetGuess(
        string gameId, int number)
    {
        var guess = await _context.Guess.FindAsync(gameId, number);
        if (guess is null)
        {
            return NotFound();
        }

        return CreateGuessView(guess);
    }

    // POST: api/Guess
    [HttpPost]
    public async Task<ActionResult<GuessView>> PostGuess(GuessDto guessDto)
    {
        // Check game.
        var game = await _context.Game
            .Include(g => g.Competition)
            .ThenInclude(c => c.Formula)
            .FirstAsync(g => g.ID == guessDto.GameID);
        if (game is null)
        {
            return NotFound();
        }

        // Check passcode.
        if (!string.IsNullOrEmpty(game.Passcode) &&
            guessDto.GamePasscode != game.Passcode)
        {
            return Unauthorized(MessageRepo.PasscodeError);
        }

        // Check deadline.
        var dateTimeNow = DateTime.Now;
        if (game.SubsDeadline is not null &&
            dateTimeNow > game.SubsDeadline)
        {
            return Conflict(MessageRepo.SubsDeadlineReached);
        }

        // Check guess count.
        int gameGuessCount = await _context.Guess
            .Where(g => g.GameID == game.ID)
            .CountAsync();
        if (gameGuessCount >= game.MaxGuessCount)
        {
            return Conflict(MessageRepo.MaxGuessCountReached);
        }

        // Check conformance of "Data" with template.
        var dataTemp = JsonDocument.Parse(game.Competition.Formula.DataTemplate);
        if (!JsonDataChecker.DataOnTemplate(dataTemp, guessDto.Data))
        {
            return BadRequest(MessageRepo.UnfitData);
        }

        // Create.
        Guess guess = BuildGuess(guessDto, game, dateTimeNow);
        _context.Guess.Add(guess);

        // Update game.
        game.NextGuessNumber++;

        // Save changes and notify game's owner of relevant events.
        await _context.SaveChangesAsync();
        await _gameObserver.WatchAsync(game);

        return CreatedAtAction(
            nameof(GetGuess),
            new { gameID = guess.GameID, number = guess.Number },
            CreateGuessView(guess));
    }

    // DELETE: api/Guess/5/5
    [HttpDelete("{gameId}/{number}"), Authorize]
    public async Task<ActionResult> DeleteGuess(string gameId, int number)
    {
        // Guess check.
        var guess = await _context.Guess
            .Where(g => g.GameID == gameId && g.Number == number)
            .FirstOrDefaultAsync();
        if (guess is null)
        {
            return NotFound();
        }

        // Only game owner and staff members can delete.
        // Conveniently reuse the "AuthorizationHandler" for Game model class.
        var game = await _context.Game.FindAsync(gameId);
        var authorization = await _authService
            .AuthorizeAsync(User, game, Operations.Delete);
        if (!authorization.Succeeded)
        {
            return Forbid();
        }

        _context.Guess.Remove(guess);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private Guess BuildGuess(
        GuessDto guessDto, Game game, DateTime creationDt) => new Guess
    {
        Creation = creationDt,
        Data = JsonSerializer.Serialize(guessDto.Data.RootElement),
        GameID = guessDto.GameID,
        Name = guessDto.Name,
        Number = game.NextGuessNumber,
        Score = GuessScorer.Evaluate(
            guessDto.Data,
            JsonDocument.Parse(game.Competition.Data),
            JsonDocument.Parse(game.ScoringRules))
    };

    private static GuessView CreateGuessView(Guess guess) => new GuessView(
        guess.Creation,
        JsonDocument.Parse(guess.Data),
        guess.GameID,
        guess.Name,
        guess.Number,
        guess.Score);
}


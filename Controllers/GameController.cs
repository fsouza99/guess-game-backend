using App.Authorization.Requirements;
using App.Controllers.ResponseMessages;
using App.Data;
using App.Identity.Data;
using App.Models;
using App.StaticTools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authService;

    public GameController(
        AppDbContext context, IAuthorizationService authorizationService)
    {
        _authService = authorizationService;
        _context = context;
    }

    // GET: api/Game/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(
        int? competitionId,
        string? appUserId,
        string? name,
        bool publicOnly = false)
    {
        var query = QueryRefiner.Games(
            _context.Game, competitionId, appUserId, name, publicOnly);
        var count = await query.CountAsync();
        return count;
    }

    // GET: api/Game/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GameView>> GetGame(string id)
    {
        var gameView = await _context.Game
            .Where(g => g.ID == id)
            .Select(g => CreateGameView(g, g.AppUser.Nickname!))
            .FirstAsync();
        if (gameView is null)
        {
            return NotFound();
        }

        return gameView;
    }

    // GET: api/Game
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameView>>> GetGames(
        int? competitionId,
        string? appUserId,
        string? name,
        bool publicOnly = false,
        int? offset = null,
        int? limit = null)
    {
        var query = QueryRefiner.Games(
            _context.Game,
            competitionId,
            appUserId,
            name,
            publicOnly,
            offset,
            limit);
        var result = await query
            .Select(g => CreateGameView(g, g.AppUser.Nickname!))
            .ToListAsync();
        return result;
    }

    // GET: api/Game/Personal
    [HttpGet("Personal"), Authorize]
    public async Task<ActionResult<IEnumerable<GameView>>> GetPersonalGames(
        int? competitionId,
        string? name,
        bool publicOnly = false,
        int? offset = null,
        int? limit = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = QueryRefiner.Games(
            _context.Game,
            competitionId,
            userId,
            name,
            publicOnly,
            offset,
            limit);
        var result = await query
            .Select(g => CreateGameView(g, userId))
            .ToListAsync();
        return result;
    }

    // PUT: api/Game/5
    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> PutGame(string id, GameDto gameDto)
    {
        // Check game.
        var game = await _context.Game.FindAsync(id);
        if (game is null)
        {
            return NotFound();
        }

        // Only the owner can edit.
        var authorization = await _authService
            .AuthorizeAsync(User, game, Operations.Update);
        if (!authorization.Succeeded)
        {
            return Forbid();
        }

        // If new, submission deadline must be either "null" or at least 5 min in future.
        var dateTimeNow = DateTime.Now;
        if (gameDto.SubsDeadline != game.SubsDeadline &&
            gameDto.SubsDeadline is not null &&
            gameDto.SubsDeadline < dateTimeNow.AddMinutes(5))
        {
            return BadRequest(MessageRepo.TooEarlySubsDeadline);
        }

        // Available updates.
        game.Description = gameDto.Description;
        game.MaxGuessCount = gameDto.MaxGuessCount;
        game.Name = gameDto.Name;
        game.Passcode = gameDto.Passcode;
        game.SubsDeadline = gameDto.SubsDeadline;

        _context.Entry(game).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GameExists(id))
            {
                return NotFound();
            }
            return Conflict(MessageRepo.UpdateConflict);
        }

        return NoContent();
    }

    // POST: api/Game
    [HttpPost, Authorize]
    public async Task<ActionResult<GameView>> PostGame(GameDto gameDto)
    {
        // Check competition.
        var competition = await _context.Competition
            .Include(c => c.Formula)
            .FirstAsync(c => c.ID == gameDto.CompetitionID);
        if (competition is null)
        {
            return NotFound();
        }
        if (!competition.Active)
        {
            return Conflict(MessageRepo.InactiveResource);
        }

        // Check whether submission deadline is at least 5 min in future.
        var dateTimeNow = DateTime.Now;
        if (gameDto.SubsDeadline is not null &&
            gameDto.SubsDeadline < dateTimeNow.AddMinutes(5))
        {
            return BadRequest(MessageRepo.TooEarlySubsDeadline);
        }

        // Check conformance of "ScoringRules" with template.
        var sRulesTemp = JsonDocument.Parse(
            competition.Formula.ScoringRulesTemplate);
        if (!JsonDataChecker.ScoringRulesOnTemplate(
            sRulesTemp, gameDto.ScoringRules))
        {
            return BadRequest(MessageRepo.UnfitData);
        }

        Game game = BuildGame(gameDto, competition, dateTimeNow);
        _context.Game.Add(game);
        await _context.SaveChangesAsync();

        string creatorNick = await _context.AppUser
            .Where(u => u.Id == game.AppUserID)
            .Select(u => u.Nickname)
            .FirstAsync()!;

        return CreatedAtAction(
            nameof(GetGame),
            new { id = game.ID },
            CreateGameView(game, creatorNick)
        );
    }

    // DELETE: api/Game/5
    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> DeleteGame(string id)
    {
        var game = await _context.Game.FindAsync(id);
        if (game is null)
        {
            return NotFound();
        }

        // Only the owner and site team can delete.
        var authorization = await _authService
            .AuthorizeAsync(User, game, Operations.Update);
        if (!authorization.Succeeded)
        {
            return Forbid();
        }

        _context.Game.Remove(game);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private Game BuildGame(
        GameDto gameDto, Competition competition, DateTime creationDt)
    {
        var compData = JsonDocument.Parse(competition.Data);
        return new Game
        {
            AppUserID = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            CompetitionID = gameDto.CompetitionID,
            Creation = creationDt,
            Description = gameDto.Description,
            ID = DataGen.GenerateID(),
            MaxScore = GuessScorer.Evaluate(
                compData, compData, gameDto.ScoringRules),
            MaxGuessCount = gameDto.MaxGuessCount,
            Name = gameDto.Name,
            Passcode = gameDto.Passcode,
            ScoringRules = JsonSerializer.Serialize(
                gameDto.ScoringRules.RootElement),
            SubsDeadline = gameDto.SubsDeadline
        };
    }

    private static GameView CreateGameView(
        Game game, string creatorNick) => new GameView(
        game.CompetitionID,
        game.Creation,
        game.AppUserID,
        creatorNick,
        game.Description,
        game.ID,
        game.MaxGuessCount,
        game.MaxScore,
        game.Name,
        game.Passcode,
        JsonDocument.Parse(game.ScoringRules),
        game.SubsDeadline);

    private bool GameExists(string id)
    {
        return _context.Game.Any(e => e.ID == id);
    }
}


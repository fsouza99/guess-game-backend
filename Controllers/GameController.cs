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

    // GET: api/Game/Meta/Personal
    [HttpGet("Meta/Personal"), Authorize]
    public async Task<ActionResult<int>> GetPersonalMetadata(
        int? competitionId,
        string? name,
        bool publicOnly = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = QueryRefiner.Games(
            _context.Game, competitionId, userId, name, publicOnly);
        var count = await query.CountAsync();
        return count;
    }

    // GET: api/Game/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GameView>> GetGame(string id)
    {
        var game = await _context.Game
            .Include(g => g.AppUser)
            .Include(g => g.Competition)
            .FirstOrDefaultAsync(g => g.ID == id);
        if (game is null)
        {
            return NotFound();
        }

        return ViewFactory.Game(game);
    }

    // GET: api/Game
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SimpleGameView>>> GetGames(
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
            .Include(g => g.Competition)
            .Include(g => g.AppUser)
            .Select(g => ViewFactory.SimpleGame(g))
            .ToListAsync();
        return result;
    }

    // GET: api/Game/Personal
    [HttpGet("Personal"), Authorize]
    public async Task<ActionResult<IEnumerable<SimpleGameView>>> GetPersonalGames(
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
            .Include(g => g.Competition)
            .Include(g => g.AppUser)
            .Select(g => ViewFactory.SimpleGame(g))
            .ToListAsync();
        return result;
    }

    // PUT: api/Game/5
    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> PutGame(string id, GameDto dto)
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
        if (dto.SubsDeadline != game.SubsDeadline
            && dto.SubsDeadline is not null
            && dto.SubsDeadline < dateTimeNow.AddMinutes(5))
        {
            return BadRequest(MessageRepo.TooEarlySubsDeadline);
        }

        // Available updates.
        game.Description = dto.Description;
        game.MaxGuessCount = dto.MaxGuessCount;
        game.Name = dto.Name;
        game.Passcode = dto.Passcode;
        game.SubsDeadline = dto.SubsDeadline;

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
    public async Task<ActionResult<GameView>> PostGame(GameDto dto)
    {
        // Check competition.
        var competition = await _context.Competition
            .Include(c => c.Formula)
            .FirstAsync(c => c.ID == dto.CompetitionID);
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
        if (dto.SubsDeadline is not null &&
            dto.SubsDeadline < dateTimeNow.AddMinutes(5))
        {
            return BadRequest(MessageRepo.TooEarlySubsDeadline);
        }

        // Check conformance of "ScoringRules" with template.
        var sRulesTemp = JsonDocument.Parse(
            competition.Formula.ScoringRulesTemplate);
        if (!JsonDataChecker.ScoringRulesOnTemplate(
            sRulesTemp, dto.ScoringRules))
        {
            return BadRequest(MessageRepo.UnfitData);
        }

        var compData = JsonDocument.Parse(competition.Data);
        var game = new Game
        {
            AppUserID = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            CompetitionID = dto.CompetitionID,
            Creation = dateTimeNow,
            Description = dto.Description,
            ID = DataGen.GenerateID(),
            MaxScore = GuessScorer.Evaluate(compData, compData, dto.ScoringRules),
            MaxGuessCount = dto.MaxGuessCount,
            Name = dto.Name,
            Passcode = dto.Passcode,
            ScoringRules = JsonSerializer.Serialize(dto.ScoringRules),
            SubsDeadline = dto.SubsDeadline
        };
        _context.Game.Add(game);
        await _context.SaveChangesAsync();

        await _context.AppUser.FindAsync(game.AppUserID);

        return CreatedAtAction(
            nameof(GetGame),
            new { id = game.ID },
            ViewFactory.Game(game)
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

    private bool GameExists(string id)
    {
        return _context.Game.Any(e => e.ID == id);
    }
}


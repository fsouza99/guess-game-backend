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

namespace App.Controllers
{
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

        private Game CreateGame(GameDto gameDto, Competition competition, DateTime creationDt)
        {
            var compData = JsonDocument.Parse(competition.Data);
            int maxScore = GuessScorer.Evaluate(compData, compData, gameDto.ScoringRules);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            string rawSRules = JsonSerializer.Serialize(gameDto.ScoringRules.RootElement);
            return new Game
            {
                AppUserID = userId,
                CompetitionID = gameDto.CompetitionID,
                Creation = creationDt,
                Description = gameDto.Description,
                ID = DataGen.GenerateID(),
                MaxScore = maxScore,
                MaxGuessCount = gameDto.MaxGuessCount,
                Name = gameDto.Name,
                Passcode = gameDto.Passcode,
                ScoringRules = rawSRules,
                SubsDeadline = gameDto.SubsDeadline
            };
        }

        private Object GameView(Game game)
        {
            var creator = _context.AppUser.Find(game.AppUserID);
            return new
            {
                CompetitionID = game.CompetitionID,
                Creation = game.Creation,
                CreatorID = game.AppUserID,
                CreatorNick = (creator is null) ? "" : creator.Nickname!,
                Description = game.Description,
                ID = game.ID,
                MaxGuessCount = game.MaxGuessCount,
                MaxScore = game.MaxScore,
                Name = game.Name,
                Passcode = game.Passcode,
                ScoringRules = JsonDocument.Parse(game.ScoringRules),
                SubsDeadline = game.SubsDeadline
            };
        }

        private IQueryable<Game> Query(
            int? competitionId, string? appUserId, string name = "", bool publicOnly = false)
        {
            var query = _context.Game.Where(g => EF.Functions.Like(g.Name, $"%{name}%"));
            if (competitionId is not null)
            {
                query = query.Where(g => g.CompetitionID == competitionId);
            }
            if (appUserId is not null)
            {
                query = query.Where(g => g.AppUserID == appUserId);
            }
            if (publicOnly)
            {
                query = query.Where(g => string.IsNullOrEmpty(g.Passcode));
            }
            return query;
        }

        // GET: api/Game/Meta
        [HttpGet("Meta")]
        public async Task<ActionResult<int>> GetMetadata(
            int? competitionId, string? appUserId, string name = "", bool publicOnly = false)
        {
            var count = await Query(
                competitionId, appUserId, name, publicOnly).CountAsync();
            return count;
        }

        // GET: api/Game
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Object>>> GetGames(
            int? offset, int? limit, int? competitionId, string? appUserId, string name = "", bool publicOnly = false)
        {
            var query = QueryRefiner.Bound(
                Query(competitionId, appUserId, name, publicOnly), offset, limit);
            var rawGames = await query.ToListAsync();
            var result = rawGames.Select(g => GameView(g)).ToList();
            return result;
        }

        // GET: api/Game/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Object>> GetGame(string id)
        {
            var game = await _context.Game.FindAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            return GameView(game);
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
        public async Task<ActionResult<Game>> PostGame(GameDto gameDto)
        {
            // Check competition.
            var competition = await _context.Competition.FindAsync(gameDto.CompetitionID);
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
            string rawSRulesTemp = await _context.Formula
                .Where(f => f.ID == competition.FormulaID)
                .Select(f => f.ScoringRulesTemplate)
                .FirstAsync();
            var sRulesTemp = JsonDocument.Parse(rawSRulesTemp);
            if (!JsonDataChecker.ScoringRulesOnTemplate(sRulesTemp, gameDto.ScoringRules))
            {
                return BadRequest(MessageRepo.UnfitData);
            }

            Game game = CreateGame(gameDto, competition, dateTimeNow);
            _context.Game.Add(game);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetGame),
                new { id = game.ID },
                GameView(game)
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
}

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public GameController(
            AppDbContext context, IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
            _context = context;
        }

        private static Object GameView(Game game, AppDbContext context)
        {
            var creator = context.AppUser.Find(game.AppUserID);
            return new {
                CompetitionID = game.CompetitionID,
                Creator = (creator is null) ? "" : creator.Nickname!,
                Creation = game.Creation,
                Description = game.Description,
                ID = game.ID,
                MaxGuessCount = game.MaxGuessCount,
                MaxScore = game.MaxScore,
                Name = game.Name,
                Public = string.IsNullOrEmpty(game.Passcode),
                ScoringRules = JsonDocument.Parse(game.ScoringRules),
                SubsDeadline = game.SubsDeadline
            };
        }

        private IQueryable<Game> Query(int? competitionId, string name = "", bool publicOnly = false)
        {
            var query = _context.Game.Where(g => EF.Functions.Like(g.Name, $"%{name}%"));
            if (competitionId is not null)
            {
                query = query.Where(g => g.CompetitionID == competitionId);
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
            int? competitionId, string name = "", bool publicOnly = false)
        {
            var count = await Query(competitionId, name, publicOnly).CountAsync();
            return count;
        }

        // GET: api/Game
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Object>>> GetGames(
            int? offset, int? limit, int? competitionId, string name = "", bool publicOnly = false)
        {
            var query = QueryRefiner.Bound(Query(competitionId, name, publicOnly), offset, limit);
            var result = await query.Select(g => GameView(g, _context)).ToListAsync();
            return result;
        }

        // GET: api/Game/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Object>> GetGame(int id)
        {
            var game = await _context.Game.FindAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            return GameView(game, _context);
        }

        // PUT: api/Game/5
        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> PutGame(int id, GameDTO gameDTO)
        {
            // Built-in model validation.
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Game check.
            var game = await _context.Game.FindAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            // Only the owner can edit.
            var authorization = await _authorizationService
                .AuthorizeAsync(User, game, Operations.Update);
            if (!authorization.Succeeded)
            {
                return Forbid();
            }

            // Available updates.
            game.Description = gameDTO.Description;
            game.MaxGuessCount = gameDTO.MaxGuessCount;
            game.Name = gameDTO.Name;
            game.Passcode = gameDTO.Passcode;
            game.SubsDeadline = gameDTO.SubsDeadline;

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
        public async Task<ActionResult<Game>> PostGame(GameDTO gameDTO)
        {
            // Built-in model validation.
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Competition check.
            var competition = await _context.Competition.FindAsync(gameDTO.CompetitionID);
            if (competition is null)
            {
                return NotFound();
            }
            if (!competition.Active)
            {
                return Conflict(MessageRepo.InactiveResource);
            }

            // "ScoringRules" must be in accordance to template.
            string rawSRulesTemp = _context.Formula
                .Where(f => f.ID == competition.FormulaID)
                .Select(f => f.ScoringRulesTemplate)
                .First();
            var sRulesTemp = JsonDocument.Parse(rawSRulesTemp);
            if (!JsonDataChecker.ScoringRulesOnTemplate(sRulesTemp, gameDTO.ScoringRules))
            {
                return BadRequest(MessageRepo.UnfitData);
            }

            // Creation.
            var compData = JsonDocument.Parse(competition.Data);
            int maxScore = GuessScorer.Evaluate(compData, compData, gameDTO.ScoringRules);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            string rawSRules = JsonSerializer.Serialize(gameDTO.ScoringRules.RootElement);
            var game = new Game
            {
                AppUserID = userId,
                CompetitionID = gameDTO.CompetitionID,
                Creation = DateTime.Now,
                Description = gameDTO.Description,
                MaxScore = maxScore,
                MaxGuessCount = gameDTO.MaxGuessCount,
                Name = gameDTO.Name,
                Passcode = gameDTO.Passcode,
                ScoringRules = rawSRules,
                SubsDeadline = gameDTO.SubsDeadline
            };

            _context.Game.Add(game);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetGame),
                new { id = game.ID },
                GameView(game, _context)
            );
        }

        // DELETE: api/Game/5
        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _context.Game.FindAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            // Only the owner and site team can delete.
            var authorization = await _authorizationService
                .AuthorizeAsync(User, game, Operations.Update);
            if (!authorization.Succeeded)
            {
                return Forbid();
            }

            _context.Game.Remove(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GameExists(int id)
        {
            return _context.Game.Any(e => e.ID == id);
        }
    }
}

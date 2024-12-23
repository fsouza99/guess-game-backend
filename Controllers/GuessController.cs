using App.Authorization.Requirements;
using App.Controllers.ResponseMessages;
using App.Data;
using App.Models;
using App.StaticTools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuessController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public GuessController(
            AppDbContext context, IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
            _context = context;
        }

        private static Object GuessView(Guess guess) => new {
            Creation = guess.Creation,
            Data = JsonDocument.Parse(guess.Data),
            GameID = guess.GameID,
            Name = guess.Name,
            Number = guess.Number,
            Score = guess.Score
        };

        private IQueryable<Guess> Query(int gameId, string name = "")
        {
            var query = _context.Guess
                .Where(g => g.GameID == gameId)
                .Where(g => EF.Functions.Like(g.Name, $"%{name}%"));
            return query;
        }

        // GET: api/Guess/Meta
        [HttpGet("Meta")]
        public async Task<ActionResult<int>> GetMetadata(int gameId, string name = "")
        {
            var count = await Query(gameId, name).CountAsync();
            return count;
        }

        // GET: api/Guess
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Object>>> GetGuesses(
            int? offset, int? limit, int gameId, string name = "")
        {
            var query = QueryRefiner.Bound(Query(gameId, name), offset, limit);
            var result = await query.Select(g => GuessView(g)).ToListAsync();
            return result;
        }

        // GET: api/Guess/5/5
        [HttpGet("{gameId}/{number}")]
        public async Task<ActionResult<Object>> GetGuess(int gameId, int number)
        {
            var guess = await _context.Guess.FindAsync(gameId, number);
            if (guess is null)
            {
                return NotFound();
            }

            return GuessView(guess);
        }

        // POST: api/Guess
        [HttpPost]
        public async Task<ActionResult<Guess>> PostGuess(GuessDTO guessDTO)
        {
            // Built-in model validation.
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Check game.
            var game = await _context.Game.FindAsync(guessDTO.GameID);
            if (game is null)
            {
                return NotFound();
            }

            // Check passcode.
            if (!string.IsNullOrEmpty(game.Passcode))
            {
                if (guessDTO.GamePasscode != game.Passcode)
                {
                    return Unauthorized(MessageRepo.PasscodeError);
                }
            }

            // Check deadline.
            var dateTimeNow = DateTime.Now;
            if (game.SubsDeadline is not null)
            {
                if (dateTimeNow > game.SubsDeadline)
                {
                    return Conflict(MessageRepo.DeadlineReached);
                }
            }

            // Check guess count.
            int gameGuessCount = _context.Guess
                .Where(g => g.GameID == game.ID)
                .Count();
            if (gameGuessCount >= game.MaxGuessCount)
            {
                return Conflict(MessageRepo.MaxObjCountReached);
            }

            // "Data" must be in accordance to template.
            int formulaId = _context.Competition
                .Where(c => c.ID == game.CompetitionID)
                .Select(c => c.FormulaID)
                .First();
            string rawDataTemp = _context.Formula
                .Where(f => f.ID == formulaId)
                .Select(f => f.DataTemplate)
                .First();
            var dataTemp = JsonDocument.Parse(rawDataTemp);
            if (!JsonDataChecker.DataOnTemplate(dataTemp, guessDTO.Data))
            {
                return BadRequest(MessageRepo.UnfitData);
            }

            // Creation.
            string rawCompData = _context.Competition
                .Where(c => c.ID == game.CompetitionID)
                .Select(c => c.Data)
                .First();
            string rawGuessData = JsonSerializer.Serialize(guessDTO.Data.RootElement);
            var compData = JsonDocument.Parse(rawCompData);
            var sRules = JsonDocument.Parse(game.ScoringRules);
            int score = GuessScorer.Evaluate(guessDTO.Data, compData, sRules);
            var guess = new Guess
            {
                Creation = dateTimeNow,
                Data = rawGuessData,
                GameID = guessDTO.GameID,
                Name = guessDTO.Name,
                Number = gameGuessCount + 1,
                Score = score
            };
            
            _context.Guess.Add(guess);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetGuess),
                new { gameID = guess.GameID, number = guess.Number },
                GuessView(guess));
        }

        // DELETE: api/Guess/5/5
        [HttpDelete("{gameId}/{number}"), Authorize]
        public async Task<ActionResult> DeleteGuess(int gameId, int number)
        {
            // Guess check.
            var guess = _context.Guess
                .Where(g => g.GameID == gameId && g.Number == number)
                .FirstOrDefault();
            if (guess is null)
            {
                return NotFound();
            }

            // Only game owner and site team can delete.
            // Conveniently reuse the AuthorizationHandler for Game model class.
            var game = await _context.Game.FindAsync(gameId);
            var authorization = await _authorizationService
                .AuthorizeAsync(User, game, Operations.Delete);
            if (!authorization.Succeeded)
            {
                return Forbid();
            }

            _context.Guess.Remove(guess);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

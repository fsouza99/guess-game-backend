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

namespace App.Controllers
{
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

        private async Task<Guess> CreateGuess(GuessDto guessDto, Game game, DateTime creationDt)
        {
            string rawCompData = await _context.Competition
                .Where(c => c.ID == game.CompetitionID)
                .Select(c => c.Data)
                .FirstAsync();
            string rawGuessData = JsonSerializer.Serialize(guessDto.Data.RootElement);
            var compData = JsonDocument.Parse(rawCompData);
            var sRules = JsonDocument.Parse(game.ScoringRules);
            int score = GuessScorer.Evaluate(guessDto.Data, compData, sRules);
            return new Guess
            {
                Creation = creationDt,
                Data = rawGuessData,
                GameID = guessDto.GameID,
                Name = guessDto.Name,
                Number = game.NextGuessNumber,
                Score = score
            };
        }

        private static Object GuessView(Guess guess) => new {
            Creation = guess.Creation,
            Data = JsonDocument.Parse(guess.Data),
            GameID = guess.GameID,
            Name = guess.Name,
            Number = guess.Number,
            Score = guess.Score
        };

        private IQueryable<Guess> Query(string gameId, string name = "")
        {
            var query = _context.Guess
                .Where(g => g.GameID == gameId)
                .Where(g => EF.Functions.Like(g.Name, $"%{name}%"));
            return query;
        }

        // GET: api/Guess/Meta
        [HttpGet("Meta")]
        public async Task<ActionResult<int>> GetMetadata(string gameId, string name = "")
        {
            var count = await Query(gameId, name).CountAsync();
            return count;
        }

        // GET: api/Guess
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Object>>> GetGuesses(
            int? offset, int? limit, string gameId, string name = "")
        {
            var query = QueryRefiner.Bound(Query(gameId, name), offset, limit);
            var result = await query.Select(g => GuessView(g)).ToListAsync();
            return result;
        }

        // GET: api/Guess/5/5
        [HttpGet("{gameId}/{number}")]
        public async Task<ActionResult<Object>> GetGuess(string gameId, int number)
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
        public async Task<ActionResult<Guess>> PostGuess(GuessDto guessDto)
        {
            // Check game.
            var game = await _context.Game.FindAsync(guessDto.GameID);
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
            int formulaId = await _context.Competition
                .Where(c => c.ID == game.CompetitionID)
                .Select(c => c.FormulaID)
                .FirstAsync();
            string rawDataTemp = await _context.Formula
                .Where(f => f.ID == formulaId)
                .Select(f => f.DataTemplate)
                .FirstAsync();
            var dataTemp = JsonDocument.Parse(rawDataTemp);
            if (!JsonDataChecker.DataOnTemplate(dataTemp, guessDto.Data))
            {
                return BadRequest(MessageRepo.UnfitData);
            }

            // Create.
            Guess guess = await CreateGuess(guessDto, game, dateTimeNow);
            _context.Guess.Add(guess);

            // Update game.
            game.NextGuessNumber++;

            // Save changes and notify game's owner of relevant events.
            await _context.SaveChangesAsync();
            await _gameObserver.WatchAsync(game);

            return CreatedAtAction(
                nameof(GetGuess),
                new { gameID = guess.GameID, number = guess.Number },
                GuessView(guess));
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
    }
}

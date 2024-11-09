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

        // GET: api/Guess
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Object>>> GetGuess()
        {
            var list = await _context.Guess.ToListAsync();
            var result = new List<Object>();
            foreach (var guess in list)
            {
                var obj = new {
                    AuthorName = guess.AuthorName,
                    Creation = guess.Creation,
                    Data = guess.Data,
                    GameID = guess.GameID,
                    Number = guess.Number,
                    Score = guess.Score
                };
                result.Add(obj);
            }
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

            var result = new
            {
                AuthorName = guess.AuthorName,
                Creation = guess.Creation,
                Data = guess.Data,
                GameID = guess.GameID,
                Number = guess.Number,
                Score = guess.Score
            };

            return result;
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
            if (game.SubsDeadline is not null)
            {
                if (DateTime.Now > game.SubsDeadline)
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
            string dataTemp = _context.Formula
                .Where(f => f.ID == formulaId)
                .Select(f => f.DataTemplate)
                .First();
            if (!JsonDataChecker.DataOnTemplate(dataTemp, guessDTO.Data))
            {
                return BadRequest(MessageRepo.UnfitData);
            }

            // Creation.
            string compData = _context.Competition
                .Where(c => c.ID == game.CompetitionID)
                .Select(c => c.Data)
                .First();
            var guess = new Guess
            {
                AuthorName = guessDTO.AuthorName,
                Creation = DateTime.Now,
                Data = guessDTO.Data,
                GameID = guessDTO.GameID,
                Number = gameGuessCount + 1,
                Score = GuessScorer.Evaluate(guessDTO.Data, compData, game.ScoringRules)
            };
            
            _context.Guess.Add(guess);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGuess), new { gameID = guess.GameID, number = guess.Number }, guess);
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

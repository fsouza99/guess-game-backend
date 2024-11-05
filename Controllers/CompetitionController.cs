using App.Authorization.References;
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
    public class CompetitionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompetitionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Competition
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Object>>> GetCompetition()
        {
            var list = await _context.Competition.ToListAsync();
            var result = new List<Object>();
            foreach (var competition in list)
            {
                var obj = new {
                    Active = competition.Active,
                    Creation = competition.Creation,
                    Description = competition.Description,
                    FormulaID = competition.FormulaID,
                    ID = competition.ID,
                    Name = competition.Name,
                    Data = competition.Data
                };
                result.Add(obj);
            }
            return result;
        }

        // GET: api/Competition/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Object>> GetCompetition(int id)
        {
            var competition = await _context.Competition.FindAsync(id);
            if (competition is null)
            {
                return NotFound();
            }

            var result = new
            {
                Active = competition.Active,
                Creation = competition.Creation,
                Description = competition.Description,
                FormulaID = competition.FormulaID,
                ID = competition.ID,
                Name = competition.Name,
                Data = competition.Data
            };

            return result;
        }

        // PUT: api/Competition/5
        [HttpPut("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
        public async Task<IActionResult> PutCompetition(int id, CompetitionDTO competitionDTO)
        {
            // Built-in model validation.
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            
            // Competition check.
            var competition = await _context.Competition.FindAsync(id);
            if (competition is null)
            {
                return NotFound();
            }

            // If "Data" is to be changed...
            if (competition.Data != competitionDTO.Data)
            {
                // Updated data must be in accordance to template.
                if (!IsDataFit(competition.FormulaID, competitionDTO.Data))
                {
                    return BadRequest(MessageRepo.UnfitData);
                }

                // Update.
                competition.Data = competitionDTO.Data;

                // Guesses must be reassessed.
                var games = _context.Game
                    .Where(g => g.CompetitionID == id)
                    .Include(g => g.Guesses);
                foreach (var game in games)
                {
                    foreach (var guess in game.Guesses)
                    {
                        guess.Score = GuessScorer.Evaluate(guess.Data, competition.Data, game.ScoringRules);
                    }
                }
            }

            // Other available updates.
            competition.Active = competitionDTO.Active;
            competition.Description = competitionDTO.Description;
            competition.Name = competitionDTO.Name;

            _context.Entry(competition).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompetitionExists(id))
                {
                    return NotFound();
                }
                return Conflict(MessageRepo.UpdateConflict);
            }

            return NoContent();
        }

        // POST: api/Competition
        [HttpPost, Authorize(Policy = PolicyReference.AccreditedOnly)]
        public async Task<ActionResult<Competition>> PostCompetition(CompetitionDTO competitionDTO)
        {
            // Built-in model validation.
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Formula check.
            var formula = await _context.Formula.FindAsync(competitionDTO.FormulaID);
            if (formula is null)
            {
                return NotFound();
            }

            // Inserted data must be in accordance to template.
            if (!IsDataFit(competitionDTO.FormulaID, competitionDTO.Data))
            {
                return BadRequest(MessageRepo.UnfitData);
            }

            var competition = new Competition
            {
                Creation = DateTime.Now,
                Data = competitionDTO.Data,
                Description = competitionDTO.Description,
                FormulaID = competitionDTO.FormulaID,
                Name = competitionDTO.Name
            };

            _context.Competition.Add(competition);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompetition), new { id = competition.ID }, competition);
        }

        // DELETE: api/Competition/5
        [HttpDelete("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
        public async Task<IActionResult> DeleteCompetition(int id)
        {
            var competition = await _context.Competition.FindAsync(id);
            if (competition is null)
            {
                return NotFound();
            }

            _context.Competition.Remove(competition);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CompetitionExists(int id)
        {
            return _context.Competition.Any(e => e.ID == id);
        }

        private bool IsDataFit(int formulaId, string compData)
        {
            string rwdt = _context.Formula
                .Where(f => f.ID == formulaId)
                .Select(f => f.DataTemplate)
                .First();
            return JsonDataChecker.DataOnTemplate(rwdt, compData);
        }
    }
}

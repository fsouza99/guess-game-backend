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
using System.Text.Json;
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
                    Data = JsonDocument.Parse(competition.Data)
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
                Data = JsonDocument.Parse(competition.Data)
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
            var rawData = JsonSerializer.Serialize(competitionDTO.Data.RootElement);
            if (competition.Data != rawData)
            {
                // Updated data must be in accordance to template.
                if (!CheckData(competitionDTO))
                {
                    return BadRequest(MessageRepo.UnfitData);
                }

                // Update.
                competition.Data = rawData;

                // Guesses must be reassessed.
                var games = _context.Game
                    .Where(g => g.CompetitionID == id)
                    .Include(g => g.Guesses);
                foreach (var game in games)
                {
                    var gameSRules = JsonDocument.Parse(game.ScoringRules);
                    foreach (var guess in game.Guesses)
                    {
                        var guessData = JsonDocument.Parse(guess.Data);
                        guess.Score = GuessScorer.Evaluate(guessData, competitionDTO.Data, gameSRules);
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

            // "Data" must be in accordance to template.
            if (!CheckData(competitionDTO))
            {
                return BadRequest(MessageRepo.UnfitData);
            }

            // Creation.
            var rawData = JsonSerializer.Serialize(competitionDTO.Data.RootElement);
            var competition = new Competition
            {
                Creation = DateTime.Now,
                Data = rawData,
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

        private bool CheckData(CompetitionDTO competitionDTO)
        {
            string rawDataTemp = _context.Formula
                .Where(f => f.ID == competitionDTO.FormulaID)
                .Select(f => f.DataTemplate)
                .First();
            var dataTemp = JsonDocument.Parse(rawDataTemp);
            return JsonDataChecker.DataOnTemplate(dataTemp, competitionDTO.Data);
        }
    }
}

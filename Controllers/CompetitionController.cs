using App.Authorization.References;
using App.Controllers.ResponseMessages;
using App.Data;
using App.Models;
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
public class CompetitionController : ControllerBase
{
    private readonly AppDbContext _context;

    public CompetitionController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Competition/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(
        int? formulaId, string? name, bool activeOnly = false)
    {
        var query = QueryRefiner.Competitions(
            _context.Competition, formulaId, name, activeOnly);
        var count = await query.CountAsync();
        return count;
    }

    // GET: api/Competition
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SimpleCompetitionView>>> GetCompetitions(
        int? formulaId,
        string? name,
        bool activeOnly = false,
        int? offset = null,
        int? limit = null)
    {
        var query = QueryRefiner.Competitions(
            _context.Competition,
            formulaId,
            name,
            activeOnly,
            offset,
            limit);
        var result = await query
            .Select(c => ViewFactory.SimpleCompetition(c))
            .ToListAsync();
        return result;
    }

    // GET: api/Competition/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CompetitionView>> GetCompetition(int id)
    {
        var competition = await _context.Competition.FindAsync(id);
        if (competition is null)
        {
            return NotFound();
        }

        return ViewFactory.Competition(competition);
    }

    // PUT: api/Competition/5
    [HttpPut("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
    public async Task<IActionResult> PutCompetition(int id, CompetitionDto dto)
    {
        // Competition check.
        var competition = await _context.Competition.FindAsync(id);
        if (competition is null)
        {
            return NotFound();
        }

        // If "Data" is to be changed...
        var rawData = JsonSerializer.Serialize(dto.Data.RootElement);
        if (competition.Data != rawData)
        {
            // Updated data must be in accordance to template.
            if (!CheckData(dto))
            {
                return BadRequest(MessageRepo.UnfitData);
            }

            // Update.
            competition.Data = rawData;

            // Games and guesses must be reassessed.
            var games = _context.Game
                .Where(g => g.CompetitionID == id)
                .Include(g => g.Guesses);
            foreach (var game in games)
            {
                var sRules = JsonDocument.Parse(game.ScoringRules);
                game.MaxScore = GuessScorer.Evaluate(
                    dto.Data, dto.Data, sRules);
                foreach (var guess in game.Guesses)
                {
                    var guessData = JsonDocument.Parse(guess.Data);
                    guess.Score = GuessScorer.Evaluate(
                        guessData, dto.Data, sRules);
                }
            }
        }

        // Other available updates.
        competition.Active = dto.Active;
        competition.Description = dto.Description;
        competition.Name = dto.Name;
        competition.Start = dto.Start;
        competition.End = dto.End;

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
    public async Task<ActionResult<CompetitionView>> PostCompetition(
        CompetitionDto dto)
    {
        // Check formula.
        var formula = await _context.Formula.FindAsync(dto.FormulaID);
        if (formula is null)
        {
            return NotFound();
        }

        // Check conformance of "Data" with template.
        if (!CheckData(dto))
        {
            return BadRequest(MessageRepo.UnfitData);
        }

        // Creation.
        var rawData = JsonSerializer.Serialize(dto.Data.RootElement);
        var competition = new Competition
        {
            Creation = DateTime.Now,
            Data = rawData,
            Description = dto.Description,
            End = dto.End,
            FormulaID = dto.FormulaID,
            Name = dto.Name,
            Start = dto.Start
        };

        _context.Competition.Add(competition);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCompetition),
            new { id = competition.ID },
            ViewFactory.Competition(competition));
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

    private bool CheckData(CompetitionDto dto)
    {
        string rawDataTemp = _context.Formula
            .Where(f => f.ID == dto.FormulaID)
            .Select(f => f.DataTemplate)
            .First();
        var dataTemp = JsonDocument.Parse(rawDataTemp);
        return JsonDataChecker.DataOnTemplate(dataTemp, dto.Data);
    }
}


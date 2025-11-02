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
    public async Task<ActionResult<IEnumerable<CompetitionView>>> GetCompetitions(
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
            .Select(c => CreateCompetitionView(c))
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

        return CreateCompetitionView(competition);
    }

    // PUT: api/Competition/5
    [HttpPut("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
    public async Task<IActionResult> PutCompetition(
        int id, CompetitionDto competitionDto)
    {
        // Competition check.
        var competition = await _context.Competition.FindAsync(id);
        if (competition is null)
        {
            return NotFound();
        }

        // If "Data" is to be changed...
        var rawData = JsonSerializer.Serialize(competitionDto.Data.RootElement);
        if (competition.Data != rawData)
        {
            // Updated data must be in accordance to template.
            if (!CheckData(competitionDto))
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
                var gameSRules = JsonDocument.Parse(game.ScoringRules);
                game.MaxScore = GuessScorer.Evaluate(
                    competitionDto.Data, competitionDto.Data, gameSRules);
                foreach (var guess in game.Guesses)
                {
                    var guessData = JsonDocument.Parse(guess.Data);
                    guess.Score = GuessScorer.Evaluate(
                        guessData, competitionDto.Data, gameSRules);
                }
            }
        }

        // Other available updates.
        competition.Active = competitionDto.Active;
        competition.Description = competitionDto.Description;
        competition.Name = competitionDto.Name;

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
        CompetitionDto competitionDto)
    {
        // Check formula.
        var formula = await _context.Formula.FindAsync(competitionDto.FormulaID);
        if (formula is null)
        {
            return NotFound();
        }

        // Check conformance of "Data" with template.
        if (!CheckData(competitionDto))
        {
            return BadRequest(MessageRepo.UnfitData);
        }

        // Creation.
        var rawData = JsonSerializer.Serialize(competitionDto.Data.RootElement);
        var competition = new Competition
        {
            Creation = DateTime.Now,
            Data = rawData,
            Description = competitionDto.Description,
            FormulaID = competitionDto.FormulaID,
            Name = competitionDto.Name
        };

        _context.Competition.Add(competition);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCompetition),
            new { id = competition.ID },
            CreateCompetitionView(competition));
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

    private static CompetitionView CreateCompetitionView(
        Competition competition) => new CompetitionView(
        competition.Active,
        competition.Creation,
        JsonDocument.Parse(competition.Data),
        competition.Description,
        competition.FormulaID,
        competition.ID,
        competition.Name);

    private bool CompetitionExists(int id)
    {
        return _context.Competition.Any(e => e.ID == id);
    }

    private bool CheckData(CompetitionDto competitionDto)
    {
        string rawDataTemp = _context.Formula
            .Where(f => f.ID == competitionDto.FormulaID)
            .Select(f => f.DataTemplate)
            .First();
        var dataTemp = JsonDocument.Parse(rawDataTemp);
        return JsonDataChecker.DataOnTemplate(dataTemp, competitionDto.Data);
    }
}


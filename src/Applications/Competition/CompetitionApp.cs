using App.Globals;
using App.Infrastructure;
using App.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace App.Applications;

public class CompetitionApp
{
    private readonly AppDbContext _context;

    public CompetitionApp(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> CountAsync(
        int? formulaId, string? name, bool activeOnly)
    {
        var query = QueryRefiner.Competitions(
            _context.Competition, formulaId, name, activeOnly);
        var count = await query.CountAsync();
        return count;
    }

    public async Task<Result<List<SimpleCompetitionView>>> ReadManyAsync(
        int? formulaId, string? name, bool activeOnly, int? offset, int? limit)
    {
        var query = QueryRefiner.Competitions(
            _context.Competition, formulaId, name, activeOnly, offset, limit);
        var result = await query
            .Select(c => new SimpleCompetitionView(c))
            .ToListAsync();
        return result;
    }

    public async Task<Result<CompetitionView>> ReadOneAsync(int id)
    {
        var competition = await _context.Competition.FindAsync(id);
        if (competition is null)
        {
            return Result.Failure<CompetitionView>(CompetitionErrors.NotFound());
        }

        return new CompetitionView(competition);
    }

    public async Task<Result> UpdateAsync(int id, CompetitionDto dto)
    {
        var competition = await _context.Competition.FindAsync(id);
        if (competition is null)
        {
            return Result.Failure(CompetitionErrors.NotFound());
        }

        // If "Data" is to be changed...
        var rawData = JsonSerializer.Serialize(dto.Data.RootElement);
        if (competition.Data != rawData)
        {
            if (!(await DataMatchesTemplateAsync(dto)))
            {
                return Result.Failure(CompetitionErrors.UnfitData());
            }

            competition.Data = rawData;
            await ReassessAssociatedGamesAsync(id, dto);
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
            if (ItemExists(id))
            {
                return Result.Failure(CompetitionErrors.UpdateConflict());
            }
            return Result.Failure(CompetitionErrors.NotFound());
        }

        return Result.Success();
    }

    public async Task<Result<CompetitionView>> CreateAsync(CompetitionDto dto)
    {
        var formula = await _context.Formula.FindAsync(dto.FormulaID);
        if (formula is null)
        {
            return Result.Failure<CompetitionView>(CompetitionErrors.FormulaNotFound());
        }

        if (!(await DataMatchesTemplateAsync(dto, formula.DataTemplate)))
        {
            return Result.Failure<CompetitionView>(CompetitionErrors.UnfitData());
        }

        var competition = new Competition
        {
            Creation = DateTime.Now,
            Data = JsonSerializer.Serialize(dto.Data.RootElement),
            Description = dto.Description,
            End = dto.End,
            FormulaID = dto.FormulaID,
            Name = dto.Name,
            Start = dto.Start
        };

        _context.Competition.Add(competition);
        await _context.SaveChangesAsync();

        return new CompetitionView(competition);
    }

    public async Task<Result> RemoveAsync(int id)
    {
        var competition = await _context.Competition.FindAsync(id);
        if (competition is null)
        {
            return Result.Failure(CompetitionErrors.NotFound());
        }

        _context.Competition.Remove(competition);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    private bool ItemExists(int id)
    {
        return _context.Competition.Any(c => c.ID == id);
    }

    // Competition data must be in accordance to template.
    private async Task<bool> DataMatchesTemplateAsync(
        CompetitionDto dto, string? rawDataTemp = null)
    {
        if (rawDataTemp is null)
        {
            rawDataTemp = await _context.Formula
                .Where(f => f.ID == dto.FormulaID)
                .Select(f => f.DataTemplate)
                .FirstOrDefaultAsync();
        }
        var dataTemp = JsonDocument.Parse((string) rawDataTemp!);
        return JsonDataChecker.DataOnTemplate(dto.Data, dataTemp);
    }

    // Reassess games and guesses associated with given competition.
    private async Task ReassessAssociatedGamesAsync(int competitionId, CompetitionDto dto)
    {
        var games = await _context.Game
            .Where(g => g.CompetitionID == competitionId)
            .Include(g => g.Guesses)
            .ToListAsync();
        foreach (var game in games)
        {
            var sRules = JsonDocument.Parse(game.ScoringRules);
            game.MaxScore = GuessScorer.Evaluate(dto.Data, dto.Data, sRules);
            foreach (var guess in game.Guesses)
            {
                var guessData = JsonDocument.Parse(guess.Data);
                guess.Score = GuessScorer.Evaluate(guessData, dto.Data, sRules);
            }
        }
    }
}


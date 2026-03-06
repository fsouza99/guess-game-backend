using App.Data;
using App.Models;
using App.StaticTools;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace App.Applications;

public class FormulaApp
{
    private readonly AppDbContext _context;

    public FormulaApp(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> CountAsync(string? name)
    {
        var query = QueryRefiner.Formulas(_context.Formula, name);
        return await query.CountAsync();
    }

    public async Task<Result<List<FormulaView>>> ReadManyAsync(
        string? name, int? offset, int? limit)
    {
        var query = QueryRefiner.Formulas(
            _context.Formula, name, offset, limit);
        var result = await query
            .Select(f => new FormulaView(f))
            .ToListAsync();
        return result;
    }

    public async Task<Result<FormulaView>> ReadOneAsync(int id)
    {
        var formula = await _context.Formula.FindAsync(id);
        if (formula is null)
        {
            return Result.Failure<FormulaView>(FormulaErrors.NotFound());
        }

        return new FormulaView(formula);
    }

    public async Task<Result> UpdateAsync(int id, FormulaDto dto)
    {
        var formula = await _context.Formula.FindAsync(id);
        if (formula is null)
        {
            return Result.Failure(FormulaErrors.NotFound());
        }

        formula.Description = dto.Description;
        formula.Name = dto.Name;

        _context.Entry(formula).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (ItemExists(id))
            {
                return Result.Failure(FormulaErrors.UpdateConflict());
            }
            return Result.Failure(FormulaErrors.NotFound());
        }

        return Result.Success();
    }

    public async Task<Result<FormulaView>> CreateAsync(FormulaDto dto)
    {
        if (!JsonDataChecker.DataTemplate(dto.DataTemplate))
        {
            return Result.Failure<FormulaView>(FormulaErrors.BadDataTemplate());
        }

        var rawDataTemp = JsonSerializer.Serialize(dto.DataTemplate.RootElement);
        var formula = new Formula
        {
            Creation = DateTime.Now,
            DataTemplate = rawDataTemp,
            Description = dto.Description,
            Name = dto.Name
        };

        _context.Formula.Add(formula);
        await _context.SaveChangesAsync();

        return new FormulaView(formula);
    }

    public async Task<Result> RemoveAsync(int id)
    {
        var formula = await _context.Formula.FindAsync(id);
        if (formula is null)
        {
            return Result.Failure(FormulaErrors.NotFound());
        }

        _context.Formula.Remove(formula);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    private bool ItemExists(int id)
    {
        return _context.Formula.Any(f => f.ID == id);
    }
}


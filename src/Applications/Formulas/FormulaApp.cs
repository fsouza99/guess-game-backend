using App.Globals;
using App.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace App.Applications;

public class FormulaApp(IAppDbContext context) : IFormulaApp
{
    public async Task<Result<int>> CountAsync(string? name)
    {
        var query = QueryRefiner.Formulas(context.Formula, name);
        return await query.CountAsync();
    }

    public async Task<Result<List<FormulaView>>> ReadManyAsync(
        string? name, int? offset, int? limit)
    {
        var query = QueryRefiner.Formulas(context.Formula, name, offset, limit);
        var result = await query
            .Select(f => new FormulaView(f))
            .ToListAsync();
        return result;
    }

    public async Task<Result<FormulaView>> ReadOneAsync(int id)
    {
        var formula = await context.Formula.FindAsync(id);
        if (formula is null)
        {
            return Result.Failure<FormulaView>(FormulaErrors.NotFound());
        }

        return new FormulaView(formula);
    }

    public async Task<Result> UpdateAsync(int id, FormulaDto dto)
    {
        var formula = await context.Formula.FindAsync(id);
        if (formula is null)
        {
            return Result.Failure(FormulaErrors.NotFound());
        }

        formula.Description = dto.Description;
        formula.Name = dto.Name;

        try
        {
            await context.SaveChangesAsync();
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

        context.Formula.Add(formula);
        await context.SaveChangesAsync();

        return new FormulaView(formula);
    }

    public async Task<Result> RemoveAsync(int id)
    {
        var formula = await context.Formula.FindAsync(id);
        if (formula is null)
        {
            return Result.Failure(FormulaErrors.NotFound());
        }

        context.Formula.Remove(formula);
        await context.SaveChangesAsync();

        return Result.Success();
    }

    private bool ItemExists(int id)
    {
        return context.Formula.Any(f => f.ID == id);
    }
}


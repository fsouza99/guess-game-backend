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
[Authorize(Policy = PolicyReference.AccreditedOnly)]
public class FormulaController : ControllerBase
{
    private readonly AppDbContext _context;

    public FormulaController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Formula/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(string? name)
    {
        var query = QueryRefiner.Formulas(_context.Formula, name);
        var count = await query.CountAsync();
        return count;
    }

    // GET: api/Formula
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FormulaView>>> GetFormulas(
        string? name, int? offset, int? limit)
    {
        var query = QueryRefiner.Formulas(
            _context.Formula, name, offset, limit);
        var result = await query
            .Select(f => ViewFactory.Formula(f))
            .ToListAsync();
        return result;
    }

    // GET: api/Formula/5
    [HttpGet("{id}")]
    public async Task<ActionResult<FormulaView>> GetFormula(int id)
    {
        var formula = await _context.Formula.FindAsync(id);
        if (formula is null)
        {
            return NotFound();
        }

        return ViewFactory.Formula(formula);
    }

    // PUT: api/Formula/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFormula(int id, FormulaDto dto)
    {
        // Formula check.
        var formula = await _context.Formula.FindAsync(id);
        if (formula is null)
        {
            return NotFound();
        }

        // Available updates.
        formula.Description = dto.Description;
        formula.Name = dto.Name;

        _context.Entry(formula).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FormulaExists(id))
            {
                return NotFound();
            }
            return Conflict(MessageRepo.UpdateConflict);
        }

        return NoContent();
    }

    // POST: api/Formula
    [HttpPost]
    public async Task<ActionResult<FormulaView>> PostFormula(FormulaDto dto)
    {
        // Check template conformance.
        if (!JsonDataChecker.DataTemplate(dto.DataTemplate))
        {
            return BadRequest(MessageRepo.BadTemplate);
        }

        // Creation.
        var rawDataTemp = JsonSerializer.Serialize(
            dto.DataTemplate.RootElement);
        var formula = new Formula
        {
            Creation = DateTime.Now,
            DataTemplate = rawDataTemp,
            Description = dto.Description,
            Name = dto.Name
        };

        _context.Formula.Add(formula);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetFormula),
            new { id = formula.ID },
            ViewFactory.Formula(formula));
    }

    // DELETE: api/Formula/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFormula(int id)
    {
        var formula = await _context.Formula.FindAsync(id);
        if (formula is null)
        {
            return NotFound();
        }

        _context.Formula.Remove(formula);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool FormulaExists(int id)
    {
        return _context.Formula.Any(f => f.ID == id);
    }
}


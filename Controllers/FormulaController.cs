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
    [Authorize(Policy = PolicyReference.AccreditedOnly)]
    public class FormulaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FormulaController(AppDbContext context)
        {
            _context = context;
        }

        private static Object FormulaView(Formula formula) => new {
            Creation = formula.Creation,
            DataTemplate = JsonDocument.Parse(formula.DataTemplate),
            Description = formula.Description,
            ID = formula.ID,
            Name = formula.Name,
            ScoringRulesTemplate = JsonDocument.Parse(formula.ScoringRulesTemplate)
        };

        private IQueryable<Formula> Query(string name = "")
        {
            return _context.Formula.Where(f => EF.Functions.Like(f.Name, $"%{name}%"));
        }

        // GET: api/Formula/Meta
        [HttpGet("Meta")]
        public async Task<ActionResult<int>> GetMetadata(string name = "")
        {
            var count = await Query(name).CountAsync();
            return count;
        }

        // GET: api/Formula
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Object>>> GetFormulas(
            int? offset, int? limit, string name = "")
        {
            var query = QueryRefiner.Bound(Query(name), offset, limit);
            var result = await query.Select(f => FormulaView(f)).ToListAsync();
            return result;
        }

        // GET: api/Formula/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Object>> GetFormula(int id)
        {
            var formula = await _context.Formula.FindAsync(id);
            if (formula is null)
            {
                return NotFound();
            }

            return FormulaView(formula);
        }

        // PUT: api/Formula/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFormula(int id, FormulaDto formulaDto)
        {
            // Built-in model validation.
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            
            // Formula check.
            var formula = await _context.Formula.FindAsync(id);
            if (formula is null)
            {
                return NotFound();
            }

            // Available updates.
            formula.Description = formulaDto.Description;
            formula.Name = formulaDto.Name;

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
        public async Task<ActionResult<Formula>> PostFormula(FormulaDto formulaDto)
        {
            // Built-in model validation.
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Check template conformance.
            if (!JsonDataChecker.DataTemplate(formulaDto.DataTemplate) ||
                !JsonDataChecker.ScoringRulesTemplate(formulaDto.ScoringRulesTemplate))
            {
                return BadRequest(MessageRepo.BadTemplate);
            }

            // Creation.
            var rawDataTemp = JsonSerializer.Serialize(formulaDto.DataTemplate.RootElement);
            var rawSRulesTemp = JsonSerializer.Serialize(formulaDto.ScoringRulesTemplate.RootElement);
            var formula = new Formula
            {
                Creation = DateTime.Now,
                Description = formulaDto.Description,
                Name = formulaDto.Name,
                DataTemplate = rawDataTemp,
                ScoringRulesTemplate = rawSRulesTemp
            };

            _context.Formula.Add(formula);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFormula), new { id = formula.ID }, FormulaView(formula));
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
}

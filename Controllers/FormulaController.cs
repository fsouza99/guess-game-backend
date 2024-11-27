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

        // GET: api/Formula
        [HttpGet, Authorize(Policy = PolicyReference.AccreditedOnly)]
        public async Task<ActionResult<IEnumerable<Object>>> GetFormula()
        {
            var list = await _context.Formula.ToListAsync();
            var result = new List<Object>();
            foreach (var formula in list)
            {
                var obj = new {
                    Creation = formula.Creation,
                    Description = formula.Description,
                    ID = formula.ID,
                    Name = formula.Name,
                    DataTemplate = JsonDocument.Parse(formula.DataTemplate),
                    ScoringRulesTemplate = JsonDocument.Parse(formula.ScoringRulesTemplate)
                };
                result.Add(obj);
            }
            return result;
        }

        // GET: api/Formula/5
        [HttpGet("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
        public async Task<ActionResult<Object>> GetFormula(int id)
        {
            var formula = await _context.Formula.FindAsync(id);
            if (formula is null)
            {
                return NotFound();
            }

            var result = new {
                Creation = formula.Creation,
                Description = formula.Description,
                ID = formula.ID,
                Name = formula.Name,
                DataTemplate = JsonDocument.Parse(formula.DataTemplate),
                ScoringRulesTemplate = JsonDocument.Parse(formula.ScoringRulesTemplate)
            };

            return result;
        }

        // PUT: api/Formula/5
        [HttpPut("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
        public async Task<IActionResult> PutFormula(int id, FormulaDTO formulaDTO)
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
            formula.Description = formulaDTO.Description;
            formula.Name = formulaDTO.Name;

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
        [HttpPost, Authorize(Policy = PolicyReference.AccreditedOnly)]
        public async Task<ActionResult<Formula>> PostFormula(FormulaDTO formulaDTO)
        {
            // Built-in model validation.
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Check template conformance.
            if (!CheckTemplates(formulaDTO))
            {
                return BadRequest(MessageRepo.BadTemplate);
            }

            // Creation.
            var rawDataTemp = JsonSerializer.Serialize(formulaDTO.DataTemplate.RootElement);
            var rawSRulesTemp = JsonSerializer.Serialize(formulaDTO.ScoringRulesTemplate.RootElement);
            var formula = new Formula
            {
                Creation = DateTime.Now,
                Description = formulaDTO.Description,
                Name = formulaDTO.Name,
                DataTemplate = rawDataTemp,
                ScoringRulesTemplate = rawSRulesTemp
            };

            _context.Formula.Add(formula);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFormula), new { id = formula.ID }, formula);
        }

        // DELETE: api/Formula/5
        [HttpDelete("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
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

        private bool CheckTemplates(FormulaDTO formulaDTO)
        {
            return
                JsonDataChecker.DataTemplate(formulaDTO.DataTemplate) &&
                JsonDataChecker.ScoringRulesTemplate(formulaDTO.ScoringRulesTemplate);
        }

        private bool FormulaExists(int id)
        {
            return _context.Formula.Any(f => f.ID == id);
        }
    }
}

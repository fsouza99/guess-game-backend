using App.Applications;
using App.Authorization;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = PolicyReference.AccreditedOnly)]
public class FormulaController : ControllerBase
{
    private readonly FormulaApp _app;

    public FormulaController(FormulaApp app)
    {
        _app = app;
    }

    // GET: api/Formula/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(string? name)
    {
        Result<int> result = await _app.CountAsync(name);
        return result.Value;
    }

    // GET: api/Formula
    [HttpGet]
    public async Task<ActionResult<List<FormulaView>>> GetFormulas(
        string? name, int? offset, int? limit)
    {
        Result<List<FormulaView>> result = await _app.ReadManyAsync(
            name, offset, limit);
        return result.Value;
    }

    // GET: api/Formula/5
    [HttpGet("{id}")]
    public async Task<ActionResult<FormulaView>> GetFormula(int id)
    {
        Result<FormulaView> result = await _app.ReadOneAsync(id);
        if (result.IsSuccess)
        {
            return result.Value;
        }
        return NotFound(result.Error.Description);
    }

    // PUT: api/Formula/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFormula(int id, FormulaDto dto)
    {
        Result result = await _app.UpdateAsync(id, dto);
        if (result.IsSuccess)
        {
            return NoContent();
        }

        switch (result.Error.Type)
        {
            case ErrorType.NotFound:
                return NotFound(result.Error.Description);
            case ErrorType.Conflict:
                return Conflict(result.Error.Description);
            default:
                return BadRequest(result.Error.Description);
        }
    }

    // POST: api/Formula
    [HttpPost]
    public async Task<ActionResult<FormulaView>> PostFormula(FormulaDto dto)
    {
        Result<FormulaView> result = await _app.CreateAsync(dto);
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetFormula), new { id = result.Value.ID }, result.Value);
        }
        return BadRequest(result.Error.Description);
    }

    // DELETE: api/Formula/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFormula(int id)
    {
        Result result = await _app.RemoveAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return NotFound(result.Error.Description);
    }
}

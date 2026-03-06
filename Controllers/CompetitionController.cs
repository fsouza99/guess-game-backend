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
public class CompetitionController : ControllerBase
{
    private readonly CompetitionApp _app;

    public CompetitionController(CompetitionApp app)
    {
        _app = app;
    }

    // GET: api/Competition/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(
        int? formulaId, string? name, bool activeOnly)
    {
        Result<int> result = await _app.CountAsync(formulaId, name, activeOnly);
        return result.Value;
    }

    // GET: api/Competition
    [HttpGet]
    public async Task<ActionResult<List<SimpleCompetitionView>>> GetCompetitions(
        int? formulaId, string? name, bool activeOnly, int? offset, int? limit)
    {
        Result<List<SimpleCompetitionView>> result = await _app.ReadManyAsync(
            formulaId, name, activeOnly, offset, limit);
        return result.Value;
    }

    // GET: api/Competition/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CompetitionView>> GetCompetition(int id)
    {
        Result<CompetitionView> result = await _app.ReadOneAsync(id);
        if (result.IsSuccess)
        {
            return result.Value;
        }
        return NotFound(result.Error.Description);
    }

    // PUT: api/Competition/5
    [HttpPut("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
    public async Task<IActionResult> PutCompetition(int id, CompetitionDto dto)
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

    // POST: api/Competition
    [HttpPost, Authorize(Policy = PolicyReference.AccreditedOnly)]
    public async Task<ActionResult<CompetitionView>> PostCompetition(CompetitionDto dto)
    {
        Result<CompetitionView> result = await _app.CreateAsync(dto);
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetCompetition), new { id = result.Value.ID }, result.Value);
        }
        if (result.Error.Type == ErrorType.NotFound)
        {
            return NotFound(result.Error.Description);
        }
        return BadRequest(result.Error.Description);
    }

    // DELETE: api/Competition/5
    [HttpDelete("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
    public async Task<IActionResult> DeleteCompetition(int id)
    {
        Result result = await _app.RemoveAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return NotFound(result.Error.Description);
    }
}


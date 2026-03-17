using App.Applications;
using App.Globals;
using App.Infrastructure;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api;

[Route("api/[controller]")]
[ApiController]
public class CompetitionController(ICompetitionApp app) : ControllerBase
{
    // GET: api/Competition/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(int? formulaId, string? name, bool activeOnly)
    {
        Result<int> result = await app.CountAsync(formulaId, name, activeOnly);
        return result.Value;
    }

    // GET: api/Competition
    [HttpGet]
    public async Task<ActionResult<List<SimpleCompetitionView>>> GetCompetitions(
        int? formulaId, string? name, bool activeOnly, int? offset, int? limit)
    {
        Result<List<SimpleCompetitionView>> result = await app.ReadManyAsync(
            formulaId, name, activeOnly, offset, limit);
        return result.Value;
    }

    // GET: api/Competition/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CompetitionView>> GetCompetition(int id)
    {
        Result<CompetitionView> result = await app.ReadOneAsync(id);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }

    // PUT: api/Competition/5
    [HttpPut("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
    public async Task<IActionResult> PutCompetition(int id, CompetitionDto dto)
    {
        Result result = await app.UpdateAsync(id, dto);
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }

    // POST: api/Competition
    [HttpPost, Authorize(Policy = PolicyReference.AccreditedOnly)]
    public async Task<ActionResult<CompetitionView>> PostCompetition(CompetitionDto dto)
    {
        Result<CompetitionView> result = await app.CreateAsync(dto);
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetCompetition), new { id = result.Value.ID }, result.Value);
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }

    // DELETE: api/Competition/5
    [HttpDelete("{id}"), Authorize(Policy = PolicyReference.AccreditedOnly)]
    public async Task<IActionResult> DeleteCompetition(int id)
    {
        Result result = await app.RemoveAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }
}

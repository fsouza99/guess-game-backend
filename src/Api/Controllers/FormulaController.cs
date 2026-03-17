using App.Applications;
using App.Globals;
using App.Infrastructure;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = PolicyReference.AccreditedOnly)]
public class FormulaController(IFormulaApp app) : ControllerBase
{
    // GET: api/Formula/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(string? name)
    {
        Result<int> result = await app.CountAsync(name);
        return result.Value;
    }

    // GET: api/Formula
    [HttpGet]
    public async Task<ActionResult<List<FormulaView>>> GetFormulas(
        string? name, int? offset, int? limit)
    {
        Result<List<FormulaView>> result = await app.ReadManyAsync(
            name, offset, limit);
        return result.Value;
    }

    // GET: api/Formula/5
    [HttpGet("{id}")]
    public async Task<ActionResult<FormulaView>> GetFormula(int id)
    {
        Result<FormulaView> result = await app.ReadOneAsync(id);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }

    // PUT: api/Formula/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFormula(int id, FormulaDto dto)
    {
        Result result = await app.UpdateAsync(id, dto);
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }

    // POST: api/Formula
    [HttpPost]
    public async Task<ActionResult<FormulaView>> PostFormula(FormulaDto dto)
    {
        Result<FormulaView> result = await app.CreateAsync(dto);
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetFormula), new { id = result.Value.ID }, result.Value);
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }

    // DELETE: api/Formula/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFormula(int id)
    {
        Result result = await app.RemoveAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }
}

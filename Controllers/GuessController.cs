using App.Applications;
using App.Authorization;
using App.Identity.Data;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GuessController : ControllerBase
{
    private readonly GuessApp _app;

    public GuessController(GuessApp app)
    {
        _app = app;
    }

    // GET: api/Guess/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(string? gameId, string? name)
    {
        Result<int> result = await _app.CountAsync(gameId, name);
        return result.Value;
    }

    // GET: api/Guess
    [HttpGet]
    public async Task<ActionResult<List<GuessView>>> GetGuesses(
        string? gameId, string? name, int? offset, int? limit)
    {
        Result<List<GuessView>> result = await _app.ReadManyAsync(
            gameId, name, offset, limit);
        return result.Value;
    }

    // GET: api/Guess/5/5
    [HttpGet("{gameId}/{number}")]
    public async Task<ActionResult<GuessView>> GetGuess(string gameId, int number)
    {
        Result<GuessView> result = await _app.ReadOneAsync(gameId, number);
        if (result.IsSuccess)
        {
            return result.Value;
        }
        return NotFound(result.Error.Description);
    }

    // POST: api/Guess
    [HttpPost]
    public async Task<ActionResult<GuessView>> PostGuess(GuessDto dto)
    {
        Result<GuessView> result = await _app.CreateAsync(dto);
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetGuess),
                new { gameId = result.Value.GameID, number = result.Value.Number },
                result.Value);
        }
        switch (result.Error.Type)
        {
            case ErrorType.NotFound:
                return NotFound(result.Error.Description);
            case ErrorType.Conflict:
                return Conflict(result.Error.Description);
            case ErrorType.Unauthorized:
                return Unauthorized(result.Error.Description);
            default:
                return BadRequest(result.Error.Description);
        }
    }

    // DELETE: api/Guess/5/5
    [HttpDelete("{gameId}/{number}"), Authorize]
    public async Task<ActionResult> DeleteGuess(string gameId, int number)
    {
        Result result = await _app.RemoveAsync(gameId, number, User);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return NotFound(result.Error.Description);
    }
}


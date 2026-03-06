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
public class GameController : ControllerBase
{
    private readonly GameApp _app;

    public GameController(GameApp app)
    {
        _app = app;
    }

    // GET: api/Game/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(
        int? competitionId, string? name, bool publicOnly)
    {
        Result<int> result = await _app.CountAsync(competitionId, name, publicOnly);
        return result.Value;
    }

    // GET: api/Game/Meta/Personal
    [HttpGet("Meta/Personal"), Authorize]
    public async Task<ActionResult<int>> GetPersonalMetadata(
        int? competitionId, string? name, bool publicOnly)
    {
        Result<int> result = await _app.CountPersonalAsync(
            User, competitionId, name, publicOnly);
        return result.Value;
    }

    // GET: api/Game/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GameView>> GetGame(string id)
    {
        Result<GameView> result = await _app.ReadOneAsync(id);
        if (result.IsSuccess)
        {
            return result.Value;
        }
        return NotFound(result.Error.Description);
    }

    // GET: api/Game
    [HttpGet]
    public async Task<ActionResult<List<SimpleGameView>>> GetGames(
        int? competitionId,
        string? userId,
        string? name,
        bool publicOnly,
        int? offset,
        int? limit)
    {
        Result<List<SimpleGameView>> result = await _app.ReadManyAsync(
            competitionId, userId, name, publicOnly, offset, limit);
        return result.Value;
    }

    // GET: api/Game/Personal
    [HttpGet("Personal"), Authorize]
    public async Task<ActionResult<List<SimpleGameView>>> GetPersonalGames(
        int? competitionId, string? name, bool publicOnly, int? offset, int? limit)
    {
        Result<List<SimpleGameView>> result = await _app.ReadManyPersonalAsync(
            User, competitionId, name, publicOnly, offset, limit);
        return result.Value;
    }

    // PUT: api/Game/5
    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> PutGame(string id, GameDto dto)
    {
        Result result = await _app.UpdateAsync(id, dto, User);
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
            case ErrorType.Forbidden:
                return Forbid();
            case ErrorType.Unauthorized:
                return Unauthorized(result.Error.Description);
            default:
                return BadRequest(result.Error.Description);
        }
    }

    // POST: api/Game
    [HttpPost, Authorize]
    public async Task<ActionResult<GameView>> PostGame(GameDto dto)
    {
        Result<GameView> result = await _app.CreateAsync(dto, User);
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetGame), new { id = result.Value.ID }, result.Value);
        }
        if (result.Error.Type == ErrorType.NotFound)
        {
            return NotFound(result.Error.Description);
        }
        return BadRequest(result.Error.Description);
    }

    // DELETE: api/Game/5
    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> DeleteGame(string id)
    {
        Result result = await _app.RemoveAsync(id, User);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return NotFound(result.Error.Description);
    }
}


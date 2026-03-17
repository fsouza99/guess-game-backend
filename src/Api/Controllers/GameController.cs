using App.Applications;
using App.Globals;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api;

[Route("api/[controller]")]
[ApiController]
public class GameController(IGameApp app) : ControllerBase
{
    // GET: api/Game/Meta
    [HttpGet("Meta")]
    public async Task<ActionResult<int>> GetMetadata(
        int? competitionId, string? name, bool publicOnly)
    {
        Result<int> result = await app.CountAsync(competitionId, name, publicOnly);
        return result.Value;
    }

    // GET: api/Game/Meta/Personal
    [HttpGet("Meta/Personal"), Authorize]
    public async Task<ActionResult<int>> GetPersonalMetadata(
        int? competitionId, string? name, bool publicOnly)
    {
        Result<int> result = await app.CountPersonalAsync(
            User, competitionId, name, publicOnly);
        return result.Value;
    }

    // GET: api/Game/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GameView>> GetGame(string id)
    {
        Result<GameView> result = await app.ReadOneAsync(id);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        return ApiErrorResponses.AppProblem(result.Error);
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
        Result<List<SimpleGameView>> result = await app.ReadManyAsync(
            competitionId, userId, name, publicOnly, offset, limit);
        return result.Value;
    }

    // GET: api/Game/Personal
    [HttpGet("Personal"), Authorize]
    public async Task<ActionResult<List<SimpleGameView>>> GetPersonalGames(
        int? competitionId, string? name, bool publicOnly, int? offset, int? limit)
    {
        Result<List<SimpleGameView>> result = await app.ReadManyPersonalAsync(
            User, competitionId, name, publicOnly, offset, limit);
        return result.Value;
    }

    // PUT: api/Game/5
    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> PutGame(string id, GameDto dto)
    {
        Result result = await app.UpdateAsync(id, dto, User);
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }

    // POST: api/Game
    [HttpPost, Authorize]
    public async Task<ActionResult<GameView>> PostGame(GameDto dto)
    {
        Result<GameView> result = await app.CreateAsync(dto, User);
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetGame), new { id = result.Value.ID }, result.Value);
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }

    // DELETE: api/Game/5
    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> DeleteGame(string id)
    {
        Result result = await app.RemoveAsync(id, User);
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return ApiErrorResponses.AppProblem(result.Error);
    }
}

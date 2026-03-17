using App.Globals;
using App.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace App.Applications;

public interface IGameApp : IApp
{
    Task<Result<int>> CountAsync(int? competitionId, string? name, bool publicOnly);

    Task<Result<int>> CountPersonalAsync(
        ClaimsPrincipal user, int? competitionId, string? name, bool publicOnly);

    Task<Result<List<SimpleGameView>>> ReadManyAsync(
        int? competitionId,
        string? userId,
        string? name,
        bool publicOnly,
        int? offset,
        int? limit);

    Task<Result<List<SimpleGameView>>> ReadManyPersonalAsync(
        ClaimsPrincipal user,
        int? competitionId,
        string? name,
        bool publicOnly,
        int? offset,
        int? limit);

    Task<Result<GameView>> ReadOneAsync(string id);

    Task<Result> UpdateAsync(string id, GameDto dto, ClaimsPrincipal user);

    Task<Result<GameView>> CreateAsync(GameDto dto, ClaimsPrincipal user);

    Task<Result> RemoveAsync(string id, ClaimsPrincipal user);
}

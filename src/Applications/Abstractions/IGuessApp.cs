using App.Globals;
using App.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace App.Applications;

public interface IGuessApp : IApp
{
    Task<Result<int>> CountAsync(string? gameId, string? name);

    Task<Result<List<GuessView>>> ReadManyAsync(
        string? gameId, string? name, int? offset, int? limit);

    Task<Result<GuessView>> ReadOneAsync(string gameId, int number);

    Task<Result<GuessView>> CreateAsync(GuessDto dto);

    Task<Result> RemoveAsync(string gameId, int number, ClaimsPrincipal user);
}

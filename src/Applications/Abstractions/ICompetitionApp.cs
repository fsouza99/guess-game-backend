using App.Globals;
using App.Models;
using System.Threading.Tasks;

namespace App.Applications;

public interface ICompetitionApp : IApp
{
    Task<Result<int>> CountAsync(int? formulaId, string? name, bool activeOnly);

    Task<Result<List<SimpleCompetitionView>>> ReadManyAsync(
        int? formulaId, string? name, bool activeOnly, int? offset, int? limit);

    Task<Result<CompetitionView>> ReadOneAsync(int id);

    Task<Result> UpdateAsync(int id, CompetitionDto dto);

    Task<Result<CompetitionView>> CreateAsync(CompetitionDto dto);

    Task<Result> RemoveAsync(int id);
}

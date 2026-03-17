using App.Globals;
using App.Models;
using System.Threading.Tasks;

namespace App.Applications;

public interface IFormulaApp : IApp
{
    Task<Result<int>> CountAsync(string? name);

    Task<Result<List<FormulaView>>> ReadManyAsync(string? name, int? offset, int? limit);

    Task<Result<FormulaView>> ReadOneAsync(int id);

    Task<Result> UpdateAsync(int id, FormulaDto dto);

    Task<Result<FormulaView>> CreateAsync(FormulaDto dto);

    Task<Result> RemoveAsync(int id);
}

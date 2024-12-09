using System.Linq;

namespace App.StaticTools;

public static class Refiner
{
	/*! Applies boundaries to query, allowing for pagination. */
	public static IQueryable<T> Bound<T>(IQueryable<T> query, int? offset, int? limit)
	{
		if (offset is not null && offset > 0)
        {
            query = query.Skip((int) offset);
        }
        if (limit is not null && limit > 0)
        {
            query = query.Take((int) limit);
        }
        return query;
	}
}
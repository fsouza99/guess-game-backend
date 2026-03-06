using App.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace App.StaticTools;

public static class QueryRefiner
{
	/*! Apply boundaries to query, allowing for pagination. */
	public static IQueryable<T> Bound<T>(
        IQueryable<T> query, int? offset = null, int? limit = null)
	{
		if (offset is not null)
        {
            query = query.Skip((int) offset);
        }
        if (limit is not null)
        {
            query = query.Take((int) limit);
        }
        return query;
	}

    public static IQueryable<Formula> Formulas(
        IQueryable <Formula> query,
        string? name = null,
        int? offset = null,
        int? limit = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(f => EF.Functions.Like(f.Name, $"%{name}%"));
        }
        return QueryRefiner.Bound(query, offset, limit);
    }

    public static IQueryable<Competition> Competitions(
        IQueryable <Competition> query,
        int? formulaId = null,
        string? name = null,
        bool activeOnly = false,
        int? offset = null,
        int? limit = null)
    {
        if (formulaId is not null)
        {
            query = query.Where(c => c.FormulaID == formulaId);
        }
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{name}%"));
        }
        if (activeOnly)
        {
            query = query.Where(c => c.Active);
        }
        return QueryRefiner.Bound(query, offset, limit);
    }

    public static IQueryable<Game> Games(
        IQueryable <Game> query,
        int? competitionId = null,
        string? appUserId = null,
        string? name = null,
        bool publicOnly = false,
        int? offset = null,
        int? limit = null)
    {
        if (competitionId is not null)
        {
            query = query.Where(g => g.CompetitionID == competitionId);
        }
        if (!string.IsNullOrEmpty(appUserId))
        {
            query = query.Where(g => g.AppUserID == appUserId);
        }
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(g => EF.Functions.Like(g.Name, $"%{name}%"));
        }
        if (publicOnly)
        {
            query = query.Where(g => string.IsNullOrEmpty(g.Passcode));
        }
        return QueryRefiner.Bound(query, offset, limit);
    }

    public static IQueryable<Guess> Guesses(
        IQueryable <Guess> query,
        string? gameId = null,
        string? name = null,
        int? offset = null,
        int? limit = null)
    {
        if (!string.IsNullOrEmpty(gameId))
        {
            query = query.Where(g => g.GameID == gameId);
        }
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(g => EF.Functions.Like(g.Name, $"%{name}%"));
        }
        return QueryRefiner.Bound(query, offset, limit);
    }
}


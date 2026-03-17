using App.Globals;
using App.Infrastructure;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace App.Applications;

public class GameApp(IAppDbContext context, IAuthorizationService authService) : IGameApp
{
    public static readonly int MinSubSpan = 5;

    public async Task<Result<int>> CountAsync(int? competitionId, string? name, bool publicOnly)
    {
        var query = QueryRefiner.Games(context.Game, competitionId, null, name, publicOnly);
        return await query.CountAsync();
    }

    public async Task<Result<int>> CountPersonalAsync(
        ClaimsPrincipal user, int? competitionId, string? name, bool publicOnly)
    {
        var query = QueryRefiner.Games(
            context.Game, competitionId, user.GetUserID(), name, publicOnly);
        return await query.CountAsync();
    }

    public async Task<Result<GameView>> ReadOneAsync(string id)
    {
        var game = await context.Game
            .Include(g => g.AppUser)
            .Include(g => g.Competition)
            .FirstOrDefaultAsync(g => g.ID == id);
        if (game is null)
        {
            return Result.Failure<GameView>(GameErrors.NotFound());
        }

        return new GameView(game);
    }

    public async Task<Result<List<SimpleGameView>>> ReadManyAsync(
        int? competitionId, string? userId, string? name, bool publicOnly, int? offset, int? limit)
    {
        var query = QueryRefiner.Games(
            context.Game, competitionId, userId, name, publicOnly, offset, limit);
        var result = await query
            .Include(g => g.Competition)
            .Include(g => g.AppUser)
            .Select(g => new SimpleGameView(g))
            .ToListAsync();
        return result;
    }

    public async Task<Result<List<SimpleGameView>>> ReadManyPersonalAsync(
        ClaimsPrincipal user,
        int? competitionId,
        string? name,
        bool publicOnly,
        int? offset,
        int? limit)
    {
        var query = QueryRefiner.Games(
            context.Game, competitionId, user.GetUserID(), name, publicOnly, offset, limit);
        var result = await query
            .Include(g => g.Competition)
            .Include(g => g.AppUser)
            .Select(g => new SimpleGameView(g))
            .ToListAsync();
        return result;
    }

    public async Task<Result> UpdateAsync(string id, GameDto dto, ClaimsPrincipal user)
    {
        var game = await context.Game.FindAsync(id);
        if (game is null)
        {
            return Result.Failure(GameErrors.NotFound());
        }

        if (!(await UserCanUpdateAsync(user, game)))
        {
            return Result.Failure(GameErrors.CannotUpdate());
        }

        if (!DeadlineUpdateIsValid(dto.SubsDeadline, game.SubsDeadline))
        {
            return Result.Failure(GameErrors.TooEarlySubsDeadline());
        }

        // Available updates.
        game.Description = dto.Description;
        game.MaxGuessCount = dto.MaxGuessCount;
        game.Name = dto.Name;
        game.Passcode = dto.Passcode;
        game.SubsDeadline = dto.SubsDeadline;

        context.Entry(game).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (ItemExists(id))
            {
                return Result.Failure<GameView>(GameErrors.UpdateConflict());
            }
            return Result.Failure<GameView>(GameErrors.NotFound());
        }

        return Result.Success();
    }

    public async Task<Result<GameView>> CreateAsync(GameDto dto, ClaimsPrincipal user)
    {
        var competition = await context.Competition
            .Include(c => c.Formula)
            .FirstOrDefaultAsync(c => c.ID == dto.CompetitionID);
        if (!CompetitionIsValid(competition))
        {
            return Result.Failure<GameView>(GameErrors.CompetitionNotFound());
        }

        var dateTimeNow = DateTime.Now;
        if (!DeadlineInitDefinitionIsValid(dto.SubsDeadline, dateTimeNow))
        {
            return Result.Failure<GameView>(GameErrors.TooEarlySubsDeadline());
        }

        if (!RulesMatchTemplate(dto.ScoringRules, competition!.Formula.DataTemplate))
        {
            return Result.Failure<GameView>(GameErrors.UnfitScoringRules());
        }

        var compData = JsonDocument.Parse(competition.Data);
        var game = new Game
        {
            AppUserID = user.GetUserID(),
            CompetitionID = dto.CompetitionID,
            Creation = dateTimeNow,
            Description = dto.Description,
            ID = DataGen.StringID(),
            MaxScore = GuessScorer.Evaluate(compData, compData, dto.ScoringRules),
            MaxGuessCount = dto.MaxGuessCount,
            Name = dto.Name,
            Passcode = dto.Passcode,
            ScoringRules = JsonSerializer.Serialize(dto.ScoringRules),
            SubsDeadline = dto.SubsDeadline
        };
        context.Game.Add(game);
        await context.SaveChangesAsync();

        // Load owner AppUser on context so view object can be built.
        AppUser gameOwner = (await context.AppUser.FindAsync(game.AppUserID))!;

        return new GameView(game);
    }

    public async Task<Result> RemoveAsync(string id, ClaimsPrincipal user)
    {
        var game = await context.Game.FindAsync(id);
        if (game is null)
        {
            return Result.Failure(GameErrors.NotFound());
        }

        if (!(await UserCanDeleteAsync(user, game)))
        {
            return Result.Failure(GameErrors.CannotDelete());
        }

        context.Game.Remove(game);
        await context.SaveChangesAsync();

        return Result.Success();
    }

    private bool ItemExists(string id)
    {
        return context.Game.Any(g => g.ID == id);
    }

    private bool DeadlineUpdateIsValid(DateTime? newDeadline, DateTime? currDeadline)
    {
        return newDeadline is null
            || newDeadline == currDeadline
            || newDeadline >= DateTime.Now.AddMinutes(MinSubSpan);
    }

    private bool DeadlineInitDefinitionIsValid(DateTime? deadline, DateTime nowReference)
    {
        return deadline is null || deadline >= nowReference.AddMinutes(MinSubSpan);
    }

    private bool CompetitionIsValid(Competition? competition)
    {
        return competition is not null && competition.Active;
    }

    private bool RulesMatchTemplate(JsonDocument rules, string temp)
    {
        return JsonDataChecker.ScoringRulesOnTemplate(rules, JsonDocument.Parse(temp));
    }

    // Check whether current user can delete game: only owner and staff are allowed.
    private async Task<bool> UserCanDeleteAsync(ClaimsPrincipal user, Game game)
    {
        var authCheck = await authService.AuthorizeAsync(user, game, Operations.Delete);
        return authCheck.Succeeded;
    }

    // Check whether current user can update game: only owner is allowed.
    private async Task<bool> UserCanUpdateAsync(ClaimsPrincipal user, Game game)
    {
        var authCheck = await authService.AuthorizeAsync(user, game, Operations.Update);
        return authCheck.Succeeded;
    }
}


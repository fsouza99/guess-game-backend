using App.Globals;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace App.Infrastructure;

public class GameAuthorization(IAuthorizationService authService) : IEntityAuthorization<Game>
{
    // Check whether current user can delete game: only owner and staff are allowed.
    public async Task<bool> UserCanDeleteAsync(ClaimsPrincipal user, Game game)
    {
        var authCheck = await authService.AuthorizeAsync(user, game, Operations.Delete);
        return authCheck.Succeeded;
    }

    // Check whether current user can update game: only owner is allowed.
    public async Task<bool> UserCanUpdateAsync(ClaimsPrincipal user, Game game)
    {
        var authCheck = await authService.AuthorizeAsync(user, game, Operations.Update);
        return authCheck.Succeeded;
    }
}

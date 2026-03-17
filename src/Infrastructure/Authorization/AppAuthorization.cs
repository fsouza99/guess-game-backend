using App.Globals;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace App.Infrastructure;

public class AppAuthorization(IAuthorizationService authService) : IAppAuthorization
{
    // Check whether current user can delete game: only owner and staff are allowed.
    public async Task<bool> UserCanDeleteGameAsync(ClaimsPrincipal user, Game game)
    {
        var authCheck = await authService.AuthorizeAsync(user, game, Operations.Delete);
        return authCheck.Succeeded;
    }

    // Check whether current user can update game: only owner is allowed.
    public async Task<bool> UserCanUpdateGameAsync(ClaimsPrincipal user, Game game)
    {
        var authCheck = await authService.AuthorizeAsync(user, game, Operations.Update);
        return authCheck.Succeeded;
    }
}

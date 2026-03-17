using App.Models;
using System.Security.Claims;

namespace App.Globals;

public interface IAppAuthorization
{
	Task<bool> UserCanDeleteGameAsync(ClaimsPrincipal user, Game game);
	Task<bool> UserCanUpdateGameAsync(ClaimsPrincipal user, Game game);
}

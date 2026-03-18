using System.Security.Claims;

namespace App.Globals;

public interface IEntityAuthorization<T> where T : class
{
	Task<bool> UserCanDeleteAsync(ClaimsPrincipal user, T entity);
	Task<bool> UserCanUpdateAsync(ClaimsPrincipal user, T entity);
}

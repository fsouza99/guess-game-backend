using System.Security.Claims;

namespace App.StaticTools;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserID(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            throw new ApplicationException("User ID is unavailable.");
        }

        return (string) userId;
    }
}

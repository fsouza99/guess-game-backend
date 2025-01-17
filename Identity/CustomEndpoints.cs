using App.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace App.Identity.Endpoints;

public static class CustomEndpoints
{
    // Adds extra endpoints to Identity, addressing particular needs in account management.
    public static void MapExtraIdentityEndpoints(this WebApplication app)
    {
        // Allows the user to set his nickname.
        app.MapPost(
            "/manage/nickname",
            async (
                ClaimsPrincipal claimsPrincipal,
                UserManager<AppUser> userManager,
                [FromBody] string nickname) =>
            {
                var user = await userManager.GetUserAsync(claimsPrincipal);
                if (user is null)
                {
                    return Results.NotFound();
                }
                
                user.Nickname = nickname;
                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return Results.BadRequest("Nickname update failed.");
                }
                
                return Results.Ok();
            }
        ).WithOpenApi().RequireAuthorization();

        // Allows the user to set his email and have the username updated accordingly.
        app.MapPost(
            "/manage/email",
            async (
                ClaimsPrincipal claimsPrincipal,
                UserManager<AppUser> userManager,
                [FromBody] string email) =>
            {
                var user = await userManager.GetUserAsync(claimsPrincipal);
                if (user is null)
                {
                    return Results.NotFound();
                }

                // "UserManager" methods update storage on their own, so we don't use them here.
                user.Email = email;
                user.UserName = email; // Allows user to log in using email.
                user.NormalizedUserName = userManager.NormalizeName(email);
                user.NormalizedEmail = userManager.NormalizeEmail(email);
                
                // Update all or nothing.
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return Results.BadRequest("User update failed.");
                }
                
                return Results.Ok();
            }
        ).WithOpenApi().RequireAuthorization();

        // Allows the user to retrieve his profile data.
        app.MapGet(
            "/manage/profile",
            async (
                ClaimsPrincipal claimsPrincipal,
                UserManager<AppUser> userManager) =>
            {
                var user = await userManager.GetUserAsync(claimsPrincipal);
                if (user is null)
                {
                    return Results.NotFound();
                }
                var profile = new
                {
                    ID = user.Id,
                    Nickname = user.Nickname,
                    Email = user.Email
                };

                return Results.Ok(profile);
            }
        ).WithOpenApi().RequireAuthorization();

        // Allows the user to log out from his account.
        app.MapPost(
            "/logout",
            async (
                SignInManager<AppUser> signInManager,
                [FromBody] object empty) =>
            {
                if (empty is null)
                {
                    return Results.Unauthorized();
                }
                await signInManager.SignOutAsync();
                return Results.Ok();
            }
        ).WithOpenApi().RequireAuthorization();

        // Allows the user to delete his own account.
        app.MapDelete(
            "/delete",
            async (
                ClaimsPrincipal claimsPrincipal,
                SignInManager<AppUser> signInManager,
                UserManager<AppUser> userManager,
                [FromBody] object empty) =>
            {
                if (empty is null)
                {
                    return Results.Unauthorized();
                }
                var user = await userManager.GetUserAsync(claimsPrincipal);
                if (user is null)
                {
                    return Results.NotFound();
                }
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    // Necessary to make any new logout requests fail.
                    await signInManager.SignOutAsync();

                    return Results.NoContent();
                }
                return Results.BadRequest();
            }
        ).WithOpenApi().RequireAuthorization();
    }
}
using App.Globals;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace App.Api;

public static class CustomEndpoints
{
    // Add extra endpoints to Identity, addressing particular needs in account management.
    public static void MapExtraIdentityEndpoints(this WebApplication app)
    {
        app.MapGetProfileEndpoint();
        app.MapPostAppUser();
        app.MapPostDeleteAccountEndpoint();
        app.MapPostEmailEndpoint();
        app.MapPostLogoutEndpoint();
        app.MapPostNicknameEndpoint();
    }

    // Add endpoint to register an account using email, nickname and password.
    public static void MapPostAppUser(this WebApplication app)
    {
        app.MapPost("/register/appuser", async (
            [FromBody] AppUserDto dto,
            UserManager<AppUser> userManager) =>
        {
            var user = new AppUser
            {
                Nickname = DataGen.AppUserNick(),
                Email = dto.Email,
                EmailConfirmed = true,
                UserName = dto.Email
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }

            return Results.Created($"/manage/profile", new { user.Id, user.Email });
        });
    }

    // Add endpoint to set personal nickname.
    public static void MapPostNicknameEndpoint(this WebApplication app)
    {
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
    }

    // Add endpoint to set email and have the username updated accordingly.
    public static void MapPostEmailEndpoint(this WebApplication app)
    {
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
                user.UserName = email; // Allow user to log in using email.
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
    }

    // Add endpoint to retrieve personal profile data.
    public static void MapGetProfileEndpoint(this WebApplication app)
    {
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
    }

    // Add endpoint to log out from personal account.
    public static void MapPostLogoutEndpoint(this WebApplication app)
    {
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
    }

    // Add endpoint to delete personal account.
    public static void MapPostDeleteAccountEndpoint(this WebApplication app)
    {
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
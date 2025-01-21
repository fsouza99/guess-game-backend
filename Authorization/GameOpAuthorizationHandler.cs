using App.Authorization.References;
using App.Authorization.Requirements;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;

namespace App.Authorization.Handlers;

public class GameOpAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Game>
{
	protected override Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		OperationAuthorizationRequirement requirement,
		Game resource)
	{
		var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is null)
		{
			context.Fail();
		}
		else if (requirement.Name == Operations.Delete.Name)
		{
			if (
				context.User.IsInRole(RoleReference.Admin) ||
				context.User.IsInRole(RoleReference.Staff) ||
				resource.AppUserID == userId
				)
			{
				context.Succeed(requirement);
			}
		}
		else if (
			requirement.Name == Operations.Update.Name &&
			resource.AppUserID == userId)
		{
			context.Succeed(requirement);
		}
		return Task.CompletedTask;
	}
}

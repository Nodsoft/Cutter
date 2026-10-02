using Microsoft.AspNetCore.Authorization;
using Nodsoft.Cutter.Server.Data.Models;

namespace Nodsoft.Cutter.Server.Infrastructure.Authorization;

/// <summary>
/// Provides a handler for the <see cref="OwnLinksRequirement"/>.
/// </summary>
public sealed class OwnLinksRequirementHandler : AuthorizationHandler<OwnLinksRequirement, Link>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnLinksRequirement requirement, Link resource)
    {
        // Evaluate the user as an administrator.
        if (context.User.IsInRole("Administrator"))
        {
            context.Succeed(requirement);
        }
        
        // Evaluate the user as the creator of the link.
        if (context.User.Identity?.Name == resource.CreatedBy.Username)
        {
            context.Succeed(requirement);
        }
        
        context.Fail();
    }
}
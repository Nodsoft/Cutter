using Microsoft.AspNetCore.Authorization;
using Nodsoft.Cutter.Server.Services;

namespace Nodsoft.Cutter.Server.Infrastructure.Authorization;

public class AccountEnabledRequirementHandler : AuthorizationHandler<AccountEnabledRequirement>
{
    private readonly UserService _userService;

    public AccountEnabledRequirementHandler(UserService userService)
    {
        _userService = userService;
    }
    
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountEnabledRequirement requirement)
    {
        if (await _userService.GetUserAsync(context.User.Identity?.Name ?? "") is { IsBlocked: false })
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
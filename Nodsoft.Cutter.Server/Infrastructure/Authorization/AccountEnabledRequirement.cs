using Microsoft.AspNetCore.Authorization;

namespace Nodsoft.Cutter.Server.Infrastructure.Authorization;

/// <summary>
/// Requires that the current user's account is enabled to access the resource.
/// </summary>
public class AccountEnabledRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountEnabledRequirement"/> class.
    /// </summary>
    public AccountEnabledRequirement() { }
}
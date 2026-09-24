using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Nodsoft.Cutter.Server.Infrastructure.Authorization;

/// <summary>
/// Specifies that the current user must be the creator of the link to access the resource.
/// </summary>
/// <remarks>
/// Administrators can bypass this requirement.
/// </remarks>
[UsedImplicitly]
public sealed class OwnLinksRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OwnLinksRequirement"/> class.
    /// </summary>
    public OwnLinksRequirement() { }
}
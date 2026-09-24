namespace Nodsoft.Cutter.Server.Infrastructure.Authorization;

/// <summary>
/// Provides a collection of authorization policies.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Requires the current user's account to be enabled.
    /// </summary>
    /// <seealso cref="AccountEnabledRequirement"/>
    public const string AccountEnabled = "AccountEnabled";

    /// <summary>
    /// Requires the current user to be the creator of the link or a platform administrator.
    /// </summary>
    /// <seealso cref="OwnLinksRequirement"/>
    public const string OwnLinks = "OwnLinks";
}
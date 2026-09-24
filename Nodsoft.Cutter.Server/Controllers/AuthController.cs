using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Nodsoft.Cutter.Server.Data.Models;
using Nodsoft.Cutter.Server.Services;
using OpenIddict.Abstractions;
using OpenIddict.Client.WebIntegration;

namespace Nodsoft.Cutter.Server.Controllers;

/// <summary>
/// Responsible for handling authentication and authorization requests.
/// </summary>
[ApiController]
public sealed class AuthController : ControllerBase
{
    private readonly UserService _userService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    public AuthController(UserService userService, LinksService linksService)
    {
        _userService = userService;
    }
    
    /// <summary>
    /// Handles authentication challenges.
    /// </summary>
    /// <returns>A challenge result.</returns>
    [HttpGet, Route("challenge")]
    public new ChallengeResult Challenge() => base.Challenge(OpenIddictClientWebIntegrationConstants.Providers.GitHub);
    
    /// <summary>
    /// Provides a callback for authenticating via GitHub.
    /// </summary>
    [HttpGet, HttpPost, Route("callback/login/github")]
    public async Task<ActionResult> LoginCallbackAsync()
    {
        // Retrieve the authorization data validated by OpenIddict as part of the callback handling.
        AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictClientWebIntegrationConstants.Providers.GitHub);

        // Build an identity based on the external claims and that will be used to create the authentication cookie.
        ClaimsIdentity identity = new(result.Principal!.Claims, "ExternalLogin");

        // Retrieve and refresh the user information from the database.
        User user = await _userService.RefreshUserAsync(result.Principal, HttpContext);

        if (user.IsBlocked)
        {
            return Forbid();
        }
        
        // By default, OpenIddict will automatically try to map the email/name and name identifier claims from
        // their standard OpenID Connect or provider-specific equivalent, if available. If needed, additional
        // claims can be resolved from the external identity and copied to the final authentication cookie.
        identity.SetClaim(ClaimTypes.Email, result.Principal!.GetClaim(ClaimTypes.Email))
            .SetClaim(ClaimTypes.Name, result.Principal!.GetClaim(ClaimTypes.Name))
            .SetClaim(ClaimTypes.NameIdentifier, result.Principal!.GetClaim(ClaimTypes.NameIdentifier));
        
        // Attach roles
        identity.SetClaims(ClaimTypes.Role, [..user.Roles.Select(role => role.Name)]);

        // Preserve the registration details to be able to resolve them later.
        identity.SetClaim(OpenIddictConstants.Claims.Private.RegistrationId, result.Principal!.GetClaim(OpenIddictConstants.Claims.Private.RegistrationId))
            .SetClaim(OpenIddictConstants.Claims.Private.ProviderName, result.Principal!.GetClaim(OpenIddictConstants.Claims.Private.ProviderName));

        // Build the authentication properties based on the properties that were added when the challenge was triggered.
        AuthenticationProperties properties = new(result.Properties?.Items ?? throw new InvalidOperationException())
        {
            RedirectUri = result.Properties.RedirectUri ?? "/"
        };

        // Ask the default sign-in handler to return a new cookie and redirect the
        // user agent to the return URL stored in the authentication properties.
        //
        // For scenarios where the default sign-in handler configured in the ASP.NET Core
        // authentication options shouldn't be used, a specific scheme can be specified here.
        return SignIn(new(identity), properties);
    }
    
    /// <summary>
    /// Handles logging out of the application.
    /// </summary>
    /// <returns>A sign-out result.</returns>
    [HttpGet, Route("logout")]
    public async Task<RedirectResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }
}
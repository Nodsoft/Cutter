using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nodsoft.Cutter.Server.Data.Models;
using Nodsoft.Cutter.Server.Infrastructure.Authorization;
using Nodsoft.Cutter.Server.Services;

namespace Nodsoft.Cutter.Server.Controllers;

/// <summary>
/// Provides functionality for managing and accessing cutter links.
/// </summary>
[ApiController]
public sealed class LinksController : ControllerBase
{
    private readonly LinksService _linksService;
    private readonly UserService _userService;

    public LinksController(LinksService linksService, UserService userService)
    {
        _linksService = linksService;
        _userService = userService;
    }
    
    /// <summary>
    /// Redirects to the destination URL of a link.
    /// </summary>
    /// <param name="id">The fragment of the link to redirect to.</param>
    /// <returns>A redirect to the destination URL of the link.</returns>
    [HttpGet("go/{id}")]
    public async ValueTask<ActionResult> RedirectToLink([FromRoute] string id)
    {
        Link? link = await _linksService.GetLinkAsync(id);

        if (link is not { IsBlocked: false, IsDisabled: false })
        {
            return NotFound();
        }

        Uri uri = new(link.Destination, UriKind.Absolute);
        return RedirectPermanent(uri.AbsoluteUri);
    }
    
    /// <summary>
    /// Inserts a new link into the database.
    /// </summary>
    /// <param name="name">The fragment of the link to insert.</param>
    /// <param name="destination">The destination URL of the link to insert.</param>
    /// <returns>The newly inserted link.</returns>
    [HttpPost("create"), Authorize(policy: AuthorizationPolicies.AccountEnabled)]
    public async ValueTask<IActionResult> CreateLink(
        [FromQuery] string name, 
        [FromQuery] string destination
    ) {
        if (await _userService.GetUserAsync(User.Identity?.Name ?? "") is { IsBlocked: true })
        {
            return Forbid();
        }
        
        Link link = await _linksService.InsertLinkAsync(name, destination);
        return Created($"/go/{link.Name}", link);
    }
    
    /// <summary>
    /// Disables a link in the database.
    /// </summary>
    /// <param name="id">The fragment of the link to disable.</param>
    /// <returns>The disabled link.</returns>
    [HttpDelete("disable/{id}"), Authorize(policy: $"{AuthorizationPolicies.AccountEnabled},{AuthorizationPolicies.OwnLinks}")]
    public async ValueTask<Link> DisableLink([FromRoute] string id)
    {
        return await _linksService.DisableLinkAsync(id);
    }
}
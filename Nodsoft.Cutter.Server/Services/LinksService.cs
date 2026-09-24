using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nodsoft.Cutter.Server.Data;
using Nodsoft.Cutter.Server.Data.Models;
using Nodsoft.Cutter.Server.Infrastructure.Configuration;

namespace Nodsoft.Cutter.Server.Services;

/// <summary>
/// Provides functionality for managing shortened links.
/// </summary>
public sealed class LinksService
{
    private readonly CutterDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsSnapshot<CutterConfiguration> _cutterConfig;
    private readonly ILogger<LinksService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinksService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context for the Cutter API.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor for the current request.</param>
    /// <param name="cutterConfig">The configuration for the Cutter API.</param>
    /// <param name="logger">The logger for the links service.</param>
    public LinksService(
        CutterDbContext dbContext, 
        IHttpContextAccessor httpContextAccessor, 
        IOptionsSnapshot<CutterConfiguration> cutterConfig,
        ILogger<LinksService> logger
    ) {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _cutterConfig = cutterConfig;
        _logger = logger;
    }

    /// <summary>
    /// Gets all links in the database.
    /// </summary>
    /// <returns>An enumerable collection of all links in the database.</returns>
    public IQueryable<Link> GetLinks() => _dbContext.Links.AsNoTrackingWithIdentityResolution()
        .Include(l => l.CreatedBy);
    
    /// <summary>
    /// Gets a link by its fragment.
    /// </summary>
    /// <param name="name">The fragment of the link to get.</param>
    /// <returns>The link with the specified fragment, or <see langword="null"/> if no link was found.</returns>
    public async ValueTask<Link?> GetLinkAsync(string name) => await _dbContext.Links.FirstOrDefaultAsync(l => l.Name == name);
    
    /// <summary>
    /// Determines if a specified link is owned by a specified user.
    /// </summary>
    /// <param name="linkId">The identifier of the link to check ownership of.</param>
    /// <param name="uid">The identifier of the user to check ownership for.</param>
    /// <returns><see langword="true"/> if the user owns the link; otherwise <see langword="false"/>.</returns>
    public async ValueTask<bool> IsLinkOwnedByUserAsync(Guid linkId, uint uid) 
        => await _dbContext.Links.AsNoTracking().AnyAsync(l => l.Id == linkId && l.CreatedBy.Id == uid);

    /// <summary>
    /// Inserts a new link into the database.
    /// </summary>
    /// <param name="name">The fragment of the link to insert.</param>
    /// <param name="destination">The destination URL of the link to insert.</param>
    /// <returns>The newly inserted link.</returns>
    public async ValueTask<Link> InsertLinkAsync(string? name, string destination)
    {
        uint uid = uint.Parse(_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("The user is not authenticated."));
        User user = await _dbContext.Users.FindAsync(uid) ?? throw new InvalidOperationException("The user does not exist.");
        
        if (name is not null && await _dbContext.Links.AnyAsync(l => l.Name == name))
        {
            _logger.LogWarning("User {Uid} attempted to create a link with the name {Name}, but the name is already in use for destination {Destination}", uid, name, destination);
            throw new InvalidOperationException("The link already exists.");
        }

        if (await _dbContext.Links.FirstOrDefaultAsync(l => l.Destination == destination) is { } existing)
        {
            _logger.LogInformation("User {Uid} attempted to create a link with destination {Destination}, but the destination already exists under another name ({Name})", uid, destination, existing.Name);
            return existing;
        }
        
        Link link = new()
        {
            Id = Guid.NewGuid(),
            Name = name is null or "" ? Base62Generator.GenerateString(6) : name,
            Destination = destination,
            CreatedBy = user,
            CreatedFromIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress ?? throw new InvalidOperationException("The remote IP address is missing.")
        };
        
        _dbContext.Links.Add(link);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("User {Uid} created link {LinkId} with name {Name} and destination {Destination}", link.CreatedBy.Id, link.Id, link.Name, link.Destination);
        return link;
    }
    
    /// <summary>
    /// Disables a link in the database.
    /// </summary>
    /// <param name="name">The fragment of the link to disable.</param>
    /// <returns>The disabled link.</returns>
    public async ValueTask<Link> DisableLinkAsync(string name)
    {
        Link link = await GetLinkAsync(name) ?? throw new InvalidOperationException("The link does not exist.");
        link.IsDisabled = true;
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Disabled link {LinkId} with name {Name} and destination {Destination}", link.Id, link.Name, link.Destination);
        return link;
    }
    
    /// <summary>
    /// Enables a link in the database.
    /// </summary>
    /// <param name="name">The fragment of the link to enable.</param>
    /// <returns>The enabled link.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the link does not exist.</exception>
    public async ValueTask<Link> EnableLinkAsync(string name)
    {
        Link link = await GetLinkAsync(name) ?? throw new InvalidOperationException("The link does not exist.");
        link.IsDisabled = false;
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Enabled link {LinkId} with name {Name} and destination {Destination}", link.Id, link.Name, link.Destination);
        return link;
    }
    
    /// <summary>
    /// Blocks a Link in the database.
    /// </summary>
    /// <param name="name">The fragment of the link to block.</param>
    /// <returns>The blocked link.</returns>
    public async ValueTask<Link> BlockLinkAsync(string name)
    {
        Link link = await GetLinkAsync(name) ?? throw new InvalidOperationException("The link does not exist.");
        link.IsBlocked = true;
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Blocked link {LinkId} with name {Name} and destination {Destination}", link.Id, link.Name, link.Destination);
        return link;
    }
    
    /// <summary>
    /// Gets the redirect URI for a specified link.
    /// </summary>
    /// <param name="link">The link to get the redirect URI for.</param>
    /// <returns>The redirect URI for the specified link.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the link domain is invalid.</exception>
    public Uri GetLinkUri(Link link) => Uri.TryCreate(new(_cutterConfig.Value.LinksDomain), link.Name, out Uri? uri) 
        ? uri 
        : throw new InvalidOperationException("The link domain is invalid.");
}
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nodsoft.Cutter.Server.Data;
using Nodsoft.Cutter.Server.Data.Models;
using OpenIddict.Abstractions;

namespace Nodsoft.Cutter.Server.Services;

/// <summary>
/// Provides functionality for managing users.
/// </summary>
public sealed class UserService
{
    private readonly CutterDbContext _dbContext;

    public UserService(CutterDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to get.</param>
    /// <returns>The user with the specified identifier, or <see langword="null"/> if no user was found.</returns>
    public async ValueTask<User?> GetUserAsync(uint id) => await _dbContext.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
    
    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    /// <returns>The user with the specified username, or <see langword="null"/> if no user was found.</returns>
    public async ValueTask<User?> GetUserAsync(string username) => await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

    /// <summary>
    /// Inserts or refreshes a user in the database.
    /// </summary>
    /// <param name="authUser">The user to insert or refresh.</param>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    public async ValueTask<User> RefreshUserAsync(ClaimsPrincipal authUser, HttpContext httpContext)
    {
        uint id = uint.Parse(authUser.GetClaim(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("The user ID claim is missing."));
        
        if (await GetUserAsync(id) is not { } user)
        {
            user = new()
            {
                Id = id,
            };
            
            _dbContext.Users.Add(user);
        }

        user.Username = authUser.GetClaim("login") ?? throw new InvalidOperationException("The username claim is missing.");
        user.MapFromClaims(authUser.Claims);
        
        IPAddress ip = httpContext.Connection.RemoteIpAddress ?? throw new InvalidOperationException("The remote IP address is missing.");
        
        if (!user.IpAddresses.Contains(ip))
        {
            user.IpAddresses.Add(ip);
        }
        
        await _dbContext.SaveChangesAsync();
        return user;
    }
    
    /// <summary>
    /// Disables a user in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the user to disable.</param>
    /// <returns>The disabled user.</returns>
    public async ValueTask<User> DisableUserAsync(uint id)
    {
        User user = await GetUserAsync(id) ?? throw new InvalidOperationException("The user does not exist.");
        user.IsBlocked = true;
        await _dbContext.SaveChangesAsync();
        return user;
    }
}
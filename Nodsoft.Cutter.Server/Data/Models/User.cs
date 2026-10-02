using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nodsoft.Cutter.Server.Infrastructure.Serialization;

namespace Nodsoft.Cutter.Server.Data.Models;

/// <summary>
/// Represents a user of the Cutter API.
/// </summary>
[Index(nameof(Username), IsUnique = true)]
public sealed class User : IDisposable
{
    /// <summary>
    /// The GitHub user ID of the user.
    /// </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public uint Id { get; set; }

    /// <summary>
    /// The username of the user (as defined on GitHub).
    /// </summary>
    [Required, MaxLength(64)]
    public string Username { get; set; } = "";

    /// <summary>
    /// The user's assigned roles.
    /// </summary>
    public List<Role> Roles { get; set; } = [];
    
    /// <summary>
    /// The date and time the user was created.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>
    /// The date and time the user was last updated / authenticated.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTimeOffset UpdatedAt { get; set; }
    
    /// <summary>
    /// The user's unique IP addresses used to authenticate to / access the API.
    /// </summary>
    [JsonConverter(typeof(IPAddressCollectionJsonConverter<List<IPAddress>>))]
    public List<IPAddress> IpAddresses { get; set; } = [];
    
    /// <summary>
    /// The user's raw object as returned by the GitHub API.
    /// </summary>
    /// <remarks>
    /// This is stored as a JSON string to allow for easy deserialization.
    /// </remarks>
    public JsonDocument RawObject { get; set; } = null!;

    /// <summary>
    /// Determines whether the user is able to use the API (blocking / unblocking).
    /// </summary>
    /// <remarks>
    /// This is used to prevent abuse of the API by users who have been blocked.
    /// </remarks>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// The links created by the user.
    /// </summary>
    /// <remarks>
    /// This is a navigation property that allows for easy access to the links created by the user.
    /// </remarks>
    [JsonIgnore]
    public List<Link> Links { get; set; } = [];
    
    public void Dispose()
    {
        RawObject.Dispose();
    }
    
    public void MapFromClaims(IEnumerable<Claim> claims)
    {
        RawObject = JsonDocument.Parse(JsonSerializer.Serialize(claims.ToDictionary(c => c.Type, c => c.Value)));
    }
}
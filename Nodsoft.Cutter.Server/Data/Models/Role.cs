namespace Nodsoft.Cutter.Server.Data.Models;

/// <summary>
/// Represents a role that can be assigned to a user.
/// </summary>
public class Role
{
    /// <summary>
    /// The unique identifier for the role.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// The name of the role.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The description of the role.
    /// </summary>
    public string Description { get; set; } = "";
}
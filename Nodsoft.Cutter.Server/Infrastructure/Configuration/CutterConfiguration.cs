namespace Nodsoft.Cutter.Server.Infrastructure.Configuration;

/// <summary>
/// Represents the configuration for the Cutter API.
/// </summary>
public class CutterConfiguration
{
    /// <summary>
    /// Gets the domain used for the cutter interface and API.
    /// </summary>
    public string CutterDomain { get; set; } = null!;
    
    /// <summary>
    /// Gets the domain used for links created by the cutter API.
    /// </summary>
    public string LinksDomain { get; set; } = null!;
}
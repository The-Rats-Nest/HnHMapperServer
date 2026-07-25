namespace HnHMapperServer.Core.Models;

/// <summary>
/// Represents grid data for a map tile
/// </summary>
public class GridData
{
    public string Id { get; set; } = string.Empty;
    public Coord Coord { get; set; } = new(0, 0);
    public DateTime NextUpdate { get; set; }
    public int Map { get; set; }
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Game world genus (version) this grid belongs to.
    /// </summary>
    public string Genus { get; set; } = string.Empty;
}

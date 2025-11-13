namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a physical location or building where fire safety inspections are conducted.
/// </summary>
public class Site
{
    /// <summary>
    /// Unique identifier for the site.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the site (e.g., "Main Office Building", "Warehouse 3").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Physical address of the site.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or notes about the site.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The tenant that owns this site.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Navigation property to the owning tenant.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    // Navigation properties
    public ICollection<FloorPlan> FloorPlans { get; set; } = new List<FloorPlan>();
    public ICollection<InspectionRound> InspectionRounds { get; set; } = new List<InspectionRound>();
}

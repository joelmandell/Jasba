namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a configurable type of fire safety equipment or inspection point.
/// Each tenant can define their own object types based on their specific needs.
/// </summary>
public class InspectionObjectType
{
    /// <summary>
    /// Unique identifier for the object type.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the object type (e.g., "6kg Fire Extinguisher", "Emergency Exit Sign").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional icon identifier for map display (e.g., "fire-extinguisher", "exit-sign").
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Optional color code for map markers (hex format, e.g., "#FF0000").
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// The tenant that owns this object type.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Navigation property to the owning tenant.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    // Navigation properties
    public ICollection<InspectionObject> InspectionObjects { get; set; } = new List<InspectionObject>();
}

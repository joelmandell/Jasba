namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a specific fire safety equipment item placed on a floor plan.
/// </summary>
public class InspectionObject
{
    /// <summary>
    /// Unique identifier for the inspection object.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The type of this inspection object.
    /// </summary>
    public Guid TypeId { get; set; }

    /// <summary>
    /// Navigation property to the object type.
    /// </summary>
    public InspectionObjectType Type { get; set; } = null!;

    /// <summary>
    /// Optional description or identifier for this specific object.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Normalized X coordinate (0.0 to 1.0) on the floor plan.
    /// Normalized coordinates are resolution-independent.
    /// </summary>
    public double NormalizedX { get; set; }

    /// <summary>
    /// Normalized Y coordinate (0.0 to 1.0) on the floor plan.
    /// Normalized coordinates are resolution-independent.
    /// </summary>
    public double NormalizedY { get; set; }

    /// <summary>
    /// The floor plan this object is placed on.
    /// </summary>
    public Guid FloorPlanId { get; set; }

    /// <summary>
    /// Navigation property to the parent floor plan.
    /// </summary>
    public FloorPlan FloorPlan { get; set; } = null!;

    // Navigation properties
    public ICollection<InspectionResult> InspectionResults { get; set; } = new List<InspectionResult>();
}

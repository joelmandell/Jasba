namespace SBAPro.Core.Entities;

/// <summary>
/// Represents the result of inspecting a specific object during an inspection round.
/// This is the core evidence of fire safety compliance.
/// </summary>
public class InspectionResult
{
    /// <summary>
    /// Unique identifier for the inspection result.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The inspection round this result belongs to.
    /// </summary>
    public Guid RoundId { get; set; }

    /// <summary>
    /// Navigation property to the inspection round.
    /// </summary>
    public InspectionRound Round { get; set; } = null!;

    /// <summary>
    /// The inspection object that was checked.
    /// </summary>
    public Guid ObjectId { get; set; }

    /// <summary>
    /// Navigation property to the inspection object.
    /// </summary>
    public InspectionObject Object { get; set; } = null!;

    /// <summary>
    /// The status determined during inspection.
    /// </summary>
    public InspectionStatus Status { get; set; }

    /// <summary>
    /// Optional comment or note about the inspection.
    /// Required if status is Deficient.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// When this specific object was inspected.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional photo evidence (stored as byte array or URL).
    /// DEPRECATED: Use Photos collection instead for multiple photos.
    /// </summary>
    public byte[]? PhotoData { get; set; }

    /// <summary>
    /// MIME type of the photo if present.
    /// DEPRECATED: Use Photos collection instead for multiple photos.
    /// </summary>
    public string? PhotoMimeType { get; set; }

    /// <summary>
    /// Collection of photos attached to this inspection result.
    /// </summary>
    public ICollection<InspectionPhoto> Photos { get; set; } = new List<InspectionPhoto>();
}

/// <summary>
/// Enumeration of possible inspection statuses for an object.
/// </summary>
public enum InspectionStatus
{
    /// <summary>
    /// The object has not been inspected yet in this round.
    /// </summary>
    NotInspected = 0,

    /// <summary>
    /// The object passed inspection with no issues.
    /// </summary>
    Ok = 1,

    /// <summary>
    /// The object has deficiencies that need attention.
    /// </summary>
    Deficient = 2,

    /// <summary>
    /// The object was not accessible for inspection.
    /// </summary>
    NotAccessible = 3
}

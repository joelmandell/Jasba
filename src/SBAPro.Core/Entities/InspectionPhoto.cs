namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a photo attached to an inspection result.
/// Allows multiple photos per inspection.
/// </summary>
public class InspectionPhoto
{
    /// <summary>
    /// Unique identifier for the photo.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The inspection result this photo belongs to.
    /// </summary>
    public Guid InspectionResultId { get; set; }

    /// <summary>
    /// Navigation property to the inspection result.
    /// </summary>
    public InspectionResult InspectionResult { get; set; } = null!;

    /// <summary>
    /// Photo data stored as byte array.
    /// </summary>
    public byte[] PhotoData { get; set; } = null!;

    /// <summary>
    /// MIME type of the photo (e.g., image/jpeg, image/png).
    /// </summary>
    public string PhotoMimeType { get; set; } = null!;

    /// <summary>
    /// When this photo was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional caption or description for the photo.
    /// </summary>
    public string? Caption { get; set; }
}

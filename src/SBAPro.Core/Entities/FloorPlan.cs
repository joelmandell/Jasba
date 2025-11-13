namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a floor plan image for a site.
/// Floor plans are used as the base layer for placing inspection objects.
/// </summary>
public class FloorPlan
{
    /// <summary>
    /// Unique identifier for the floor plan.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the floor plan (e.g., "Ground Floor", "Basement Level").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The floor plan image data stored as a byte array.
    /// In production, consider using cloud blob storage instead.
    /// </summary>
    public byte[]? ImageData { get; set; }

    /// <summary>
    /// MIME type of the image (e.g., "image/png", "image/jpeg").
    /// </summary>
    public string? ImageMimeType { get; set; }

    /// <summary>
    /// Original image width in pixels (for coordinate normalization).
    /// </summary>
    public int ImageWidth { get; set; }

    /// <summary>
    /// Original image height in pixels (for coordinate normalization).
    /// </summary>
    public int ImageHeight { get; set; }

    /// <summary>
    /// The site this floor plan belongs to.
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Navigation property to the parent site.
    /// </summary>
    public Site Site { get; set; } = null!;

    // Navigation properties
    public ICollection<InspectionObject> InspectionObjects { get; set; } = new List<InspectionObject>();
}

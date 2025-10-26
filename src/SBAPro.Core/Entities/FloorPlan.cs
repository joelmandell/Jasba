namespace SBAPro.Core.Entities;

public class FloorPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string ImageMimeType { get; set; } = string.Empty;
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public Guid SiteId { get; set; }
    public Site Site { get; set; } = null!;
    public ICollection<InspectionObject> InspectionObjects { get; set; } = new List<InspectionObject>();
}

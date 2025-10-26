namespace SBAPro.Core.Entities;

public class InspectionObject
{
    public Guid Id { get; set; }
    public Guid TypeId { get; set; }
    public InspectionObjectType Type { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public double NormalizedX { get; set; } // 0.0 to 1.0
    public double NormalizedY { get; set; } // 0.0 to 1.0
    public Guid FloorPlanId { get; set; }
    public FloorPlan FloorPlan { get; set; } = null!;
    public ICollection<InspectionResult> InspectionResults { get; set; } = new List<InspectionResult>();
}

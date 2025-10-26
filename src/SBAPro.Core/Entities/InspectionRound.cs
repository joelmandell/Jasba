namespace SBAPro.Core.Entities;

public class InspectionRound
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public Site Site { get; set; } = null!;
    public Guid FloorPlanId { get; set; }
    public FloorPlan FloorPlan { get; set; } = null!;
    public string InspectorId { get; set; } = string.Empty;
    public ApplicationUser Inspector { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "InProgress"; // InProgress, Completed
    public ICollection<InspectionResult> InspectionResults { get; set; } = new List<InspectionResult>();
}

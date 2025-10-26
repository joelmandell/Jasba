namespace SBAPro.Core.Entities;

public class InspectionResult
{
    public Guid Id { get; set; }
    public Guid RoundId { get; set; }
    public InspectionRound Round { get; set; } = null!;
    public Guid ObjectId { get; set; }
    public InspectionObject Object { get; set; } = null!;
    public string Status { get; set; } = "NotChecked"; // OK, Issue, NotChecked
    public string Comment { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

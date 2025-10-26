namespace SBAPro.Core.Entities;

public class Site
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public ICollection<FloorPlan> FloorPlans { get; set; } = new List<FloorPlan>();
    public ICollection<InspectionRound> InspectionRounds { get; set; } = new List<InspectionRound>();
}

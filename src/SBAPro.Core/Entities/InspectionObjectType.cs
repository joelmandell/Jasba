namespace SBAPro.Core.Entities;

public class InspectionObjectType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public ICollection<InspectionObject> InspectionObjects { get; set; } = new List<InspectionObject>();
}

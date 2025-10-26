namespace SBAPro.Core.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Site> Sites { get; set; } = new List<Site>();
    public ICollection<InspectionObjectType> InspectionObjectTypes { get; set; } = new List<InspectionObjectType>();
}

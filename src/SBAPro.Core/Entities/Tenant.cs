namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a customer organization in the multi-tenant system.
/// Each tenant has isolated data and their own users and sites.
/// </summary>
public class Tenant
{
    /// <summary>
    /// Unique identifier for the tenant.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the organization.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional logo URL for PDF reports and branding.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Date when the tenant account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the tenant account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Site> Sites { get; set; } = new List<Site>();
    public ICollection<InspectionObjectType> InspectionObjectTypes { get; set; } = new List<InspectionObjectType>();
    public ICollection<SecurityPlan> SecurityPlans { get; set; } = new List<SecurityPlan>();
}

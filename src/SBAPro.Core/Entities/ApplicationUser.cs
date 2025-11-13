using Microsoft.AspNetCore.Identity;

namespace SBAPro.Core.Entities;

/// <summary>
/// Extended user entity that includes tenant association.
/// Inherits from IdentityUser for ASP.NET Core Identity integration.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// The tenant this user belongs to.
    /// Nullable for SystemAdmin users who don't belong to a specific tenant.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Navigation property to the tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// User's full name for display purposes.
    /// </summary>
    public string? FullName { get; set; }
}

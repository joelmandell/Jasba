using Microsoft.AspNetCore.Identity;

namespace SBAPro.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public Guid? TenantId { get; set; } // Nullable for SystemAdmin
    public Tenant? Tenant { get; set; }
}

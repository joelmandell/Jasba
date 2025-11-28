
namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a security plan guideline document for a tenant.
/// Contains rich text content created with a WYSIWYG editor.
/// </summary>
public class SecurityPlan
{
    /// <summary>
    /// Unique identifier for the security plan.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title of the security plan guideline.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Rich HTML content of the security plan created with WYSIWYG editor.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or summary of the security plan.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The tenant that owns this security plan.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Navigation property to the owning tenant.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// When the security plan was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the security plan was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// User who created the plan (optional reference).
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User who last updated the plan (optional reference).
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Whether this plan is currently active/published.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

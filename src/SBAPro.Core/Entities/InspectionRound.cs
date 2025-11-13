namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a single inspection session where an inspector checks multiple objects.
/// </summary>
public class InspectionRound
{
    /// <summary>
    /// Unique identifier for the inspection round.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The site where this inspection is conducted.
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Navigation property to the site.
    /// </summary>
    public Site Site { get; set; } = null!;

    /// <summary>
    /// The user (inspector) conducting this round.
    /// </summary>
    public string InspectorId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the inspector.
    /// </summary>
    public ApplicationUser Inspector { get; set; } = null!;

    /// <summary>
    /// When the inspection round was started.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the inspection round was completed (null if still in progress).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Current status of the inspection round.
    /// </summary>
    public InspectionRoundStatus Status { get; set; } = InspectionRoundStatus.InProgress;

    /// <summary>
    /// Optional notes or summary for the entire round.
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public ICollection<InspectionResult> InspectionResults { get; set; } = new List<InspectionResult>();
}

/// <summary>
/// Enumeration of possible inspection round statuses.
/// </summary>
public enum InspectionRoundStatus
{
    /// <summary>
    /// The round has been started but not completed.
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// The round has been completed successfully.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// The round was cancelled before completion.
    /// </summary>
    Cancelled = 2
}

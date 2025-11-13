using SBAPro.Core.Entities;

namespace SBAPro.Core.Interfaces;

/// <summary>
/// Service for generating PDF reports from inspection data.
/// </summary>
public interface IReportGenerator
{
    /// <summary>
    /// Generates a PDF report for a completed inspection round.
    /// </summary>
    /// <param name="inspectionRound">The inspection round to generate a report for.</param>
    /// <returns>A byte array containing the PDF document.</returns>
    Task<byte[]> GenerateInspectionReportAsync(InspectionRound inspectionRound);

    /// <summary>
    /// Generates a summary PDF report for multiple inspection rounds (e.g., monthly summary).
    /// </summary>
    /// <param name="site">The site to generate the report for.</param>
    /// <param name="startDate">Start date for the report period.</param>
    /// <param name="endDate">End date for the report period.</param>
    /// <returns>A byte array containing the PDF document.</returns>
    Task<byte[]> GenerateSummaryReportAsync(Site site, DateTime startDate, DateTime endDate);
}

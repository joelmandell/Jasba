namespace SBAPro.Core.Interfaces;

public interface IReportGenerator
{
    Task<byte[]> GenerateInspectionReportAsync(Guid inspectionRoundId);
}

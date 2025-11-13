using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SBAPro.Core.Entities;
using SBAPro.Core.Interfaces;
using SBAPro.Infrastructure.Data;

namespace SBAPro.Infrastructure.Services;

public class QuestPdfReportGenerator : IReportGenerator
{
    private readonly ApplicationDbContext _context;

    public QuestPdfReportGenerator(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInspectionReportAsync(InspectionRound inspectionRound)
    {
        var round = inspectionRound;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text("Protokoll från egenkontroll")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(10);

                        // Summary section
                        col.Item().Text($"Anläggning: {round.Site.Name}").SemiBold();
                        col.Item().Text($"Adress: {round.Site.Address}");
                        col.Item().Text($"Kontrollant: {round.Inspector.UserName}");
                        col.Item().Text($"Datum: {round.StartedAt:yyyy-MM-dd HH:mm}");
                        col.Item().Text($"Status: {round.Status}");

                        var issueCount = round.InspectionResults.Count(r => r.Status == InspectionStatus.Deficient);
                        var okCount = round.InspectionResults.Count(r => r.Status == InspectionStatus.Ok);
                        
                        col.Item().Text($"Sammanfattning: {okCount} OK, {issueCount} Anmärkningar").SemiBold();

                        // Results table
                        col.Item().PaddingTop(20).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(3);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Objekt").SemiBold();
                                header.Cell().Element(CellStyle).Text("Typ").SemiBold();
                                header.Cell().Element(CellStyle).Text("Status").SemiBold();
                                header.Cell().Element(CellStyle).Text("Kommentar").SemiBold();
                            });

                            foreach (var result in round.InspectionResults.OrderBy(r => r.Timestamp))
                            {
                                table.Cell().Element(CellStyle).Text(result.Object.Description ?? "");
                                table.Cell().Element(CellStyle).Text(result.Object.Type.Name);
                                table.Cell().Element(CellStyle).Text(result.Status.ToString());
                                table.Cell().Element(CellStyle).Text(result.Comment ?? "");
                            }
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Sida ");
                        x.CurrentPageNumber();
                        x.Span(" av ");
                        x.TotalPages();
                        x.Span(" - Genererad: ");
                        x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateSummaryReportAsync(Site site, DateTime startDate, DateTime endDate)
    {
        var rounds = await _context.InspectionRounds
            .Include(r => r.Inspector)
            .Include(r => r.InspectionResults)
            .Where(r => r.SiteId == site.Id && r.StartedAt >= startDate && r.StartedAt <= endDate)
            .OrderBy(r => r.StartedAt)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text("Sammanfattning av egenkontroller")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Anläggning: {site.Name}").SemiBold();
                        col.Item().Text($"Adress: {site.Address}");
                        col.Item().Text($"Period: {startDate:yyyy-MM-dd} till {endDate:yyyy-MM-dd}");
                        col.Item().Text($"Antal kontroller: {rounds.Count}").SemiBold();

                        col.Item().PaddingTop(20).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Datum").SemiBold();
                                header.Cell().Element(CellStyle).Text("Kontrollant").SemiBold();
                                header.Cell().Element(CellStyle).Text("Status").SemiBold();
                                header.Cell().Element(CellStyle).Text("OK").SemiBold();
                                header.Cell().Element(CellStyle).Text("Anmärkningar").SemiBold();
                            });

                            foreach (var round in rounds)
                            {
                                var okCount = round.InspectionResults.Count(r => r.Status == InspectionStatus.Ok);
                                var issueCount = round.InspectionResults.Count(r => r.Status == InspectionStatus.Deficient);

                                table.Cell().Element(CellStyle).Text(round.StartedAt.ToString("yyyy-MM-dd"));
                                table.Cell().Element(CellStyle).Text(round.Inspector?.UserName ?? "N/A");
                                table.Cell().Element(CellStyle).Text(round.Status.ToString());
                                table.Cell().Element(CellStyle).Text(okCount.ToString());
                                table.Cell().Element(CellStyle).Text(issueCount.ToString());
                            }
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Sida ");
                        x.CurrentPageNumber();
                        x.Span(" av ");
                        x.TotalPages();
                        x.Span(" - Genererad: ");
                        x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
    }
}

# 03-Infrastructure-Layer: PDF Report Generation with QuestPDF

## Objective
Implement PDF report generation service using QuestPDF for creating inspection reports compliant with Swedish fire safety regulations (SBA 03:07).

## Prerequisites
- Completed: 03-infrastructure-layer/04-email-service.md
- Understanding of PDF generation concepts
- Understanding of Swedish text encoding (ÅÄÖ characters)
- QuestPDF NuGet package installed

## Overview

The PDF Report Generator provides:
1. Professional PDF reports for inspection rounds
2. Swedish language support (ÅÄÖ characters)
3. Structured layout compliant with SBA requirements
4. Data visualization (tables, summaries)
5. Page numbering and metadata

## Swedish Fire Safety Context

Reports must include:
- **Anläggning** (Site/Facility)
- **Kontrollant** (Inspector)
- **Datum** (Date)
- **Status** (OK / Anmärkning)
- **Kommentarer** (Comments)

Reference: Myndigheten för samhällsskydd och beredskap (MSB) - SBA 03:07

## Instructions

### 1. Install QuestPDF NuGet Package

```bash
cd src/SBAPro.Infrastructure
dotnet add package QuestPDF
```

### 2. Create QuestPdfReportGenerator Implementation

**File**: `src/SBAPro.Infrastructure/Services/QuestPdfReportGenerator.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SBAPro.Core.Interfaces;
using SBAPro.Infrastructure.Data;

namespace SBAPro.Infrastructure.Services;

/// <summary>
/// PDF report generator using QuestPDF for inspection reports.
/// </summary>
public class QuestPdfReportGenerator : IReportGenerator
{
    private readonly ApplicationDbContext _context;

    public QuestPdfReportGenerator(ApplicationDbContext context)
    {
        _context = context;
        
        // Set QuestPDF license (Community license is free)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generates a PDF report for a completed inspection round.
    /// </summary>
    public async Task<byte[]> GenerateInspectionReportAsync(Guid inspectionRoundId)
    {
        // Load inspection round with all related data
        var round = await _context.InspectionRounds
            .Include(r => r.Site)
            .Include(r => r.Inspector)
            .Include(r => r.InspectionResults)
                .ThenInclude(res => res.Object)
                    .ThenInclude(obj => obj.Type)
            .FirstOrDefaultAsync(r => r.Id == inspectionRoundId);

        if (round == null)
        {
            throw new ArgumentException("Inspection round not found", nameof(inspectionRoundId));
        }

        // Generate PDF document
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Page setup
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Header
                page.Header()
                    .Text("Protokoll från egenkontroll")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                // Content
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

                        // Count results
                        var issueCount = round.InspectionResults.Count(r => r.Status == "Issue");
                        var okCount = round.InspectionResults.Count(r => r.Status == "OK");
                        
                        col.Item().Text($"Sammanfattning: {okCount} OK, {issueCount} Anmärkningar").SemiBold();

                        // Results table
                        col.Item().PaddingTop(20).Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Objekt
                                columns.RelativeColumn(2); // Typ
                                columns.RelativeColumn(1); // Status
                                columns.RelativeColumn(3); // Kommentar
                            });

                            // Table header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Objekt").SemiBold();
                                header.Cell().Element(CellStyle).Text("Typ").SemiBold();
                                header.Cell().Element(CellStyle).Text("Status").SemiBold();
                                header.Cell().Element(CellStyle).Text("Kommentar").SemiBold();
                            });

                            // Table rows
                            foreach (var result in round.InspectionResults.OrderBy(r => r.Timestamp))
                            {
                                table.Cell().Element(CellStyle).Text(result.Object.Description);
                                table.Cell().Element(CellStyle).Text(result.Object.Type.Name);
                                table.Cell().Element(CellStyle).Text(result.Status);
                                table.Cell().Element(CellStyle).Text(result.Comment ?? "");
                            }
                        });
                    });

                // Footer with page numbers
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

        // Generate and return PDF as byte array
        return document.GeneratePdf();
    }

    /// <summary>
    /// Helper method to style table cells with borders and padding.
    /// </summary>
    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(5);
    }
}
```

### 3. Register ReportGenerator in DI

**File**: `src/SBAPro.WebApp/Program.cs`

```csharp
// Register Report Generator
builder.Services.AddScoped<IReportGenerator, QuestPdfReportGenerator>();
```

## Usage Examples

### Generate Report from Controller

```csharp
public class InspectionRoundsController : Controller
{
    private readonly IReportGenerator _reportGenerator;

    public InspectionRoundsController(IReportGenerator reportGenerator)
    {
        _reportGenerator = reportGenerator;
    }

    [HttpGet("download-report/{roundId}")]
    public async Task<IActionResult> DownloadReport(Guid roundId)
    {
        try
        {
            var pdfBytes = await _reportGenerator.GenerateInspectionReportAsync(roundId);
            
            return File(
                pdfBytes,
                "application/pdf",
                $"kontrollrapport_{roundId}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
}
```

### Generate Report from Blazor Page

```razor
@page "/inspection-rounds/{RoundId:guid}/report"
@inject IReportGenerator ReportGenerator
@inject IJSRuntime JS

<h3>Ladda ner rapport</h3>

<button class="btn btn-primary" @onclick="DownloadReport">
    <span class="oi oi-download"></span> Ladda ner PDF
</button>

@code {
    [Parameter]
    public Guid RoundId { get; set; }

    private async Task DownloadReport()
    {
        try
        {
            var pdfBytes = await ReportGenerator.GenerateInspectionReportAsync(RoundId);
            var base64 = Convert.ToBase64String(pdfBytes);
            
            await JS.InvokeVoidAsync("downloadPdf", base64, $"rapport_{RoundId}.pdf");
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }
}
```

Add JavaScript for download:

```javascript
// wwwroot/js/site.js
window.downloadPdf = (base64, filename) => {
    const link = document.createElement('a');
    link.href = 'data:application/pdf;base64,' + base64;
    link.download = filename;
    link.click();
};
```

## Customizing Report Layout

### Add Company Logo

```csharp
page.Header()
    .Row(row =>
    {
        row.RelativeItem().Column(col =>
        {
            col.Item().Text("Protokoll från egenkontroll")
                .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
        });
        
        row.ConstantItem(100).Image("path/to/logo.png");
    });
```

### Add Signature Section

```csharp
col.Item().PaddingTop(30).Column(signatureCol =>
{
    signatureCol.Item().BorderTop(1).BorderColor(Colors.Black);
    signatureCol.Item().Text("Underskrift kontrollant");
    signatureCol.Item().PaddingTop(10).Text($"Namn: {round.Inspector.UserName}");
    signatureCol.Item().Text($"Datum: {DateTime.Now:yyyy-MM-dd}");
});
```

### Add Color-Coded Status

```csharp
private static TextStyle GetStatusStyle(string status)
{
    return status switch
    {
        "OK" => TextStyle.Default.FontColor(Colors.Green.Medium).SemiBold(),
        "Issue" => TextStyle.Default.FontColor(Colors.Red.Medium).SemiBold(),
        _ => TextStyle.Default
    };
}

// Use in table:
table.Cell().Element(CellStyle).Text(result.Status).Style(GetStatusStyle(result.Status));
```

### Add Charts/Graphs

QuestPDF doesn't have built-in charting, but you can:
1. Generate chart images with a library (e.g., ScottPlot)
2. Embed images in the PDF

```csharp
var chart = new ScottPlot.Plot(400, 300);
chart.AddPie(new double[] { okCount, issueCount });
var imageBytes = chart.Render();

col.Item().Image(imageBytes);
```

## QuestPDF License

### Community License (Free)

✅ **Allowed**:
- Non-commercial use
- Hobbyist projects
- Educational purposes
- Evaluation

❌ **Not Allowed**:
- Commercial applications
- Generating revenue

### Professional License ($99/developer)

For commercial use, purchase at: https://www.questpdf.com/pricing.html

Configure in code:

```csharp
QuestPDF.Settings.License = LicenseType.Professional;
```

Or use environment variable:

```bash
export QUESTPDF_LICENSE_KEY="your-license-key"
```

## Swedish Character Support

QuestPDF automatically handles Swedish characters (ÅÄÖ) correctly. No special configuration needed.

Test with:

```csharp
col.Item().Text("Test: Å Ä Ö å ä ö");
```

Expected: Characters render correctly in PDF

## Performance Considerations

### Caching

For frequently accessed reports, consider caching:

```csharp
public class CachedReportGenerator : IReportGenerator
{
    private readonly IReportGenerator _innerGenerator;
    private readonly IMemoryCache _cache;

    public async Task<byte[]> GenerateInspectionReportAsync(Guid inspectionRoundId)
    {
        var cacheKey = $"report_{inspectionRoundId}";
        
        if (_cache.TryGetValue<byte[]>(cacheKey, out var cached))
        {
            return cached;
        }

        var report = await _innerGenerator.GenerateInspectionReportAsync(inspectionRoundId);
        
        _cache.Set(cacheKey, report, TimeSpan.FromMinutes(30));
        
        return report;
    }
}
```

### Async Generation

For large reports, generate asynchronously:

```csharp
public async Task<Guid> QueueReportGenerationAsync(Guid inspectionRoundId)
{
    var jobId = Guid.NewGuid();
    
    _ = Task.Run(async () =>
    {
        var report = await GenerateInspectionReportAsync(inspectionRoundId);
        await SaveReportAsync(jobId, report);
    });
    
    return jobId;
}
```

## Testing

### Unit Tests

```csharp
public class QuestPdfReportGeneratorTests
{
    [Fact]
    public async Task GenerateInspectionReportAsync_ValidRound_ReturnsPdfBytes()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        
        var context = new ApplicationDbContext(options, Mock.Of<ITenantService>());
        
        var round = new InspectionRound
        {
            Id = Guid.NewGuid(),
            Site = new Site { Name = "Test Site", Address = "Test Address" },
            Inspector = new ApplicationUser { UserName = "test@test.com" },
            StartedAt = DateTime.Now,
            Status = "Completed",
            InspectionResults = new List<InspectionResult>
            {
                new InspectionResult
                {
                    Object = new InspectionObject
                    {
                        Description = "Test Object",
                        Type = new InspectionObjectType { Name = "Test Type" }
                    },
                    Status = "OK",
                    Comment = "All good"
                }
            }
        };
        
        context.InspectionRounds.Add(round);
        await context.SaveChangesAsync();
        
        var generator = new QuestPdfReportGenerator(context);

        // Act
        var result = await generator.GenerateInspectionReportAsync(round.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length > 100); // PDF should have content
    }

    [Fact]
    public async Task GenerateInspectionReportAsync_InvalidRound_ThrowsException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb2")
            .Options;
        
        var context = new ApplicationDbContext(options, Mock.Of<ITenantService>());
        var generator = new QuestPdfReportGenerator(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            generator.GenerateInspectionReportAsync(Guid.NewGuid()));
    }
}
```

### Integration Tests

```csharp
[Fact]
public async Task GeneratedReport_ContainsSwedishCharacters()
{
    // Arrange
    var round = CreateTestRound(siteName: "Höglunda Förskola");
    var generator = new QuestPdfReportGenerator(_context);

    // Act
    var pdfBytes = await generator.GenerateInspectionReportAsync(round.Id);

    // Assert
    var pdfText = ExtractTextFromPdf(pdfBytes);
    Assert.Contains("Höglunda Förskola", pdfText);
}
```

## Validation Steps

### 1. Verify File Creation

```bash
ls -la src/SBAPro.Infrastructure/Services/QuestPdfReportGenerator.cs
```

Expected: File exists

### 2. Verify QuestPDF Package Installed

```bash
dotnet list src/SBAPro.Infrastructure package | grep QuestPDF
```

Expected: QuestPDF package listed

### 3. Build the Solution

```bash
dotnet build
```

Expected: Build succeeds with no errors

### 4. Test Report Generation

Create a test endpoint:

```csharp
app.MapGet("/test-pdf", async (IReportGenerator reportGenerator, ApplicationDbContext context) =>
{
    // Create test data
    var testRound = new InspectionRound { /* ... */ };
    context.InspectionRounds.Add(testRound);
    await context.SaveChangesAsync();
    
    var pdf = await reportGenerator.GenerateInspectionReportAsync(testRound.Id);
    
    return Results.File(pdf, "application/pdf", "test.pdf");
});
```

Navigate to: http://localhost:5000/test-pdf

Expected: PDF downloads with correct content

### 5. Verify PDF Content

Open the downloaded PDF and check:
- ✅ Swedish characters (ÅÄÖ) render correctly
- ✅ Site name and address present
- ✅ Inspector name present
- ✅ Date formatted correctly
- ✅ Table with inspection results
- ✅ Page numbers in footer
- ✅ Professional layout

### 6. Test with Real Data

1. Complete an inspection round through the UI
2. Click "Ladda ner rapport" button
3. Download PDF
4. Verify all inspection results are included

## Success Criteria

✅ QuestPDF package installed  
✅ QuestPdfReportGenerator implemented  
✅ Service registered in DI  
✅ Community license configured  
✅ Test PDF generates successfully  
✅ Swedish characters render correctly  
✅ Report layout is professional  
✅ All inspection data included  

## Troubleshooting

### Issue: License exception

**Error**: "QuestPDF license is not set"

**Solution**: Add license configuration:
```csharp
QuestPDF.Settings.License = LicenseType.Community;
```

### Issue: Swedish characters show as boxes

**Cause**: Font doesn't support characters

**Solution**: QuestPDF uses system fonts that support Unicode by default. This should not be an issue.

### Issue: PDF generation is slow

**Causes**:
1. Large amount of data
2. Complex layout
3. Missing indexes on database

**Solutions**:
- Use `.AsNoTracking()` for read-only queries
- Add indexes on frequently queried fields
- Consider caching generated reports

### Issue: Out of memory

**Cause**: Generating very large reports

**Solutions**:
- Paginate results (e.g., 100 items per page)
- Generate reports asynchronously
- Stream PDF instead of loading all into memory

## Related Files

- `src/SBAPro.Core/Interfaces/IReportGenerator.cs` - Interface definition
- `src/SBAPro.Infrastructure/Data/ApplicationDbContext.cs` - Data access
- `src/SBAPro.WebApp/Program.cs` - Service registration

## Next Steps

After completing this module:
1. Proceed to **06-infrastructure-validation.md** to validate the infrastructure layer
2. Test PDF generation with various scenarios
3. Customize report layout as needed

## Additional Resources

- [QuestPDF Documentation](https://www.questpdf.com/documentation/getting-started.html)
- [QuestPDF Examples](https://www.questpdf.com/documentation/examples.html)
- [Swedish Fire Safety Regulations (MSB)](https://www.msb.se/)

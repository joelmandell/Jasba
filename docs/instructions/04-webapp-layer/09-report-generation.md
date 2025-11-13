# 04-WebApp-Layer: PDF Report Download

## Objective
Implement PDF report download functionality in the UI.

## Prerequisites
- Completed: 04-webapp-layer/08-inspection-rounds.md
- Infrastructure PDF service working

## Instructions

Add download button to inspection round pages:

```razor
@page "/inspector/rounds/{RoundId:guid}"
@inject IReportGenerator ReportGenerator
@inject IJSRuntime JS

<button @onclick="DownloadReport" class="btn btn-primary">
    Download PDF Report
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
            // Show error message
        }
    }
}
```

Add JavaScript helper:

**File**: `src/SBAPro.WebApp/wwwroot/js/site.js`

```javascript
window.downloadPdf = (base64, filename) => {
    const link = document.createElement('a');
    link.href = 'data:application/pdf;base64,' + base64;
    link.download = filename;
    link.click();
};
```

## Validation

1. Complete an inspection round
2. Click "Download PDF Report"
3. Verify PDF downloads
4. Open PDF and verify content

## Success Criteria

✅ Download button appears  
✅ PDF downloads successfully  
✅ PDF contains all inspection data  
✅ Swedish characters render correctly  

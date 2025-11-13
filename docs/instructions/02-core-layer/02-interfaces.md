# 02-Core-Layer: Service Interfaces

## Objective
Define interfaces for all services that will be implemented in the Infrastructure layer.

## Prerequisites
- Completed: 02-core-layer/01-domain-entities.md
- Understanding of Dependency Inversion Principle

## Interface Overview

These interfaces define contracts for:
- Multi-tenant context management
- Email notifications
- PDF report generation

## Instructions

### 1. Create ITenantService Interface

**File**: `src/SBAPro.Core/Interfaces/ITenantService.cs`

```csharp
namespace SBAPro.Core.Interfaces;

/// <summary>
/// Service for managing tenant context in a multi-tenant application.
/// Provides the current tenant ID based on the authenticated user.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the current tenant ID from the authenticated user's claims.
    /// </summary>
    /// <returns>The tenant ID of the current user.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no tenant context is available.</exception>
    Guid GetTenantId();

    /// <summary>
    /// Attempts to get the current tenant ID.
    /// </summary>
    /// <returns>The tenant ID if available, otherwise null.</returns>
    Guid? TryGetTenantId();
}
```

### 2. Create IEmailService Interface

**File**: `src/SBAPro.Core/Interfaces/IEmailService.cs`

```csharp
namespace SBAPro.Core.Interfaces;

/// <summary>
/// Service for sending email notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="body">Email body content (can be HTML).</param>
    /// <param name="isHtml">Whether the body content is HTML (default: true).</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Sends an email to multiple recipients asynchronously.
    /// </summary>
    /// <param name="recipients">List of recipient email addresses.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="body">Email body content (can be HTML).</param>
    /// <param name="isHtml">Whether the body content is HTML (default: true).</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = true);
}
```

### 3. Create IReportGenerator Interface

**File**: `src/SBAPro.Core/Interfaces/IReportGenerator.cs`

```csharp
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
```

### 4. Create IFileStorageService Interface (Optional Future Extension)

**File**: `src/SBAPro.Core/Interfaces/IFileStorageService.cs`

```csharp
namespace SBAPro.Core.Interfaces;

/// <summary>
/// Service for storing and retrieving files (floor plans, photos).
/// Allows for abstraction over different storage backends (local, cloud).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contentType">MIME type of the file.</param>
    /// <param name="fileData">File content as byte array.</param>
    /// <returns>A URL or identifier for accessing the file.</returns>
    Task<string> UploadFileAsync(string fileName, string contentType, byte[] fileData);

    /// <summary>
    /// Downloads a file from storage.
    /// </summary>
    /// <param name="fileId">The file identifier or URL.</param>
    /// <returns>The file content as byte array.</returns>
    Task<byte[]> DownloadFileAsync(string fileId);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="fileId">The file identifier or URL.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFileAsync(string fileId);

    /// <summary>
    /// Checks if a file exists in storage.
    /// </summary>
    /// <param name="fileId">The file identifier or URL.</param>
    /// <returns>True if the file exists, otherwise false.</returns>
    Task<bool> FileExistsAsync(string fileId);
}
```

## Validation Steps

### 1. Verify All Interface Files Created

```bash
ls -la src/SBAPro.Core/Interfaces/
```

Expected files:
- ITenantService.cs
- IEmailService.cs
- IReportGenerator.cs
- IFileStorageService.cs (optional)

### 2. Check for Compilation Errors

```bash
dotnet build src/SBAPro.Core/SBAPro.Core.csproj
```

Expected: Build succeeds with no errors.

### 3. Verify Interface Design Principles

Check that interfaces follow these principles:

```bash
# Interfaces should only define contracts, no implementation
grep -r "class.*:" src/SBAPro.Core/Interfaces/
```

Expected: No matches (interfaces don't contain classes)

```bash
# Check that all methods are public and don't have implementation
grep -r "{.*}" src/SBAPro.Core/Interfaces/*.cs
```

Expected: Only empty braces from namespace declarations

### 4. Check XML Documentation

```bash
# All public methods should have XML documentation
grep -B1 "Task\|Guid" src/SBAPro.Core/Interfaces/*.cs | grep "///"
```

Expected: All interface members have XML doc comments

## Success Criteria

- ✅ All interface files created in the Interfaces folder
- ✅ All interfaces properly documented with XML comments
- ✅ Interfaces define contracts only (no implementation)
- ✅ Core project builds successfully
- ✅ Interfaces use only Core types (no Infrastructure dependencies)

## Design Principles

### Dependency Inversion Principle
- Core defines the interfaces (contracts)
- Infrastructure provides concrete implementations
- WebApp depends on Core interfaces, not concrete implementations
- This allows for easy testing and swapping of implementations

### Interface Segregation
- Each interface has a single, focused responsibility
- ITenantService: Tenant context only
- IEmailService: Email sending only
- IReportGenerator: PDF generation only
- IFileStorageService: File storage only

### Async by Default
- All I/O operations return `Task` or `Task<T>`
- Supports modern async/await patterns
- Improves scalability

## Implementation Notes

### ITenantService
- Will be implemented using ASP.NET Core's claims-based authentication
- Scoped service lifetime (per HTTP request / Blazor circuit)
- Critical for multi-tenancy data isolation

### IEmailService
- Will be implemented using MailKit
- Configuration stored in appsettings.json
- Should handle SMTP failures gracefully

### IReportGenerator
- Will be implemented using QuestPDF
- Should include tenant branding (logo, colors)
- Must comply with Swedish fire safety documentation requirements

### IFileStorageService
- Initial implementation: Database storage (SQLite)
- Future implementation: Azure Blob Storage or AWS S3
- Abstraction allows migration without changing business logic

## Next Steps

Proceed to **03-core-validation.md** to validate the complete Core layer.

## Troubleshooting

**Issue**: Build fails with unknown type references
- **Solution**: Ensure using statements reference only Core namespace

**Issue**: Interface methods have implementation
- **Solution**: Remove any method bodies - interfaces only declare signatures

**Issue**: Task type not recognized
- **Solution**: Add `using System.Threading.Tasks;` to the file

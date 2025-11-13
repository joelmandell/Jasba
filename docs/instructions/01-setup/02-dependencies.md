# 01-Setup: NuGet Package Dependencies

## Objective
Install all required NuGet packages for each project layer.

## Prerequisites
- Completed: 01-project-structure.md
- Projects created and referenced correctly

## Package Overview

| Project | Package | Purpose |
|---------|---------|---------|
| **SBAPro.Core** | None | Pure domain logic, no external dependencies |
| **SBAPro.Infrastructure** | Microsoft.EntityFrameworkCore.Sqlite | SQLite database provider |
| **SBAPro.Infrastructure** | Microsoft.AspNetCore.Identity.EntityFrameworkCore | Identity integration with EF Core |
| **SBAPro.Infrastructure** | MailKit | Modern email library for SMTP |
| **SBAPro.WebApp** | QuestPDF | PDF report generation |
| **SBAPro.WebApp** | Microsoft.EntityFrameworkCore.Tools | EF Core migrations tooling |

## Instructions

### 1. SBAPro.Core - No Packages

The Core project intentionally has **no external dependencies**. This maintains Clean Architecture principles.

**Verify Core has no packages:**
```bash
dotnet list src/SBAPro.Core/SBAPro.Core.csproj package
```

Expected: Empty list or default SDK references only.

### 2. SBAPro.Infrastructure - Data and Services

Install packages for database, identity, and email functionality:

```bash
# Entity Framework Core for SQLite
dotnet add src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite

# ASP.NET Core Identity with EF Core
dotnet add src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# MailKit for email notifications
dotnet add src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj package MailKit
```

### 3. SBAPro.WebApp - UI and Tools

Install packages for PDF generation and EF Core tooling:

```bash
# QuestPDF for generating PDF reports
dotnet add src/SBAPro.WebApp/SBAPro.WebApp.csproj package QuestPDF

# EF Core tools for running migrations
dotnet add src/SBAPro.WebApp/SBAPro.WebApp.csproj package Microsoft.EntityFrameworkCore.Tools
```

### 4. QuestPDF License Configuration

QuestPDF requires license acceptance for use. Add the following to the WebApp's Program.cs (we'll create the full file later, but note this requirement):

```csharp
// Required for QuestPDF
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
```

## Validation Steps

### 1. Verify Package Installation

Check Infrastructure packages:
```bash
dotnet list src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj package
```

Expected packages:
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- MailKit

Check WebApp packages:
```bash
dotnet list src/SBAPro.WebApp/SBAPro.WebApp.csproj package
```

Expected packages:
- QuestPDF
- Microsoft.EntityFrameworkCore.Tools

### 2. Restore All Packages

```bash
dotnet restore
```

Expected: All packages download successfully with no errors.

### 3. Build Solution

```bash
dotnet build
```

Expected: Build succeeds with no errors or warnings about missing packages.

### 4. Check for Vulnerabilities

Run security audit on dependencies:
```bash
dotnet list package --vulnerable --include-transitive
```

Expected: No vulnerable packages reported.

## Package Details and Rationale

### Microsoft.EntityFrameworkCore.Sqlite
- **Version**: Latest stable
- **Purpose**: Database provider for development environment
- **Note**: In production, this can be swapped for SQL Server or PostgreSQL providers

### Microsoft.AspNetCore.Identity.EntityFrameworkCore
- **Version**: Latest stable
- **Purpose**: Integrates ASP.NET Core Identity with Entity Framework
- **Features**: User management, roles, authentication, password hashing

### MailKit
- **Version**: Latest stable
- **Purpose**: Modern, secure email library
- **Why not System.Net.Mail**: SmtpClient is obsolete and MailKit is recommended by Microsoft
- **Features**: SMTP, IMAP, POP3 support with OAuth2 authentication

### QuestPDF
- **Version**: Latest stable
- **Purpose**: Fluent API for PDF generation
- **License**: Community license (free for open source and commercial use under certain conditions)
- **Features**: Modern C# API, high performance, rich formatting options

### Microsoft.EntityFrameworkCore.Tools
- **Version**: Latest stable
- **Purpose**: Design-time tools for EF Core migrations
- **Features**: `dotnet ef` commands for migrations and scaffolding

## Success Criteria

- ✅ Core project has no external dependencies
- ✅ Infrastructure has 3 required packages
- ✅ WebApp has 2 required packages
- ✅ All packages restored successfully
- ✅ Solution builds without errors
- ✅ No vulnerable packages detected

## Next Steps

Proceed to **03-architecture-validation.md** to verify the complete setup.

## Troubleshooting

**Issue**: Package installation fails with version conflicts
- **Solution**: Use the latest stable versions and ensure all projects target the same .NET version

**Issue**: QuestPDF license warnings
- **Solution**: Add license configuration to Program.cs as shown above

**Issue**: MailKit conflicts with other email libraries
- **Solution**: Remove any System.Net.Mail references if present

**Issue**: EF Core Tools not found
- **Solution**: Ensure Microsoft.EntityFrameworkCore.Tools is installed in the startup project (WebApp)

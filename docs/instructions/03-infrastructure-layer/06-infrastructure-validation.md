# 03-Infrastructure-Layer: Infrastructure Validation

## Objective
Validate that the Infrastructure layer is correctly implemented, all services work together, and multi-tenancy security is enforced.

## Prerequisites
- Completed: All files in 03-infrastructure-layer (01-05)
- Understanding of testing principles
- Understanding of multi-tenancy security

## Overview

This validation ensures:
1. All services are properly registered and injectable
2. Database context works with multi-tenancy
3. TenantService correctly retrieves tenant context
4. Email service can send emails
5. PDF generation works correctly
6. Database seeding completes successfully
7. Multi-tenancy isolation is enforced

## Validation Checklist

### Phase 1: Build and Compilation

#### 1.1 Clean Build

```bash
cd /path/to/SBAPro
dotnet clean
dotnet build
```

**Expected**: Build succeeds with no errors

**Success Criteria**:
- ✅ All projects compile without errors
- ✅ No missing dependencies
- ✅ No type or namespace errors

#### 1.2 Check for Warnings

```bash
dotnet build --no-incremental
```

**Expected**: No critical warnings (some informational warnings are acceptable)

**Watch for**:
- ❌ Nullable reference warnings
- ❌ Unused variable warnings
- ❌ Async method warnings

### Phase 2: Service Registration

#### 2.1 Verify All Services Registered

Check `Program.cs` contains all service registrations:

```bash
grep -E "AddScoped|AddSingleton|AddTransient" src/SBAPro.WebApp/Program.cs
```

**Expected Services**:
- ✅ `AddDbContext<ApplicationDbContext>`
- ✅ `AddScoped<ITenantService, TenantService>`
- ✅ `AddScoped<IEmailService, MailKitEmailService>`
- ✅ `AddScoped<IReportGenerator, QuestPdfReportGenerator>`
- ✅ `AddHttpContextAccessor()`
- ✅ `AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>`

#### 2.2 Verify Service Injection

Create a test endpoint to verify services can be injected:

```csharp
app.MapGet("/test-services", (
    ApplicationDbContext dbContext,
    ITenantService tenantService,
    IEmailService emailService,
    IReportGenerator reportGenerator) =>
{
    return Results.Ok(new
    {
        DbContext = dbContext != null ? "✓" : "✗",
        TenantService = tenantService != null ? "✓" : "✗",
        EmailService = emailService != null ? "✓" : "✗",
        ReportGenerator = reportGenerator != null ? "✓" : "✗"
    });
});
```

**Expected**: All services return "✓"

### Phase 3: Database Validation

#### 3.1 Database Creation

```bash
cd src/SBAPro.WebApp
dotnet run
```

**Expected**:
- ✅ Application starts without errors
- ✅ Database file created (SQLite) or database created (SQL Server)
- ✅ No error messages in console

#### 3.2 Verify Database Schema

```bash
# For SQLite
sqlite3 sbapro.db ".schema" | head -20

# For SQL Server
# Use SQL Server Management Studio or Azure Data Studio
```

**Expected Tables**:
- ✅ AspNetUsers
- ✅ AspNetRoles
- ✅ AspNetUserRoles
- ✅ Tenants
- ✅ Sites
- ✅ FloorPlans
- ✅ InspectionObjects
- ✅ InspectionObjectTypes
- ✅ InspectionRounds
- ✅ InspectionResults

#### 3.3 Verify Seeded Data

```sql
-- Check roles
SELECT Name FROM AspNetRoles;
-- Expected: SystemAdmin, TenantAdmin, Inspector

-- Check users
SELECT UserName, Email, TenantId FROM AspNetUsers;
-- Expected: 3 users (admin, demo admin, demo inspector)

-- Check tenants
SELECT Name FROM Tenants;
-- Expected: Demo Company AB

-- Check object types
SELECT Name, Icon FROM InspectionObjectTypes;
-- Expected: 4 types (Brandsläckare, Brandvarnare, etc.)
```

### Phase 4: Multi-Tenancy Validation

#### 4.1 Test TenantService

Create test code to verify TenantService:

```csharp
app.MapGet("/test-tenant-service", (ITenantService tenantService, HttpContext httpContext) =>
{
    var user = httpContext.User;
    var tenantId = tenantService.GetTenantId();
    
    return Results.Ok(new
    {
        IsAuthenticated = user.Identity?.IsAuthenticated,
        UserName = user.Identity?.Name,
        TenantId = tenantId,
        TenantIdClaim = user.FindFirst("TenantId")?.Value
    });
});
```

**Test Cases**:

1. **Unauthenticated User**:
   - Expected TenantId: `Guid.Empty`

2. **SystemAdmin User** (admin@sbapro.com):
   - Expected TenantId: `Guid.Empty`
   - Expected TenantIdClaim: `null`

3. **TenantAdmin User** (demo@democompany.se):
   - Expected TenantId: `[specific tenant guid]`
   - Expected TenantIdClaim: `[specific tenant guid]`

#### 4.2 Test Global Query Filters

```csharp
app.MapGet("/test-query-filters", async (ApplicationDbContext context, ITenantService tenantService) =>
{
    var tenantId = tenantService.GetTenantId();
    
    // Query without IgnoreQueryFilters - should only see tenant data
    var sitesFiltered = await context.Sites.CountAsync();
    
    // Query with IgnoreQueryFilters - should see all data
    var sitesAll = await context.Sites.IgnoreQueryFilters().CountAsync();
    
    return Results.Ok(new
    {
        CurrentTenantId = tenantId,
        SitesFiltered = sitesFiltered,
        SitesAll = sitesAll,
        FiltersWorking = sitesFiltered <= sitesAll
    });
});
```

**Test Cases**:

1. **As TenantAdmin**:
   - Login as demo@democompany.se
   - Navigate to /test-query-filters
   - Expected: `SitesFiltered` <= `SitesAll`
   - Expected: `FiltersWorking` = true

2. **As SystemAdmin**:
   - Login as admin@sbapro.com
   - Navigate to /test-query-filters
   - Expected: `SitesFiltered` = `SitesAll` (no filters applied)

#### 4.3 Test Tenant Isolation

Manual test:
1. Create two tenants via SystemAdmin
2. Create users for each tenant
3. Create sites for each tenant
4. Login as Tenant1 admin
5. Verify: Can only see Tenant1 sites
6. Login as Tenant2 admin
7. Verify: Can only see Tenant2 sites

**Expected**: Complete data isolation between tenants

### Phase 5: Email Service Validation

#### 5.1 Start MailHog

```bash
mailhog
```

**Expected**: MailHog starts on ports 1025 (SMTP) and 8025 (Web UI)

#### 5.2 Test Email Sending

```csharp
app.MapGet("/test-email", async (IEmailService emailService) =>
{
    try
    {
        await emailService.SendEmailAsync(
            "test@example.com",
            "Test Email from SBA Pro",
            "<h1>Test Email</h1><p>This is a test email with <strong>HTML</strong> content.</p>"
        );
        
        return Results.Ok("Email sent successfully");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Email failed: {ex.Message}");
    }
});
```

**Test Steps**:
1. Navigate to /test-email
2. Open MailHog Web UI: http://localhost:8025
3. Verify email appears

**Expected**:
- ✅ Email received in MailHog
- ✅ Subject is correct
- ✅ HTML content renders
- ✅ No errors in console

#### 5.3 Test Swedish Characters in Email

```csharp
await emailService.SendEmailAsync(
    "test@example.com",
    "Test - Svenska tecken: Å Ä Ö",
    "<p>Test med svenska tecken: åäö ÅÄÖ</p>"
);
```

**Expected**: Swedish characters display correctly in MailHog

### Phase 6: PDF Generation Validation

#### 6.1 Create Test Data

```csharp
app.MapGet("/create-test-round", async (
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) =>
{
    var tenant = await context.Tenants.FirstAsync();
    var site = await context.Sites.FirstAsync();
    var inspector = await userManager.FindByEmailAsync("inspector@democompany.se");
    var objectType = await context.InspectionObjectTypes.FirstAsync();

    var round = new InspectionRound
    {
        Id = Guid.NewGuid(),
        SiteId = site.Id,
        InspectorId = inspector.Id,
        StartedAt = DateTime.Now,
        CompletedAt = DateTime.Now.AddHours(1),
        Status = "Completed"
    };

    var inspectionObject = new InspectionObject
    {
        Id = Guid.NewGuid(),
        TypeId = objectType.Id,
        FloorPlanId = null,
        Description = "Test object",
        PositionX = 0,
        PositionY = 0
    };
    context.InspectionObjects.Add(inspectionObject);
    await context.SaveChangesAsync();

    var result = new InspectionResult
    {
        Id = Guid.NewGuid(),
        RoundId = round.Id,
        ObjectId = inspectionObject.Id,
        Status = "OK",
        Comment = "Test åäö ÅÄÖ",
        Timestamp = DateTime.Now
    };

    context.InspectionRounds.Add(round);
    context.InspectionResults.Add(result);
    await context.SaveChangesAsync();

    return Results.Ok(new { RoundId = round.Id });
});
```

#### 6.2 Test PDF Generation

```csharp
app.MapGet("/test-pdf/{roundId:guid}", async (
    Guid roundId,
    IReportGenerator reportGenerator) =>
{
    try
    {
        var pdfBytes = await reportGenerator.GenerateInspectionReportAsync(roundId);
        
        return Results.File(
            pdfBytes,
            "application/pdf",
            $"test-report-{roundId}.pdf");
    }
    catch (Exception ex)
    {
        return Results.Problem($"PDF generation failed: {ex.Message}");
    }
});
```

**Test Steps**:
1. Navigate to /create-test-round → Note the RoundId
2. Navigate to /test-pdf/{roundId}
3. PDF should download

**Expected**:
- ✅ PDF downloads successfully
- ✅ PDF opens without errors
- ✅ Contains site name and address
- ✅ Contains inspector name
- ✅ Contains inspection results
- ✅ Swedish characters (åäö ÅÄÖ) render correctly
- ✅ Table is formatted properly
- ✅ Page numbers in footer

#### 6.3 Verify PDF Content

Open the PDF and manually verify:
- ✅ Header: "Protokoll från egenkontroll"
- ✅ Site information
- ✅ Inspector information
- ✅ Date and time
- ✅ Summary (OK count, Issue count)
- ✅ Table with all inspection results
- ✅ Footer with page numbers and generation timestamp

### Phase 7: Integration Testing

#### 7.1 End-to-End Flow Test

Complete a full workflow:

1. **Login as SystemAdmin** (admin@sbapro.com)
   - Create a new tenant
   - Create TenantAdmin user for that tenant

2. **Login as TenantAdmin**
   - Create a site
   - Create inspection object types
   - Upload a floor plan (optional)
   - Create inspection objects
   - Create Inspector user

3. **Login as Inspector**
   - Start inspection round
   - Mark objects as OK/Issue
   - Complete round
   - Download PDF report

4. **Verify Report**
   - PDF contains all inspected objects
   - Status correctly reflected
   - Comments included

**Expected**: Complete workflow works without errors

#### 7.2 Multi-Tenancy Security Test

1. Create two tenants with different data
2. Login as Tenant1 admin
3. Try to access Tenant2's site by manipulating URL/parameters
4. **Expected**: Access denied or data not visible

#### 7.3 Performance Test

```csharp
app.MapGet("/test-performance", async (ApplicationDbContext context) =>
{
    var stopwatch = Stopwatch.StartNew();
    
    // Test 1: Query with filters
    var filtered = await context.Sites.ToListAsync();
    var filteredTime = stopwatch.ElapsedMilliseconds;
    
    stopwatch.Restart();
    
    // Test 2: Query without filters (SystemAdmin)
    var all = await context.Sites.IgnoreQueryFilters().ToListAsync();
    var allTime = stopwatch.ElapsedMilliseconds;
    
    return Results.Ok(new
    {
        FilteredCount = filtered.Count,
        FilteredTimeMs = filteredTime,
        AllCount = all.Count,
        AllTimeMs = allTime
    });
});
```

**Expected**: Both queries complete in < 100ms (for small datasets)

### Phase 8: Error Handling

#### 8.1 Test Missing Tenant Context

1. Login as user without TenantId claim
2. Try to create a site
3. **Expected**: Appropriate error message

#### 8.2 Test Invalid Email Configuration

1. Change SMTP server to invalid value
2. Try to send email
3. **Expected**: Exception caught and logged

#### 8.3 Test Missing Inspection Round

```csharp
app.MapGet("/test-invalid-pdf", async (IReportGenerator reportGenerator) =>
{
    try
    {
        await reportGenerator.GenerateInspectionReportAsync(Guid.NewGuid());
        return Results.Ok("Should not reach here");
    }
    catch (ArgumentException ex)
    {
        return Results.Ok($"Correctly threw exception: {ex.Message}");
    }
});
```

**Expected**: ArgumentException thrown with message "Inspection round not found"

## Success Criteria

### Build and Compilation
- ✅ Clean build succeeds
- ✅ No errors
- ✅ Minimal warnings

### Service Registration
- ✅ All services registered
- ✅ All services injectable
- ✅ Correct lifetimes (Scoped/Singleton/Transient)

### Database
- ✅ Database created
- ✅ All tables exist
- ✅ Seed data loaded
- ✅ 3 roles created
- ✅ 3 users created
- ✅ 1 tenant created
- ✅ 4 object types created

### Multi-Tenancy
- ✅ TenantService returns correct tenant ID
- ✅ Global query filters work
- ✅ Tenant isolation enforced
- ✅ SystemAdmin has no tenant context
- ✅ Cannot access other tenant's data

### Email Service
- ✅ Emails send successfully
- ✅ HTML content renders
- ✅ Swedish characters work
- ✅ MailHog receives emails

### PDF Generation
- ✅ PDFs generate successfully
- ✅ Swedish characters render
- ✅ Content is correct
- ✅ Layout is professional

### Integration
- ✅ End-to-end workflow works
- ✅ All roles function correctly
- ✅ Security is enforced

## Troubleshooting

### Build Fails

**Check**:
- All NuGet packages installed
- Correct .NET version
- No syntax errors

**Solutions**:
```bash
dotnet restore
dotnet clean
dotnet build
```

### Database Not Created

**Check**:
- Connection string correct
- DbInitializer called in Program.cs
- No exceptions during startup

**Solutions**:
- Check console output for errors
- Verify appsettings.json
- Delete existing database and retry

### Query Filters Not Working

**Check**:
- TenantService registered
- TenantService injected into DbContext
- OnModelCreating calls ApplyGlobalQueryFilters

**Solutions**:
- Verify service registration order
- Check DbContext constructor
- Add logging to TenantService

### Email Sending Fails

**Check**:
- MailHog running
- SMTP settings correct
- No firewall blocking

**Solutions**:
```bash
# Restart MailHog
killall mailhog
mailhog

# Check MailHog is running
curl http://localhost:8025
```

### PDF Generation Fails

**Check**:
- QuestPDF license configured
- Inspection round exists
- All related data loaded

**Solutions**:
- Add logging
- Verify data with debugger
- Check InspectionRound includes

## Next Steps

After successful validation:
1. Proceed to **Phase 4: WebApp Layer** (04-webapp-layer/README.md)
2. Document any issues found
3. Create unit tests for critical components

## Validation Report Template

After completing validation, document results:

```markdown
# Infrastructure Layer Validation Report
Date: YYYY-MM-DD
Validator: [Your Name]

## Summary
- Build: ✅ / ✗
- Services: ✅ / ✗
- Database: ✅ / ✗
- Multi-Tenancy: ✅ / ✗
- Email: ✅ / ✗
- PDF: ✅ / ✗

## Issues Found
1. [Description of issue if any]
2. [Description of issue if any]

## Recommendations
1. [Any recommendations]
2. [Any recommendations]

## Overall Status
✅ PASS / ✗ FAIL
```

## Related Files

- All files in `03-infrastructure-layer/`
- `src/SBAPro.WebApp/Program.cs`
- `src/SBAPro.WebApp/appsettings.json`

## Additional Resources

- [Testing in .NET](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [Entity Framework Core Testing](https://docs.microsoft.com/en-us/ef/core/testing/)
- [Multi-Tenancy Testing Patterns](https://docs.microsoft.com/en-us/azure/architecture/guide/multitenant/testing)

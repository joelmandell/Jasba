# 03-Infrastructure-Layer: Overview

## Purpose

The Infrastructure layer provides concrete implementations of the interfaces defined in the Core layer. This layer handles all external concerns:

- **Data Persistence**: Entity Framework Core, database access
- **Email Services**: SMTP email sending via MailKit
- **PDF Generation**: Report generation using QuestPDF  
- **Multi-Tenancy**: Tenant context management
- **Identity**: User authentication and authorization

## Layer Responsibilities

### ✅ Infrastructure SHOULD contain:
- Database context and migrations
- Service implementations (ITenantService, IEmailService, IReportGenerator)
- External API integrations
- File system operations
- Configuration loading

### ❌ Infrastructure SHOULD NOT contain:
- UI components or pages (belongs in WebApp)
- Business logic (belongs in Core)
- Direct HTTP request handling (belongs in WebApp)

## File Organization

```
src/SBAPro.Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs       # EF Core context with multi-tenancy
│   ├── DbInitializer.cs              # Seed data for development
│   └── Migrations/                   # EF Core migrations
├── Services/
│   ├── TenantService.cs              # ITenantService implementation
│   ├── MailKitEmailService.cs        # IEmailService implementation
│   ├── QuestPdfReportGenerator.cs    # IReportGenerator implementation
│   └── ApplicationUserClaimsPrincipalFactory.cs  # Custom claims for Identity
└── SBAPro.Infrastructure.csproj
```

## Instruction Files

Complete these instructions in order:

1. **01-database-context.md** - Create ApplicationDbContext with multi-tenancy
2. **02-tenant-service.md** - Implement tenant context management
3. **03-database-seeding.md** - Create seed data for development
4. **04-email-service.md** - Implement email notifications with MailKit
5. **05-pdf-service.md** - Implement PDF report generation with QuestPDF
6. **06-infrastructure-validation.md** - Validate complete Infrastructure layer

## Key Concepts

### Multi-Tenancy Implementation

The Infrastructure layer is responsible for enforcing tenant isolation at the database level:

```csharp
// Global query filter (in ApplicationDbContext)
builder.Entity<Site>()
    .HasQueryFilter(e => e.TenantId == tenantId);

// Automatic TenantId setting (in SaveChanges)
if (entry.Entity is Site site && site.TenantId == Guid.Empty)
    site.TenantId = tenantId.Value;
```

### Configuration Pattern

All external services use the Options pattern:

```csharp
// appsettings.json
{
  "Email": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587
  }
}

// Service configuration
public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
}

// In Program.cs
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));
```

### Service Lifetime

Choose appropriate service lifetimes:

- **Scoped**: DbContext, TenantService (per HTTP request / Blazor circuit)
- **Transient**: EmailService, ReportGenerator (new instance per use)
- **Singleton**: Configuration, caching services (shared across application)

## Testing Strategy

### Unit Tests

Test each service in isolation:

```csharp
public class TenantServiceTests
{
    [Fact]
    public void GetTenantId_WithValidClaim_ReturnsTenantId()
    {
        // Test implementation logic
    }
}
```

### Integration Tests

Test services with real database:

```csharp
public class ApplicationDbContextTests : IDisposable
{
    private ApplicationDbContext _context;

    [Fact]
    public async Task QueryFilter_OnlyReturnsTenantData()
    {
        // Test multi-tenancy isolation
    }
}
```

## Common Patterns

### Repository Pattern (Optional)

While not required initially, the repository pattern can be added later:

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
```

### Unit of Work Pattern (Optional)

For complex transactions:

```csharp
public interface IUnitOfWork
{
    IRepository<Site> Sites { get; }
    IRepository<FloorPlan> FloorPlans { get; }
    Task<int> SaveChangesAsync();
}
```

## Migration Management

### Create Migration

```bash
dotnet ef migrations add InitialCreate --project src/SBAPro.Infrastructure --startup-project src/SBAPro.WebApp
```

### Update Database

```bash
dotnet ef database update --project src/SBAPro.Infrastructure --startup-project src/SBAPro.WebApp
```

### Remove Last Migration

```bash
dotnet ef migrations remove --project src/SBAPro.Infrastructure --startup-project src/SBAPro.WebApp
```

## Security Considerations

### 1. SQL Injection Prevention

✅ EF Core parameterizes all queries automatically
❌ Never concatenate user input into SQL strings

### 2. Connection String Security

✅ Store in User Secrets for development
✅ Use Azure Key Vault or environment variables in production
❌ Never commit connection strings to source control

### 3. Email Security

✅ Use TLS/SSL for SMTP connections
✅ Store credentials securely
✅ Validate recipient addresses
❌ Never send to unvalidated addresses

### 4. Multi-Tenancy Security

✅ Always use global query filters
✅ Test tenant isolation thoroughly
✅ Automatically set TenantId
❌ Never allow manual TenantId in user input

## Performance Optimization

### Database Indexes

```csharp
// Already configured in ApplicationDbContext
builder.Entity<Site>().HasIndex(s => s.TenantId);
builder.Entity<InspectionRound>().HasIndex(ir => new { ir.SiteId, ir.CompletedAt });
```

### Query Optimization

```csharp
// ✅ Good - uses Include for eager loading
var site = await context.Sites
    .Include(s => s.FloorPlans)
    .ThenInclude(fp => fp.InspectionObjects)
    .FirstOrDefaultAsync(s => s.Id == siteId);

// ❌ Bad - causes N+1 query problem
var site = await context.Sites.FirstOrDefaultAsync(s => s.Id == siteId);
var floorPlans = await context.FloorPlans.Where(fp => fp.SiteId == siteId).ToListAsync();
```

### Async/Await

Always use async operations for I/O:

```csharp
// ✅ Good
await context.SaveChangesAsync();
await emailService.SendEmailAsync(...);

// ❌ Bad - blocks thread
context.SaveChanges();
emailService.SendEmail(...);
```

## Troubleshooting Guide

### Database Connection Issues

1. Verify connection string in appsettings.json
2. Check database exists and is accessible
3. Ensure migrations are up to date
4. Check EF Core logs for SQL errors

### Multi-Tenancy Issues

1. Verify ITenantService returns correct TenantId
2. Check global query filters are applied
3. Test with multiple tenants
4. Review SaveChanges for automatic TenantId setting

### Email Sending Issues

1. Verify SMTP server settings
2. Check firewall allows SMTP port
3. Verify credentials are correct
4. Test with a simple email first

### Migration Issues

1. Ensure DbContext is properly configured
2. Check startup project is set correctly
3. Verify all entity relationships
4. Review migration SQL for correctness

## Dependencies

### NuGet Packages Required

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="MailKit" Version="4.3.0" />
</ItemGroup>
```

### Project References

```xml
<ItemGroup>
  <ProjectReference Include="..\SBAPro.Core\SBAPro.Core.csproj" />
</ItemGroup>
```

## Next Steps

1. Complete all instruction files in order (01-06)
2. Run validation after each file
3. Create comprehensive tests for each service
4. Verify multi-tenancy isolation works correctly
5. Test all services with real dependencies

## Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [MailKit Documentation](https://github.com/jstedfast/MailKit)
- [QuestPDF Documentation](https://www.questpdf.com/)
- [ASP.NET Core Identity Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)

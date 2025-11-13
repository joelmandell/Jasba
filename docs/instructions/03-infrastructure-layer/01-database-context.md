# 03-Infrastructure-Layer: Database Context and Multi-Tenancy

## Objective
Implement the ApplicationDbContext with Entity Framework Core, including multi-tenancy support through global query filters.

## Prerequisites
- Completed: Phase 2 (Core Layer)
- Understanding of Entity Framework Core
- Understanding of multi-tenancy patterns

## Overview

The ApplicationDbContext is the central component for data access. It must:
1. Integrate with ASP.NET Core Identity
2. Implement global query filters for tenant isolation
3. Configure entity relationships
4. Automatically set TenantId on save

## Instructions

### 1. Create ApplicationDbContext

**File**: `src/SBAPro.Infrastructure/Data/ApplicationDbContext.cs`

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SBAPro.Core.Entities;
using SBAPro.Core.Interfaces;

namespace SBAPro.Infrastructure.Data;

/// <summary>
/// Application database context with multi-tenancy support.
/// Inherits from IdentityDbContext to support ASP.NET Core Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantService _tenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    // DbSet properties for all entities
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<FloorPlan> FloorPlans => Set<FloorPlan>();
    public DbSet<InspectionObject> InspectionObjects => Set<InspectionObject>();
    public DbSet<InspectionObjectType> InspectionObjectTypes => Set<InspectionObjectType>();
    public DbSet<InspectionRound> InspectionRounds => Set<InspectionRound>();
    public DbSet<InspectionResult> InspectionResults => Set<InspectionResult>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply global query filters for multi-tenancy
        ApplyGlobalQueryFilters(builder);

        // Configure entity relationships and constraints
        ConfigureEntityRelationships(builder);

        // Configure indexes for performance
        ConfigureIndexes(builder);
    }

    /// <summary>
    /// Applies global query filters to ensure tenant data isolation.
    /// This is the CRITICAL security component for multi-tenancy.
    /// </summary>
    private void ApplyGlobalQueryFilters(ModelBuilder builder)
    {
        var tenantId = _tenantService.TryGetTenantId();

        // Only apply filters if a tenant context exists
        // (SystemAdmin operations may not have a tenant context)
        if (tenantId.HasValue)
        {
            builder.Entity<Site>()
                .HasQueryFilter(e => e.TenantId == tenantId.Value);

            builder.Entity<InspectionObjectType>()
                .HasQueryFilter(e => e.TenantId == tenantId.Value);

            // ApplicationUser filter includes null TenantId for SystemAdmin
            builder.Entity<ApplicationUser>()
                .HasQueryFilter(e => e.TenantId == null || e.TenantId == tenantId.Value);
        }
    }

    /// <summary>
    /// Configures entity relationships using Fluent API.
    /// </summary>
    private void ConfigureEntityRelationships(ModelBuilder builder)
    {
        // Tenant relationships
        builder.Entity<Tenant>()
            .HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Tenant>()
            .HasMany(t => t.Sites)
            .WithOne(s => s.Tenant)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Tenant>()
            .HasMany(t => t.InspectionObjectTypes)
            .WithOne(iot => iot.Tenant)
            .HasForeignKey(iot => iot.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Site relationships
        builder.Entity<Site>()
            .HasMany(s => s.FloorPlans)
            .WithOne(fp => fp.Site)
            .HasForeignKey(fp => fp.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Site>()
            .HasMany(s => s.InspectionRounds)
            .WithOne(ir => ir.Site)
            .HasForeignKey(ir => ir.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // FloorPlan relationships
        builder.Entity<FloorPlan>()
            .HasMany(fp => fp.InspectionObjects)
            .WithOne(io => io.FloorPlan)
            .HasForeignKey(io => io.FloorPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // InspectionObjectType relationships
        builder.Entity<InspectionObjectType>()
            .HasMany(iot => iot.InspectionObjects)
            .WithOne(io => io.Type)
            .HasForeignKey(io => io.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // InspectionObject relationships
        builder.Entity<InspectionObject>()
            .HasMany(io => io.InspectionResults)
            .WithOne(ir => ir.Object)
            .HasForeignKey(ir => ir.ObjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // InspectionRound relationships
        builder.Entity<InspectionRound>()
            .HasOne(ir => ir.Inspector)
            .WithMany()
            .HasForeignKey(ir => ir.InspectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InspectionRound>()
            .HasMany(ir => ir.InspectionResults)
            .WithOne(r => r.Round)
            .HasForeignKey(r => r.RoundId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure required string properties
        builder.Entity<Tenant>()
            .Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Entity<Site>()
            .Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Entity<Site>()
            .Property(s => s.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Entity<FloorPlan>()
            .Property(fp => fp.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Entity<InspectionObjectType>()
            .Property(iot => iot.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Configure decimal precision for normalized coordinates
        builder.Entity<InspectionObject>()
            .Property(io => io.NormalizedX)
            .HasPrecision(18, 10);

        builder.Entity<InspectionObject>()
            .Property(io => io.NormalizedY)
            .HasPrecision(18, 10);
    }

    /// <summary>
    /// Configures database indexes for query performance.
    /// </summary>
    private void ConfigureIndexes(ModelBuilder builder)
    {
        // Index on TenantId for all tenant-specific entities
        builder.Entity<Site>()
            .HasIndex(s => s.TenantId);

        builder.Entity<InspectionObjectType>()
            .HasIndex(iot => iot.TenantId);

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.TenantId);

        // Index on foreign keys for efficient joins
        builder.Entity<FloorPlan>()
            .HasIndex(fp => fp.SiteId);

        builder.Entity<InspectionObject>()
            .HasIndex(io => io.FloorPlanId);

        builder.Entity<InspectionObject>()
            .HasIndex(io => io.TypeId);

        builder.Entity<InspectionResult>()
            .HasIndex(ir => ir.RoundId);

        builder.Entity<InspectionResult>()
            .HasIndex(ir => ir.ObjectId);

        builder.Entity<InspectionRound>()
            .HasIndex(ir => ir.SiteId);

        builder.Entity<InspectionRound>()
            .HasIndex(ir => ir.InspectorId);

        // Composite indexes for common queries
        builder.Entity<InspectionRound>()
            .HasIndex(ir => new { ir.SiteId, ir.CompletedAt });

        builder.Entity<InspectionResult>()
            .HasIndex(ir => new { ir.RoundId, ir.Status });
    }

    /// <summary>
    /// Automatically sets TenantId on entities before saving.
    /// This ensures tenant isolation even if TenantId is not explicitly set.
    /// </summary>
    public override int SaveChanges()
    {
        SetTenantId();
        return base.SaveChanges();
    }

    /// <summary>
    /// Automatically sets TenantId on entities before saving (async version).
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantId();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Sets TenantId on new entities that are tenant-specific.
    /// </summary>
    private void SetTenantId()
    {
        var tenantId = _tenantService.TryGetTenantId();
        if (!tenantId.HasValue)
            return; // No tenant context (SystemAdmin operation)

        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        foreach (var entry in entries)
        {
            // Set TenantId on tenant-specific entities
            if (entry.Entity is Site site && site.TenantId == Guid.Empty)
                site.TenantId = tenantId.Value;

            if (entry.Entity is InspectionObjectType type && type.TenantId == Guid.Empty)
                type.TenantId = tenantId.Value;

            if (entry.Entity is ApplicationUser user && user.TenantId == null)
                user.TenantId = tenantId.Value;
        }
    }
}
```

## Validation Steps

### 1. Verify File Created

```bash
ls -la src/SBAPro.Infrastructure/Data/ApplicationDbContext.cs
```

### 2. Build Infrastructure Project

```bash
dotnet build src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj
```

Expected: Build succeeds (may have warnings about ITenantService not being implemented yet)

### 3. Verify DbSet Properties

```bash
grep "DbSet<" src/SBAPro.Infrastructure/Data/ApplicationDbContext.cs | wc -l
```

Expected: 7 DbSet properties

### 4. Verify Global Query Filters

```bash
grep "HasQueryFilter" src/SBAPro.Infrastructure/Data/ApplicationDbContext.cs
```

Expected: Filters on Site, InspectionObjectType, and ApplicationUser

## Success Criteria

- ✅ ApplicationDbContext inherits from IdentityDbContext
- ✅ ITenantService injected in constructor
- ✅ All entity DbSets defined
- ✅ Global query filters configured for multi-tenancy
- ✅ Entity relationships configured
- ✅ Indexes configured for performance
- ✅ SaveChanges overridden to automatically set TenantId
- ✅ Infrastructure project builds

## Security Notes

### Multi-Tenancy Critical Points

1. **Global Query Filters**: Automatically filter queries by TenantId
2. **Automatic TenantId Setting**: Prevents manual TenantId mistakes
3. **Delete Behavior**: Cascade deletes respect tenant boundaries
4. **Index on TenantId**: Ensures query performance

### Testing Requirements

Multi-tenancy MUST be tested:
- Verify query filters work correctly
- Test that one tenant cannot access another's data
- Verify TenantId is automatically set
- Test SystemAdmin can access all tenants

## Next Steps

Proceed to **02-tenant-service.md** to implement the ITenantService.

## Design Decisions

### Why Global Query Filters?

- Automatic: Can't forget to filter by tenant
- Consistent: All queries filtered the same way
- Secure: Can't accidentally query cross-tenant
- Performance: Index on TenantId enables efficient filtering

### Why Override SaveChanges?

- Safety: Automatic TenantId prevents mistakes
- Convenience: Don't need to set TenantId manually
- Consistency: All entities handled the same way

### Delete Behavior Choices

- **Cascade**: When parent deletion should delete children (e.g., Site → FloorPlan)
- **Restrict**: When child references should prevent deletion (e.g., InspectionResult → InspectionObject)

## Troubleshooting

**Issue**: Build fails with ITenantService not found
- **Solution**: This is expected until ITenantService is implemented. Continue to next file.

**Issue**: Global query filter not working in tests
- **Solution**: Ensure mock ITenantService returns a valid TenantId

**Issue**: SaveChanges setting wrong TenantId
- **Solution**: Verify ITenantService.GetTenantId() returns correct value

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SBAPro.Core.Entities;
using SBAPro.Core.Interfaces;

namespace SBAPro.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantService? _tenantService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService? tenantService = null)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<FloorPlan> FloorPlans => Set<FloorPlan>();
    public DbSet<InspectionObject> InspectionObjects => Set<InspectionObject>();
    public DbSet<InspectionObjectType> InspectionObjectTypes => Set<InspectionObjectType>();
    public DbSet<InspectionRound> InspectionRounds => Set<InspectionRound>();
    public DbSet<InspectionResult> InspectionResults => Set<InspectionResult>();
    public DbSet<InspectionPhoto> InspectionPhotos => Set<InspectionPhoto>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply global query filters for multi-tenancy if tenant service is available
        if (_tenantService != null)
        {
            // Direct tenant filtering - evaluate tenant ID at query time, not model creation time
            builder.Entity<Site>().HasQueryFilter(s => s.TenantId == _tenantService.GetTenantId());
            builder.Entity<InspectionObjectType>().HasQueryFilter(iot => iot.TenantId == _tenantService.GetTenantId());
            
            // Indirect tenant filtering through relationships
            builder.Entity<FloorPlan>().HasQueryFilter(fp => fp.Site.TenantId == _tenantService.GetTenantId());
            builder.Entity<InspectionObject>().HasQueryFilter(io => io.FloorPlan.Site.TenantId == _tenantService.GetTenantId());
            builder.Entity<InspectionRound>().HasQueryFilter(ir => ir.Site.TenantId == _tenantService.GetTenantId());
            builder.Entity<InspectionResult>().HasQueryFilter(ir => ir.Round.Site.TenantId == _tenantService.GetTenantId());
            builder.Entity<InspectionPhoto>().HasQueryFilter(ip => ip.InspectionResult.Round.Site.TenantId == _tenantService.GetTenantId());
        }

        // Configure relationships
        builder.Entity<Tenant>()
            .HasMany(t => t.Sites)
            .WithOne(s => s.Tenant)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Tenant>()
            .HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Tenant>()
            .HasMany(t => t.InspectionObjectTypes)
            .WithOne(iot => iot.Tenant)
            .HasForeignKey(iot => iot.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Site>()
            .HasMany(s => s.FloorPlans)
            .WithOne(fp => fp.Site)
            .HasForeignKey(fp => fp.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Site>()
            .HasMany(s => s.InspectionRounds)
            .WithOne(ir => ir.Site)
            .HasForeignKey(ir => ir.SiteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FloorPlan>()
            .HasMany(fp => fp.InspectionObjects)
            .WithOne(io => io.FloorPlan)
            .HasForeignKey(io => io.FloorPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InspectionObjectType>()
            .HasMany(iot => iot.InspectionObjects)
            .WithOne(io => io.Type)
            .HasForeignKey(io => io.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InspectionRound>()
            .HasMany(ir => ir.InspectionResults)
            .WithOne(res => res.Round)
            .HasForeignKey(res => res.RoundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InspectionObject>()
            .HasMany(io => io.InspectionResults)
            .WithOne(res => res.Object)
            .HasForeignKey(res => res.ObjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InspectionRound>()
            .HasOne(ir => ir.Inspector)
            .WithMany()
            .HasForeignKey(ir => ir.InspectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InspectionResult>()
            .HasMany(ir => ir.Photos)
            .WithOne(p => p.InspectionResult)
            .HasForeignKey(p => p.InspectionResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SBAPro.Core.Entities;

namespace SBAPro.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Apply any pending migrations
        await context.Database.MigrateAsync();

        // Create roles if they don't exist
        string[] roles = { "SystemAdmin", "TenantAdmin", "Inspector" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create default SystemAdmin user if no users exist
        if (!await userManager.Users.AnyAsync())
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@sbapro.com",
                Email = "admin@sbapro.com",
                EmailConfirmed = true,
                TenantId = null // SystemAdmin has no tenant
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SystemAdmin");
            }
        }

        // Fix tenants without users - create a default admin for each
        await FixTenantsWithoutUsers(context, userManager);

        // Create demo tenant if no tenants exist
        if (!await context.Tenants.AnyAsync())
        {
            var demoTenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Demo Company AB"
            };
            context.Tenants.Add(demoTenant);
            await context.SaveChangesAsync();

            // Create demo tenant admin
            var tenantAdmin = new ApplicationUser
            {
                UserName = "demo@democompany.se",
                Email = "demo@democompany.se",
                EmailConfirmed = true,
                TenantId = demoTenant.Id
            };

            var result = await userManager.CreateAsync(tenantAdmin, "Demo@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(tenantAdmin, "TenantAdmin");
            }

            // Create demo inspector
            var inspector = new ApplicationUser
            {
                UserName = "inspector@democompany.se",
                Email = "inspector@democompany.se",
                EmailConfirmed = true,
                TenantId = demoTenant.Id
            };

            var inspectorResult = await userManager.CreateAsync(inspector, "Inspector@123");
            if (inspectorResult.Succeeded)
            {
                await userManager.AddToRoleAsync(inspector, "Inspector");
            }

            // Create some default inspection object types
            var objectTypes = new[]
            {
                new InspectionObjectType { Id = Guid.NewGuid(), Name = "Brandsläckare 6kg", Icon = "🧯", TenantId = demoTenant.Id },
                new InspectionObjectType { Id = Guid.NewGuid(), Name = "Brandvarnare", Icon = "🔔", TenantId = demoTenant.Id },
                new InspectionObjectType { Id = Guid.NewGuid(), Name = "Nödutgång", Icon = "🚪", TenantId = demoTenant.Id },
                new InspectionObjectType { Id = Guid.NewGuid(), Name = "Nödbelysning", Icon = "💡", TenantId = demoTenant.Id }
            };

            context.InspectionObjectTypes.AddRange(objectTypes);
            await context.SaveChangesAsync();

            // Create sample site
            var sampleSite = new Site
            {
                Id = Guid.NewGuid(),
                Name = "Demo Huvudkontor",
                Address = "Drottninggatan 1, Stockholm",
                TenantId = demoTenant.Id
            };

            context.Sites.Add(sampleSite);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Fixes tenants that were created without users by creating a default admin account for each.
    /// This handles the migration case where tenants exist in the database but have no associated users.
    /// </summary>
    private static async Task FixTenantsWithoutUsers(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Get all tenants (ignore query filters to see all tenants as SystemAdmin)
        var allTenants = await context.Tenants
            .IgnoreQueryFilters()
            .Include(t => t.Users)
            .ToListAsync();

        foreach (var tenant in allTenants)
        {
            // Check if this tenant has any users
            if (tenant.Users == null || !tenant.Users.Any())
            {
                // Create a default admin user for this tenant
                var defaultEmail = $"admin@{tenant.Name.ToLower().Replace(" ", "")}.com";
                var defaultPassword = "ChangeMeNow@123"; // Strong default password that should be changed

                var adminUser = new ApplicationUser
                {
                    UserName = defaultEmail,
                    Email = defaultEmail,
                    EmailConfirmed = true,
                    TenantId = tenant.Id,
                    FullName = $"{tenant.Name} Admin"
                };

                var result = await userManager.CreateAsync(adminUser, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "TenantAdmin");
                    Console.WriteLine($"✓ Created admin user for tenant '{tenant.Name}': {defaultEmail} / {defaultPassword}");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to create admin user for tenant '{tenant.Name}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}

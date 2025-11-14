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
}

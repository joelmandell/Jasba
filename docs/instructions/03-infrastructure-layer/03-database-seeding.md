# 03-Infrastructure-Layer: Database Seeding

## Objective
Implement database initialization and seeding to create default users, roles, and demo data for development and testing.

## Prerequisites
- Completed: 03-infrastructure-layer/02-tenant-service.md
- Understanding of ASP.NET Core Identity
- Understanding of Entity Framework Core
- Understanding of multi-tenancy

## Overview

Database seeding provides:
1. Initial roles (SystemAdmin, TenantAdmin, Inspector)
2. Default SystemAdmin user
3. Demo tenant with sample data
4. Sample inspection object types
5. Consistent development environment

## Security Considerations

⚠️ **IMPORTANT**: 
- Default passwords should only be used in development
- Production environments should require password changes on first login
- Never commit production credentials to source control

## Instructions

### 1. Create DbInitializer Class

**File**: `src/SBAPro.Infrastructure/Data/DbInitializer.cs`

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SBAPro.Core.Entities;

namespace SBAPro.Infrastructure.Data;

/// <summary>
/// Initializes the database with default data for development and testing.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database with roles, default users, and demo data.
    /// This method is idempotent - it can be called multiple times safely.
    /// </summary>
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Create roles if they don't exist
        await CreateRolesAsync(roleManager);

        // Create default SystemAdmin user if no users exist
        await CreateSystemAdminAsync(userManager);

        // Create demo tenant with sample data
        await CreateDemoTenantAsync(context, userManager);
    }

    /// <summary>
    /// Creates the three required roles: SystemAdmin, TenantAdmin, Inspector.
    /// </summary>
    private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "SystemAdmin", "TenantAdmin", "Inspector" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    /// <summary>
    /// Creates the default SystemAdmin user for initial system access.
    /// </summary>
    private static async Task CreateSystemAdminAsync(UserManager<ApplicationUser> userManager)
    {
        // Only create if no users exist
        if (await userManager.Users.AnyAsync())
        {
            return;
        }

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
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create SystemAdmin user: {errors}");
        }
    }

    /// <summary>
    /// Creates a demo tenant with sample data for development and testing.
    /// </summary>
    private static async Task CreateDemoTenantAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        // Only create if no tenants exist
        if (await context.Tenants.AnyAsync())
        {
            return;
        }

        // Create demo tenant
        var demoTenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Demo Company AB"
        };
        context.Tenants.Add(demoTenant);
        await context.SaveChangesAsync();

        // Create demo tenant admin
        await CreateDemoTenantAdminAsync(userManager, demoTenant.Id);

        // Create demo inspector
        await CreateDemoInspectorAsync(userManager, demoTenant.Id);

        // Create sample inspection object types
        await CreateSampleObjectTypesAsync(context, demoTenant.Id);

        // Create sample site (optional)
        await CreateSampleSiteAsync(context, demoTenant.Id);
    }

    /// <summary>
    /// Creates a demo tenant admin user.
    /// </summary>
    private static async Task CreateDemoTenantAdminAsync(
        UserManager<ApplicationUser> userManager,
        Guid tenantId)
    {
        var tenantAdmin = new ApplicationUser
        {
            UserName = "demo@democompany.se",
            Email = "demo@democompany.se",
            EmailConfirmed = true,
            TenantId = tenantId
        };

        var result = await userManager.CreateAsync(tenantAdmin, "Demo@123");
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(tenantAdmin, "TenantAdmin");
        }
    }

    /// <summary>
    /// Creates a demo inspector user.
    /// </summary>
    private static async Task CreateDemoInspectorAsync(
        UserManager<ApplicationUser> userManager,
        Guid tenantId)
    {
        var inspector = new ApplicationUser
        {
            UserName = "inspector@democompany.se",
            Email = "inspector@democompany.se",
            EmailConfirmed = true,
            TenantId = tenantId
        };

        var result = await userManager.CreateAsync(inspector, "Inspector@123");
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(inspector, "Inspector");
        }
    }

    /// <summary>
    /// Creates sample inspection object types for the demo tenant.
    /// These represent common fire safety equipment in Swedish regulations.
    /// </summary>
    private static async Task CreateSampleObjectTypesAsync(
        ApplicationDbContext context,
        Guid tenantId)
    {
        var objectTypes = new[]
        {
            new InspectionObjectType 
            { 
                Id = Guid.NewGuid(), 
                Name = "Brandsläckare 6kg", 
                Icon = "🧯", 
                TenantId = tenantId 
            },
            new InspectionObjectType 
            { 
                Id = Guid.NewGuid(), 
                Name = "Brandvarnare", 
                Icon = "🔔", 
                TenantId = tenantId 
            },
            new InspectionObjectType 
            { 
                Id = Guid.NewGuid(), 
                Name = "Nödutgång", 
                Icon = "🚪", 
                TenantId = tenantId 
            },
            new InspectionObjectType 
            { 
                Id = Guid.NewGuid(), 
                Name = "Nödbelysning", 
                Icon = "💡", 
                TenantId = tenantId 
            }
        };

        context.InspectionObjectTypes.AddRange(objectTypes);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a sample site for the demo tenant (optional).
    /// </summary>
    private static async Task CreateSampleSiteAsync(
        ApplicationDbContext context,
        Guid tenantId)
    {
        var sampleSite = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Demo Huvudkontor",
            Address = "Drottninggatan 1, Stockholm",
            TenantId = tenantId
        };

        context.Sites.Add(sampleSite);
        await context.SaveChangesAsync();
    }
}
```

### 2. Call DbInitializer from Program.cs

Update `Program.cs` to call the initializer after the app is built:

**File**: `src/SBAPro.WebApp/Program.cs`

```csharp
var app = builder.Build();

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await DbInitializer.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline...
```

## Default Credentials

The seeding creates the following default users:

### SystemAdmin
- **Email**: admin@sbapro.com
- **Password**: Admin@123
- **Role**: SystemAdmin
- **Capabilities**: Manage tenants, view all data

### Demo Tenant Admin
- **Email**: demo@democompany.se
- **Password**: Demo@123
- **Role**: TenantAdmin
- **Tenant**: Demo Company AB
- **Capabilities**: Manage sites, floor plans, users, object types

### Demo Inspector
- **Email**: inspector@democompany.se
- **Password**: Inspector@123
- **Role**: Inspector
- **Tenant**: Demo Company AB
- **Capabilities**: Execute inspection rounds, view reports

## Sample Data Created

### Roles
1. SystemAdmin
2. TenantAdmin
3. Inspector

### Tenant
- **Name**: Demo Company AB
- **Has**: 4 inspection object types, 1 sample site

### Inspection Object Types (Swedish Fire Safety Equipment)
1. **Brandsläckare 6kg** (Fire Extinguisher 6kg) - 🧯
2. **Brandvarnare** (Smoke Detector) - 🔔
3. **Nödutgång** (Emergency Exit) - 🚪
4. **Nödbelysning** (Emergency Lighting) - 💡

### Site
- **Name**: Demo Huvudkontor
- **Address**: Drottninggatan 1, Stockholm

## Development vs Production

### Development Environment

In development, seeding should:
- ✅ Create default users with known passwords
- ✅ Create demo tenant with sample data
- ✅ Use `EnsureCreatedAsync()` for quick setup

### Production Environment

In production, seeding should:
- ❌ NOT create users with default passwords
- ❌ NOT create demo data
- ✅ Only create roles
- ✅ Use migrations instead of `EnsureCreatedAsync()`

**Example production-safe initializer**:

```csharp
public static async Task InitializeAsync(
    ApplicationDbContext context,
    RoleManager<IdentityRole> roleManager,
    bool isDevelopment)
{
    // Always apply migrations in production
    if (!isDevelopment)
    {
        await context.Database.MigrateAsync();
    }
    else
    {
        await context.Database.EnsureCreatedAsync();
    }

    // Always create roles
    await CreateRolesAsync(roleManager);

    // Only create demo data in development
    if (isDevelopment)
    {
        await CreateSystemAdminAsync(userManager);
        await CreateDemoTenantAsync(context, userManager);
    }
}
```

## Extending the Seeder

### Adding More Object Types

To add more inspection object types:

```csharp
new InspectionObjectType 
{ 
    Id = Guid.NewGuid(), 
    Name = "Brandslang", 
    Icon = "🚿", 
    TenantId = tenantId 
},
new InspectionObjectType 
{ 
    Id = Guid.NewGuid(), 
    Name = "HJLR-skylt", 
    Icon = "🏥", 
    TenantId = tenantId 
}
```

### Adding Multiple Demo Tenants

```csharp
private static async Task CreateMultipleDemoTenantsAsync(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
{
    var tenants = new[]
    {
        new { Name = "Demo Company AB", AdminEmail = "admin1@demo.se" },
        new { Name = "Test Corporation AB", AdminEmail = "admin2@test.se" },
        new { Name = "Sample Enterprises AB", AdminEmail = "admin3@sample.se" }
    };

    foreach (var tenantInfo in tenants)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = tenantInfo.Name
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        // Create admin for this tenant
        // ... (similar to CreateDemoTenantAdminAsync)
    }
}
```

## Validation Steps

### 1. Verify File Creation

```bash
ls -la src/SBAPro.Infrastructure/Data/DbInitializer.cs
```

Expected: File exists

### 2. Build the Solution

```bash
dotnet build
```

Expected: Build succeeds with no errors

### 3. Run the Application

```bash
cd src/SBAPro.WebApp
dotnet run
```

Expected: Application starts without errors, database is created and seeded

### 4. Verify Roles Created

Connect to the database and query:

```sql
SELECT * FROM AspNetRoles;
```

Expected: Three roles (SystemAdmin, TenantAdmin, Inspector)

### 5. Verify Users Created

```sql
SELECT UserName, Email, TenantId FROM AspNetUsers;
```

Expected: 
- admin@sbapro.com (TenantId = NULL)
- demo@democompany.se (TenantId = [guid])
- inspector@democompany.se (TenantId = [guid])

### 6. Verify User Roles

```sql
SELECT u.UserName, r.Name 
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id;
```

Expected:
- admin@sbapro.com → SystemAdmin
- demo@democompany.se → TenantAdmin
- inspector@democompany.se → Inspector

### 7. Verify Tenant Data

```sql
SELECT * FROM Tenants;
```

Expected: One tenant (Demo Company AB)

### 8. Verify Object Types

```sql
SELECT Name, Icon, TenantId FROM InspectionObjectTypes;
```

Expected: Four object types for the demo tenant

### 9. Test Login

Navigate to the login page and test each user:

1. Login as SystemAdmin: admin@sbapro.com / Admin@123
2. Login as TenantAdmin: demo@democompany.se / Demo@123
3. Login as Inspector: inspector@democompany.se / Inspector@123

Expected: All logins succeed

### 10. Verify Multi-Tenancy

1. Login as TenantAdmin (demo@democompany.se)
2. Navigate to Sites page
3. Expected: Can see and manage sites
4. Create a new site
5. Logout and login as Inspector
6. Expected: Can see the same sites (same tenant)

### 11. Verify SystemAdmin Access

1. Login as SystemAdmin (admin@sbapro.com)
2. Navigate to Tenants page
3. Expected: Can see and manage all tenants
4. Try to access Sites page
5. Expected: Should have limited access (no tenant context)

## Success Criteria

✅ DbInitializer class created and implements all seeding methods  
✅ Roles created correctly (3 roles)  
✅ Default SystemAdmin created  
✅ Demo tenant created with sample data  
✅ Demo tenant admin and inspector created  
✅ Sample object types created  
✅ All users can log in successfully  
✅ Multi-tenancy isolation verified  
✅ SystemAdmin has no tenant context  

## Troubleshooting

### Issue: Database already exists error

**Solution**: Delete the database and run again, or use migrations

```bash
# Delete SQLite database
rm src/SBAPro.WebApp/sbapro.db

# Or use migrations
dotnet ef database drop
dotnet ef database update
```

### Issue: User creation fails

**Causes**:
1. Password doesn't meet complexity requirements
2. Email already exists
3. Invalid characters in username

**Solutions**:
- Check IdentityOptions password requirements
- Verify no duplicate emails
- Use valid email format for usernames

### Issue: Role assignment fails

**Causes**:
1. Role doesn't exist
2. User doesn't exist
3. User already has role

**Solutions**:
- Ensure roles are created before assigning
- Check that user creation succeeded
- Use idempotent checks (if already has role, skip)

### Issue: Tenant data not visible

**Causes**:
1. TenantId claim not added
2. Global query filters not working
3. User has wrong TenantId

**Solutions**:
- Verify ApplicationUserClaimsPrincipalFactory is registered
- Check TenantService implementation
- Verify user's TenantId matches tenant

## Related Files

- `src/SBAPro.Infrastructure/Data/ApplicationDbContext.cs` - Database context
- `src/SBAPro.WebApp/Program.cs` - Calls DbInitializer
- `src/SBAPro.Core/Entities/Tenant.cs` - Tenant entity
- `src/SBAPro.Core/Entities/ApplicationUser.cs` - User entity

## Next Steps

After completing this module:
1. Proceed to **04-email-service.md** to implement email notifications
2. Test the seeded data by logging in with each user role
3. Verify multi-tenancy isolation is working correctly

## Additional Resources

- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Entity Framework Core Data Seeding](https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding)
- [Database Initialization Strategies](https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/intro)

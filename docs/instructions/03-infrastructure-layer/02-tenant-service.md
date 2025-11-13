# 03-Infrastructure-Layer: Tenant Service Implementation

## Objective
Implement the TenantService to provide tenant context throughout the application, ensuring multi-tenant data isolation.

## Prerequisites
- Completed: 03-infrastructure-layer/01-database-context.md
- Understanding of ASP.NET Core HttpContext and Claims
- Understanding of Dependency Injection

## Overview

The TenantService retrieves the current tenant ID from the authenticated user's claims. This is a **critical security component** that enables:
1. Global query filters in Entity Framework
2. Automatic tenant assignment on entity creation
3. Authorization checks in the application

## Security Importance

⚠️ **CRITICAL**: This service is the foundation of multi-tenancy security. It must:
- Always return the correct tenant ID for authenticated users
- Return `Guid.Empty` for unauthenticated users or SystemAdmin
- Never allow one tenant to access another tenant's data

## Instructions

### 1. Create TenantService Implementation

**File**: `src/SBAPro.Infrastructure/Services/TenantService.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SBAPro.Core.Interfaces;

namespace SBAPro.Infrastructure.Services;

/// <summary>
/// Service for managing tenant context in the application.
/// Retrieves the tenant ID from the authenticated user's claims.
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current tenant ID from the authenticated user's claims.
    /// </summary>
    /// <returns>
    /// The tenant ID if the user is authenticated and has a TenantId claim,
    /// otherwise Guid.Empty (for SystemAdmin or unauthenticated users).
    /// </returns>
    public Guid GetTenantId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            return Guid.Empty;
        }

        var tenantIdClaim = user.FindFirst("TenantId");
        if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            return tenantId;
        }

        // Return Guid.Empty for SystemAdmin (no TenantId claim)
        return Guid.Empty;
    }
}
```

### 2. Register TenantService in Dependency Injection

**File**: `src/SBAPro.WebApp/Program.cs`

Add the following registrations in the service configuration section:

```csharp
// Register HttpContextAccessor (required by TenantService)
builder.Services.AddHttpContextAccessor();

// Register TenantService
builder.Services.AddScoped<ITenantService, TenantService>();
```

**Important**: 
- Use `AddScoped` lifecycle for TenantService (per HTTP request)
- HttpContextAccessor must be registered before TenantService
- TenantService must be registered before ApplicationDbContext

### 3. Configure Claims in ApplicationUserClaimsPrincipalFactory

The TenantId claim is added to the user's claims when they authenticate. This is typically done in a custom UserClaimsPrincipalFactory.

**File**: `src/SBAPro.Infrastructure/Services/ApplicationUserClaimsPrincipalFactory.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SBAPro.Core.Entities;

namespace SBAPro.Infrastructure.Services;

/// <summary>
/// Custom claims principal factory that adds the TenantId claim to authenticated users.
/// </summary>
public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Add TenantId claim if user belongs to a tenant
        if (user.TenantId.HasValue)
        {
            identity.AddClaim(new Claim("TenantId", user.TenantId.Value.ToString()));
        }

        return identity;
    }
}
```

Register the custom factory in `Program.cs`:

```csharp
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, 
    ApplicationUserClaimsPrincipalFactory>();
```

## How It Works

### Authentication Flow

1. **User logs in** → ASP.NET Identity authenticates credentials
2. **Claims generation** → ApplicationUserClaimsPrincipalFactory adds TenantId claim
3. **HTTP request** → TenantService reads TenantId from claims
4. **Database query** → Global query filters use TenantId automatically

### Example Usage in Controllers

```csharp
public class SitesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public SitesController(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<IActionResult> Create(Site site)
    {
        // Automatically set TenantId for new entities
        site.TenantId = _tenantService.GetTenantId();
        
        _context.Sites.Add(site);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }
}
```

### SystemAdmin Special Case

SystemAdmin users have **no TenantId** claim:
- `GetTenantId()` returns `Guid.Empty`
- Global query filters are **not applied** (can see all tenants)
- Can create and manage Tenant entities
- Cannot access tenant-specific data (Sites, InspectionRounds, etc.)

## Security Considerations

### ⚠️ What NOT to Do

❌ **DO NOT** allow TenantId to be passed as a parameter:
```csharp
// WRONG - Security vulnerability!
public async Task<Site> GetSite(Guid siteId, Guid tenantId)
{
    return await _context.Sites.FirstOrDefaultAsync(s => 
        s.Id == siteId && s.TenantId == tenantId);
}
```

✅ **DO** always use TenantService:
```csharp
// CORRECT - Secure
public async Task<Site> GetSite(Guid siteId)
{
    // Global query filter automatically applies tenant isolation
    return await _context.Sites.FirstOrDefaultAsync(s => s.Id == siteId);
}
```

❌ **DO NOT** bypass query filters without authorization:
```csharp
// WRONG - Bypasses tenant isolation!
var allSites = await _context.Sites.IgnoreQueryFilters().ToListAsync();
```

✅ **DO** check for SystemAdmin role before bypassing filters:
```csharp
// CORRECT - Only SystemAdmin can see all data
if (User.IsInRole("SystemAdmin"))
{
    var allSites = await _context.Sites.IgnoreQueryFilters().ToListAsync();
}
```

## Testing Requirements

### Unit Tests

Create tests to verify TenantService behavior:

```csharp
[Fact]
public void GetTenantId_WithAuthenticatedUser_ReturnsTenantId()
{
    // Arrange
    var tenantId = Guid.NewGuid();
    var claims = new List<Claim>
    {
        new Claim("TenantId", tenantId.ToString())
    };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var user = new ClaimsPrincipal(identity);
    
    var httpContext = new DefaultHttpContext { User = user };
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
    
    var tenantService = new TenantService(httpContextAccessor);

    // Act
    var result = tenantService.GetTenantId();

    // Assert
    Assert.Equal(tenantId, result);
}

[Fact]
public void GetTenantId_WithoutTenantIdClaim_ReturnsEmptyGuid()
{
    // Arrange
    var claims = new List<Claim>();
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var user = new ClaimsPrincipal(identity);
    
    var httpContext = new DefaultHttpContext { User = user };
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
    
    var tenantService = new TenantService(httpContextAccessor);

    // Act
    var result = tenantService.GetTenantId();

    // Assert
    Assert.Equal(Guid.Empty, result);
}

[Fact]
public void GetTenantId_WithUnauthenticatedUser_ReturnsEmptyGuid()
{
    // Arrange
    var httpContext = new DefaultHttpContext();
    var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
    
    var tenantService = new TenantService(httpContextAccessor);

    // Act
    var result = tenantService.GetTenantId();

    // Assert
    Assert.Equal(Guid.Empty, result);
}
```

## Validation Steps

### 1. Verify File Creation

```bash
ls -la src/SBAPro.Infrastructure/Services/TenantService.cs
ls -la src/SBAPro.Infrastructure/Services/ApplicationUserClaimsPrincipalFactory.cs
```

Expected: Both files exist

### 2. Build the Infrastructure Project

```bash
dotnet build src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj
```

Expected: Build succeeds with no errors

### 3. Verify Service Registration

```bash
grep "AddScoped<ITenantService" src/SBAPro.WebApp/Program.cs
grep "AddHttpContextAccessor" src/SBAPro.WebApp/Program.cs
```

Expected: Both services are registered

### 4. Test TenantId Claim Generation

Run the application and log in as a tenant user. Check that the TenantId claim is present:

```bash
# In a Blazor component or controller, inspect the user's claims
var tenantIdClaim = User.FindFirst("TenantId");
Console.WriteLine($"TenantId: {tenantIdClaim?.Value}");
```

Expected: TenantId claim is present for tenant users, absent for SystemAdmin

### 5. Verify Multi-Tenancy Isolation

Create a test to ensure one tenant cannot access another tenant's data:

```csharp
// Create two tenants with data
var tenant1 = new Tenant { Id = Guid.NewGuid(), Name = "Tenant 1" };
var tenant2 = new Tenant { Id = Guid.NewGuid(), Name = "Tenant 2" };

var site1 = new Site { Id = Guid.NewGuid(), Name = "Site 1", TenantId = tenant1.Id };
var site2 = new Site { Id = Guid.NewGuid(), Name = "Site 2", TenantId = tenant2.Id };

// Authenticate as tenant 1
var tenantService = CreateTenantServiceForTenant(tenant1.Id);

// Query sites
var sites = await _context.Sites.ToListAsync();

// Verify: Should only see tenant 1's site
Assert.Single(sites);
Assert.Equal(site1.Id, sites[0].Id);
```

## Success Criteria

✅ TenantService correctly retrieves TenantId from claims  
✅ HttpContextAccessor is registered and injected  
✅ ApplicationUserClaimsPrincipalFactory adds TenantId claim  
✅ Global query filters work with TenantService  
✅ SystemAdmin users have no TenantId (Guid.Empty)  
✅ Unit tests verify all scenarios  
✅ Multi-tenancy isolation is enforced  

## Troubleshooting

### Issue: GetTenantId() always returns Guid.Empty

**Causes**:
1. TenantId claim not added during authentication
2. HttpContextAccessor not registered
3. User not authenticated

**Solutions**:
- Verify ApplicationUserClaimsPrincipalFactory is registered
- Check that user has TenantId property set in database
- Ensure user is authenticated before calling GetTenantId()

### Issue: Global query filters not working

**Causes**:
1. TenantService not registered before ApplicationDbContext
2. TenantService not injected into ApplicationDbContext
3. Query filters not configured in OnModelCreating

**Solutions**:
- Check service registration order in Program.cs
- Verify ApplicationDbContext constructor receives ITenantService
- Review OnModelCreating implementation

### Issue: SystemAdmin can't access tenant management

**Causes**:
1. SystemAdmin has TenantId set (should be null)
2. Query filters applied to Tenant entity

**Solutions**:
- Verify SystemAdmin users have TenantId = null in database
- Ensure Tenant entity has no global query filter

## Related Files

- `src/SBAPro.Core/Interfaces/ITenantService.cs` - Interface definition
- `src/SBAPro.Infrastructure/Data/ApplicationDbContext.cs` - Uses TenantService
- `src/SBAPro.Core/Entities/ApplicationUser.cs` - Has TenantId property
- `src/SBAPro.WebApp/Program.cs` - Service registration

## Next Steps

After completing this module:
1. Proceed to **03-database-seeding.md** to create initial data
2. Ensure TenantService is working before proceeding
3. Test multi-tenancy isolation thoroughly

## Additional Resources

- [ASP.NET Core HttpContext](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-context)
- [Claims-based Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims)
- [Multi-tenancy Patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/multi-tenancy)

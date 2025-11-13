# 04-WebApp-Layer: Identity Setup

## Objective
Configure ASP.NET Core Identity with role-based authentication, custom claims for multi-tenancy, and secure cookie authentication.

## Prerequisites
- Completed: Phase 3 (Infrastructure Layer)
- Understanding of ASP.NET Core Identity
- Understanding of authentication vs authorization

## Overview

Identity setup provides:
1. User authentication with passwords
2. Three roles: SystemAdmin, TenantAdmin, Inspector
3. Custom TenantId claim for multi-tenancy
4. Cookie-based authentication for Blazor Server
5. Password complexity requirements

## Instructions

### 1. Install Identity Packages

Already installed in Infrastructure project:
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
```

### 2. Configure Identity in Program.cs

**File**: `src/SBAPro.WebApp/Program.cs`

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SBAPro.Core.Entities;
using SBAPro.Core.Interfaces;
using SBAPro.Infrastructure.Data;
using SBAPro.Infrastructure.Services;
using SBAPro.WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpContextAccessor (required for TenantService)
builder.Services.AddHttpContextAccessor();

// Configure Database with DbContextFactory for Blazor
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false; // Set true in production
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Add authorization
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Register application services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddScoped<IReportGenerator, QuestPdfReportGenerator>();

var app = builder.Build();

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var contextFactory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        await using var context = await contextFactory.CreateDbContextAsync();
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

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

### 3. Configure Connection String

**File**: `src/SBAPro.WebApp/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=sbapro.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

For SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SBAPro;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 4. Update DbInitializer to Work with DbContextFactory

Since Blazor Server uses `IDbContextFactory`, update the initializer call:

**File**: `src/SBAPro.Infrastructure/Data/DbInitializer.cs`

Ensure it accepts regular `ApplicationDbContext` (already implemented).

### 5. Configure Routes Component

**File**: `src/SBAPro.WebApp/Components/Routes.razor`

```razor
<Router AppAssembly="@typeof(Program).Assembly">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(Layout.MainLayout)">
            <NotAuthorized>
                @if (context.User.Identity?.IsAuthenticated == true)
                {
                    <p>You are not authorized to access this page.</p>
                    <p><a href="/Account/Logout">Logout</a></p>
                }
                else
                {
                    <RedirectToLogin />
                }
            </NotAuthorized>
        </AuthorizeRouteView>
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(Layout.MainLayout)">
            <p role="alert">Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>

@code {
    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;
}
```

Create `RedirectToLogin` component:

**File**: `src/SBAPro.WebApp/Components/Account/RedirectToLogin.razor`

```razor
@inject NavigationManager Navigation

@code {
    protected override void OnInitialized()
    {
        Navigation.NavigateTo($"/Account/Login?returnUrl={Uri.EscapeDataString(Navigation.Uri)}", forceLoad: true);
    }
}
```

### 6. Add Authentication State Provider

**File**: `src/SBAPro.WebApp/Components/App.razor`

```razor
<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <link rel="stylesheet" href="css/bootstrap/bootstrap.min.css" />
    <link rel="stylesheet" href="css/app.css" />
    <link rel="stylesheet" href="SBAPro.WebApp.styles.css" />
    <HeadOutlet />
</head>

<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>

</html>
```

### 7. Update _Imports.razor

**File**: `src/SBAPro.WebApp/Components/_Imports.razor`

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using Microsoft.JSInterop
@using SBAPro.WebApp
@using SBAPro.WebApp.Components
@using SBAPro.WebApp.Components.Layout
@using Microsoft.EntityFrameworkCore
@using SBAPro.Core.Entities
@using SBAPro.Core.Interfaces
@using SBAPro.Infrastructure.Data
```

## Identity Roles

### Three Roles Defined

1. **SystemAdmin**
   - Manages tenants
   - Creates tenant admins
   - No TenantId (can see all data with IgnoreQueryFilters)

2. **TenantAdmin**
   - Manages sites, floor plans, object types
   - Creates inspectors
   - Has TenantId (isolated to tenant data)

3. **Inspector**
   - Executes inspection rounds
   - Views reports
   - Has TenantId (isolated to tenant data)

### Role Hierarchy

```
SystemAdmin (no tenant)
└── Tenant
    ├── TenantAdmin (tenant-specific)
    └── Inspector (tenant-specific)
```

## Password Requirements

Default configuration:
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one non-alphanumeric character

Example valid passwords:
- `Admin@123`
- `Password1!`
- `Secure#Pass9`

## Custom Claims

### TenantId Claim

Added by `ApplicationUserClaimsPrincipalFactory`:

```csharp
if (user.TenantId.HasValue)
{
    identity.AddClaim(new Claim("TenantId", user.TenantId.Value.ToString()));
}
```

Access in code:
```csharp
var tenantIdClaim = User.FindFirst("TenantId");
if (tenantIdClaim != null)
{
    var tenantId = Guid.Parse(tenantIdClaim.Value);
}
```

Or use `ITenantService`:
```csharp
var tenantId = tenantService.GetTenantId();
```

## Testing Identity Setup

### 1. Verify Build

```bash
dotnet build src/SBAPro.WebApp
```

Expected: Build succeeds

### 2. Run Application

```bash
cd src/SBAPro.WebApp
dotnet run
```

Expected: Application starts, database created

### 3. Verify Database Tables

```bash
sqlite3 sbapro.db ".tables"
```

Expected: Identity tables created:
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- etc.

### 4. Verify Seeded Data

```sql
SELECT UserName, Email FROM AspNetUsers;
-- Expected: admin@sbapro.com, demo@democompany.se, inspector@democompany.se

SELECT Name FROM AspNetRoles;
-- Expected: SystemAdmin, TenantAdmin, Inspector
```

### 5. Test Authentication

Navigate to: http://localhost:5000/Account/Login

Try to login with:
- Email: admin@sbapro.com
- Password: Admin@123

Expected: Login succeeds (after implementing login page)

## Validation Steps

### 1. Services Registered

```bash
grep -E "AddIdentity|AddAuthorization|AddCascadingAuthenticationState" src/SBAPro.WebApp/Program.cs
```

Expected: All three present

### 2. Cookie Configuration

```bash
grep "ConfigureApplicationCookie" src/SBAPro.WebApp/Program.cs
```

Expected: Cookie paths configured

### 3. DbContextFactory

```bash
grep "AddDbContextFactory" src/SBAPro.WebApp/Program.cs
```

Expected: Factory registered (not direct DbContext)

### 4. Claims Factory

```bash
grep "AddClaimsPrincipalFactory" src/SBAPro.WebApp/Program.cs
```

Expected: Custom factory registered

## Success Criteria

✅ Identity configured in Program.cs  
✅ Three roles defined  
✅ Password requirements set  
✅ Cookie authentication configured  
✅ DbContextFactory registered  
✅ Custom claims factory registered  
✅ Database seeding works  
✅ Application starts without errors  

## Troubleshooting

### Issue: "Unable to resolve service for ApplicationDbContext"

**Cause**: Using direct `DbContext` injection instead of factory

**Solution**: Use `IDbContextFactory<ApplicationDbContext>`

### Issue: "No authentication handler is registered"

**Cause**: `UseAuthentication()` not called or in wrong order

**Solution**: Ensure middleware order:
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### Issue: TenantId claim not present

**Cause**: Custom claims factory not registered

**Solution**: Verify registration:
```csharp
.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
```

## Next Steps

After completing this module:
1. Proceed to **02-authentication-pages.md** to implement login/logout
2. Test that Identity is working correctly
3. Verify all three roles can be assigned

## Additional Resources

- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Blazor Authentication](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/)
- [Claims-based Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims)

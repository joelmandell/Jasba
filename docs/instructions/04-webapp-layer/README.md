# Phase 4: WebApp Layer

## Overview

The WebApp layer implements the user interface using Blazor Server with three distinct role-based interfaces:

1. **SystemAdmin** - Tenant management
2. **TenantAdmin** - Site, floor plan, object type, and user management
3. **Inspector** - Inspection round execution and reporting

## Architecture

### Technology Stack
- **Framework**: Blazor Server (.NET 9)
- **UI**: Tailwind CSS for styling
- **Authentication**: ASP.NET Core Identity
- **Authorization**: Role-based with claims
- **Mapping**: Leaflet.js for floor plan visualization
- **State Management**: Blazor component state + scoped services

### Clean Architecture Principles

The WebApp layer:
- ✅ Depends on Core and Infrastructure
- ✅ Handles presentation logic only
- ✅ Uses services through interfaces
- ✅ Implements security through authorization attributes

## Module Sequence

Complete these instruction files in order:

### 1. Identity Setup (01-identity-setup.md)
Configure ASP.NET Core Identity with:
- User registration and authentication
- Role-based authorization
- Custom claims (TenantId)
- Cookie authentication

### 2. Authentication Pages (02-authentication-pages.md)
Implement:
- Login page
- Logout functionality
- Access denied page
- Registration (optional)

### 3. SystemAdmin - Tenant Management (03-admin-tenant-management.md)
SystemAdmin interface for:
- Creating tenants
- Managing tenant details
- Creating tenant admins
- Viewing all tenants

### 4. TenantAdmin - Site Management (04-tenant-site-management.md)
TenantAdmin interface for:
- Creating sites
- Editing site details
- Viewing tenant's sites
- Deleting sites

### 5. TenantAdmin - Floor Plan Management (05-tenant-floorplan-management.md)
TenantAdmin interface for:
- Uploading floor plans (images)
- Managing floor plans per site
- Viewing floor plan metadata

### 6. Leaflet Integration (06-leaflet-integration.md)
Integrate Leaflet.js for:
- Displaying floor plans as map tiles
- Coordinate system setup
- Interactive map controls

### 7. Object Placement (07-object-placement.md)
Interactive floor plan editor for:
- Placing inspection objects on floor plans
- Drag-and-drop interface
- Object type selection
- Position saving

### 8. Inspector - Inspection Rounds (08-inspection-rounds.md)
Inspector interface for:
- Starting new inspection rounds
- Viewing assigned rounds
- Executing inspections
- Completing rounds

### 9. Report Generation (09-report-generation.md)
PDF report functionality:
- Download button in UI
- PDF generation trigger
- File download handling

### 10. Validation (10-webapp-validation.md)
Comprehensive testing of:
- All three role interfaces
- Authentication and authorization
- End-to-end workflows
- UI/UX validation

## Key Concepts

### Role-Based Access

```csharp
// Restrict entire page to role
@attribute [Authorize(Roles = "SystemAdmin")]

// Check role in code
@if (User.IsInRole("TenantAdmin"))
{
    // Show admin features
}
```

### Tenant Context

All TenantAdmin and Inspector pages should:
1. Access tenant data only
2. Automatically filter by TenantId
3. Never expose other tenant's data

Example:
```csharp
// Automatic tenant filtering via global query filters
var sites = await Context.Sites.ToListAsync();
// Returns only current tenant's sites
```

### Navigation Structure

```
/                           → Home (public)
/Account/Login             → Login page
/Account/Logout            → Logout handler
/Admin/Tenants             → SystemAdmin tenant management
/Tenant/Sites              → TenantAdmin site management
/Tenant/FloorPlans         → TenantAdmin floor plan management
/Tenant/ObjectTypes        → TenantAdmin object type management
/Tenant/FloorPlanEditor/{id} → Floor plan editing with Leaflet
/Inspector/Rounds          → Inspector rounds list
/Inspector/StartRound      → Start new round
/Inspector/ExecuteRound/{id} → Execute inspection
```

### Layout and Navigation

**MainLayout.razor** provides:
- Navigation menu (role-specific)
- User display with logout
- Responsive design

**NavMenu.razor** shows links based on role:
- SystemAdmin sees: Tenants
- TenantAdmin sees: Sites, Floor Plans, Object Types, Users
- Inspector sees: Rounds, Start Round

## Security Requirements

### Authentication
- ✅ All pages except Home and Login require authentication
- ✅ Use `@attribute [Authorize]` on protected pages
- ✅ Redirect unauthenticated users to login

### Authorization
- ✅ Verify user role before showing features
- ✅ Check TenantId matches for tenant-specific operations
- ✅ SystemAdmin should not access tenant features

### Data Protection
- ✅ Never pass TenantId as parameter
- ✅ Always use TenantService to get current tenant
- ✅ Rely on global query filters for isolation
- ✅ Validate user has access to requested resources

## Common Patterns

### Page Structure

```razor
@page "/tenant/sites"
@attribute [Authorize(Roles = "TenantAdmin")]
@using SBAPro.Core.Entities
@using SBAPro.Infrastructure.Data
@inject IDbContextFactory<ApplicationDbContext> DbFactory
@inject NavigationManager Navigation

<h3>Sites</h3>

<button @onclick="CreateSite">Create New Site</button>

@if (sites == null)
{
    <p>Loading...</p>
}
else
{
    <table>
        @foreach (var site in sites)
        {
            <tr>
                <td>@site.Name</td>
                <td>@site.Address</td>
            </tr>
        }
    </table>
}

@code {
    private List<Site>? sites;

    protected override async Task OnInitializedAsync()
    {
        await using var context = await DbFactory.CreateDbContextAsync();
        sites = await context.Sites.ToListAsync();
    }

    private void CreateSite()
    {
        Navigation.NavigateTo("/tenant/sites/create");
    }
}
```

### Form Handling

```razor
<EditForm Model="model" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <div class="form-group">
        <label>Name:</label>
        <InputText @bind-Value="model.Name" class="form-control" />
    </div>
    
    <button type="submit" class="btn btn-primary">Save</button>
</EditForm>

@code {
    private SiteModel model = new();

    private async Task HandleSubmit()
    {
        await using var context = await DbFactory.CreateDbContextAsync();
        
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Address = model.Address,
            TenantId = tenantService.GetTenantId()
        };
        
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        
        Navigation.NavigateTo("/tenant/sites");
    }
}
```

### Error Handling

```razor
@if (errorMessage != null)
{
    <div class="alert alert-danger">@errorMessage</div>
}

@code {
    private string? errorMessage;

    private async Task HandleAction()
    {
        try
        {
            // Perform action
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
    }
}
```

## Styling Guidelines

### Tailwind CSS

The project uses Tailwind CSS for styling:

```html
<div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Title</h1>
    <button class="bg-blue-500 text-white px-4 py-2 rounded">
        Click Me
    </button>
</div>
```

### Consistent UI Elements

- **Buttons**: `bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600`
- **Forms**: `form-control` with Tailwind utilities
- **Tables**: Striped rows with `table-auto w-full`
- **Cards**: `bg-white shadow-md rounded p-4`

## Testing Approach

### Manual Testing
1. Test each role separately
2. Verify authorization works
3. Test CRUD operations
4. Verify data isolation

### Integration Testing
1. Complete workflows end-to-end
2. Multi-user scenarios
3. Cross-tenant access attempts

## Prerequisites

Before starting this phase:
- ✅ Phase 3 (Infrastructure) completed and validated
- ✅ Database seeded with test data
- ✅ All services working correctly

## Success Criteria

After completing this phase:
- ✅ All three role interfaces functional
- ✅ Authentication and authorization working
- ✅ Tenant data isolation enforced
- ✅ Floor plan visualization working
- ✅ Inspection workflow complete
- ✅ PDF reports downloadable
- ✅ UI is responsive and user-friendly

## Common Issues

### DbContext Lifetime

❌ **Wrong**: Injecting DbContext directly
```csharp
@inject ApplicationDbContext Context
```

✅ **Correct**: Using DbContextFactory
```csharp
@inject IDbContextFactory<ApplicationDbContext> DbFactory

protected override async Task OnInitializedAsync()
{
    await using var context = await DbFactory.CreateDbContextAsync();
    // Use context
}
```

### Async/Await

Always use async methods for database operations:
```csharp
// Good
var sites = await context.Sites.ToListAsync();

// Bad - blocks UI
var sites = context.Sites.ToList();
```

### State Management

Blazor Server maintains state on the server. Be aware:
- Component state persists across renders
- Use `StateHasChanged()` to force re-render
- Dispose resources properly

## Resources

- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Tailwind CSS](https://tailwindcss.com/docs)
- [Leaflet.js](https://leafletjs.com/reference.html)

## Next Steps

Start with **01-identity-setup.md** and proceed sequentially through all 10 instruction files.

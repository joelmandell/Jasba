# 02-Core-Layer: Domain Entities

## Objective
Define all core domain entities that represent the business logic and data model for SBA Pro.

## Prerequisites
- Completed: Phase 1 (Setup)
- Understanding of multi-tenant architecture
- Understanding of Swedish fire safety regulations (LSO 2003:778)

## Domain Model Overview

The domain model is designed to support:
- Multi-tenant data isolation
- Hierarchical organization (Tenant → Site → FloorPlan → InspectionObject)
- Inspection workflow and documentation
- Legal compliance with Swedish fire safety law

## Entity Relationship Diagram

```
Tenant (1) ─────< (N) Site
  │                    │
  │                    └─< (N) FloorPlan
  │                             │
  │                             └─< (N) InspectionObject
  │                                      │
  │                                      └─< (N) InspectionResult
  │
  ├─────< (N) ApplicationUser
  │
  └─────< (N) InspectionObjectType

InspectionRound (1) ─────< (N) InspectionResult
```

## Instructions

### 1. Create Tenant Entity

**File**: `src/SBAPro.Core/Entities/Tenant.cs`

```csharp
namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a customer organization in the multi-tenant system.
/// Each tenant has isolated data and their own users and sites.
/// </summary>
public class Tenant
{
    /// <summary>
    /// Unique identifier for the tenant.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the organization.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional logo URL for PDF reports and branding.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Date when the tenant account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the tenant account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Site> Sites { get; set; } = new List<Site>();
    public ICollection<InspectionObjectType> InspectionObjectTypes { get; set; } = new List<InspectionObjectType>();
}
```

### 2. Create ApplicationUser Entity

**File**: `src/SBAPro.Core/Entities/ApplicationUser.cs`

```csharp
using Microsoft.AspNetCore.Identity;

namespace SBAPro.Core.Entities;

/// <summary>
/// Extended user entity that includes tenant association.
/// Inherits from IdentityUser for ASP.NET Core Identity integration.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// The tenant this user belongs to.
    /// Nullable for SystemAdmin users who don't belong to a specific tenant.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Navigation property to the tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// User's full name for display purposes.
    /// </summary>
    public string? FullName { get; set; }
}
```

### 3. Create Site Entity

**File**: `src/SBAPro.Core/Entities/Site.cs`

```csharp
namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a physical location or building where fire safety inspections are conducted.
/// </summary>
public class Site
{
    /// <summary>
    /// Unique identifier for the site.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the site (e.g., "Main Office Building", "Warehouse 3").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Physical address of the site.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or notes about the site.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The tenant that owns this site.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Navigation property to the owning tenant.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    // Navigation properties
    public ICollection<FloorPlan> FloorPlans { get; set; } = new List<FloorPlan>();
    public ICollection<InspectionRound> InspectionRounds { get; set; } = new List<InspectionRound>();
}
```

### 4. Create FloorPlan Entity

**File**: `src/SBAPro.Core/Entities/FloorPlan.cs`

```csharp
namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a floor plan image for a site.
/// Floor plans are used as the base layer for placing inspection objects.
/// </summary>
public class FloorPlan
{
    /// <summary>
    /// Unique identifier for the floor plan.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the floor plan (e.g., "Ground Floor", "Basement Level").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The floor plan image data stored as a byte array.
    /// In production, consider using cloud blob storage instead.
    /// </summary>
    public byte[]? ImageData { get; set; }

    /// <summary>
    /// MIME type of the image (e.g., "image/png", "image/jpeg").
    /// </summary>
    public string? ImageMimeType { get; set; }

    /// <summary>
    /// Original image width in pixels (for coordinate normalization).
    /// </summary>
    public int ImageWidth { get; set; }

    /// <summary>
    /// Original image height in pixels (for coordinate normalization).
    /// </summary>
    public int ImageHeight { get; set; }

    /// <summary>
    /// The site this floor plan belongs to.
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Navigation property to the parent site.
    /// </summary>
    public Site Site { get; set; } = null!;

    // Navigation properties
    public ICollection<InspectionObject> InspectionObjects { get; set; } = new List<InspectionObject>();
}
```

### 5. Create InspectionObjectType Entity

**File**: `src/SBAPro.Core/Entities/InspectionObjectType.cs`

```csharp
namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a configurable type of fire safety equipment or inspection point.
/// Each tenant can define their own object types based on their specific needs.
/// </summary>
public class InspectionObjectType
{
    /// <summary>
    /// Unique identifier for the object type.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the object type (e.g., "6kg Fire Extinguisher", "Emergency Exit Sign").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional icon identifier for map display (e.g., "fire-extinguisher", "exit-sign").
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Optional color code for map markers (hex format, e.g., "#FF0000").
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// The tenant that owns this object type.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Navigation property to the owning tenant.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    // Navigation properties
    public ICollection<InspectionObject> InspectionObjects { get; set; } = new List<InspectionObject>();
}
```

### 6. Create InspectionObject Entity

**File**: `src/SBAPro.Core/Entities/InspectionObject.cs`

```csharp
namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a specific fire safety equipment item placed on a floor plan.
/// </summary>
public class InspectionObject
{
    /// <summary>
    /// Unique identifier for the inspection object.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The type of this inspection object.
    /// </summary>
    public Guid TypeId { get; set; }

    /// <summary>
    /// Navigation property to the object type.
    /// </summary>
    public InspectionObjectType Type { get; set; } = null!;

    /// <summary>
    /// Optional description or identifier for this specific object.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Normalized X coordinate (0.0 to 1.0) on the floor plan.
    /// Normalized coordinates are resolution-independent.
    /// </summary>
    public double NormalizedX { get; set; }

    /// <summary>
    /// Normalized Y coordinate (0.0 to 1.0) on the floor plan.
    /// Normalized coordinates are resolution-independent.
    /// </summary>
    public double NormalizedY { get; set; }

    /// <summary>
    /// The floor plan this object is placed on.
    /// </summary>
    public Guid FloorPlanId { get; set; }

    /// <summary>
    /// Navigation property to the parent floor plan.
    /// </summary>
    public FloorPlan FloorPlan { get; set; } = null!;

    // Navigation properties
    public ICollection<InspectionResult> InspectionResults { get; set; } = new List<InspectionResult>();
}
```

### 7. Create InspectionRound Entity

**File**: `src/SBAPro.Core/Entities/InspectionRound.cs`

```csharp
namespace SBAPro.Core.Entities;

/// <summary>
/// Represents a single inspection session where an inspector checks multiple objects.
/// </summary>
public class InspectionRound
{
    /// <summary>
    /// Unique identifier for the inspection round.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The site where this inspection is conducted.
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Navigation property to the site.
    /// </summary>
    public Site Site { get; set; } = null!;

    /// <summary>
    /// The user (inspector) conducting this round.
    /// </summary>
    public string InspectorId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the inspector.
    /// </summary>
    public ApplicationUser Inspector { get; set; } = null!;

    /// <summary>
    /// When the inspection round was started.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the inspection round was completed (null if still in progress).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Current status of the inspection round.
    /// </summary>
    public InspectionRoundStatus Status { get; set; } = InspectionRoundStatus.InProgress;

    /// <summary>
    /// Optional notes or summary for the entire round.
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public ICollection<InspectionResult> InspectionResults { get; set; } = new List<InspectionResult>();
}

/// <summary>
/// Enumeration of possible inspection round statuses.
/// </summary>
public enum InspectionRoundStatus
{
    /// <summary>
    /// The round has been started but not completed.
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// The round has been completed successfully.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// The round was cancelled before completion.
    /// </summary>
    Cancelled = 2
}
```

### 8. Create InspectionResult Entity

**File**: `src/SBAPro.Core/Entities/InspectionResult.cs`

```csharp
namespace SBAPro.Core.Entities;

/// <summary>
/// Represents the result of inspecting a specific object during an inspection round.
/// This is the core evidence of fire safety compliance.
/// </summary>
public class InspectionResult
{
    /// <summary>
    /// Unique identifier for the inspection result.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The inspection round this result belongs to.
    /// </summary>
    public Guid RoundId { get; set; }

    /// <summary>
    /// Navigation property to the inspection round.
    /// </summary>
    public InspectionRound Round { get; set; } = null!;

    /// <summary>
    /// The inspection object that was checked.
    /// </summary>
    public Guid ObjectId { get; set; }

    /// <summary>
    /// Navigation property to the inspection object.
    /// </summary>
    public InspectionObject Object { get; set; } = null!;

    /// <summary>
    /// The status determined during inspection.
    /// </summary>
    public InspectionStatus Status { get; set; }

    /// <summary>
    /// Optional comment or note about the inspection.
    /// Required if status is Deficient.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// When this specific object was inspected.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional photo evidence (stored as byte array or URL).
    /// </summary>
    public byte[]? PhotoData { get; set; }

    /// <summary>
    /// MIME type of the photo if present.
    /// </summary>
    public string? PhotoMimeType { get; set; }
}

/// <summary>
/// Enumeration of possible inspection statuses for an object.
/// </summary>
public enum InspectionStatus
{
    /// <summary>
    /// The object has not been inspected yet in this round.
    /// </summary>
    NotInspected = 0,

    /// <summary>
    /// The object passed inspection with no issues.
    /// </summary>
    Ok = 1,

    /// <summary>
    /// The object has deficiencies that need attention.
    /// </summary>
    Deficient = 2,

    /// <summary>
    /// The object was not accessible for inspection.
    /// </summary>
    NotAccessible = 3
}
```

## Validation Steps

### 1. Verify All Files Created

```bash
ls -la src/SBAPro.Core/Entities/
```

Expected files:
- Tenant.cs
- ApplicationUser.cs
- Site.cs
- FloorPlan.cs
- InspectionObjectType.cs
- InspectionObject.cs
- InspectionRound.cs
- InspectionResult.cs

### 2. Check for Compilation Errors

```bash
dotnet build src/SBAPro.Core/SBAPro.Core.csproj
```

Expected: Build succeeds with no errors.

### 3. Verify No External Dependencies

```bash
dotnet list src/SBAPro.Core/SBAPro.Core.csproj package
```

Expected: No packages listed (except framework references).

### 4. Check Namespace Consistency

```bash
grep -r "^namespace" src/SBAPro.Core/Entities/
```

Expected: All files use `namespace SBAPro.Core.Entities;`

## Success Criteria

- ✅ All 8 entity files created
- ✅ All entities properly documented with XML comments
- ✅ Navigation properties defined for all relationships
- ✅ Enums defined for status fields
- ✅ Core project builds successfully
- ✅ No external dependencies in Core layer

## Next Steps

Proceed to **02-interfaces.md** to define service interfaces.

## Design Notes

### Multi-Tenancy
- `TenantId` property exists on: Site, InspectionObjectType
- ApplicationUser has nullable TenantId (SystemAdmin users don't belong to a tenant)
- Tenant is the root of the tenant-specific data hierarchy

### Coordinate Normalization
- InspectionObject uses normalized coordinates (0.0 to 1.0)
- This makes the system resilient to floor plan image changes
- Absolute pixel coordinates are calculated at render time

### Legal Compliance
- InspectionResult is the legal evidence of fire safety work
- Timestamps are critical for compliance
- Comments are required for deficient items
- Optional photo evidence supports documentation

## Troubleshooting

**Issue**: Build fails on ApplicationUser
- **Solution**: Ensure Microsoft.AspNetCore.Identity is available (it will be added via Infrastructure reference later)

**Issue**: Circular reference warnings
- **Solution**: Verify navigation properties don't create cycles in the same class

**Issue**: Namespace not resolving
- **Solution**: Check that namespace matches folder structure

# 02-Core-Layer: Core Layer Validation

## Objective
Validate that the Core layer is complete, follows Clean Architecture principles, and is ready for Infrastructure implementation.

## Prerequisites
- Completed: 02-core-layer/01-domain-entities.md
- Completed: 02-core-layer/02-interfaces.md

## Validation Checklist

### 1. File Structure Validation

**Check Core project structure:**
```bash
tree src/SBAPro.Core/ -L 2
```

Expected structure:
```
src/SBAPro.Core/
├── Entities/
│   ├── ApplicationUser.cs
│   ├── FloorPlan.cs
│   ├── InspectionObject.cs
│   ├── InspectionObjectType.cs
│   ├── InspectionResult.cs
│   ├── InspectionRound.cs
│   ├── Site.cs
│   └── Tenant.cs
├── Interfaces/
│   ├── IEmailService.cs
│   ├── IFileStorageService.cs
│   ├── IReportGenerator.cs
│   └── ITenantService.cs
└── SBAPro.Core.csproj
```

### 2. Entity Validation

**Count entities:**
```bash
ls -1 src/SBAPro.Core/Entities/*.cs | wc -l
```
Expected: 8 files

**Check all entities have proper namespace:**
```bash
grep -h "^namespace" src/SBAPro.Core/Entities/*.cs | sort -u
```
Expected: `namespace SBAPro.Core.Entities;` (single line)

**Verify key entities exist:**
```bash
for entity in Tenant ApplicationUser Site FloorPlan InspectionObjectType InspectionObject InspectionRound InspectionResult; do
  if [ -f "src/SBAPro.Core/Entities/$entity.cs" ]; then
    echo "✅ $entity.cs exists"
  else
    echo "❌ $entity.cs missing"
  fi
done
```

### 3. Interface Validation

**Count interfaces:**
```bash
ls -1 src/SBAPro.Core/Interfaces/*.cs | wc -l
```
Expected: At least 3 files (ITenantService, IEmailService, IReportGenerator)

**Check all interfaces have proper namespace:**
```bash
grep -h "^namespace" src/SBAPro.Core/Interfaces/*.cs | sort -u
```
Expected: `namespace SBAPro.Core.Interfaces;` (single line)

**Verify interfaces are public:**
```bash
grep "public interface" src/SBAPro.Core/Interfaces/*.cs
```
Expected: All interfaces declared as `public interface`

### 4. Dependency Check

**Verify Core has no external dependencies:**
```bash
dotnet list src/SBAPro.Core/SBAPro.Core.csproj package
```
Expected: No packages or only framework references

**Check for prohibited references:**
```bash
grep -r "EntityFramework\|Blazor\|AspNetCore" src/SBAPro.Core/*.csproj
```
Expected: No matches (except AspNetCore.Identity which is allowed for ApplicationUser)

### 5. Compilation Validation

**Build Core project:**
```bash
dotnet build src/SBAPro.Core/SBAPro.Core.csproj
```
Expected: Build succeeds with exit code 0

**Check for warnings:**
```bash
dotnet build src/SBAPro.Core/SBAPro.Core.csproj --nologo 2>&1 | grep -i warning
```
Expected: No warnings (or only nullable reference warnings which are acceptable)

### 6. Code Quality Checks

**Check for XML documentation:**
```bash
# Count documented public classes/interfaces
grep -r "/// <summary>" src/SBAPro.Core/ | wc -l
```
Expected: At least 20 (one per entity, interface, and major properties)

**Verify navigation properties:**
```bash
# Check that collection navigation properties use ICollection
grep "ICollection<" src/SBAPro.Core/Entities/*.cs | wc -l
```
Expected: At least 10 (multiple per parent entity)

**Check enum definitions:**
```bash
grep -r "public enum" src/SBAPro.Core/Entities/
```
Expected: At least 2 enums (InspectionRoundStatus, InspectionStatus)

### 7. Domain Model Validation

**Check multi-tenancy support:**
```bash
# Entities that should have TenantId
grep "public Guid TenantId" src/SBAPro.Core/Entities/*.cs
```
Expected: Found in Site, InspectionObjectType, and Tenant entities

**Verify Guid keys:**
```bash
# All entities should use Guid as primary key
grep "public Guid Id { get; set; }" src/SBAPro.Core/Entities/*.cs | wc -l
```
Expected: 8 (one per entity except ApplicationUser which uses string Id from IdentityUser)

**Check timestamp fields:**
```bash
# Important entities should have DateTime fields
grep "DateTime" src/SBAPro.Core/Entities/*.cs | wc -l
```
Expected: Multiple matches (CreatedAt, StartedAt, CompletedAt, Timestamp)

### 8. Relationship Validation

Create a relationship validation checklist:

- [ ] Tenant → ApplicationUser (1:many) ✓
- [ ] Tenant → Site (1:many) ✓
- [ ] Tenant → InspectionObjectType (1:many) ✓
- [ ] Site → FloorPlan (1:many) ✓
- [ ] Site → InspectionRound (1:many) ✓
- [ ] FloorPlan → InspectionObject (1:many) ✓
- [ ] InspectionObjectType → InspectionObject (1:many) ✓
- [ ] InspectionRound → InspectionResult (1:many) ✓
- [ ] InspectionObject → InspectionResult (1:many) ✓
- [ ] ApplicationUser → InspectionRound (1:many) ✓

**Verify foreign key properties:**
```bash
# Check for foreign key properties (should match navigation properties)
grep -E "(TenantId|SiteId|FloorPlanId|TypeId|RoundId|ObjectId|InspectorId)" src/SBAPro.Core/Entities/*.cs | wc -l
```
Expected: At least 15 matches

## Success Criteria

All of the following must be true:

- ✅ All 8 entity classes exist
- ✅ All required interfaces defined (minimum 3)
- ✅ All files use correct namespaces
- ✅ Core project has no external dependencies (except Microsoft.AspNetCore.Identity for ApplicationUser)
- ✅ Core project builds without errors
- ✅ All public APIs documented with XML comments
- ✅ Multi-tenancy properly supported with TenantId
- ✅ All entity relationships properly defined
- ✅ Enumerations defined for status fields
- ✅ Navigation properties use ICollection

## Design Review Checklist

### Clean Architecture Compliance
- [ ] Core has no dependencies on Infrastructure or WebApp
- [ ] All external services abstracted behind interfaces
- [ ] Domain logic is free of infrastructure concerns
- [ ] Entities represent pure business concepts

### Domain Model Quality
- [ ] Entity names are clear and meaningful
- [ ] Relationships accurately model the business domain
- [ ] All required properties are non-nullable (with `= null!;` for EF navigation properties)
- [ ] Optional properties are properly nullable

### Multi-Tenancy
- [ ] Tenant-specific entities have TenantId property
- [ ] ApplicationUser correctly links to Tenant
- [ ] Data isolation can be enforced at database level

### Future-Proofing
- [ ] Normalized coordinates used for floor plan objects
- [ ] File storage abstracted (IFileStorageService)
- [ ] Status enums allow for future extension
- [ ] Timestamps captured for audit trail

## Common Issues and Fixes

### Issue: Build fails with CS0234 (type or namespace not found)
**Fix**: Ensure using statements only reference Core namespace and System namespaces

### Issue: Navigation property warnings
**Fix**: Initialize collections in property declarations: `= new List<T>()`

### Issue: Nullable reference warnings
**Fix**: Use `= null!;` for required navigation properties that will be set by EF Core

### Issue: Circular dependency errors
**Fix**: Check that Core doesn't reference Infrastructure or WebApp projects

## Documentation

Create a validation report:

**File**: `docs/validation-reports/02-core-validation.md`

```markdown
# Core Layer Validation Report

**Date**: [YYYY-MM-DD]
**Validator**: [Name/AI Agent]

## Results

### File Structure
✅ All entity files present (8/8)
✅ All interface files present (4/4)

### Compilation
✅ Core project builds successfully
⚠️ [Number] warnings (acceptable if nullable reference warnings only)

### Dependencies
✅ No external dependencies (Clean Architecture maintained)

### Code Quality
✅ XML documentation complete
✅ Navigation properties properly defined
✅ Enums defined

### Domain Model
✅ Multi-tenancy supported
✅ All relationships modeled
✅ Timestamp fields included

## Issues Found
[List any issues that need to be addressed]

## Recommendations
[Any suggestions for improvement]

## Conclusion
✅ Core layer is ready for Infrastructure implementation
```

## Next Steps

If all validations pass, proceed to Phase 3: **03-infrastructure-layer/01-database-context.md**

## Troubleshooting

**Issue**: Missing entity files
- **Solution**: Re-create missing entities following 01-domain-entities.md

**Issue**: Core has external dependencies
- **Solution**: Remove any packages except framework references and Microsoft.AspNetCore.Identity

**Issue**: Build fails
- **Solution**: Check error messages carefully, ensure all using statements are correct

**Issue**: Navigation property null reference warnings
- **Solution**: Use `= null!;` pattern for required navigation properties

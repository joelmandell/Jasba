# 01-Setup: Architecture Validation

## Objective
Verify that the project structure and dependencies follow Clean Architecture principles.

## Prerequisites
- Completed: 01-project-structure.md
- Completed: 02-dependencies.md

## Validation Checklist

### 1. Dependency Direction Validation

The dependency flow must be strictly: **WebApp → Infrastructure → Core**

**Check WebApp references:**
```bash
dotnet list src/SBAPro.WebApp/SBAPro.WebApp.csproj reference
```
✅ Expected: References Infrastructure (and transitively, Core)
❌ Must NOT: Have any circular references

**Check Infrastructure references:**
```bash
dotnet list src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj reference
```
✅ Expected: References Core only
❌ Must NOT: Reference WebApp or have circular references

**Check Core references:**
```bash
dotnet list src/SBAPro.Core/SBAPro.Core.csproj reference
```
✅ Expected: No project references (may have framework references)
❌ Must NOT: Reference any other project in the solution

### 2. Package Dependencies Validation

**Core Layer - Must remain pure:**
```bash
dotnet list src/SBAPro.Core/SBAPro.Core.csproj package
```
✅ Expected: No external NuGet packages (or only Microsoft.NETCore.App)
❌ Must NOT: Have Entity Framework, ASP.NET, or any infrastructure packages

**Infrastructure Layer:**
```bash
dotnet list src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj package
```
✅ Expected packages:
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- MailKit

**WebApp Layer:**
```bash
dotnet list src/SBAPro.WebApp/SBAPro.WebApp.csproj package
```
✅ Expected packages:
- QuestPDF
- Microsoft.EntityFrameworkCore.Tools

### 3. Folder Structure Validation

**Verify Core structure:**
```bash
ls -la src/SBAPro.Core/
```
✅ Expected folders: `Entities/`, `Interfaces/`
❌ Must NOT have: `Data/`, `Services/` (these belong in Infrastructure)

**Verify Infrastructure structure:**
```bash
ls -la src/SBAPro.Infrastructure/
```
✅ Expected folders: `Data/`, `Services/`, `Migrations/`

**Verify WebApp structure:**
```bash
ls -la src/SBAPro.WebApp/Components/Pages/
```
✅ Expected folders: `Admin/`, `Tenant/`, `Inspector/`, `Account/`

### 4. Build Validation

**Clean build:**
```bash
dotnet clean
dotnet build
```
✅ Expected: Build succeeds with exit code 0
✅ Expected: No errors
⚠️  Warnings are acceptable at this stage but should be documented

**Check build output:**
```bash
ls -la src/SBAPro.WebApp/bin/Debug/net9.0/
```
✅ Expected: All three DLLs present:
- SBAPro.Core.dll
- SBAPro.Infrastructure.dll
- SBAPro.WebApp.dll

### 5. Architecture Principles Checklist

Review the following Clean Architecture principles:

- [ ] **Core Layer Independence**: Core has no dependencies on other layers
- [ ] **Dependency Inversion**: Infrastructure implements interfaces defined in Core
- [ ] **Single Responsibility**: Each project has a clear, single purpose
- [ ] **Separation of Concerns**: Domain logic (Core) separate from infrastructure (Infrastructure) and UI (WebApp)

### 6. Security and Vulnerability Check

```bash
dotnet list package --vulnerable --include-transitive
```
✅ Expected: No vulnerabilities found
❌ If vulnerabilities found: Update packages or document mitigation strategy

## Detailed Architecture Review

### Core Layer Responsibilities
- ✅ Domain entities (models)
- ✅ Business logic interfaces
- ✅ Domain exceptions
- ❌ No database access
- ❌ No external service calls
- ❌ No UI concerns

### Infrastructure Layer Responsibilities
- ✅ Database context and migrations
- ✅ Implementation of Core interfaces
- ✅ External service integrations (email, PDF)
- ✅ Data persistence logic
- ❌ No UI components
- ❌ No direct user interaction

### WebApp Layer Responsibilities
- ✅ Blazor components and pages
- ✅ User interface logic
- ✅ Dependency injection configuration
- ✅ Startup configuration
- ❌ No direct database access (use Infrastructure services)
- ❌ No business logic (delegate to Core/Infrastructure)

## Common Architecture Violations to Check

### ❌ Anti-Pattern 1: Core references Infrastructure
```bash
# This should return empty or error:
grep -r "SBAPro.Infrastructure" src/SBAPro.Core/ 2>/dev/null
```
Expected: No matches

### ❌ Anti-Pattern 2: Core has EF Core dependencies
```bash
# Check Core csproj for EF packages
grep -i "EntityFramework" src/SBAPro.Core/SBAPro.Core.csproj
```
Expected: No matches

### ❌ Anti-Pattern 3: WebApp directly accesses database
This will be checked in later modules when DbContext is implemented.

## Success Criteria

All of the following must be true:

- ✅ Solution builds successfully
- ✅ No circular dependencies
- ✅ Core layer has no external dependencies
- ✅ Dependency flow is correct (WebApp → Infrastructure → Core)
- ✅ Folder structure matches Clean Architecture principles
- ✅ No security vulnerabilities in packages
- ✅ All required packages installed in correct projects

## Next Steps

If all validations pass, proceed to Phase 2: **02-core-layer/01-domain-entities.md**

## Troubleshooting

**Issue**: Build fails with missing reference errors
- **Solution**: Re-run project reference commands from 01-project-structure.md

**Issue**: Core has external dependencies
- **Solution**: Remove any external packages from Core, move implementations to Infrastructure

**Issue**: Circular reference detected
- **Solution**: Review and fix project references - Core should never reference other projects

**Issue**: Packages show vulnerabilities
- **Solution**: Update to latest stable versions or document accepted risk

## Documentation

Document your validation results:

```markdown
## Architecture Validation Results

Date: [YYYY-MM-DD]
Validator: [Your Name/AI Agent]

### Dependency Graph
✅ WebApp → Infrastructure → Core

### Package Audit
✅ No vulnerabilities found

### Build Status
✅ Clean build successful

### Notes
[Any warnings or observations]
```

Save this to `docs/validation-reports/01-setup-validation.md` for future reference.

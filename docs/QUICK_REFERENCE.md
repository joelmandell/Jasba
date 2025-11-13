# SBA Pro - Quick Reference for AI Agents

## 🎯 Primary Objective

Build a secure, multi-tenant fire safety management system following the organized instructions in `/docs/instructions/`.

## 📋 Golden Rules

1. **Sequential Only**: Complete files 1-30 in exact order
2. **Validate Everything**: Every file ends with validation - complete it
3. **No Hallucinations**: Only implement what's specified
4. **Tests Are Mandatory**: Not optional, especially multi-tenancy tests
5. **Security First**: Multi-tenancy isolation is critical

## 🚀 Getting Started

```bash
# Start here
cat docs/instructions/README.md

# Then proceed to
docs/instructions/01-setup/01-project-structure.md
```

## 📂 File Order (30 Files Total)

### Setup (1-3)
1. Project structure
2. Dependencies  
3. Architecture validation

### Core Layer (4-6)
4. Domain entities
5. Service interfaces
6. Core validation

### Infrastructure (7-12)
7. Database context
8. Tenant service
9. Database seeding
10. Email service
11. PDF service
12. Infrastructure validation

### WebApp (13-22)
13. Identity setup
14. Authentication pages
15. Admin tenant management
16. Tenant site management
17. Tenant floor plan management
18. Leaflet integration
19. Object placement
20. Inspection rounds
21. Report generation
22. WebApp validation

### Advanced (23-25)
23. Offline capability
24. Email reminders
25. Advanced validation

### Testing (26-30)
26. Test setup
27. Core tests
28. Infrastructure tests
29. Integration tests
30. Test validation

## ✅ Validation Commands

### After Each File
```bash
# Build
dotnet build

# Run tests
dotnet test

# Check dependencies
dotnet list reference
dotnet list package
```

### Critical Multi-Tenancy Check
```bash
# After file #12
dotnet test --filter "MultiTenancy"
```

## 🏗️ Architecture Rules

```
WebApp → Infrastructure → Core
  ↓           ↓            ↓
 Many       Some         NONE
(external dependencies)
```

**Core Rules:**
- ❌ No EF Core in Core
- ❌ No ASP.NET in Core
- ❌ No external packages in Core
- ✅ Only domain entities and interfaces

**Infrastructure Rules:**
- ✅ Implements Core interfaces
- ✅ Has EF Core, MailKit packages
- ❌ No Blazor components

**WebApp Rules:**
- ✅ Blazor components and pages
- ✅ References Infrastructure
- ❌ No direct database access

## 🔐 Security Checklist

### Multi-Tenancy (CRITICAL)
- [ ] Global query filters on all tenant entities
- [ ] TenantId automatically set on save
- [ ] Cross-tenant queries return empty
- [ ] Tests verify tenant isolation

### Authorization
- [ ] SystemAdmin: Full system access
- [ ] TenantAdmin: Own tenant only
- [ ] Inspector: Create inspections only

### Data Validation
- [ ] Email addresses validated
- [ ] File uploads checked (type, size)
- [ ] Required fields enforced
- [ ] SQL injection prevented (use EF Core)

## 🧪 Testing Requirements

### Coverage Targets
- Core: 90%+
- Infrastructure: 80%+
- WebApp: 60%+
- Overall: 70%+

### Critical Tests
```csharp
// MUST HAVE: Multi-tenancy isolation
[Fact]
public async Task QueryFilter_OnlyReturnsTenantData()

// MUST HAVE: Automatic TenantId
[Fact]
public async Task SaveChanges_SetsTenantIdAutomatically()

// MUST HAVE: Authorization
[Fact]
public async Task TenantAdmin_CannotAccessOtherTenant()
```

## 🐛 Common Errors

### Error: Core references Infrastructure
**Fix**: Remove reference, move code to Infrastructure

### Error: Multi-tenancy test fails
**Fix**: Check global query filter in ApplicationDbContext

### Error: Build fails with missing types
**Fix**: Check using statements and project references

### Error: Tests not discovered
**Fix**: Ensure [Fact] attribute and public class

## 📊 Progress Tracking

```markdown
Current Phase: [1-6]
Current File: [1-30]
Files Completed: [X/30]
Tests Passing: [X/Y]
Coverage: [X%]
```

## 🔄 Typical File Workflow

```
1. Read instruction file
2. Implement EXACTLY what's specified
3. Build and check for errors
4. Run validation steps
5. Run tests
6. Fix any failures
7. Mark file complete
8. Proceed to next file
```

## 📝 Key Files Location

### Instructions
- Master: `docs/instructions/README.md`
- Guide: `docs/INSTRUCTIONS_GUIDE.md`
- This file: `docs/QUICK_REFERENCE.md`

### Original Spec
- `SPECIFICATIONS.md` (reference only)

### Projects
- `src/SBAPro.Core/` - Domain
- `src/SBAPro.Infrastructure/` - Services  
- `src/SBAPro.WebApp/` - UI

### Tests
- `tests/SBAPro.Core.Tests/`
- `tests/SBAPro.Infrastructure.Tests/`
- `tests/SBAPro.WebApp.Tests/`

## 🎓 Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| Framework | .NET 9.0 | Platform |
| UI | Blazor Server | Web interface |
| Database | SQLite | Development DB |
| ORM | EF Core | Data access |
| Identity | ASP.NET Identity | Auth |
| Maps | Leaflet.js | Floor plans |
| PDF | QuestPDF | Reports |
| Email | MailKit | Notifications |
| Testing | xUnit + Moq | Tests |

## 🚨 Red Flags

Stop and review if you see:

- ❌ `using SBAPro.Infrastructure` in Core project
- ❌ Database access in WebApp (use services instead)
- ❌ Hardcoded TenantId values
- ❌ Missing validation steps
- ❌ Skipped test files
- ❌ No [Fact] or [Theory] in test files
- ❌ Copy-paste without understanding

## 💡 Success Indicators

You're on track when:

- ✅ Each file builds successfully
- ✅ All validations pass
- ✅ Tests are green
- ✅ Multi-tenancy tests pass
- ✅ No dependency violations
- ✅ Following instructions exactly
- ✅ Progress is steady

## 📞 Need Help?

1. Re-read current instruction file
2. Check INSTRUCTIONS_GUIDE.md
3. Review SPECIFICATIONS.md for context
4. Check troubleshooting section in instruction file
5. Verify prerequisites were completed

## ⏱️ Time Estimates

| Phase | Files | Time |
|-------|-------|------|
| Setup | 1-3 | 30 min |
| Core | 4-6 | 1-2 hrs |
| Infrastructure | 7-12 | 3-4 hrs |
| WebApp | 13-22 | 6-8 hrs |
| Advanced | 23-25 | 4-6 hrs |
| Testing | 26-30 | 4-6 hrs |
| **Total** | **30** | **~20-25 hrs** |

## 🎯 Final Validation

Before marking complete:

```bash
# All tests pass
dotnet test
# Expected: 100% pass rate

# No vulnerabilities
dotnet list package --vulnerable
# Expected: No vulnerabilities found

# Good coverage
dotnet test --collect:"XPlat Code Coverage"
# Expected: ≥70% overall

# Clean build
dotnet clean && dotnet build
# Expected: No errors or warnings

# Multi-tenancy verified
dotnet test --filter "MultiTenancy"
# Expected: All isolation tests pass
```

## 🏁 Completion Criteria

Project is complete when:

- ✅ All 30 instruction files completed
- ✅ All validation steps passed
- ✅ Test coverage ≥ 70%
- ✅ Multi-tenancy isolation verified
- ✅ All three user roles functional
- ✅ PDF reports generate correctly
- ✅ Email service configured
- ✅ No security vulnerabilities
- ✅ Clean Architecture maintained

## 📌 Remember

> "Follow the instructions exactly. Validate everything. Test all the things. Multi-tenancy security is non-negotiable."

Good luck! 🚀

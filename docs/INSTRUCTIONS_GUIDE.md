# SBA Pro - Complete Instructions Guide for Agentic AI

## Document Purpose

This guide explains how to use the organized instruction files in `/docs/instructions/` to build the SBA Pro application using agentic AI. The instructions are designed to:

1. **Prevent Hallucinations**: Each step is explicit with validation
2. **Enable Sequential Building**: Clear dependencies and order
3. **Ensure Testability**: Testing is integrated throughout
4. **Maintain Consistency**: Follow Clean Architecture principles
5. **Support Future Extensions**: Well-documented, maintainable code

## Quick Start

### For AI Agents

1. Start with `/docs/instructions/README.md` - Read this first
2. Follow the instruction files in numerical order
3. Complete all validation steps before proceeding
4. Write tests as you implement features
5. Report progress after completing each phase

### For Human Developers

1. Review the master `/docs/instructions/README.md`
2. Use instructions as a checklist for implementation
3. Adapt instructions as needed for your environment
4. Maintain the validation and testing discipline

## Instruction Structure

### File Organization

```
docs/instructions/
├── README.md                          # Master index (START HERE)
├── 01-setup/                          # Phase 1: Project Setup
│   ├── 01-project-structure.md
│   ├── 02-dependencies.md
│   └── 03-architecture-validation.md
├── 02-core-layer/                     # Phase 2: Domain Model
│   ├── 01-domain-entities.md
│   ├── 02-interfaces.md
│   └── 03-core-validation.md
├── 03-infrastructure-layer/           # Phase 3: Data & Services
│   ├── README.md
│   ├── 01-database-context.md
│   ├── 02-tenant-service.md
│   ├── 03-database-seeding.md
│   ├── 04-email-service.md
│   ├── 05-pdf-service.md
│   └── 06-infrastructure-validation.md
├── 04-webapp-layer/                   # Phase 4: User Interface
│   ├── README.md
│   ├── 01-identity-setup.md
│   ├── 02-authentication-pages.md
│   ├── 03-admin-tenant-management.md
│   ├── 04-tenant-site-management.md
│   ├── 05-tenant-floorplan-management.md
│   ├── 06-leaflet-integration.md
│   ├── 07-object-placement.md
│   ├── 08-inspection-rounds.md
│   ├── 09-report-generation.md
│   └── 10-webapp-validation.md
├── 05-advanced-features/              # Phase 5: Advanced Features
│   ├── README.md
│   ├── 01-offline-capability.md
│   ├── 02-email-reminders.md
│   └── 03-advanced-validation.md
└── 06-testing/                        # Phase 6: Comprehensive Testing
    ├── README.md
    ├── 01-unit-test-setup.md
    ├── 02-core-tests.md
    ├── 03-infrastructure-tests.md
    ├── 04-integration-tests.md
    └── 05-test-validation.md
```

### File Format

Each instruction file follows this format:

```markdown
# [Phase]-[Module]: [Title]

## Objective
Clear statement of what will be accomplished

## Prerequisites
What must be completed before starting

## Instructions
Step-by-step implementation details with code examples

## Validation Steps
How to verify the implementation is correct

## Success Criteria
Checklist of requirements to proceed

## Next Steps
What to do after completing this file
```

## Implementation Sequence

### Phase 1: Setup (Files 1-3)
**Time Estimate**: 30 minutes  
**Goal**: Create solution structure with proper Clean Architecture

- Initialize solution and three projects (Core, Infrastructure, WebApp)
- Install all required NuGet packages
- Validate architecture and dependencies

**Critical Success Factor**: Core layer has NO external dependencies

### Phase 2: Core Layer (Files 4-6)
**Time Estimate**: 1-2 hours  
**Goal**: Define domain model and service interfaces

- Create 8 domain entities (Tenant, Site, FloorPlan, etc.)
- Define 4 service interfaces (ITenantService, IEmailService, etc.)
- Validate core layer completeness

**Critical Success Factor**: All entities and relationships properly defined

### Phase 3: Infrastructure Layer (Files 7-12)
**Time Estimate**: 3-4 hours  
**Goal**: Implement data access and external services

- Create ApplicationDbContext with multi-tenancy
- Implement TenantService for tenant isolation
- Create database seed data
- Implement email service with MailKit
- Implement PDF generation with QuestPDF
- Validate all services

**Critical Success Factor**: Multi-tenancy isolation works correctly

### Phase 4: WebApp Layer (Files 13-22)
**Time Estimate**: 6-8 hours  
**Goal**: Build Blazor UI for all user roles

- Configure ASP.NET Core Identity
- Create authentication pages (login/logout)
- Build SystemAdmin pages (tenant management)
- Build TenantAdmin pages (site, floor plan management)
- Integrate Leaflet.js for interactive maps
- Build Inspector pages (inspection rounds)
- Implement report download
- Validate all UI features

**Critical Success Factor**: All three user roles can access their features

### Phase 5: Advanced Features (Files 23-25)
**Time Estimate**: 4-6 hours  
**Goal**: Add offline capability and automation

- Implement offline-first data capture with CacheStorage
- Create background service for email reminders
- Validate advanced features

**Critical Success Factor**: Offline mode works and syncs correctly

### Phase 6: Testing (Files 26-30)
**Time Estimate**: 4-6 hours  
**Goal**: Comprehensive test coverage

- Setup test projects with xUnit, Moq, FluentAssertions
- Write unit tests for Core layer
- Write unit tests for Infrastructure layer
- Write integration tests for workflows
- Validate test coverage (70%+ target)

**Critical Success Factor**: Multi-tenancy isolation tests pass

## Key Principles for AI Implementation

### 1. Sequential Execution (MANDATORY)

❌ **DON'T**: Skip files or work out of order  
✅ **DO**: Complete each file before proceeding to next

**Why**: Each file builds on previous work. Skipping creates gaps.

### 2. Validation Gates (MANDATORY)

❌ **DON'T**: Proceed if validation fails  
✅ **DO**: Fix all issues before moving forward

**Why**: Issues compound. Fix problems immediately.

### 3. Minimal Changes Only

❌ **DON'T**: Add extra features or "improvements"  
✅ **DO**: Implement exactly what the instruction specifies

**Why**: Extra features introduce bugs and complexity.

### 4. Test-Driven Development

❌ **DON'T**: Skip writing tests  
✅ **DO**: Write tests for all business logic

**Why**: Tests prevent regressions and verify correctness.

### 5. Clean Architecture Compliance

❌ **DON'T**: Violate dependency rules  
✅ **DO**: Maintain proper layer separation

**Why**: Architecture violations make code unmaintainable.

## Critical Security Checkpoints

### Multi-Tenancy Isolation (HIGHEST PRIORITY)

**When**: After completing Infrastructure layer  
**Test**: Verify one tenant cannot access another's data

```csharp
// This test MUST pass
[Fact]
public async Task QueryFilter_PreventsAccessToOtherTenantData()
{
    var tenant1 = Guid.NewGuid();
    var tenant2 = Guid.NewGuid();
    
    // Create data for both tenants
    context.Sites.Add(new Site { TenantId = tenant1, Name = "Tenant 1" });
    context.Sites.Add(new Site { TenantId = tenant2, Name = "Tenant 2" });
    await context.SaveChangesAsync();
    
    // Switch to tenant1 context
    mockTenantService.Setup(x => x.GetTenantId()).Returns(tenant1);
    
    // Query should only return tenant1 data
    var sites = await context.Sites.ToListAsync();
    Assert.Single(sites);
    Assert.Equal(tenant1, sites[0].TenantId);
}
```

### Authorization Checks

**When**: After completing WebApp layer  
**Test**: Verify role-based access control works

- SystemAdmin can create tenants
- TenantAdmin cannot access other tenants
- Inspector cannot modify floor plans

### Input Validation

**When**: Throughout implementation  
**Test**: Verify all user inputs are validated

- Email addresses are valid
- File uploads are checked (size, type)
- Required fields are enforced

## Common Pitfalls and Solutions

### Pitfall 1: Breaking Multi-Tenancy

**Problem**: Forgetting to add TenantId to entity  
**Solution**: Follow 02-core-layer/01-domain-entities.md exactly

**Problem**: Query bypasses global filter  
**Solution**: Always use DbContext, never raw SQL

### Pitfall 2: Wrong Dependency Direction

**Problem**: Core references Infrastructure  
**Solution**: Run validation after each phase

**Problem**: Infrastructure depends on WebApp  
**Solution**: Check dependencies with `dotnet list reference`

### Pitfall 3: Skipping Tests

**Problem**: "Will write tests later" (never happens)  
**Solution**: Write tests immediately after implementing

**Problem**: Tests don't test multi-tenancy  
**Solution**: Follow 06-testing/03-infrastructure-tests.md

### Pitfall 4: Hardcoded Configuration

**Problem**: SMTP credentials in code  
**Solution**: Use appsettings.json and IConfiguration

**Problem**: Database connection string committed  
**Solution**: Use User Secrets for development

### Pitfall 5: Not Validating

**Problem**: Skipping validation steps  
**Solution**: Complete every validation checklist

**Problem**: Proceeding despite failures  
**Solution**: Fix all issues before continuing

## Progress Tracking

### Completion Checklist

Use this to track overall progress:

**Phase 1: Setup**
- [ ] Project structure created
- [ ] Dependencies installed
- [ ] Architecture validated

**Phase 2: Core Layer**
- [ ] Domain entities created
- [ ] Service interfaces defined
- [ ] Core layer validated

**Phase 3: Infrastructure**
- [ ] Database context implemented
- [ ] Tenant service implemented
- [ ] Database seeded
- [ ] Email service implemented
- [ ] PDF service implemented
- [ ] Infrastructure validated

**Phase 4: WebApp**
- [ ] Identity configured
- [ ] Authentication implemented
- [ ] Admin UI complete
- [ ] Tenant UI complete
- [ ] Inspector UI complete
- [ ] WebApp validated

**Phase 5: Advanced**
- [ ] Offline capability implemented
- [ ] Email reminders implemented
- [ ] Advanced features validated

**Phase 6: Testing**
- [ ] Test projects setup
- [ ] Core tests written
- [ ] Infrastructure tests written
- [ ] Integration tests written
- [ ] Test coverage validated

### Quality Gates

Before considering the project complete:

- [ ] All instruction files completed in order
- [ ] All validation steps passed
- [ ] Test coverage ≥ 70%
- [ ] Multi-tenancy isolation verified
- [ ] No security vulnerabilities
- [ ] All three user roles functional
- [ ] Documentation updated

## Extending the Application

Once the base implementation is complete, these extensions can be added:

### Database Migration
- Replace SQLite with PostgreSQL or SQL Server
- Move file storage to Azure Blob Storage or AWS S3
- Add database connection pooling

### Advanced Features
- Add mobile app (Blazor Hybrid or native)
- Implement real-time notifications (SignalR)
- Add audit logging for compliance
- Implement advanced reporting and analytics

### Localization
- Add Swedish and English language support
- Localize date/time formats
- Support multiple currencies

### Performance
- Add Redis caching layer
- Implement query result caching
- Add application monitoring (Application Insights)

## Getting Help

### If Instructions Are Unclear

1. Check the original SPECIFICATIONS.md for context
2. Review related instruction files
3. Check code comments in existing implementation
4. Consult technology documentation (EF Core, Blazor, etc.)

### If Validation Fails

1. Read error messages carefully
2. Check previous instruction files for missed steps
3. Verify all prerequisites are met
4. Review troubleshooting sections
5. Run validation commands again

### If Tests Fail

1. Read test failure message
2. Check if issue is in test or implementation
3. Verify test data setup is correct
4. Check mock configurations
5. Run single test to isolate issue

## Maintenance

### Keeping Instructions Updated

As the application evolves:

1. Update instruction files when making architectural changes
2. Add new instruction files for new features
3. Keep validation steps current
4. Update prerequisites and dependencies
5. Revise time estimates based on experience

### Version Control

- Keep instructions in sync with code
- Document breaking changes
- Maintain changelog for instructions
- Tag instruction versions with code releases

## Conclusion

These instructions provide a comprehensive, step-by-step guide to building SBA Pro with agentic AI. The key to success is:

1. **Follow the sequence** - Don't skip ahead
2. **Validate everything** - Test before proceeding  
3. **Maintain discipline** - Stick to the minimal changes
4. **Focus on quality** - Tests and security are not optional
5. **Document as you go** - Keep instructions current

By following these instructions carefully, you will build a secure, maintainable, and legally compliant fire safety management system.

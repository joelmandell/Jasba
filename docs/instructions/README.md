# SBA Pro - Agentic AI Build Instructions

## Overview

This directory contains sequential, modular instructions for building the SBA Pro application using agentic AI. The instructions are organized to minimize hallucinations and ensure consistent, testable implementation.

## Build Sequence

Follow these instruction sets **in order**. Each module builds upon the previous ones and includes validation steps.

### Phase 1: Project Setup & Architecture (01-setup/)
1. **01-project-structure.md** - Initialize solution and project structure
2. **02-dependencies.md** - Install and configure NuGet packages
3. **03-architecture-validation.md** - Verify project setup and references

### Phase 2: Core Layer (02-core-layer/)
4. **01-domain-entities.md** - Define core domain entities
5. **02-interfaces.md** - Define service interfaces
6. **03-core-validation.md** - Validate core layer structure

### Phase 3: Infrastructure Layer (03-infrastructure-layer/)
7. **01-database-context.md** - Configure Entity Framework and multi-tenancy
8. **02-tenant-service.md** - Implement tenant isolation service
9. **03-database-seeding.md** - Implement initial data seeding
10. **04-email-service.md** - Implement MailKit email service
11. **05-pdf-service.md** - Implement QuestPDF report generation
12. **06-infrastructure-validation.md** - Validate infrastructure layer

### Phase 4: Web Application Layer (04-webapp-layer/)
13. **01-identity-setup.md** - Configure ASP.NET Core Identity
14. **02-authentication-pages.md** - Implement login/logout pages
15. **03-admin-tenant-management.md** - System admin tenant management UI
16. **04-tenant-site-management.md** - Tenant admin site management UI
17. **05-tenant-floorplan-management.md** - Floor plan upload and management UI
18. **06-leaflet-integration.md** - Integrate Leaflet.js for floor plans
19. **07-object-placement.md** - Interactive object placement on floor plans
20. **08-inspection-rounds.md** - Inspector inspection round execution
21. **09-report-generation.md** - PDF report download functionality
22. **10-webapp-validation.md** - Validate web application features

### Phase 5: Advanced Features (05-advanced-features/)
23. **01-offline-capability.md** - Implement offline-first data capture
24. **02-email-reminders.md** - Implement background service for reminders
25. **03-advanced-validation.md** - Validate advanced features

### Phase 6: Testing (06-testing/)
26. **01-unit-test-setup.md** - Setup unit test projects
27. **02-core-tests.md** - Unit tests for Core layer
28. **03-infrastructure-tests.md** - Unit tests for Infrastructure layer
29. **04-integration-tests.md** - Integration tests for key workflows
30. **05-test-validation.md** - Run and validate all tests

## Principles for Agentic AI Implementation

### 1. Sequential Execution
- Complete each file in order
- Do not skip validation steps
- Each module must pass validation before proceeding

### 2. Minimal Changes
- Implement only what's specified in each instruction file
- Don't add features not explicitly mentioned
- Follow the existing code style

### 3. Validation After Each Module
- Each module ends with validation steps
- Run builds and tests after each module
- Fix issues before proceeding to next module

### 4. Testing Requirements
- Write unit tests for business logic
- Write integration tests for database operations
- Validate multi-tenancy isolation in tests
- Achieve minimum 70% code coverage

### 5. Error Prevention
- Follow Clean Architecture principles strictly
- Maintain proper dependency direction (WebApp → Infrastructure → Core)
- Use interfaces for all external dependencies
- Implement proper error handling and logging

### 6. Documentation
- Keep code comments minimal but meaningful
- Document public APIs
- Update README.md with any configuration changes

## Validation Checklist

After completing all modules, verify:

- [ ] Solution builds without errors
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Database migrations are up to date
- [ ] Multi-tenancy isolation is working correctly
- [ ] All three user roles (SystemAdmin, TenantAdmin, Inspector) function properly
- [ ] PDF reports generate correctly
- [ ] Email service is configured (even if using test SMTP)
- [ ] Floor plan upload and visualization works
- [ ] Inspection rounds can be completed end-to-end

## Technology Stack Reference

- **Framework**: .NET 9.0
- **UI**: Blazor Server
- **ORM**: Entity Framework Core
- **Database**: SQLite (development), ready for PostgreSQL/SQL Server (production)
- **Identity**: ASP.NET Core Identity
- **Mapping**: Leaflet.js
- **PDF**: QuestPDF
- **Email**: MailKit

## Key Architectural Decisions

### Multi-Tenancy Strategy
- Single database, shared tables
- TenantId discriminator column
- Global query filters in EF Core
- Scoped ITenantService for tenant context

### Clean Architecture Layers
- **Core**: Domain entities and interfaces (no external dependencies)
- **Infrastructure**: Concrete implementations of Core interfaces
- **WebApp**: Blazor Server UI components and pages

### Security
- Role-based access control (SystemAdmin, TenantAdmin, Inspector)
- Data isolation through global query filters
- ASP.NET Core Identity for authentication

## Common Pitfalls to Avoid

1. **Breaking Multi-Tenancy**: Always use ITenantService, never hardcode tenant IDs
2. **Wrong Dependency Direction**: Core must not reference Infrastructure or WebApp
3. **Skipping Validation**: Each module has validation steps - don't skip them
4. **Hardcoding Configuration**: Use appsettings.json and IConfiguration
5. **Missing Error Handling**: Always handle potential null references and exceptions
6. **Ignoring Tests**: Tests are not optional - they ensure correctness

## Future Extensions

The architecture supports:
- Migration to cloud database (PostgreSQL/SQL Server)
- Azure Blob Storage for floor plan images
- Advanced conflict resolution for offline sync
- Multiple languages/localization
- Mobile-specific UI optimizations
- Advanced reporting and analytics

## Support

For questions about these instructions:
1. Review the original SPECIFICATIONS.md for detailed context
2. Check the README.md for user-facing documentation
3. Refer to the code comments in implemented features

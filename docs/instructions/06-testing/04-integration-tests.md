# 06-Testing: Integration Tests

## Objective
Implement integration tests for end-to-end workflows across all layers.

## Prerequisites
- Completed: 06-testing/03-infrastructure-tests.md
- All phases completed

## Overview

Integration tests validate:
- Complete user workflows
- Cross-layer integration
- Database operations
- Authentication/authorization
- Multi-tenancy in practice

## Instructions

### 1. Create Test Project

```bash
dotnet new xunit -n SBAPro.Integration.Tests
dotnet sln add tests/SBAPro.Integration.Tests
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

### 2. WebApplicationFactory Setup

**File**: `tests/SBAPro.Integration.Tests/CustomWebApplicationFactory.cs`

```csharp
public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real database with test database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
        });
    }
}
```

### 3. End-to-End Workflow Tests

**File**: `tests/SBAPro.Integration.Tests/InspectionWorkflowTests.cs`

```csharp
public class InspectionWorkflowTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public InspectionWorkflowTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompleteInspectionWorkflow_Success()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act & Assert

        // 1. Login as TenantAdmin
        // 2. Create site
        // 3. Upload floor plan
        // 4. Place objects
        // 5. Login as Inspector
        // 6. Start inspection round
        // 7. Mark objects
        // 8. Complete round
        // 9. Download PDF

        // Each step should succeed
    }

    [Fact]
    public async Task CrossTenantAccess_Blocked()
    {
        // Arrange
        // Create two tenants with data

        // Act
        // Try to access other tenant's data

        // Assert
        // Should be denied
    }
}
```

### 4. Multi-Tenancy Integration Tests

```csharp
public class MultiTenancyIntegrationTests
{
    [Fact]
    public async Task TwoTenants_DataIsolated()
    {
        // Create two tenants
        // Each creates sites
        // Verify each tenant only sees their own sites
    }
}
```

## Validation

```bash
dotnet test tests/SBAPro.Integration.Tests
```

Expected: All integration tests pass

## Success Criteria

✅ Integration tests implemented  
✅ End-to-end workflows tested  
✅ Multi-tenancy verified  
✅ Authentication tested  
✅ All tests pass  

## Next Steps

After all tests complete:
1. Review code coverage
2. Document any issues found
3. System is ready for deployment

## Final Validation

Run all tests:

```bash
dotnet test
```

Expected: All unit and integration tests pass

Generate coverage report:

```bash
dotnet test /p:CollectCoverage=true
```

Expected: Coverage > 70%

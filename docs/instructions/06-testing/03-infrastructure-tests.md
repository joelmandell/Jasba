# 06-Testing: Infrastructure Layer Unit Tests

## Objective
Implement unit tests for Infrastructure services and data access.

## Prerequisites
- Completed: 06-testing/02-core-tests.md
- Phase 3 (Infrastructure) completed

## Overview

Infrastructure tests focus on:
- Service implementations
- Database access
- Multi-tenancy isolation
- External service mocking

## Instructions

### 1. Create Test Project

```bash
dotnet new xunit -n SBAPro.Infrastructure.Tests
dotnet sln add tests/SBAPro.Infrastructure.Tests
cd tests/SBAPro.Infrastructure.Tests
dotnet add reference ../../src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### 2. TenantService Tests

**File**: `tests/SBAPro.Infrastructure.Tests/Services/TenantServiceTests.cs`

```csharp
public class TenantServiceTests
{
    [Fact]
    public void GetTenantId_WithTenantIdClaim_ReturnsTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim("TenantId", tenantId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext { User = user };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        
        var service = new TenantService(accessor);

        // Act
        var result = service.GetTenantId();

        // Assert
        result.Should().Be(tenantId);
    }

    [Fact]
    public void GetTenantId_WithoutClaim_ReturnsEmptyGuid()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var service = new TenantService(accessor);

        // Act
        var result = service.GetTenantId();

        // Assert
        result.Should().Be(Guid.Empty);
    }
}
```

### 3. Database Context Tests

**File**: `tests/SBAPro.Infrastructure.Tests/Data/ApplicationDbContextTests.cs`

```csharp
public class ApplicationDbContextTests
{
    [Fact]
    public async Task QueryFilter_PreventsCrossTenantAccess()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        var tenantService = new Mock<ITenantService>();
        tenantService.Setup(x => x.GetTenantId()).Returns(tenant1);

        var context = new ApplicationDbContext(options, tenantService.Object);

        // Add data for two tenants
        context.Sites.Add(new Site { Id = Guid.NewGuid(), Name = "Site 1", TenantId = tenant1 });
        context.Sites.Add(new Site { Id = Guid.NewGuid(), Name = "Site 2", TenantId = tenant2 });
        await context.SaveChangesAsync();

        // Act
        var sites = await context.Sites.ToListAsync();

        // Assert
        sites.Should().HaveCount(1);
        sites[0].TenantId.Should().Be(tenant1);
    }
}
```

### 4. Email Service Tests

```csharp
public class MailKitEmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_WithValidConfig_SendsEmail()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Email:SmtpServer"] = "localhost",
                ["Email:SmtpPort"] = "1025"
            })
            .Build();

        var service = new MailKitEmailService(config);

        // Act
        Func<Task> act = async () => await service.SendEmailAsync(
            "test@example.com", 
            "Test", 
            "Body");

        // Assert
        // With MailHog running, this should not throw
        await act.Should().NotThrowAsync();
    }
}
```

## Validation

```bash
dotnet test tests/SBAPro.Infrastructure.Tests
```

Expected: All tests pass

## Success Criteria

✅ Infrastructure tests implemented  
✅ Multi-tenancy tests pass  
✅ Service tests cover all services  
✅ Database tests validate isolation  
✅ Code coverage > 70%  

## Next Steps

Proceed to 04-integration-tests.md

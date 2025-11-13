# 06-Testing: Unit Test Setup

## Objective
Set up comprehensive unit testing infrastructure for the SBA Pro application.

## Prerequisites
- Completed: Phase 2 (Core Layer)
- Understanding of unit testing principles
- Familiarity with xUnit testing framework

## Testing Strategy

### Test Projects Structure

Create separate test projects for each layer:
- **SBAPro.Core.Tests**: Unit tests for domain entities and logic
- **SBAPro.Infrastructure.Tests**: Unit tests for service implementations
- **SBAPro.WebApp.Tests**: Unit tests for Blazor components and pages

### Testing Frameworks

- **xUnit**: Primary testing framework (modern, extensible)
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Readable assertion library
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing

## Instructions

### 1. Create Test Projects

```bash
# Create Core tests
dotnet new xunit -n SBAPro.Core.Tests -o tests/SBAPro.Core.Tests

# Create Infrastructure tests
dotnet new xunit -n SBAPro.Infrastructure.Tests -o tests/SBAPro.Infrastructure.Tests

# Create WebApp tests
dotnet new xunit -n SBAPro.WebApp.Tests -o tests/SBAPro.WebApp.Tests
```

### 2. Add Test Projects to Solution

```bash
dotnet sln SBAPro.sln add tests/SBAPro.Core.Tests/SBAPro.Core.Tests.csproj
dotnet sln SBAPro.sln add tests/SBAPro.Infrastructure.Tests/SBAPro.Infrastructure.Tests.csproj
dotnet sln SBAPro.sln add tests/SBAPro.WebApp.Tests/SBAPro.WebApp.Tests.csproj
```

### 3. Add Project References

Each test project needs to reference the project it tests:

```bash
# Core tests reference Core
dotnet add tests/SBAPro.Core.Tests/SBAPro.Core.Tests.csproj reference src/SBAPro.Core/SBAPro.Core.csproj

# Infrastructure tests reference Infrastructure (and transitively Core)
dotnet add tests/SBAPro.Infrastructure.Tests/SBAPro.Infrastructure.Tests.csproj reference src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj

# WebApp tests reference WebApp (and transitively Infrastructure and Core)
dotnet add tests/SBAPro.WebApp.Tests/SBAPro.WebApp.Tests.csproj reference src/SBAPro.WebApp/SBAPro.WebApp.csproj
```

### 4. Install Testing Packages

Install necessary NuGet packages for all test projects:

```bash
# Moq for mocking dependencies
dotnet add tests/SBAPro.Core.Tests/SBAPro.Core.Tests.csproj package Moq
dotnet add tests/SBAPro.Infrastructure.Tests/SBAPro.Infrastructure.Tests.csproj package Moq
dotnet add tests/SBAPro.WebApp.Tests/SBAPro.WebApp.Tests.csproj package Moq

# FluentAssertions for readable assertions
dotnet add tests/SBAPro.Core.Tests/SBAPro.Core.Tests.csproj package FluentAssertions
dotnet add tests/SBAPro.Infrastructure.Tests/SBAPro.Infrastructure.Tests.csproj package FluentAssertions
dotnet add tests/SBAPro.WebApp.Tests/SBAPro.WebApp.Tests.csproj package FluentAssertions

# In-memory database for Infrastructure and WebApp tests
dotnet add tests/SBAPro.Infrastructure.Tests/SBAPro.Infrastructure.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory
dotnet add tests/SBAPro.WebApp.Tests/SBAPro.WebApp.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory

# bUnit for Blazor component testing
dotnet add tests/SBAPro.WebApp.Tests/SBAPro.WebApp.Tests.csproj package bunit
```

### 5. Create Test Base Classes

Create helper classes for common test setup:

#### Core Tests Base

**File**: `tests/SBAPro.Core.Tests/TestBase.cs`

```csharp
using Xunit.Abstractions;

namespace SBAPro.Core.Tests;

/// <summary>
/// Base class for Core layer unit tests.
/// Provides common test utilities and helpers.
/// </summary>
public abstract class TestBase
{
    protected readonly ITestOutputHelper Output;

    protected TestBase(ITestOutputHelper output)
    {
        Output = output;
    }

    /// <summary>
    /// Writes a message to test output for debugging.
    /// </summary>
    protected void WriteLine(string message)
    {
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
    }
}
```

#### Infrastructure Tests Base

**File**: `tests/SBAPro.Infrastructure.Tests/InfrastructureTestBase.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using SBAPro.Core.Interfaces;
using SBAPro.Infrastructure.Data;
using Moq;
using Xunit.Abstractions;

namespace SBAPro.Infrastructure.Tests;

/// <summary>
/// Base class for Infrastructure layer tests.
/// Provides in-memory database and common mocks.
/// </summary>
public abstract class InfrastructureTestBase : IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected readonly ApplicationDbContext DbContext;
    protected readonly Mock<ITenantService> MockTenantService;
    protected readonly Guid TestTenantId;

    protected InfrastructureTestBase(ITestOutputHelper output)
    {
        Output = output;
        TestTenantId = Guid.NewGuid();

        // Setup mock tenant service
        MockTenantService = new Mock<ITenantService>();
        MockTenantService.Setup(x => x.GetTenantId()).Returns(TestTenantId);
        MockTenantService.Setup(x => x.TryGetTenantId()).Returns(TestTenantId);

        // Create in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new ApplicationDbContext(options, MockTenantService.Object);
    }

    protected void WriteLine(string message)
    {
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }
}
```

#### WebApp Tests Base

**File**: `tests/SBAPro.WebApp.Tests/WebAppTestBase.cs`

```csharp
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SBAPro.Core.Interfaces;
using Moq;
using Xunit.Abstractions;

namespace SBAPro.WebApp.Tests;

/// <summary>
/// Base class for WebApp layer tests.
/// Provides Blazor test context and common service mocks.
/// </summary>
public abstract class WebAppTestBase : TestContextWrapper, IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected readonly Mock<ITenantService> MockTenantService;
    protected readonly Mock<IEmailService> MockEmailService;
    protected readonly Mock<IReportGenerator> MockReportGenerator;
    protected readonly Guid TestTenantId;

    protected WebAppTestBase(ITestOutputHelper output)
    {
        Output = output;
        TestTenantId = Guid.NewGuid();

        // Setup common mocks
        MockTenantService = new Mock<ITenantService>();
        MockTenantService.Setup(x => x.GetTenantId()).Returns(TestTenantId);

        MockEmailService = new Mock<IEmailService>();
        MockReportGenerator = new Mock<IReportGenerator>();

        // Register mocks in test services
        Services.AddSingleton(MockTenantService.Object);
        Services.AddSingleton(MockEmailService.Object);
        Services.AddSingleton(MockReportGenerator.Object);
    }

    protected void WriteLine(string message)
    {
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
    }

    public new void Dispose()
    {
        base.Dispose();
    }
}
```

### 6. Create Sample Tests

Create sample test files to verify setup:

#### Core Sample Test

**File**: `tests/SBAPro.Core.Tests/Entities/TenantTests.cs`

```csharp
using FluentAssertions;
using SBAPro.Core.Entities;
using Xunit;
using Xunit.Abstractions;

namespace SBAPro.Core.Tests.Entities;

public class TenantTests : TestBase
{
    public TenantTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Tenant_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Company"
        };

        // Assert
        tenant.Id.Should().NotBe(Guid.Empty);
        tenant.Name.Should().Be("Test Company");
        tenant.IsActive.Should().BeTrue();
        tenant.Users.Should().BeEmpty();
        tenant.Sites.Should().BeEmpty();
        WriteLine($"Created tenant: {tenant.Name}");
    }

    [Fact]
    public void Tenant_CreatedAt_ShouldBeRecentTime()
    {
        // Arrange & Act
        var beforeCreation = DateTime.UtcNow;
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test" };
        var afterCreation = DateTime.UtcNow;

        // Assert
        tenant.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        tenant.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }
}
```

### 7. Remove Default Test Files

```bash
rm -f tests/SBAPro.Core.Tests/UnitTest1.cs
rm -f tests/SBAPro.Infrastructure.Tests/UnitTest1.cs
rm -f tests/SBAPro.WebApp.Tests/UnitTest1.cs
```

### 8. Create Test Folder Structure

```bash
mkdir -p tests/SBAPro.Core.Tests/Entities
mkdir -p tests/SBAPro.Infrastructure.Tests/Services
mkdir -p tests/SBAPro.Infrastructure.Tests/Data
mkdir -p tests/SBAPro.WebApp.Tests/Components
mkdir -p tests/SBAPro.WebApp.Tests/Pages
```

## Validation Steps

### 1. Verify Test Project Structure

```bash
tree tests/ -L 2
```

Expected structure:
```
tests/
├── SBAPro.Core.Tests/
│   ├── Entities/
│   ├── TestBase.cs
│   └── SBAPro.Core.Tests.csproj
├── SBAPro.Infrastructure.Tests/
│   ├── Data/
│   ├── Services/
│   ├── InfrastructureTestBase.cs
│   └── SBAPro.Infrastructure.Tests.csproj
└── SBAPro.WebApp.Tests/
    ├── Components/
    ├── Pages/
    ├── WebAppTestBase.cs
    └── SBAPro.WebApp.Tests.csproj
```

### 2. Build All Test Projects

```bash
dotnet build tests/SBAPro.Core.Tests/SBAPro.Core.Tests.csproj
dotnet build tests/SBAPro.Infrastructure.Tests/SBAPro.Infrastructure.Tests.csproj
dotnet build tests/SBAPro.WebApp.Tests/SBAPro.WebApp.Tests.csproj
```

Expected: All builds succeed

### 3. Run Sample Tests

```bash
dotnet test tests/SBAPro.Core.Tests/SBAPro.Core.Tests.csproj
```

Expected: All tests pass

### 4. Check Test Coverage Setup

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Expected: Coverage data collected (even if coverage is low initially)

## Testing Guidelines

### Unit Test Naming Convention

Use the pattern: `[MethodName]_[Scenario]_[ExpectedResult]`

Examples:
- `GetTenantId_WhenUserAuthenticated_ReturnsTenantId`
- `SendEmail_WithInvalidAddress_ThrowsArgumentException`
- `GenerateReport_WithValidData_ReturnsValidPdf`

### Test Organization

- One test class per production class
- Group related tests using nested classes or theory data
- Use descriptive test names that explain the scenario

### AAA Pattern

Structure all tests using Arrange-Act-Assert:

```csharp
[Fact]
public void Method_Scenario_ExpectedResult()
{
    // Arrange
    var service = new MyService();
    var input = "test";

    // Act
    var result = service.Method(input);

    // Assert
    result.Should().Be("expected");
}
```

### Mocking Guidelines

- Mock external dependencies (database, email, file system)
- Don't mock the class under test
- Use strict mocks when behavior is critical
- Verify mock interactions when important

### Test Data

- Use realistic but minimal test data
- Create test data builders for complex entities
- Don't rely on specific database state
- Each test should be independent

## Success Criteria

- ✅ Three test projects created
- ✅ All test projects added to solution
- ✅ All required packages installed
- ✅ Base test classes created
- ✅ Sample tests created and passing
- ✅ Test projects build successfully
- ✅ All tests can be run with `dotnet test`

## Next Steps

Proceed to **02-core-tests.md** to write comprehensive tests for the Core layer.

## Troubleshooting

**Issue**: bUnit package not found
- **Solution**: Ensure you're using the correct package name: `bunit` (lowercase)

**Issue**: In-memory database tests fail
- **Solution**: Ensure each test uses a unique database name (use Guid.NewGuid())

**Issue**: Mock setup not working
- **Solution**: Verify Moq version is compatible and setup syntax is correct

**Issue**: Tests not discovered
- **Solution**: Ensure test classes are public and test methods have [Fact] or [Theory] attributes

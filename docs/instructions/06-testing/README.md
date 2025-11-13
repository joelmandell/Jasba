# Testing Guidelines for SBA Pro

## Overview

Comprehensive testing is **crucial** for the SBA Pro application due to:
1. **Multi-tenancy security**: Data isolation must be verified
2. **Legal compliance**: Fire safety documentation must be accurate
3. **Complex workflows**: Inspection processes involve multiple steps
4. **Future extensions**: Tests enable safe refactoring and new features

## Testing Philosophy

### What to Test

**✅ MUST TEST:**
- Multi-tenancy data isolation (CRITICAL - security issue if broken)
- Business logic in Core entities
- Service implementations in Infrastructure
- Database queries and filtering
- Authorization and authentication
- Report generation accuracy
- Email sending functionality

**✅ SHOULD TEST:**
- Blazor component rendering
- User input validation
- Edge cases and error conditions
- Navigation and routing
- Data transformation logic

**⚠️ OPTIONAL:**
- Simple property getters/setters
- Auto-implemented properties
- Framework code
- Third-party library internals

### Test Coverage Goals

- **Core Layer**: 90%+ coverage (pure business logic)
- **Infrastructure Layer**: 80%+ coverage (service implementations)
- **WebApp Layer**: 60%+ coverage (UI components)
- **Overall**: 70%+ coverage minimum

## Testing Layers

### 1. Unit Tests

**Purpose**: Test individual components in isolation

**Characteristics**:
- Fast execution (< 100ms per test)
- No external dependencies
- Use mocking for dependencies
- Test one thing at a time

**Example**:
```csharp
[Fact]
public void GetTenantId_WhenClaimExists_ReturnsTenantId()
{
    // Arrange
    var tenantId = Guid.NewGuid();
    var mockClaimsPrincipal = CreateMockUser(tenantId);
    var service = new TenantService(mockClaimsPrincipal);

    // Act
    var result = service.GetTenantId();

    // Assert
    result.Should().Be(tenantId);
}
```

### 2. Integration Tests

**Purpose**: Test components working together

**Characteristics**:
- Moderate execution time (< 1s per test)
- May use real database (in-memory)
- Test multiple layers together
- Verify data persistence

**Example**:
```csharp
[Fact]
public async Task CreateSite_ShouldPersistToDatabase()
{
    // Arrange
    var site = new Site { Name = "Test Site", TenantId = TestTenantId };

    // Act
    DbContext.Sites.Add(site);
    await DbContext.SaveChangesAsync();

    // Assert
    var savedSite = await DbContext.Sites.FindAsync(site.Id);
    savedSite.Should().NotBeNull();
    savedSite.Name.Should().Be("Test Site");
}
```

### 3. End-to-End Tests (Optional)

**Purpose**: Test complete user workflows

**Characteristics**:
- Slow execution (seconds per test)
- Use real browser (Playwright/Selenium)
- Test critical paths only
- Run less frequently

## Test Organization

### Project Structure

```
tests/
├── SBAPro.Core.Tests/              # Unit tests for Core layer
│   ├── Entities/
│   │   ├── TenantTests.cs
│   │   ├── SiteTests.cs
│   │   └── InspectionRoundTests.cs
│   └── TestBase.cs
├── SBAPro.Infrastructure.Tests/    # Unit + Integration tests
│   ├── Data/
│   │   ├── ApplicationDbContextTests.cs
│   │   └── MultiTenancyTests.cs    # CRITICAL
│   ├── Services/
│   │   ├── TenantServiceTests.cs
│   │   ├── EmailServiceTests.cs
│   │   └── ReportGeneratorTests.cs
│   └── InfrastructureTestBase.cs
└── SBAPro.WebApp.Tests/            # Component tests
    ├── Pages/
    │   ├── Admin/
    │   ├── Tenant/
    │   └── Inspector/
    └── WebAppTestBase.cs
```

### Naming Conventions

**Test Classes**: `[ClassName]Tests`
```csharp
public class TenantServiceTests { }
```

**Test Methods**: `[Method]_[Scenario]_[ExpectedResult]`
```csharp
public void GetTenantId_WhenNotAuthenticated_ThrowsException() { }
```

## Critical Test Scenarios

### 1. Multi-Tenancy Isolation (HIGHEST PRIORITY)

**File**: `tests/SBAPro.Infrastructure.Tests/Data/MultiTenancyTests.cs`

Tests to include:
- ✅ User can only see their tenant's data
- ✅ Global query filter applied to all tenant-specific entities
- ✅ Creating entities automatically sets TenantId
- ✅ Cross-tenant queries return no results
- ✅ SystemAdmin can access all tenants

Example:
```csharp
[Fact]
public async Task Sites_QueryFilter_OnlyReturnsTenantData()
{
    // Arrange
    var tenant1 = Guid.NewGuid();
    var tenant2 = Guid.NewGuid();
    
    DbContext.Sites.Add(new Site { Name = "Tenant 1 Site", TenantId = tenant1 });
    DbContext.Sites.Add(new Site { Name = "Tenant 2 Site", TenantId = tenant2 });
    await DbContext.SaveChangesAsync();
    
    // Act - Set context to tenant1
    MockTenantService.Setup(x => x.GetTenantId()).Returns(tenant1);
    var sites = await DbContext.Sites.ToListAsync();
    
    // Assert
    sites.Should().HaveCount(1);
    sites.First().TenantId.Should().Be(tenant1);
}
```

### 2. Inspection Workflow

Tests to include:
- ✅ Starting an inspection round
- ✅ Recording inspection results
- ✅ Completing an inspection round
- ✅ Generating PDF reports
- ✅ Status transitions

### 3. Authorization

Tests to include:
- ✅ SystemAdmin can access tenant management
- ✅ TenantAdmin can only access own tenant data
- ✅ Inspector can only create inspection results
- ✅ Unauthorized users are blocked

### 4. Data Validation

Tests to include:
- ✅ Required fields validated
- ✅ Email addresses valid
- ✅ Coordinates within valid range (0-1)
- ✅ File uploads validated (type, size)

## Testing Tools and Patterns

### FluentAssertions

Use for readable assertions:

```csharp
// ✅ Good
result.Should().NotBeNull();
result.Name.Should().Be("Expected Name");
result.Items.Should().HaveCount(5);

// ❌ Avoid
Assert.NotNull(result);
Assert.Equal("Expected Name", result.Name);
Assert.Equal(5, result.Items.Count);
```

### Moq

Use for mocking dependencies:

```csharp
// Setup
var mockService = new Mock<IEmailService>();
mockService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
    .ReturnsAsync(true);

// Verify
mockService.Verify(x => x.SendEmailAsync(
    "test@example.com",
    "Subject",
    It.IsAny<string>(),
    true), Times.Once);
```

### xUnit Theories

Use for parameterized tests:

```csharp
[Theory]
[InlineData(InspectionStatus.Ok, "green")]
[InlineData(InspectionStatus.Deficient, "red")]
[InlineData(InspectionStatus.NotAccessible, "yellow")]
public void GetMarkerColor_ReturnsCorrectColor(InspectionStatus status, string expectedColor)
{
    // Arrange & Act
    var color = MarkerHelper.GetColor(status);

    // Assert
    color.Should().Be(expectedColor);
}
```

### Test Data Builders

Create builders for complex entities:

```csharp
public class InspectionRoundBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _siteId = Guid.NewGuid();
    private string _inspectorId = "test-inspector";
    private DateTime _startedAt = DateTime.UtcNow;

    public InspectionRoundBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public InspectionRoundBuilder WithSite(Guid siteId)
    {
        _siteId = siteId;
        return this;
    }

    public InspectionRound Build()
    {
        return new InspectionRound
        {
            Id = _id,
            SiteId = _siteId,
            InspectorId = _inspectorId,
            StartedAt = _startedAt
        };
    }
}

// Usage
var round = new InspectionRoundBuilder()
    .WithSite(testSiteId)
    .Build();
```

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Project
```bash
dotnet test tests/SBAPro.Core.Tests/SBAPro.Core.Tests.csproj
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~TenantTests"
```

### Run Specific Test Method
```bash
dotnet test --filter "FullyQualifiedName~TenantTests.Tenant_ShouldInitializeWithDefaults"
```

### Run Tests in Parallel
```bash
dotnet test --parallel
```

## Continuous Integration

Tests should run automatically on:
- Every pull request
- Every commit to main branch
- Nightly builds

### CI Configuration Example (GitHub Actions)

```yaml
- name: Run Tests
  run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

- name: Check Coverage
  run: |
    dotnet tool install -g dotnet-reportgenerator-globaltool
    reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage -reporttypes:Html
```

## Test Maintenance

### When to Update Tests

- ✅ When adding new features
- ✅ When fixing bugs (add test to prevent regression)
- ✅ When refactoring (tests should still pass)
- ✅ When tests become flaky (fix immediately)

### When to Delete Tests

- ✅ When feature is removed
- ✅ When test is redundant (covered by other tests)
- ❌ Never delete tests just because they fail

## Common Pitfalls

### ❌ Not Testing Multi-Tenancy
**Problem**: Tenant data leaks
**Solution**: Create comprehensive multi-tenancy isolation tests

### ❌ Testing Implementation Details
**Problem**: Tests break with refactoring
**Solution**: Test behavior, not implementation

### ❌ Interdependent Tests
**Problem**: Tests fail when run in different order
**Solution**: Each test should be completely independent

### ❌ Slow Tests
**Problem**: Developers skip running tests
**Solution**: Keep unit tests fast, use in-memory database

### ❌ No Assertions
**Problem**: Tests pass but don't verify anything
**Solution**: Every test must have at least one assertion

## Test Quality Checklist

For each test, verify:

- [ ] Test name clearly describes what is being tested
- [ ] Test has Arrange, Act, Assert sections
- [ ] Test is independent (can run in any order)
- [ ] Test uses appropriate assertions
- [ ] Test mocks external dependencies
- [ ] Test has a single, clear purpose
- [ ] Test executes quickly (< 100ms for unit tests)
- [ ] Test is maintainable and easy to understand

## Resources

- xUnit Documentation: https://xunit.net/
- Moq Quickstart: https://github.com/moq/moq4
- FluentAssertions: https://fluentassertions.com/
- bUnit Documentation: https://bunit.dev/

## Next Steps

1. Complete **01-unit-test-setup.md** to set up test infrastructure
2. Work through **02-core-tests.md** to test Core layer
3. Work through **03-infrastructure-tests.md** to test Infrastructure
4. Work through **04-integration-tests.md** for end-to-end testing
5. Run **05-test-validation.md** to verify test coverage

# 06-Testing: Core Layer Unit Tests

## Objective
Implement comprehensive unit tests for the Core layer entities and interfaces.

## Prerequisites
- Completed: Phase 2 (Core Layer)
- xUnit, Moq, FluentAssertions installed

## Overview

Core layer tests focus on:
- Entity validation
- Business logic
- Domain rules
- Interface contracts

## Instructions

### 1. Create Test Project

```bash
dotnet new xunit -n SBAPro.Core.Tests
dotnet sln add tests/SBAPro.Core.Tests/SBAPro.Core.Tests.csproj
cd tests/SBAPro.Core.Tests
dotnet add reference ../../src/SBAPro.Core/SBAPro.Core.csproj
```

### 2. Install Test Packages

```bash
dotnet add package xUnit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package FluentAssertions
```

### 3. Entity Tests

**File**: `tests/SBAPro.Core.Tests/Entities/SiteTests.cs`

```csharp
public class SiteTests
{
    [Fact]
    public void Site_WithValidData_CreatesSuccessfully()
    {
        // Arrange & Act
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Address = "Test Address",
            TenantId = Guid.NewGuid()
        };

        // Assert
        site.Name.Should().Be("Test Site");
        site.Address.Should().Be("Test Address");
        site.TenantId.Should().NotBeEmpty();
    }

    [Fact]
    public void Site_RequiresTenantId()
    {
        // Arrange & Act
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Address = "Test Address"
        };

        // Assert
        site.TenantId.Should().BeEmpty();
    }
}
```

### 4. Inspection Logic Tests

```csharp
public class InspectionRoundTests
{
    [Fact]
    public void InspectionRound_Complete_SetsCompletedAt()
    {
        // Arrange
        var round = new InspectionRound
        {
            Id = Guid.NewGuid(),
            StartedAt = DateTime.Now,
            Status = "InProgress"
        };

        // Act
        round.CompletedAt = DateTime.Now;
        round.Status = "Completed";

        // Assert
        round.CompletedAt.Should().NotBeNull();
        round.Status.Should().Be("Completed");
    }
}
```

## Validation

```bash
dotnet test tests/SBAPro.Core.Tests
```

Expected: All tests pass

## Success Criteria

✅ Test project created  
✅ Entity tests cover all entities  
✅ Business logic tests implemented  
✅ All tests pass  
✅ Code coverage > 70%  

## Next Steps

Proceed to 03-infrastructure-tests.md

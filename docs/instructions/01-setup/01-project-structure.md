# 01-Setup: Project Structure

## Objective
Initialize the SBA Pro solution with proper Clean Architecture project structure.

## Prerequisites
- .NET 9.0 SDK installed
- Basic understanding of Clean Architecture principles

## Instructions

### 1. Create Solution File

```bash
dotnet new sln -n SBAPro
```

### 2. Create Core Layer Project

The Core layer contains domain entities and interfaces with no external dependencies.

```bash
dotnet new classlib -n SBAPro.Core -o src/SBAPro.Core
```

### 3. Create Infrastructure Layer Project

The Infrastructure layer implements Core interfaces and handles external concerns (database, email, etc.).

```bash
dotnet new classlib -n SBAPro.Infrastructure -o src/SBAPro.Infrastructure
```

### 4. Create Web Application Project

The WebApp is a Blazor Server application for the user interface.

```bash
dotnet new blazorserver -n SBAPro.WebApp -o src/SBAPro.WebApp
```

### 5. Add Projects to Solution

```bash
dotnet sln SBAPro.sln add src/SBAPro.Core/SBAPro.Core.csproj
dotnet sln SBAPro.sln add src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj
dotnet sln SBAPro.sln add src/SBAPro.WebApp/SBAPro.WebApp.csproj
```

### 6. Establish Project References

The dependency flow must be: **WebApp → Infrastructure → Core**

```bash
# WebApp references Infrastructure
dotnet add src/SBAPro.WebApp/SBAPro.WebApp.csproj reference src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj

# Infrastructure references Core
dotnet add src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj reference src/SBAPro.Core/SBAPro.Core.csproj
```

### 7. Create Folder Structure

Create the following folder structure within each project:

#### SBAPro.Core
```bash
mkdir -p src/SBAPro.Core/Entities
mkdir -p src/SBAPro.Core/Interfaces
```

#### SBAPro.Infrastructure
```bash
mkdir -p src/SBAPro.Infrastructure/Data
mkdir -p src/SBAPro.Infrastructure/Services
mkdir -p src/SBAPro.Infrastructure/Migrations
```

#### SBAPro.WebApp
```bash
mkdir -p src/SBAPro.WebApp/Components/Pages/Admin
mkdir -p src/SBAPro.WebApp/Components/Pages/Tenant
mkdir -p src/SBAPro.WebApp/Components/Pages/Inspector
mkdir -p src/SBAPro.WebApp/Components/Pages/Account
mkdir -p src/SBAPro.WebApp/wwwroot/js
mkdir -p src/SBAPro.WebApp/wwwroot/css
```

### 8. Remove Default Files

Remove the default Class1.cs files from the class library projects:

```bash
rm -f src/SBAPro.Core/Class1.cs
rm -f src/SBAPro.Infrastructure/Class1.cs
```

## Validation Steps

1. **Build the solution**:
   ```bash
   dotnet build
   ```
   Expected: Build succeeds with no errors.

2. **Verify project structure**:
   ```bash
   dotnet sln SBAPro.sln list
   ```
   Expected output should show all three projects.

3. **Check project references**:
   ```bash
   dotnet list src/SBAPro.WebApp/SBAPro.WebApp.csproj reference
   dotnet list src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj reference
   ```
   Expected:
   - WebApp should reference Infrastructure
   - Infrastructure should reference Core
   - Core should have no references

4. **Verify folder structure**:
   ```bash
   tree src/ -L 3
   ```
   Expected: All specified folders exist.

## Success Criteria

- ✅ Solution file created
- ✅ Three projects created (Core, Infrastructure, WebApp)
- ✅ Projects added to solution
- ✅ Project references established correctly
- ✅ Folder structure created
- ✅ Solution builds successfully
- ✅ No dependency violations (Core remains dependency-free)

## Next Steps

Proceed to **02-dependencies.md** to install required NuGet packages.

## Troubleshooting

**Issue**: Build fails with reference errors
- **Solution**: Verify project references are correct using `dotnet list reference`

**Issue**: Folder creation fails
- **Solution**: Ensure you're running commands from the repository root directory

**Issue**: Projects not showing in solution
- **Solution**: Re-run `dotnet sln add` commands

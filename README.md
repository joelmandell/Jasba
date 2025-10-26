# 🧯 SBA Pro - Fire Safety Management System

**SBA Pro** (Systematiskt Brandskyddsarbete Pro) is a comprehensive, cloud-based SaaS system for managing Systematic Fire Protection Work in accordance with Swedish legislation (LSO 2003:778).

## 🌟 Features

- **Multi-Tenant Architecture** - Secure data isolation for multiple organizations
- **Interactive Floor Plans** - Visual management of inspection rounds using Leaflet.js
- **Role-Based Access Control** - Different permissions for System Admins, Tenant Admins, and Inspectors
- **Digital Inspections** - Document fire safety equipment status with interactive checklists
- **Professional Reports** - Generate PDF reports with QuestPDF
- **Email Notifications** - Automated reminders for upcoming inspections
- **Offline Capability** - Continue inspections even without stable internet connection (future feature)

## 🏗️ Architecture

The application follows **Clean Architecture** principles with three main layers:

```
SBAPro.sln
└── src/
    ├── SBAPro.Core/          # Domain entities and interfaces
    ├── SBAPro.Infrastructure/ # Data access, email, PDF services
    └── SBAPro.WebApp/        # Blazor Server UI
```

### Technology Stack

- **Backend & Frontend**: .NET 9.0 with Blazor Server
- **Database**: Entity Framework Core with SQLite
- **Map Functionality**: Leaflet.js for interactive floor plan displays
- **PDF Generation**: QuestPDF for inspection reports
- **Email**: MailKit for automated notifications
- **Authentication**: ASP.NET Core Identity

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/joelmandell/Jasba.git
   cd Jasba
   ```

2. **Build the solution**
   ```bash
   dotnet build
   ```

3. **Run the application**
   ```bash
   cd src/SBAPro.WebApp
   dotnet run
   ```

4. **Access the application**
   
   Navigate to `http://localhost:5000` in your web browser

### Default Login Credentials

The application comes with demo accounts:

**System Administrator:**
- Email: `admin@sbapro.com`
- Password: `Admin@123`

**Tenant Administrator (Demo Company AB):**
- Email: `demo@democompany.se`
- Password: `Demo@123`

## 📖 User Guide

### For System Administrators

System Administrators can:
- Create and manage tenants (customer organizations)
- Create tenant admin accounts
- View system-wide statistics

**Creating a New Tenant:**
1. Login as System Admin
2. Navigate to "Tenants" in the menu
3. Click "Create New Tenant"
4. Enter tenant name and admin credentials
5. Click "Create"

### For Tenant Administrators

Tenant Administrators can:
- Manage sites (buildings/facilities)
- Upload and manage floor plans
- Configure inspection object types
- Place inspection objects on floor plans
- View inspection reports

**Adding a Site:**
1. Login as Tenant Admin
2. Navigate to "Sites" in the menu
3. Click "Add New Site"
4. Enter site name and address
5. Click "Create"

**Uploading a Floor Plan:**
1. Go to a site and click "Manage Floor Plans"
2. Click "Upload Floor Plan"
3. Enter a name and select an image file
4. Click "Upload"

**Placing Inspection Objects:**
1. Open a floor plan in edit mode
2. Click on the map where you want to place an object
3. Select the object type and enter a description
4. Click "Save"

### For Inspectors

Inspectors can:
- View assigned inspection rounds
- Perform inspections on-site
- Document equipment status and issues
- Generate PDF reports

**Performing an Inspection:**
1. Login as Inspector
2. Navigate to "Inspection Rounds"
3. Select a round to perform
4. Check each object and record status
5. Add comments for any issues found
6. Complete the round

## 🗂️ Project Structure

### Core Layer (`SBAPro.Core`)

Contains domain entities and business logic interfaces:

- **Entities/**
  - `Tenant` - Customer organization
  - `Site` - Physical location/building
  - `FloorPlan` - Building floor plan image
  - `InspectionObject` - Fire safety equipment on floor plan
  - `InspectionObjectType` - Type of equipment (e.g., fire extinguisher)
  - `InspectionRound` - A single inspection session
  - `InspectionResult` - Result for a specific object in a round
  - `ApplicationUser` - User with tenant association

- **Interfaces/**
  - `ITenantService` - Multi-tenancy service
  - `IEmailService` - Email notifications
  - `IReportGenerator` - PDF report generation

### Infrastructure Layer (`SBAPro.Infrastructure`)

Implements data access and external services:

- **Data/**
  - `ApplicationDbContext` - EF Core database context with multi-tenancy
  - `DbInitializer` - Seeds initial data (roles, default users)

- **Services/**
  - `TenantService` - Manages current tenant context
  - `MailKitEmailService` - Email notifications via SMTP
  - `QuestPdfReportGenerator` - PDF report generation

### Presentation Layer (`SBAPro.WebApp`)

Blazor Server web application:

- **Components/Pages/**
  - **Account/** - Login/Logout pages
  - **Admin/** - System admin pages (tenant management)
  - **Tenant/** - Tenant admin pages (sites, floor plans, object types)
  - **Inspector/** - Inspector pages (inspection rounds)

- **wwwroot/js/**
  - `leafletMap.js` - Leaflet.js integration for floor plans

## ⚙️ Configuration

### Database

The application uses SQLite by default. The connection string is configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=sbapro.db"
  }
}
```

### Email Settings

Configure SMTP settings in `appsettings.json`:

```json
{
  "Email": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": "587",
    "SmtpUsername": "your-email@example.com",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@sbapro.com",
    "FromName": "SBA Pro"
  }
}
```

## 🔒 Security Features

- **Multi-Tenancy**: Data isolation using global query filters
- **Role-Based Authorization**: Three distinct roles with appropriate permissions
- **Password Requirements**: Strong password policy enforced
- **Secure Authentication**: ASP.NET Core Identity with cookie authentication

## 📊 Database Schema

Key relationships:
- `Tenant` → `Sites` (1:many)
- `Tenant` → `Users` (1:many)
- `Tenant` → `InspectionObjectTypes` (1:many)
- `Site` → `FloorPlans` (1:many)
- `FloorPlan` → `InspectionObjects` (1:many)
- `InspectionRound` → `InspectionResults` (1:many)

All tenant-specific data is filtered automatically based on the logged-in user's tenant.

## 🧪 Testing

Run tests with:
```bash
dotnet test
```

## 📝 License

This project is created as a demonstration of a comprehensive SBA management system.

## 🤝 Contributing

This is a demonstration project. For the full technical specifications and implementation details, see [SPECIFICATIONS.md](SPECIFICATIONS.md).

## 🔗 Links

- Full Technical Specifications: [SPECIFICATIONS.md](SPECIFICATIONS.md)
- Report Issues: [GitHub Issues](https://github.com/joelmandell/Jasba/issues)

## 📞 Support

For questions or support, please open an issue on GitHub.

---

**Built with ❤️ using .NET 9.0 and Blazor Server**

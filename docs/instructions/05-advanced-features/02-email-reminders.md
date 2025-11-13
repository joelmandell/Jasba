# 05-Advanced-Features: Email Reminders

## Objective
Implement background service for sending scheduled inspection reminders via email.

## Prerequisites
- Completed: 05-advanced-features/01-offline-capability.md
- Email service configured

## Overview

Reminder system sends emails to inspectors:
- 7 days before inspection due date
- 3 days before inspection due date
- Day of inspection due date
- Configurable reminder intervals

## Instructions

### 1. Create Background Service

**File**: `src/SBAPro.Infrastructure/Services/InspectionReminderService.cs`

```csharp
public class InspectionReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InspectionReminderService> _logger;

    public InspectionReminderService(
        IServiceProvider serviceProvider,
        ILogger<InspectionReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendReminders();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminders");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task SendReminders()
    {
        using var scope = _serviceProvider.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        await using var context = await contextFactory.CreateDbContextAsync();

        // Find sites that need inspection
        var sitesNeedingInspection = await context.Sites
            .Include(s => s.FloorPlans)
            .Where(s => /* logic for sites needing inspection */)
            .ToListAsync();

        foreach (var site in sitesNeedingInspection)
        {
            // Get inspectors for tenant
            var inspectors = await context.Users
                .Where(u => u.TenantId == site.TenantId)
                .ToListAsync();

            foreach (var inspector in inspectors)
            {
                await SendReminderEmail(emailService, inspector, site);
            }
        }
    }

    private async Task SendReminderEmail(IEmailService emailService, ApplicationUser inspector, Site site)
    {
        var subject = $"Påminnelse: Egenkontroll för {site.Name}";
        var body = $@"
            <h2>Påminnelse om egenkontroll</h2>
            <p>Hej {inspector.UserName},</p>
            <p>Detta är en påminnelse om att genomföra egenkontroll för:</p>
            <p><strong>{site.Name}</strong><br>{site.Address}</p>
            <p>Vänligen logga in i SBA Pro och genomför kontrollen.</p>
        ";

        await emailService.SendEmailAsync(inspector.Email, subject, body);
    }
}
```

### 2. Register Service

**File**: `src/SBAPro.WebApp/Program.cs`

```csharp
builder.Services.AddHostedService<InspectionReminderService>();
```

### 3. Configure Reminder Schedule

**File**: `src/SBAPro.WebApp/appsettings.json`

```json
{
  "Reminders": {
    "Enabled": true,
    "IntervalHours": 1,
    "DaysBefore": [7, 3, 0]
  }
}
```

## Validation

1. Create site with inspection schedule
2. Set inspection due date to 7 days from now
3. Wait for reminder service to run (or manually trigger)
4. Check MailHog for reminder email
5. Verify email contains correct information

## Success Criteria

✅ Background service runs on schedule  
✅ Reminders sent at configured intervals  
✅ Emails contain correct information  
✅ Service handles errors gracefully  
✅ Can be enabled/disabled via configuration  

## Next Steps

Proceed to 03-advanced-validation.md

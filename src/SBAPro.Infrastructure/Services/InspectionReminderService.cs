using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SBAPro.Core.Entities;
using SBAPro.Core.Interfaces;
using SBAPro.Infrastructure.Data;

namespace SBAPro.Infrastructure.Services;

/// <summary>
/// Background service that sends reminder emails for sites needing inspection.
/// </summary>
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
        _logger.LogInformation("Inspection Reminder Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendReminders();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending inspection reminders");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("Inspection Reminder Service is stopping.");
    }

    private async Task SendReminders()
    {
        using var scope = _serviceProvider.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        await using var context = await contextFactory.CreateDbContextAsync();

        // Find sites that need inspection (no inspection in last 30 days or never inspected)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        
        // Use IgnoreQueryFilters to bypass tenant filtering in background service
        var sitesNeedingInspection = await context.Sites
            .IgnoreQueryFilters()
            .Include(s => s.Tenant)
            .Include(s => s.InspectionRounds)
            .Where(s => !s.InspectionRounds.Any() || 
                       s.InspectionRounds.OrderByDescending(r => r.CompletedAt).First().CompletedAt < thirtyDaysAgo)
            .ToListAsync();

        _logger.LogInformation("Found {Count} sites needing inspection reminders", sitesNeedingInspection.Count);

        foreach (var site in sitesNeedingInspection)
        {
            // Get inspectors for tenant
            var inspectors = await context.Users
                .Where(u => u.TenantId == site.TenantId)
                .ToListAsync();

            foreach (var inspector in inspectors)
            {
                try
                {
                    await SendReminderEmail(emailService, inspector, site);
                    _logger.LogInformation("Sent reminder email to {Email} for site {SiteName}", 
                        inspector.Email, site.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder email to {Email}", inspector.Email);
                }
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
            <br>
            <p>Med vänliga hälsningar,<br>SBA Pro</p>
        ";

        await emailService.SendEmailAsync(inspector.Email!, subject, body);
    }
}

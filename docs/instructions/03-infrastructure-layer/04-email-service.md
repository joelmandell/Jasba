# 03-Infrastructure-Layer: Email Service with MailKit

## Objective
Implement email notification service using MailKit for sending inspection reminders, reports, and system notifications.

## Prerequisites
- Completed: 03-infrastructure-layer/03-database-seeding.md
- Understanding of SMTP protocol
- Understanding of async/await in C#
- MailKit NuGet package installed

## Overview

The Email Service provides:
1. Asynchronous email sending via SMTP
2. HTML email support
3. Configuration-based SMTP settings
4. Error handling and logging
5. Support for development/production environments

## Instructions

### 1. Install MailKit NuGet Package

```bash
cd src/SBAPro.Infrastructure
dotnet add package MailKit
```

### 2. Create MailKitEmailService Implementation

**File**: `src/SBAPro.Infrastructure/Services/MailKitEmailService.cs`

```csharp
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using SBAPro.Core.Interfaces;

namespace SBAPro.Infrastructure.Services;

/// <summary>
/// Email service implementation using MailKit for sending emails via SMTP.
/// </summary>
public class MailKitEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public MailKitEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Get SMTP configuration from appsettings
        var smtpServer = _configuration["Email:SmtpServer"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
        var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
        var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@sbapro.com";
        var fromName = _configuration["Email:FromName"] ?? "SBA Pro";

        // Create email message
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;

        // Build HTML body
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };
        message.Body = bodyBuilder.ToMessageBody();

        // Send via SMTP
        using var client = new SmtpClient();
        
        try
        {
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            
            // Authenticate if credentials provided
            if (!string.IsNullOrEmpty(smtpUsername))
            {
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
            }
            
            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
```

### 3. Configure Email Settings

**File**: `src/SBAPro.WebApp/appsettings.json`

Add email configuration section:

```json
{
  "Email": {
    "SmtpServer": "localhost",
    "SmtpPort": "1025",
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "noreply@sbapro.com",
    "FromName": "SBA Pro"
  }
}
```

**File**: `src/SBAPro.WebApp/appsettings.Development.json`

Development settings for local testing:

```json
{
  "Email": {
    "SmtpServer": "localhost",
    "SmtpPort": "1025",
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "dev@sbapro.local",
    "FromName": "SBA Pro Dev"
  }
}
```

### 4. Register EmailService in DI

**File**: `src/SBAPro.WebApp/Program.cs`

```csharp
// Register Email Service
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
```

## Usage Examples

### Send Inspection Reminder

```csharp
public class InspectionReminderService
{
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _context;

    public InspectionReminderService(
        IEmailService emailService,
        ApplicationDbContext context)
    {
        _emailService = emailService;
        _context = context;
    }

    public async Task SendReminderAsync(Guid inspectorId, Guid siteId)
    {
        var inspector = await _context.Users.FindAsync(inspectorId.ToString());
        var site = await _context.Sites.FindAsync(siteId);

        var subject = "Påminnelse: Egenkontroll förfaller snart";
        var body = $@"
            <h2>Påminnelse om egenkontroll</h2>
            <p>Hej {inspector.UserName},</p>
            <p>Detta är en påminnelse om att egenkontroll för <strong>{site.Name}</strong> förfaller inom 7 dagar.</p>
            <p>Vänligen logga in och genomför kontrollen.</p>
            <p>Med vänlig hälsning,<br>SBA Pro</p>
        ";

        await _emailService.SendEmailAsync(inspector.Email, subject, body);
    }
}
```

### Send Report Notification

```csharp
public async Task SendReportAsync(string recipientEmail, byte[] pdfReport, string siteName)
{
    var subject = $"Kontrollrapport för {siteName}";
    var body = $@"
        <h2>Kontrollrapport</h2>
        <p>Bifogad finner du kontrollrapporten för {siteName}.</p>
        <p>Rapporten innehåller resultaten från den senaste egenkontrollen.</p>
        <p>Med vänlig hälsning,<br>SBA Pro</p>
    ";

    // Note: Current interface doesn't support attachments
    // For now, send link to download or extend interface
    await _emailService.SendEmailAsync(recipientEmail, subject, body);
}
```

## Development Testing with MailHog

For local development, use [MailHog](https://github.com/mailhog/MailHog) to test emails without sending real emails.

### Install MailHog

**macOS**:
```bash
brew install mailhog
mailhog
```

**Windows** (via Chocolatey):
```bash
choco install mailhog
mailhog
```

**Linux**:
```bash
# Download from GitHub releases
wget https://github.com/mailhog/MailHog/releases/download/v1.0.1/MailHog_linux_amd64
chmod +x MailHog_linux_amd64
./MailHog_linux_amd64
```

**Docker**:
```bash
docker run -d -p 1025:1025 -p 8025:8025 mailhog/mailhog
```

### Access MailHog Web UI

Open browser: http://localhost:8025

All emails sent from the application will appear here.

### Configure appsettings for MailHog

```json
{
  "Email": {
    "SmtpServer": "localhost",
    "SmtpPort": "1025",
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "noreply@sbapro.local",
    "FromName": "SBA Pro Dev"
  }
}
```

## Production Configuration

### Using Gmail SMTP

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "SBA Pro"
  }
}
```

**Note**: Gmail requires an [App Password](https://support.google.com/accounts/answer/185833) for this to work.

### Using SendGrid

```json
{
  "Email": {
    "SmtpServer": "smtp.sendgrid.net",
    "SmtpPort": "587",
    "SmtpUsername": "apikey",
    "SmtpPassword": "your-sendgrid-api-key",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "SBA Pro"
  }
}
```

### Using Azure Communication Services

For production with Azure:

```json
{
  "Email": {
    "SmtpServer": "smtp.azurecomm.net",
    "SmtpPort": "587",
    "SmtpUsername": "your-acs-connection-string",
    "SmtpPassword": "your-acs-key",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "SBA Pro"
  }
}
```

## Security Best Practices

### ⚠️ Never Commit Secrets

❌ **DO NOT** commit SMTP credentials to source control:

```json
// WRONG - Never do this
{
  "Email": {
    "SmtpPassword": "actual-password-here"
  }
}
```

✅ **DO** use User Secrets for development:

```bash
cd src/SBAPro.WebApp
dotnet user-secrets set "Email:SmtpPassword" "your-password"
```

✅ **DO** use environment variables in production:

```bash
export Email__SmtpPassword="your-password"
```

✅ **DO** use Azure Key Vault or AWS Secrets Manager:

```csharp
builder.Configuration.AddAzureKeyVault(/* ... */);
```

### Email Validation

Always validate email addresses before sending:

```csharp
public async Task SendEmailAsync(string to, string subject, string body)
{
    if (string.IsNullOrWhiteSpace(to))
    {
        throw new ArgumentException("Email address is required", nameof(to));
    }

    if (!IsValidEmail(to))
    {
        throw new ArgumentException("Invalid email address", nameof(to));
    }

    // Send email...
}

private static bool IsValidEmail(string email)
{
    try
    {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}
```

## Error Handling

### Add Logging

```csharp
public class MailKitEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(
        IConfiguration configuration,
        ILogger<MailKitEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
            
            // Send email...
            
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw; // Re-throw to let caller handle
        }
    }
}
```

### Graceful Degradation

For non-critical emails, you may want to catch and log errors without throwing:

```csharp
public async Task SendReminderAsync(string to, string subject, string body)
{
    try
    {
        await _emailService.SendEmailAsync(to, subject, body);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Failed to send reminder email to {To}, but continuing", to);
        // Don't throw - reminders are not critical
    }
}
```

## Testing

### Unit Tests

```csharp
public class EmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_ValidParameters_SendsEmail()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Email:SmtpServer"] = "localhost",
                ["Email:SmtpPort"] = "1025",
                ["Email:FromEmail"] = "test@test.com"
            })
            .Build();

        var emailService = new MailKitEmailService(configuration);

        // Act & Assert
        // Note: This requires MailHog running
        await emailService.SendEmailAsync(
            "recipient@test.com",
            "Test Subject",
            "<p>Test body</p>");
    }
}
```

### Integration Tests

```csharp
[Fact]
public async Task SendInspectionReminder_CreatesEmailInMailHog()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    var emailService = factory.Services.GetRequiredService<IEmailService>();

    // Act
    await emailService.SendEmailAsync(
        "inspector@test.com",
        "Test Reminder",
        "<p>Test reminder body</p>");

    // Assert
    // Check MailHog API that email was received
    var mailHogClient = new HttpClient();
    var response = await mailHogClient.GetAsync("http://localhost:8025/api/v2/messages");
    var content = await response.Content.ReadAsStringAsync();
    
    Assert.Contains("inspector@test.com", content);
}
```

## Validation Steps

### 1. Verify File Creation

```bash
ls -la src/SBAPro.Infrastructure/Services/MailKitEmailService.cs
```

Expected: File exists

### 2. Verify MailKit Package Installed

```bash
dotnet list src/SBAPro.Infrastructure package | grep MailKit
```

Expected: MailKit package listed

### 3. Build the Solution

```bash
dotnet build
```

Expected: Build succeeds with no errors

### 4. Verify Configuration

```bash
grep -A 7 '"Email"' src/SBAPro.WebApp/appsettings.json
```

Expected: Email configuration section exists

### 5. Start MailHog

```bash
mailhog
```

Expected: MailHog starts on ports 1025 (SMTP) and 8025 (Web UI)

### 6. Test Email Sending

Create a simple test endpoint:

```csharp
app.MapGet("/test-email", async (IEmailService emailService) =>
{
    await emailService.SendEmailAsync(
        "test@example.com",
        "Test Email",
        "<h1>Test</h1><p>This is a test email.</p>");
    
    return Results.Ok("Email sent");
});
```

Navigate to: http://localhost:5000/test-email

Expected: Email appears in MailHog web UI

### 7. Check MailHog Web UI

Open: http://localhost:8025

Expected: Email appears in inbox with correct subject and body

## Success Criteria

✅ MailKit package installed  
✅ MailKitEmailService implemented  
✅ Email configuration in appsettings  
✅ Service registered in DI  
✅ MailHog running for development testing  
✅ Test email sends successfully  
✅ Email appears in MailHog web UI  
✅ HTML formatting works correctly  

## Troubleshooting

### Issue: Could not connect to SMTP server

**Causes**:
1. MailHog not running
2. Wrong SMTP server/port
3. Firewall blocking connection

**Solutions**:
- Start MailHog: `mailhog`
- Verify settings in appsettings.json
- Check firewall rules

### Issue: Authentication failed

**Causes**:
1. Wrong username/password
2. App password not configured (Gmail)
3. Less secure apps not enabled

**Solutions**:
- Verify credentials
- Use App Password for Gmail
- Enable "Less secure apps" (Gmail) or use OAuth2

### Issue: Email not appearing in MailHog

**Causes**:
1. Wrong port configuration
2. Email sent to different SMTP server
3. MailHog not running

**Solutions**:
- Check SmtpPort is 1025
- Verify SmtpServer is "localhost"
- Restart MailHog

## Related Files

- `src/SBAPro.Core/Interfaces/IEmailService.cs` - Interface definition
- `src/SBAPro.WebApp/appsettings.json` - Email configuration
- `src/SBAPro.WebApp/Program.cs` - Service registration

## Next Steps

After completing this module:
1. Proceed to **05-pdf-service.md** to implement report generation
2. Test email sending with different scenarios
3. Configure production SMTP settings

## Additional Resources

- [MailKit Documentation](https://github.com/jstedfast/MailKit)
- [MailHog Documentation](https://github.com/mailhog/MailHog)
- [SMTP Configuration in .NET](https://docs.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient)

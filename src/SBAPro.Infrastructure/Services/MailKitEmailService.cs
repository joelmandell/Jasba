using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using SBAPro.Core.Interfaces;

namespace SBAPro.Infrastructure.Services;

public class MailKitEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public MailKitEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        await SendEmailAsync(new[] { to }, subject, body, isHtml);
    }

    public async Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = true)
    {
        var smtpServer = _configuration["Email:SmtpServer"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
        var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
        var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@sbapro.com";
        var fromName = _configuration["Email:FromName"] ?? "SBA Pro";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        
        foreach (var recipient in recipients)
        {
            message.To.Add(new MailboxAddress(recipient, recipient));
        }
        
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder();
        if (isHtml)
        {
            bodyBuilder.HtmlBody = body;
        }
        else
        {
            bodyBuilder.TextBody = body;
        }
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        
        try
        {
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            
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

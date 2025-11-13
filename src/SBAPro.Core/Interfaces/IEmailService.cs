namespace SBAPro.Core.Interfaces;

/// <summary>
/// Service for sending email notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="body">Email body content (can be HTML).</param>
    /// <param name="isHtml">Whether the body content is HTML (default: true).</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Sends an email to multiple recipients asynchronously.
    /// </summary>
    /// <param name="recipients">List of recipient email addresses.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="body">Email body content (can be HTML).</param>
    /// <param name="isHtml">Whether the body content is HTML (default: true).</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = true);
}

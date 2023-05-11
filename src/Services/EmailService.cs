using Lib.Email;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Services;

/// <summary>
/// Email service
/// </summary>
public class EmailService
{
    private readonly IOptions<Configuration> _options;
    public EmailService(IOptions<Configuration> options)
    {
        _options = options;
    }

    /// <summary>
    /// Send email
    /// </summary>
    /// <param name="to">Recipient's email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    public void SendEmail(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.Value.SenderName, _options.Value.Username));
        message.To.Add(new MailboxAddress("Receiver Name", to));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using (var client = new SmtpClient())
        {
            client.Connect(_options.Value.Host, _options.Value.Port, _options.Value.UseSsl);
            client.Authenticate(_options.Value.Username, _options.Value.Password);
            client.Send(message);
            client.Disconnect(true);
        }
    }

    /// <summary>
    /// Retrieve the email address of the administrator who have configured to receive email notifications in the system.
    /// If the returned result is empty, it means that no administrator have been configured to receive email notifications in the system.
    /// </summary>
    /// <returns>Admin email address</returns>
    public string? NotificationAdminEmail()
    {
        return string.IsNullOrWhiteSpace(_options.Value.NotificationAdminEmail) ? null : _options.Value.NotificationAdminEmail;
    }
}
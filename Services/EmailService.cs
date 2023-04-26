using Lib.Email;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Services;

public class EmailService
{
    private readonly IOptions<Configuration> _options;
    public EmailService(IOptions<Configuration> options)
    {
        _options = options;
    }

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

    public string? NotificationAdminEmail()
    {
        return string.IsNullOrWhiteSpace(_options.Value.NotificationAdminEmail) ? null : _options.Value.NotificationAdminEmail;
    }
}
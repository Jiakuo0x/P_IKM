using MailKit.Net.Smtp;
using MimeKit;

namespace Lib;

public static class Email
{
    public static void SendEmail()
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sender Name", ""));
        message.To.Add(new MailboxAddress("Receiver Name", ""));
        message.Subject = "Subject";
        message.Body = new TextPart("plain")
        {
            Text = @"Hello World!"
        };

        using (var client = new SmtpClient())
        {
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate("", "");
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
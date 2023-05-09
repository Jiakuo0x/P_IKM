using MailKit.Net.Smtp;
using MimeKit;

var message = new MimeMessage();
message.From.Add(new MailboxAddress("ISCN-ES", "ISCN.docusign@ISCN.com.cn"));
message.To.Add(new MailboxAddress("Receiver Name", "jiakuo.zhang@quest-global.com"));
// message.To.Add(new MailboxAddress("Receiver Name", "john.li14@inter.ikea.com"));
message.Subject = "Test Email";
message.Body = new TextPart("plain")
{
    Text = "Test Email body"
};

using (var client = new SmtpClient())
{
    client.Connect("smtp.qiye.aliyun.com", 465, true);
    client.Authenticate("ISCN.docusign@ISCN.com.cn", "tZqFcRVeeZPLFriF");
    client.Send(message);
    client.Disconnect(true);
}
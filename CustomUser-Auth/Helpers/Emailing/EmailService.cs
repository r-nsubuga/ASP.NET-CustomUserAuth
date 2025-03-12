using MailKit.Net.Smtp;
using MimeKit;

namespace CustomUser_Auth.Helpers.Emailing;

public class EmailService:IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Racoon", "rayrmond.kizito@gmail.com"));
        message.To.Add(new MailboxAddress("Zebra", to));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 465, true);
        await client.AuthenticateAsync("rayrmond.kizito@gmail.com", "gbhu bckz yoew cxmp");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
    
}
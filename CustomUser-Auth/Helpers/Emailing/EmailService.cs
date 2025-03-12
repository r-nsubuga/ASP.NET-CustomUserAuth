using MailKit.Net.Smtp;
using MimeKit;

namespace CustomUser_Auth.Helpers.Emailing;

public class EmailService:IEmailService
{
    private string MAIL_HOST = Environment.GetEnvironmentVariable("MAIL_HOST");
    private string MAIL_ADDRESS = Environment.GetEnvironmentVariable("MAIL_ADDRESS");
    private string MAIL_PASSWORD = Environment.GetEnvironmentVariable("MAIL_PASSWORD");
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Racoon", MAIL_ADDRESS));
        message.To.Add(new MailboxAddress("Zebra", to));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(MAIL_HOST, 465, true);
        await client.AuthenticateAsync(MAIL_ADDRESS, MAIL_PASSWORD);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
    
}
namespace CustomUser_Auth.Helpers.Emailing;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
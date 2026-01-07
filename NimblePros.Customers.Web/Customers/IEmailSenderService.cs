
public interface IEmailSenderService
{
    Task SendEmailAsync(string from, string to, string subject, string body);
}
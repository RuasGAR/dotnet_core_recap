using MailKit.Net.Smtp;
using MimeKit;

public record MailSettings(int Port, string Server); 

public class MailKitEmailSenderService(IConfiguration config, MailSettings mailSettings) : IEmailSenderService
{

    private readonly IConfiguration _config = config;
    private readonly MailSettings _mailSettings = mailSettings;

    public async Task SendEmailAsync(string from, string to, string subject, string body)
    {
        using (SmtpClient client = new SmtpClient())
        {
            client.Connect(_mailSettings.Server, _mailSettings.Port, false);
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(from, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;
            message.Body = new TextPart { Text = body };

            // Email sending logic would go here
            await client.SendAsync(message);
            Console.WriteLine("Welcome email sent.");

            client.Disconnect(true);

        }
    }
}
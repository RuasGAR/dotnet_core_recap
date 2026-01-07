using MailKit.Net.Smtp;
using MimeKit;
using NimblePros.Customers.Web.Customers;

namespace NimblePros.Customers.Web.Customers
{
    internal class CustomerEmailService
    {
        private readonly IEmailMessageFactory _emailMessageFactory;
        private readonly IEmailSenderService _emailSenderService;

        // The ideia of passin in Services to the constructor is both the main principle of the Strategy Design Pattern
        // as well as a demonstration of Dependency Injection, reducing the coupling between components
        public CustomerEmailService(IEmailMessageFactory emailMessageFactory, IEmailSenderService emailSenderService)
        {
            _emailMessageFactory = emailMessageFactory;
            _emailSenderService = emailSenderService;
        }

        internal async Task SendWelcomeEmail(Customer newCustomer)
        {

            string from = "crazy_dotnet";
            string to = newCustomer.Email;
            string subject = "Welcome to Nimble Pros!";

            string body = _emailMessageFactory.GenerateWelcomeMessage(newCustomer);

            // Simulate sending a welcome email to the customer
            Console.WriteLine("Attempting to send email to {to} from {from} with subject {subject}...", to, from, subject);

            await _emailSenderService.SendEmailAsync(from, to, subject, body);
        }
    }
}


public class MailKitEmailSenderService : IEmailSenderService
{
    public async Task SendEmailAsync(string from, string to, string subject, string body)
    {
        using (SmtpClient client = new SmtpClient())
        {
            client.Connect("localhost", 25, false);
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


public class EmailMessageFactory : IEmailMessageFactory
{
    public string GenerateWelcomeMessage(Customer newCustomer)
    {
        string template = "Welcome {{CompanyName}} to the Nimble Pros family!";
        string body = template.Replace("{{CompanyName}}", newCustomer.CompanyName);
        return body;
    }
}





    
// do not respect SINGLE RESPONSABILITY and OPEN-CLOSED SOLID principles 

/*
internal async Task SendWelcomeEmail(Customer newCustomer)
{

    string from = "crazy_dotnet";
    string to = newCustomer.Email;
    string subject = "Welcome to Nimble Pros!";

    string template = "Welcome {{CompanyName}} to the  Nimble Pros family!";
    string body = template.Replace("{{CompanyName}}", newCustomer.CompanyName);

    // Simulate sending a welcome email to the customer
    Console.WriteLine("Attempting to send email to {to} from {from} with subject {subject}...", to, from, subject);


    using (SmtpClient client = new SmtpClient())
    {
        client.Connect("localhost", 25, false);
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
*/
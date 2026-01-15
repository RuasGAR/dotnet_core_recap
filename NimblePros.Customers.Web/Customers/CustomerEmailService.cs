using MailKit.Net.Smtp;
using MimeKit;
using NimblePros.Customers.Web.Customers;

namespace NimblePros.Customers.Web.Customers
{
    public class CustomerEmailService : ICustomerEmailService
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

        public async Task SendWelcomeEmail(Customer newCustomer)
        {

            string from = "crazy_dotnet";
            string to = newCustomer.EmailAddress;
            string subject = "Welcome to Nimble Pros!";

            string body = _emailMessageFactory.GenerateWelcomeMessage(newCustomer);

            // Simulate sending a welcome email to the customer
            Console.WriteLine("Attempting to send email to {to} from {from} with subject {subject}...", to, from, subject);

            await _emailSenderService.SendEmailAsync(from, to, subject, body);
        }
    }
}

// Email Sender Services -----------------------------------------------------------------------------------

// Service for generating email messages in development environment

public class ConsoleOnlyEmailSenderService : IEmailSenderService
{
    public Task SendEmailAsync(string from, string to, string subject, string body)
    {
        // Simulate sending email by writing to console

        Console.WriteLine("Simulated Email Sending:");
        Console.WriteLine($"From: {from}");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {body}");

        return Task.CompletedTask;
    }
}

// ----------------------------------------------------------------------------------------------------

public class EmailMessageFactory : IEmailMessageFactory
{
    public string GenerateWelcomeMessage(Customer newCustomer)
    {
        string template = "Welcome {{CompanyName}} to the Nimble Pros family!";
        string body = template.Replace("{{CompanyName}}", newCustomer.CompanyName);
        return body;
    }
}


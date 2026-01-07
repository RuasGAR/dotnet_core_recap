using NimblePros.Customers.Web.Customers;

public interface IEmailMessageFactory
{
    string GenerateWelcomeMessage(Customer newCustomer);
}
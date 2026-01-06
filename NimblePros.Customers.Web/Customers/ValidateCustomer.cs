using NimblePros.Customers.Web.Customers; // Namespace loaded

public class ValidateCustomer : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Using a little bit of LINQ here
        var customer = context.Arguments.FirstOrDefault(a => a is Customer) as Customer; // can be a problem in case of more than one Customer parameter
        
        if (customer is not null && String.IsNullOrEmpty(customer.CompanyName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "Company Name" , new[] { "CompanyName cannot be empty" }}
            });
        }

        return await next(context);
    }
}


using NimblePros.Customers.Web.Customers; // Namespace loaded
                                          // -----------------------------------------------------------------------------------------

// Records for Customer and Project



// just didn't do delete because it's very similar to update
// the differences are: no customer parameter is needed,  and the Results method used is Results.NoContent();

// OBS: it's a goot practice to always return if the correspondent id passed to deletion actually exists.

// -----------------------------------------------------------------------------------------

// Validation Helpers

// This strategy can get quite verbose, and will also not be responsible to different orders of parameters.
public class ValidationHelpers
{
    internal static async ValueTask<object?> ValidateAddCustomer(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var customer = context.GetArgument<Customer>(0);
        if (customer is not null && String.IsNullOrEmpty(customer.CompanyName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "Id" , new[] { "Id cannot be null." }}
            });
        }
        return await next(context);
    }
}



// Data Annotations: there are a bunch of them, including Url, Regex, Integer Range, String Length, even Phone and Email format annotations
// THEY DO NOT APPLY THE LOGIC BY THEIR OWN, though. Its purely for indication purposes. They need to be "enforced". 
// There is, though, a NuGetPackage that enforce that for us, which we'll see briefly.

// Fluent Validation is basically the standard NuGetPackage for validation purposes in .NetCore.
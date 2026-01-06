using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations; // brings also the hinting for [FromBody] and [FromRoute]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<CustomerData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//this is a middleware
app.UseHttpsRedirection();

// ----------------------------------------------------------------------------

// Endpoints

app.MapGet("/customers", async (CustomerData data) =>
{
    var customers = await data.ListAsync();
    return customers;
})
.WithName("ListCustomers");

app.MapGet("/customers/{id:guid}", async (Guid id, CustomerData data) =>

    /* Customer? customer = await data.GetByIdAsync(id);
    if (customer is null)
    {
        return Results.NotFound();
    }
    return customer; */

    await data.GetByIdAsync(id) is Customer customer
        ? TypedResults.Ok(customer)
        : Results.NotFound()

)
.WithName("GetCustomerById");

app.MapPost("/customers", async (Customer customer, CustomerData data) =>
{

    // Usually, we would only take the information that we do need inside of customer, instead of using the whole body.
    // It helps with validation
    var newCustomer = customer with { Id = Guid.NewGuid(), Projects = new() };
    await data.AddAsync(newCustomer);
    return Results.Created($"/customers/{newCustomer.Id}", newCustomer);
})
.WithName("AddCustomer")
.AddEndpointFilter<ValidateCustomer>();

app.MapPut("/customers/{id:Guid}", async ([FromRoute(Name = "id")]Guid id, [FromBody]Customer customer, CustomerData data) =>
{
    var existingCustomer = await data.GetByIdAsync(id);
    if (existingCustomer is null) return Results.NotFound(); //"If" in one line

    var updatedCustomer = existingCustomer with { CompanyName = customer.CompanyName, Projects = customer.Projects };

    await data.UpdateAsync(updatedCustomer);

    return TypedResults.Ok(updatedCustomer);

})
.WithName("UpdateCustomer")
.AddEndpointFilter<ValidateCustomer>();

// Uses the struct idea and the Data Annotation [As Parameters]
app.MapDelete("/customers/{id:Guid}",
    async ([AsParameters] DeleteRequest request) =>
    {
        await request.Data.DeleteById(request.Id);
        // should have some type of logic to check for error or actual completion
        return Results.NoContent();
    })
.WithName("DeleteCustomer")
.WithParameterValidation();

// ----------------------------------------------------------------------------

// Run the app

app.Run();

// -----------------------------------------------------------------------------------------

// Records for Customer and Project

public record Customer(Guid Id, [MinLength(5)]string CompanyName, List<Project> Projects);
public record Project(Guid Id, [MaxLength(15)]string ProjectName, Guid CustomerId);

// -----------------------------------------------------------------------------------------

// Can be seen as a service
public class CustomerData
{
    private readonly Guid _customer1Id = Guid.Parse("8903bfdd-d68f-4e95-8f6f-ee1757d93862"); //Guid.NewGuid();
    private readonly Guid _customer2Id = Guid.Parse("58912e6e-d6d1-4bcc-8a68-3a889a1c0f84");
    private readonly List<Customer> _customers;

    public CustomerData() {

        _customers = new List<Customer>
        {
            new Customer(_customer1Id,"Fender", new List<Project>
            {
                new Project(Guid.NewGuid(), "Stratocaster", _customer1Id),
                new Project(Guid.NewGuid(), "Telecaster", _customer2Id)
            }),

            new Customer(_customer2Id, "Gibson", new List<Project>
            {
                new Project(Guid.NewGuid(), "LesPaul", _customer1Id),
                new Project(Guid.NewGuid(), "SG", _customer2Id)
            })
        };
    }

    public async Task<List<Customer>> ListAsync()
    {
        return _customers;
    }

    public Task<Customer?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));
    }

    public Task AddAsync(Customer newCustomer)
    {
        _customers.Add(newCustomer);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Customer customer)
    {   
        // Note how both Any and FindIndex have lambda functions as parameter
            
        if (_customers.Any(c => c.Id == customer.Id))
        {
            var index = _customers.FindIndex(c => c.Id == customer.Id);
            _customers[index] = customer;
        }

        return Task.CompletedTask;
    }

    public Task DeleteById(Guid id)
    {
        if(_customers.Any(c=>c.Id == id))
        {
            _customers.RemoveAt(_customers.FindIndex(c => c.Id == id));
            
        }
        return Task.CompletedTask;
    }


}

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

// -------------------------------------------

// Better solution -- Works for both POST and PUT
// Create one validator per model type

public class  ValidateCustomer: IEndpointFilter 
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

// -------------------------------------------

// Another approach of validation -- using record structs binded to parameters on a specific route 

public readonly record struct DeleteRequest : IValidatableObject
{
    [FromRoute(Name = "id")]
    [Required] // is a Data Annotation, which are responsible for bringing more validation capabilities
    public Guid Id { get; init; }

    public CustomerData Data { get; init; }

    //The method for validation result goes inside the struct! 

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Id == Guid.Empty )
        {
            yield return new ValidationResult("Id is required.", new[] { nameof(Id) });
        }
    }

}



// Data Annotations: there are a bunch of them, including Url, Regex, Integer Range, String Length, even Phone and Email format annotations
// THEY DO NOT APPLY THE LOGIC BY THEIR OWN, though. Its purely for indication purposes. They need to be "enforced". 
// There is, though, a NuGetPackage that enforce that for us, which we'll see briefly.

// Fluent Validation is basically the standard NuGetPackage for validation purposes in .NetCore.
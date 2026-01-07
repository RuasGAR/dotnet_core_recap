using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace NimblePros.Customers.Web.Customers;

public static class CustomerEndpoints
{


    public static void MapCustomerEndpoints(this WebApplication app)
    {
        // Endpoints


        // Route Groups

        var _customerGroup = app.MapGroup("/customers");

        var _customerGroupWithValidation = app.MapGroup("/")
            .WithParameterValidation();

        // Routes

        _customerGroup.MapGet("/", async (CustomerData data) =>
        {
            var customers = await data.ListAsync();
            return customers;
        })
        .WithName("ListCustomers");

        _customerGroup.MapGet("/{id:guid}", async (Guid id, CustomerData data) =>

            await data.GetByIdAsync(id) is Customer customer
                ? TypedResults.Ok(customer)
                : Results.NotFound()

        )
        .WithName("GetCustomerById");

        _customerGroup.MapPost("/", async (Customer customer, CustomerData data) =>
        {
            var newCustomer = customer with { Id = Guid.NewGuid(), Projects = new(), Email = "test@test.com"  };
            await data.AddAsync(newCustomer);

            var customerEmailService = new CustomerEmailService();
            customerEmailService.SendWelcomeEmail(newCustomer);

            return Results.Created($"/customers/{newCustomer.Id}", newCustomer);
        })
        .WithName("AddCustomer");

        _customerGroup.MapPut("/{id:Guid}", async ([FromRoute(Name = "id")] Guid id, [FromBody] Customer customer, CustomerData data) =>
        {
            var existingCustomer = await data.GetByIdAsync(id);
            if (existingCustomer is null) return Results.NotFound(); //"If" in one line

            var updatedCustomer = existingCustomer with { CompanyName = customer.CompanyName, Projects = customer.Projects };

            await data.UpdateAsync(updatedCustomer);

            return TypedResults.Ok(updatedCustomer); 

        })
        .WithName("UpdateCustomer");

        // Uses the struct idea and the Data Annotation [As Parameters]
        _customerGroupWithValidation.MapDelete("/customers/{id:Guid}",
            async ([AsParameters] DeleteRequest request) =>
            {
                await request.Data.DeleteById(request.Id);
                // should have some type of logic to check for error or actual completion
                return Results.NoContent();
            })
        .WithName("DeleteCustomer");

    }
}

// -----------------------------------------------------------------------------------------

// Models

public record Customer(Guid Id, [MinLength(5)] string CompanyName, List<Project> Projects, string Email);
public record Project(Guid Id, [MaxLength(15)] string ProjectName, Guid CustomerId);

// -----------------------------------------------------------------------------------------

// Can be seen as a service
public class CustomerData
{

    // We are using in-memory data storage for simplicity

    private readonly Guid _customer1Id = Guid.Parse("8903bfdd-d68f-4e95-8f6f-ee1757d93862"); //Guid.NewGuid();
    private readonly Guid _customer2Id = Guid.Parse("58912e6e-d6d1-4bcc-8a68-3a889a1c0f84");
    private readonly List<Customer> _customers;

    public CustomerData()
    {
        string testEmail = "test@test.com";

        _customers = new List<Customer>
        {
            new Customer(_customer1Id,"Fender", new List<Project>
            {
                new Project(Guid.NewGuid(), "Stratocaster", _customer1Id),
                new Project(Guid.NewGuid(), "Telecaster", _customer2Id)
            }, testEmail
            ),

            new Customer(_customer2Id, "Gibson", new List<Project>
            {
                new Project(Guid.NewGuid(), "LesPaul", _customer1Id),
                new Project(Guid.NewGuid(), "SG", _customer2Id)
            }, testEmail
            )
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

        if (_customers.Any(c => c.Id == customer.Id))
        {
            var index = _customers.FindIndex(c => c.Id == customer.Id);
            _customers[index] = customer;
        }

        return Task.CompletedTask;
    }

    public Task DeleteById(Guid id)
    {
        if (_customers.Any(c => c.Id == id))
        {
            _customers.RemoveAt(_customers.FindIndex(c => c.Id == id));

        }
        return Task.CompletedTask;
    }
}

public readonly record struct DeleteRequest : IValidatableObject
{
    [FromRoute(Name = "id")]
    [Required] // is a Data Annotation, which are responsible for bringing more validation capabilities
    public Guid Id { get; init; }

    public CustomerData Data { get; init; }

    //The method for validation result goes inside the struct! 

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Id == Guid.Empty)
        {
            yield return new ValidationResult("Id is required.", new[] { nameof(Id) });
        }
    }

}
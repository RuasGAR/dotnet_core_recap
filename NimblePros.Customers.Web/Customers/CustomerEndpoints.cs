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

        _customerGroup.MapGet("/", async (ICustomerData data) =>
        {
            var customers = await data.List();
            return customers;
        })
        .WithName("ListCustomers");

        _customerGroup.MapGet("/{id:guid}", async (Guid id, ICustomerData data) =>

            await data.GetById(id) is Customer customer
                ? TypedResults.Ok(customer)
                : Results.NotFound()

        )
        .WithName("GetCustomerById");

        _customerGroup.MapPost("/", async (Customer customer, ICustomerData data, ICustomerEmailService customerEmailService) =>
        {
            var newCustomer = customer with { Id = Guid.NewGuid(), Projects = new(), EmailAddress = "test@test.com"  };
            
            await data.Add(newCustomer);

            await customerEmailService.SendWelcomeEmail(newCustomer);

            return Results.Created($"/customers/{newCustomer.Id}", newCustomer);
        })
        .WithName("AddCustomer");

        _customerGroup.MapPut("/{id:Guid}", async ([FromRoute(Name = "id")] Guid id, [FromBody] Customer customer, ICustomerData data) =>
        {
            var existingCustomer = await data.GetById(id);
            if (existingCustomer is null) return Results.NotFound(); //"If" in one line

            var updatedCustomer = existingCustomer with { CompanyName = customer.CompanyName, Projects = customer.Projects };

            await data.Update(id, updatedCustomer);

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

public readonly record struct DeleteRequest : IValidatableObject
{
    [FromRoute(Name = "id")]
    [Required] // is a Data Annotation, which are responsible for bringing more validation capabilities
    public Guid Id { get; init; }

    public ICustomerData Data { get; init; }

    //The method for validation result goes inside the struct! 

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Id == Guid.Empty)
        {
            yield return new ValidationResult("Id is required.", new[] { nameof(Id) });
        }
    }

}
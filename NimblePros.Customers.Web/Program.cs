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
        ? Results.Ok(customer)
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
.WithName("AddCustomer");

app.MapPut("/customers/{id:Guid}", async (Guid id, Customer customer, CustomerData data) =>
{
    var existingCustomer = await data.GetByIdAsync(id);
    if (existingCustomer is null) return Results.NotFound(); //"If" in one line

    var updatedCustomer = existingCustomer with { CompanyName = customer.CompanyName, Projects = customer.Projects };

    await data.UpdateAsync(updatedCustomer);

    return Results.Ok(updatedCustomer);

})
.WithName("UpdateCustomer");


app.Run();

public record Customer(Guid Id, string CompanyName, List<Project> Projects);
public record Project(Guid Id, string ProjectName, Guid CustomerId);

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
}

// just didn't do delete because it's very similar to update
// the differences are: no customer parameter is needed,  and the Results method used is Results.NoContent();

// OBS: it's a goot practice to always return if the correspondent id passed to deletion actually exists.
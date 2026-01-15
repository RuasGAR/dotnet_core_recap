namespace NimblePros.Customers.Web.Customers;


public class CustomerData : ICustomerData
{

    private readonly Guid _customer1Id = Guid.Parse("8903bfdd-d68f-4e95-8f6f-ee1757d93862"); //Guid.NewGuid();
    private readonly Guid _customer2Id = Guid.Parse("58912e6e-d6d1-4bcc-8a68-3a889a1c0f84");
    private readonly List<Customer> _customers;

    public CustomerData()
    {
        string testEmail = "test@test.com";

        _customers = new List<Customer>
        {
            new Customer(_customer1Id,"Fender", testEmail, new List<Project>
            {
                new Project(Guid.NewGuid(), "Stratocaster", _customer1Id),
                new Project(Guid.NewGuid(), "Telecaster", _customer2Id)
            }),

            new Customer(_customer2Id, "Gibson", testEmail, new List<Project>
            {
                new Project(Guid.NewGuid(), "LesPaul", _customer1Id),
                new Project(Guid.NewGuid(), "SG", _customer2Id)
            })
        };
    }

    public async Task<IEnumerable<Customer>> List()
    {
        return _customers;
    }

    public Task<Customer?> GetById(Guid id)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));
    }

    public Task Add(Customer newCustomer)
    {
        _customers.Add(newCustomer);
        return Task.CompletedTask;
    }

    public Task Update(Guid Id, Customer customer)
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

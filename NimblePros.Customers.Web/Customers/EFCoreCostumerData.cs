using Microsoft.EntityFrameworkCore;

namespace NimblePros.Customers.Web.Customers;

public class EFCoreCostumerData(AppDbContext dbContext) : ICustomerData
{

    private readonly AppDbContext _dbContext = dbContext;

    public async Task Add(Customer newCustomer)
    {
        _dbContext.Customers.Add(newCustomer); // just queue the addition, but do not save yet
        await _dbContext.SaveChangesAsync(); // persist changes to the database

    }

    public async Task DeleteById(Guid id)
    {
        var customer = await _dbContext.Customers.FindAsync(id);
        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Customer?> GetById(Guid id)
    {
        var customer = await _dbContext.Customers.FindAsync(id);
        return customer;
    }

    public async Task<IEnumerable<Customer>> List()
    {
        return await _dbContext.Customers.ToListAsync();
    }

    public async Task Update(Guid id, Customer customer)
    {
        // As long as we change the exact record we fetched, all we need to do is save changes
        await _dbContext.SaveChangesAsync();
    }
}

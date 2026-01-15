
namespace NimblePros.Customers.Web.Customers
{
    public interface ICustomerData
    {
        Task Add(Customer newCustomer);
        Task DeleteById(Guid id);
        Task<Customer?> GetById(Guid id);
        Task<IEnumerable<Customer>> List();
        Task Update(Guid id, Customer customer);
    }
}
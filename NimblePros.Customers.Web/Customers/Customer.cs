namespace NimblePros.Customers.Web.Customers;

// We keep it as a record because of the use of "with" expression in the endpoints
public record Customer { 

    public Guid Id { get; set; }
    public string CompanyName { get; set; }
    public string EmailAddress { get; set; }
    public List<Project> Projects { get; set; } = new();

    public Customer (Guid Id, string CompanyName, string EmailAdress, List<Project> Projects)
    {
        this.Id = Id;
        this.CompanyName = CompanyName; 
        this.EmailAddress = EmailAdress;
        this.Projects = Projects;
    }

    private Customer()
    {
        // For EF Core
    }
}
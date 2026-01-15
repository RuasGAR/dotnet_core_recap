using System.ComponentModel.DataAnnotations;

namespace NimblePros.Customers.Web.Customers;


public record Project
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; }
    public Guid CustomerId { get; set; }

    public Project(Guid Id, string ProjectName, Guid CustomerId)
    {
        this.Id = Id;
        this.ProjectName = ProjectName;
        this.CustomerId = CustomerId;
    }

}


namespace App1Backend.Entities;

public class ServiceCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Service> Services { get; set; } = [];
}

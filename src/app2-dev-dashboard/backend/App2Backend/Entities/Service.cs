namespace App2Backend.Entities;

public class Service
{
    public Guid Id { get; set; }
    public Guid BusinessUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public BusinessUnit BusinessUnit { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = [];
}

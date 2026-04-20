namespace App1Backend.Entities;

public class AbTestVariant
{
    public Guid Id { get; set; }
    public Guid AbTestId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TrafficAllocation { get; set; }
    public DateTime CreatedAt { get; set; }

    public AbTest AbTest { get; set; } = null!;
    public ICollection<AbTestResult> Results { get; set; } = [];
}

namespace App1Backend.Entities;

public class AbTest
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PrimaryMetric { get; set; } = string.Empty;
    public DateOnly StartedAt { get; set; }
    public DateOnly? EndedAt { get; set; }
    public string Status { get; set; } = "running";
    public Guid? WinnerVariantId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Service Service { get; set; } = null!;
    public ICollection<AbTestVariant> Variants { get; set; } = [];
}

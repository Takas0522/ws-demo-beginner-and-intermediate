namespace App1Backend.Entities;

public class ServicePlan
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }

    public Service Service { get; set; } = null!;
    public ICollection<RevenueDaily> Revenues { get; set; } = [];
}

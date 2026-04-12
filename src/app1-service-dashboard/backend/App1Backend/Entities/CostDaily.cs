namespace App1Backend.Entities;

public class CostDaily
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public DateOnly Date { get; set; }
    public string CostType { get; set; } = "other";
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public Service Service { get; set; } = null!;
}

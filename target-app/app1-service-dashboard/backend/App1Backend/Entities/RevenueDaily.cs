namespace App1Backend.Entities;

public class RevenueDaily
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public Guid PlanId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public int SubscriptionCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public Service Service { get; set; } = null!;
    public ServicePlan Plan { get; set; } = null!;
}

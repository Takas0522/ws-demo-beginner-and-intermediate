namespace App1Backend.Entities;

public class UserMetricDaily
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public DateOnly Date { get; set; }
    public int Mau { get; set; }
    public int Dau { get; set; }
    public int NewUsers { get; set; }
    public int ChurnedUsers { get; set; }
    public int TotalSubscriptions { get; set; }
    public DateTime CreatedAt { get; set; }

    public Service Service { get; set; } = null!;
}

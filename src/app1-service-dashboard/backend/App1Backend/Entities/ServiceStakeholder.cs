namespace App1Backend.Entities;

public class ServiceStakeholder
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public Guid AuthUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal AllocatedHoursMonthly { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Service Service { get; set; } = null!;
}

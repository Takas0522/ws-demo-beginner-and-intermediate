namespace App1Backend.Entities;

public class Service
{
    public Guid Id { get; set; }
    public Guid BusinessUnitId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? LaunchedAt { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public BusinessUnit BusinessUnit { get; set; } = null!;
    public ServiceCategory Category { get; set; } = null!;
    public ICollection<ServicePlan> Plans { get; set; } = [];
    public ICollection<UserMetricDaily> UserMetrics { get; set; } = [];
    public ICollection<RevenueDaily> Revenues { get; set; } = [];
    public ICollection<CostDaily> Costs { get; set; } = [];
    public ICollection<AbTest> AbTests { get; set; } = [];
    public ICollection<ServiceStakeholder> Stakeholders { get; set; } = [];
}

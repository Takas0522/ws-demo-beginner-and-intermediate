namespace App2Backend.Entities;

public class Sprint
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public string Status { get; set; } = "planning";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int PlannedVelocity { get; set; }
    public int ActualVelocity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Project Project { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = [];
    public ICollection<SprintMetricDaily> Metrics { get; set; } = [];
}

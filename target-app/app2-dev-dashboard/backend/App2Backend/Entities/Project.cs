namespace App2Backend.Entities;

public class Project
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "planning";
    public DateOnly? PlannedStartDate { get; set; }
    public DateOnly? PlannedEndDate { get; set; }
    public DateOnly? ActualStartDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public decimal Budget { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Service Service { get; set; } = null!;
    public ICollection<Sprint> Sprints { get; set; } = [];
    public ICollection<Ticket> Tickets { get; set; } = [];
    public ICollection<PullRequest> PullRequests { get; set; } = [];
}

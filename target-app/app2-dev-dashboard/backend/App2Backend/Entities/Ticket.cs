namespace App2Backend.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? SprintId { get; set; }
    public Guid? AssigneeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TicketType { get; set; } = "task";
    public string Priority { get; set; } = "medium";
    public string Status { get; set; } = "open";
    public int? StoryPoints { get; set; }
    public decimal? EstimatedHours { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Project Project { get; set; } = null!;
    public Sprint? Sprint { get; set; }
    public Member? Assignee { get; set; }
    public ICollection<WorkLog> WorkLogs { get; set; } = [];
    public ICollection<PrTicketLink> PrLinks { get; set; } = [];
}

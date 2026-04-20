namespace App2Backend.Entities;

public class SprintMetricDaily
{
    public Guid Id { get; set; }
    public Guid SprintId { get; set; }
    public DateOnly Date { get; set; }
    public int RemainingStoryPoints { get; set; }
    public decimal RemainingHours { get; set; }
    public int CompletedTickets { get; set; }
    public DateTime CreatedAt { get; set; }
    public Sprint Sprint { get; set; } = null!;
}

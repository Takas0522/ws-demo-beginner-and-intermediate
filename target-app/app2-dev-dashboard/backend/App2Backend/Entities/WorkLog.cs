namespace App2Backend.Entities;

public class WorkLog
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid MemberId { get; set; }
    public DateOnly WorkDate { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public decimal HourlyRateSnapshot { get; set; }
    public decimal Cost { get; set; }
    public DateTime CreatedAt { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public Member Member { get; set; } = null!;
}

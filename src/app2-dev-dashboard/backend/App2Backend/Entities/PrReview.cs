namespace App2Backend.Entities;

public class PrReview
{
    public Guid Id { get; set; }
    public Guid PullRequestId { get; set; }
    public Guid ReviewerId { get; set; }
    public string Status { get; set; } = "commented";
    public DateTime SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public PullRequest PullRequest { get; set; } = null!;
    public Member Reviewer { get; set; } = null!;
}

public class PrTicketLink
{
    public Guid PullRequestId { get; set; }
    public Guid TicketId { get; set; }
    public PullRequest PullRequest { get; set; } = null!;
    public Ticket Ticket { get; set; } = null!;
}

namespace App2Backend.Entities;

public class PullRequest
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public int PrNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
    public string Status { get; set; } = "open";
    public string BaseBranch { get; set; } = "main";
    public string HeadBranch { get; set; } = string.Empty;
    public int ChangedFiles { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? MergedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Project Project { get; set; } = null!;
    public Member Author { get; set; } = null!;
    public ICollection<PrReview> Reviews { get; set; } = [];
    public ICollection<PrTicketLink> TicketLinks { get; set; } = [];
}

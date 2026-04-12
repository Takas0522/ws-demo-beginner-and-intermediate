namespace App2Backend.Entities;

public class Member
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? HourlyRate { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Department Department { get; set; } = null!;
    public ICollection<WorkLog> WorkLogs { get; set; } = [];
    public ICollection<PullRequest> AuthoredPrs { get; set; } = [];
}

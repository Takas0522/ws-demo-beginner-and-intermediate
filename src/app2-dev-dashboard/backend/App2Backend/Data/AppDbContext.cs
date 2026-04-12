using App2Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace App2Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BusinessUnit> BusinessUnits => Set<BusinessUnit>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<WorkLog> WorkLogs => Set<WorkLog>();
    public DbSet<PullRequest> PullRequests => Set<PullRequest>();
    public DbSet<PrReview> PrReviews => Set<PrReview>();
    public DbSet<PrTicketLink> PrTicketLinks => Set<PrTicketLink>();
    public DbSet<SprintMetricDaily> SprintMetricDailies => Set<SprintMetricDaily>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessUnit>(e => e.ToTable("business_units"));
        modelBuilder.Entity<Service>(e => e.ToTable("services"));
        modelBuilder.Entity<Department>(e => e.ToTable("departments"));
        modelBuilder.Entity<Member>(e => e.ToTable("members"));

        modelBuilder.Entity<Project>(e =>
        {
            e.ToTable("projects");
            e.HasOne(p => p.Service).WithMany(s => s.Projects).HasForeignKey(p => p.ServiceId);
        });

        modelBuilder.Entity<Sprint>(e =>
        {
            e.ToTable("sprints");
            e.HasOne(s => s.Project).WithMany(p => p.Sprints).HasForeignKey(s => s.ProjectId);
        });

        modelBuilder.Entity<Ticket>(e =>
        {
            e.ToTable("tickets");
            e.HasOne(t => t.Sprint).WithMany(s => s.Tickets).HasForeignKey(t => t.SprintId);
            e.HasOne(t => t.Assignee).WithMany().HasForeignKey(t => t.AssigneeId);
        });

        modelBuilder.Entity<WorkLog>(e => e.ToTable("work_logs"));

        modelBuilder.Entity<PullRequest>(e =>
        {
            e.ToTable("pull_requests");
            e.HasOne(pr => pr.Author).WithMany(m => m.AuthoredPrs).HasForeignKey(pr => pr.AuthorId);
            e.HasIndex(pr => new { pr.ProjectId, pr.PrNumber }).IsUnique();
        });

        modelBuilder.Entity<PrReview>(e => e.ToTable("pr_reviews"));

        modelBuilder.Entity<PrTicketLink>(e =>
        {
            e.ToTable("pr_ticket_links");
            e.HasKey(l => new { l.PullRequestId, l.TicketId });
            e.HasOne(l => l.PullRequest).WithMany(pr => pr.TicketLinks).HasForeignKey(l => l.PullRequestId);
            e.HasOne(l => l.Ticket).WithMany(t => t.PrLinks).HasForeignKey(l => l.TicketId);
        });

        modelBuilder.Entity<SprintMetricDaily>(e =>
        {
            e.ToTable("sprint_metric_daily");
            e.HasIndex(m => new { m.SprintId, m.Date }).IsUnique();
        });
    }
}

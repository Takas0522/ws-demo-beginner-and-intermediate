using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using App2Backend.Data;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App2Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/export")]
public class ExportController(AppDbContext db) : ControllerBase
{
    private static readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture);

    [HttpGet("project-summary")]
    public async Task<IActionResult> ExportProjectSummary([FromQuery] Guid[]? businessUnitIds)
    {
        var query = db.Projects
            .Include(p => p.Service).ThenInclude(s => s.BusinessUnit)
            .Include(p => p.Tickets).ThenInclude(t => t.WorkLogs)
            .AsQueryable();

        if (businessUnitIds is { Length: > 0 })
            query = query.Where(p => businessUnitIds.Contains(p.Service.BusinessUnitId));

        var projects = await query.ToListAsync();

        var rows = projects.Select(p =>
        {
            var totalTickets    = p.Tickets.Count;
            var doneTickets     = p.Tickets.Count(t => t.Status == "done");
            var progressRate    = totalTickets > 0 ? Math.Round((decimal)doneTickets / totalTickets * 100, 2) : 0;
            var actualHours     = p.Tickets.Sum(t => t.WorkLogs.Sum(w => w.Hours));
            var actualCost      = p.Tickets.Sum(t => t.WorkLogs.Sum(w => w.Cost));
            var remainingCost   = p.Budget - actualCost;
            var budgetRate      = p.Budget > 0 ? Math.Round(actualCost / p.Budget * 100, 2) : 0;

            return new
            {
                ProjectId           = p.Id,
                ProjectName         = p.Name,
                ServiceId           = p.ServiceId,
                Service             = p.Service.Name,
                BusinessUnitId      = p.Service.BusinessUnitId,
                BusinessUnit        = p.Service.BusinessUnit.Name,
                Status              = p.Status,
                PlannedStartDate    = p.PlannedStartDate?.ToString("yyyy-MM-dd") ?? "",
                PlannedEndDate      = p.PlannedEndDate?.ToString("yyyy-MM-dd") ?? "",
                Budget              = p.Budget,
                TotalTickets        = totalTickets,
                CompletedTickets    = doneTickets,
                ProgressRate        = progressRate,
                ActualHours         = actualHours,
                ActualCost          = actualCost,
                RemainingBudget     = remainingCost,
                BudgetConsumptionRate = budgetRate
            };
        });

        return CsvResult(rows, $"project-summary_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> ExportTasks(
        [FromQuery] Guid[]? projectIds,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var query = db.Tickets
            .Include(t => t.Project).ThenInclude(p => p.Service)
            .Include(t => t.Sprint)
            .Include(t => t.Assignee).ThenInclude(a => a!.Department)
            .Include(t => t.WorkLogs)
            .AsQueryable();

        if (projectIds is { Length: > 0 })
            query = query.Where(t => projectIds.Contains(t.ProjectId));
        if (from.HasValue)
            query = query.Where(t => t.CreatedAt >= from.Value.ToDateTime(TimeOnly.MinValue));
        if (to.HasValue)
            query = query.Where(t => t.CreatedAt <= to.Value.ToDateTime(TimeOnly.MaxValue));

        var tickets = await query.ToListAsync();

        var rows = tickets.Select(t => new
        {
            TicketId         = t.Id,
            ProjectId        = t.ProjectId,
            ProjectName      = t.Project.Name,
            ServiceId        = t.Project.ServiceId,
            ServiceName      = t.Project.Service.Name,
            SprintId         = t.SprintId?.ToString() ?? "",
            SprintName       = t.Sprint?.Name ?? "",
            Title            = t.Title,
            TicketType       = t.TicketType,
            Priority         = t.Priority,
            Status           = t.Status,
            AssigneeId       = t.AssigneeId?.ToString() ?? "",
            AuthUserId       = t.Assignee?.AuthUserId?.ToString() ?? "",
            AssigneeName     = t.Assignee?.Name ?? "",
            DepartmentId     = t.Assignee?.DepartmentId.ToString() ?? "",
            DepartmentName   = t.Assignee?.Department?.Name ?? "",
            StoryPoints      = t.StoryPoints?.ToString() ?? "",
            EstimatedHours   = t.EstimatedHours?.ToString() ?? "",
            ActualHours      = t.WorkLogs.Sum(w => w.Hours),
            DueDate          = t.DueDate?.ToString("yyyy-MM-dd") ?? "",
            StartedAt        = t.StartedAt?.ToString("yyyy-MM-dd") ?? "",
            CompletedAt      = t.CompletedAt?.ToString("yyyy-MM-dd") ?? ""
        });

        return CsvResult(rows, $"tasks_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("pull-requests")]
    public async Task<IActionResult> ExportPullRequests([FromQuery] Guid[]? projectIds)
    {
        var query = db.PullRequests
            .Include(pr => pr.Project).ThenInclude(p => p.Service)
            .Include(pr => pr.Author)
            .Include(pr => pr.Reviews).ThenInclude(r => r.Reviewer)
            .AsQueryable();

        if (projectIds is { Length: > 0 })
            query = query.Where(pr => projectIds.Contains(pr.ProjectId));

        var prs = await query.ToListAsync();

        var rows = prs.Select(pr =>
        {
            var mergeTime = pr.MergedAt.HasValue
                ? Math.Round((pr.MergedAt.Value - pr.OpenedAt).TotalHours, 1).ToString()
                : "";
            var reviewers = string.Join(";", pr.Reviews.Select(r => r.Reviewer.Name).Distinct());

            return new
            {
                PrId         = pr.Id,
                PrNumber     = pr.PrNumber,
                ProjectId    = pr.ProjectId,
                ProjectName  = pr.Project.Name,
                ServiceId    = pr.Project.ServiceId,
                ServiceName  = pr.Project.Service.Name,
                Title        = pr.Title,
                Status       = pr.Status,
                AuthorId     = pr.AuthorId,
                AuthUserId   = pr.Author.AuthUserId?.ToString() ?? "",
                AuthorName   = pr.Author.Name,
                Reviewers    = reviewers,
                BaseBranch   = pr.BaseBranch,
                HeadBranch   = pr.HeadBranch,
                ChangedFiles = pr.ChangedFiles,
                Additions    = pr.Additions,
                Deletions    = pr.Deletions,
                OpenedAt     = pr.OpenedAt.ToString("yyyy-MM-dd HH:mm"),
                MergedAt     = pr.MergedAt?.ToString("yyyy-MM-dd HH:mm") ?? "",
                MergeTimeHours = mergeTime
            };
        });

        return CsvResult(rows, $"pull-requests_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("work-cost")]
    public async Task<IActionResult> ExportWorkCost(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid[]? projectIds)
    {
        var dateFrom = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));
        var dateTo   = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var query = db.WorkLogs
            .Include(w => w.Member).ThenInclude(m => m.Department)
            .Include(w => w.Ticket).ThenInclude(t => t.Project).ThenInclude(p => p.Service)
            .Where(w => w.WorkDate >= dateFrom && w.WorkDate <= dateTo);

        if (projectIds is { Length: > 0 })
            query = query.Where(w => projectIds.Contains(w.Ticket.ProjectId));

        var logs = await query.OrderBy(w => w.WorkDate).ToListAsync();

        var rows = logs.Select(w => new
        {
            WorkLogId      = w.Id,
            MemberId       = w.MemberId,
            AuthUserId     = w.Member.AuthUserId?.ToString() ?? "",
            MemberName     = w.Member.Name,
            DepartmentId   = w.Member.DepartmentId,
            Department     = w.Member.Department.Name,
            ProjectId      = w.Ticket.ProjectId,
            ProjectName    = w.Ticket.Project.Name,
            ServiceId      = w.Ticket.Project.ServiceId,
            ServiceName    = w.Ticket.Project.Service.Name,
            TicketId       = w.TicketId,
            TicketTitle    = w.Ticket.Title,
            WorkDate       = w.WorkDate.ToString("yyyy-MM-dd"),
            Hours          = w.Hours,
            HourlyRate     = w.HourlyRateSnapshot,
            Cost           = w.Cost,
            Description    = w.Description ?? ""
        });

        return CsvResult(rows, $"work-cost_{dateFrom:yyyyMMdd}-{dateTo:yyyyMMdd}.csv");
    }

    private FileStreamResult CsvResult<T>(IEnumerable<T> rows, string filename)
    {
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CsvConfig))
        {
            csv.WriteRecords(rows);
        }
        stream.Position = 0;
        Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{filename}\"");
        return File(stream, "text/csv; charset=utf-8");
    }
}

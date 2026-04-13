using Microsoft.AspNetCore.Authorization;
using App2Backend.Data;
using App2Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App2Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/projects")]
public class ProjectsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid[]? businessUnitIds,
        [FromQuery] string? status)
    {
        var query = db.Projects
            .Include(p => p.Service).ThenInclude(s => s.BusinessUnit)
            .AsQueryable();

        if (businessUnitIds is { Length: > 0 })
            query = query.Where(p => businessUnitIds.Contains(p.Service.BusinessUnitId));
        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        var projects = await query
            .Select(p => new
            {
                p.Id, p.Name, p.Status, p.Budget,
                p.PlannedStartDate, p.PlannedEndDate,
                Service = new { p.Service.Id, p.Service.Name },
                BusinessUnit = new { p.Service.BusinessUnit.Id, p.Service.BusinessUnit.Name },
                TotalTickets    = p.Tickets.Count,
                DoneTickets     = p.Tickets.Count(t => t.Status == "done"),
                OpenPrs         = p.PullRequests.Count(pr => pr.Status == "open")
            })
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var project = await db.Projects
            .Include(p => p.Service).ThenInclude(s => s.BusinessUnit)
            .Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null) return NotFound();

        var ticketStats = await db.Tickets
            .Where(t => t.ProjectId == id)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var bugStats = await db.Tickets
            .Where(t => t.ProjectId == id && t.TicketType == "bug")
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync();

        var prStats = await db.PullRequests
            .Where(pr => pr.ProjectId == id)
            .GroupBy(pr => pr.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var avgMergeTime = await db.PullRequests
            .Where(pr => pr.ProjectId == id && pr.MergedAt.HasValue)
            .Select(pr => (pr.MergedAt!.Value - pr.OpenedAt).TotalHours)
            .AverageAsync(h => (double?)h);

        var actualCost = await db.WorkLogs
            .Where(w => w.Ticket.ProjectId == id)
            .SumAsync(w => w.Cost);

        var actualHours = await db.WorkLogs
            .Where(w => w.Ticket.ProjectId == id)
            .SumAsync(w => w.Hours);

        var totalTickets = ticketStats.Sum(t => t.Count);
        var doneTickets  = ticketStats.FirstOrDefault(t => t.Status == "done")?.Count ?? 0;
        var actualProgress  = totalTickets > 0 ? (decimal)doneTickets / totalTickets : 0m;

        var today = DateTime.UtcNow;
        decimal plannedProgress = 0m;
        if (project.PlannedStartDate.HasValue && project.PlannedEndDate.HasValue)
        {
            var start     = project.PlannedStartDate.Value.ToDateTime(TimeOnly.MinValue);
            var end       = project.PlannedEndDate.Value.ToDateTime(TimeOnly.MinValue);
            var totalDays = (end - start).TotalDays;
            var elapsed   = Math.Min((today - start).TotalDays, totalDays);
            plannedProgress = totalDays > 0 ? (decimal)(elapsed / totalDays) : 0m;
        }

        var evm = EvmCalculator.Calculate(project.Budget, plannedProgress, actualProgress, actualCost);

        return Ok(new
        {
            project.Id,
            project.Name,
            project.Description,
            project.Status,
            project.Budget,
            project.PlannedStartDate,
            project.PlannedEndDate,
            project.ActualStartDate,
            Service = new { project.Service.Id, project.Service.Name },
            BusinessUnit = new { project.Service.BusinessUnit.Id, project.Service.BusinessUnit.Name },
            Sprints  = project.Sprints.Select(s => new
            {
                s.Id, s.Name, s.Status, s.StartDate, s.EndDate,
                s.PlannedVelocity, s.ActualVelocity
            }),
            TicketStats    = ticketStats,
            BugStats       = bugStats,
            PrStats        = prStats,
            AvgMergeTimeHours = Math.Round(avgMergeTime ?? 0, 1),
            ActualHours    = actualHours,
            ActualCost     = actualCost,
            Evm            = evm
        });
    }

    [HttpGet("{id:guid}/sprints")]
    public async Task<IActionResult> GetSprints(Guid id)
    {
        var sprints = await db.Sprints
            .Where(s => s.ProjectId == id)
            .OrderBy(s => s.StartDate)
            .Select(s => new
            {
                s.Id, s.Name, s.Goal, s.Status,
                s.StartDate, s.EndDate,
                s.PlannedVelocity, s.ActualVelocity,
                TotalTickets = s.Tickets.Count,
                DoneTickets  = s.Tickets.Count(t => t.Status == "done")
            })
            .ToListAsync();
        return Ok(sprints);
    }

    [HttpGet("{id:guid}/tickets")]
    public async Task<IActionResult> GetTickets(
        Guid id,
        [FromQuery] string? status,
        [FromQuery] string? ticketType,
        [FromQuery] string? priority)
    {
        var query = db.Tickets
            .Include(t => t.Assignee).ThenInclude(a => a!.Department)
            .Include(t => t.Sprint)
            .Where(t => t.ProjectId == id);

        if (!string.IsNullOrEmpty(status))     query = query.Where(t => t.Status == status);
        if (!string.IsNullOrEmpty(ticketType)) query = query.Where(t => t.TicketType == ticketType);
        if (!string.IsNullOrEmpty(priority))   query = query.Where(t => t.Priority == priority);

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id, t.Title, t.TicketType, t.Priority, t.Status,
                t.StoryPoints, t.EstimatedHours, t.DueDate,
                t.StartedAt, t.CompletedAt,
                Sprint   = t.Sprint == null ? null : new { t.Sprint.Id, t.Sprint.Name },
                Assignee = t.Assignee == null ? null : new
                {
                    t.Assignee.Id,
                    t.Assignee.Name,
                    Department = new { t.Assignee.Department.Id, t.Assignee.Department.Name }
                },
                ActualHours = (decimal?)t.WorkLogs.Sum(w => (decimal?)w.Hours) ?? 0
            })
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpGet("{id:guid}/pull-requests")]
    public async Task<IActionResult> GetPullRequests(Guid id, [FromQuery] string? status)
    {
        var query = db.PullRequests
            .Include(pr => pr.Author)
            .Include(pr => pr.Reviews)
            .Where(pr => pr.ProjectId == id);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(pr => pr.Status == status);

        var prs = await query
            .OrderByDescending(pr => pr.OpenedAt)
            .Select(pr => new
            {
                pr.Id, pr.PrNumber, pr.Title, pr.Status,
                pr.BaseBranch, pr.HeadBranch,
                pr.ChangedFiles, pr.Additions, pr.Deletions,
                pr.OpenedAt, pr.MergedAt, pr.ClosedAt,
                Author = new { pr.Author.Id, pr.Author.Name },
                ApprovalCount        = pr.Reviews.Count(r => r.Status == "approved"),
                ChangesRequestedCount = pr.Reviews.Count(r => r.Status == "changes_requested")
            })
            .ToListAsync();

        return Ok(prs);
    }
}

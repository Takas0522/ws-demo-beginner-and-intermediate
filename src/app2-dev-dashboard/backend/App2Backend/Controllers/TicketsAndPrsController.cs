using App2Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App2Backend.Controllers;

[ApiController]
[Route("api")]
public class TicketsAndPrsController(AppDbContext db) : ControllerBase
{
    [HttpGet("tickets/{id:guid}")]
    public async Task<IActionResult> GetTicket(Guid id)
    {
        var ticket = await db.Tickets
            .Include(t => t.Project).ThenInclude(p => p.Service)
            .Include(t => t.Sprint)
            .Include(t => t.Assignee).ThenInclude(a => a!.Department)
            .Include(t => t.WorkLogs).ThenInclude(w => w.Member)
            .Include(t => t.PrLinks).ThenInclude(l => l.PullRequest)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null) return NotFound();

        return Ok(new
        {
            ticket.Id, ticket.Title, ticket.Description,
            ticket.TicketType, ticket.Priority, ticket.Status,
            ticket.StoryPoints, ticket.EstimatedHours, ticket.DueDate,
            ticket.StartedAt, ticket.CompletedAt,
            Project  = new { ticket.Project.Id, ticket.Project.Name, ServiceName = ticket.Project.Service.Name },
            Sprint   = ticket.Sprint == null ? null : new { ticket.Sprint.Id, ticket.Sprint.Name },
            Assignee = ticket.Assignee == null ? null : new
            {
                ticket.Assignee.Id, ticket.Assignee.Name,
                Department = new { ticket.Assignee.Department.Id, ticket.Assignee.Department.Name }
            },
            WorkLogs = ticket.WorkLogs.OrderByDescending(w => w.WorkDate).Select(w => new
            {
                w.Id, w.WorkDate, w.Hours, w.Description, w.Cost,
                Member = new { w.Member.Id, w.Member.Name }
            }),
            RelatedPrs = ticket.PrLinks.Select(l => new
            {
                l.PullRequest.Id, l.PullRequest.PrNumber, l.PullRequest.Title, l.PullRequest.Status
            })
        });
    }

    [HttpGet("pull-requests/{id:guid}")]
    public async Task<IActionResult> GetPullRequest(Guid id)
    {
        var pr = await db.PullRequests
            .Include(pr => pr.Project).ThenInclude(p => p.Service)
            .Include(pr => pr.Author).ThenInclude(a => a.Department)
            .Include(pr => pr.Reviews).ThenInclude(r => r.Reviewer)
            .Include(pr => pr.TicketLinks).ThenInclude(l => l.Ticket)
            .FirstOrDefaultAsync(pr => pr.Id == id);

        if (pr is null) return NotFound();

        var mergeTimeHours = pr.MergedAt.HasValue
            ? Math.Round((pr.MergedAt.Value - pr.OpenedAt).TotalHours, 1)
            : (double?)null;

        return Ok(new
        {
            pr.Id, pr.PrNumber, pr.Title, pr.Description, pr.Status,
            pr.BaseBranch, pr.HeadBranch,
            pr.ChangedFiles, pr.Additions, pr.Deletions,
            pr.OpenedAt, pr.MergedAt, pr.ClosedAt,
            MergeTimeHours = mergeTimeHours,
            Project = new { pr.Project.Id, pr.Project.Name, ServiceName = pr.Project.Service.Name },
            Author  = new
            {
                pr.Author.Id, pr.Author.Name,
                Department = new { pr.Author.Department.Id, pr.Author.Department.Name }
            },
            Reviews = pr.Reviews.Select(r => new
            {
                r.Id, r.Status, r.SubmittedAt,
                Reviewer = new { r.Reviewer.Id, r.Reviewer.Name }
            }),
            LinkedTickets = pr.TicketLinks.Select(l => new
            {
                l.Ticket.Id, l.Ticket.Title, l.Ticket.Status
            })
        });
    }
}

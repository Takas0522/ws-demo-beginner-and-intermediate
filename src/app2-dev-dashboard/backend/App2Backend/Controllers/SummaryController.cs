using App2Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App2Backend.Controllers;

[ApiController]
[Route("api/summary")]
public class SummaryController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        var activeProjects = await db.Projects.CountAsync(p => p.Status == "active");

        var ticketStats = await db.Tickets
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var openPrs = await db.PullRequests.CountAsync(pr => pr.Status == "open");

        var thisMonthStart = DateOnly.FromDateTime(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var monthlyHours = await db.WorkLogs
            .Where(w => w.WorkDate >= thisMonthStart && w.WorkDate <= today)
            .SumAsync(w => w.Hours);

        var monthlyCost = await db.WorkLogs
            .Where(w => w.WorkDate >= thisMonthStart && w.WorkDate <= today)
            .SumAsync(w => w.Cost);

        var totalBudget = await db.Projects
            .Where(p => p.Status == "active")
            .SumAsync(p => p.Budget);

        var totalActualCost = await db.WorkLogs
            .Include(w => w.Ticket)
            .Where(w => w.Ticket.Project.Status == "active")
            .SumAsync(w => w.Cost);

        var budgetConsumptionRate = totalBudget > 0
            ? Math.Round(totalActualCost / totalBudget * 100, 2)
            : 0m;

        var thisWeekStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek));
        var completedThisWeek = await db.Tickets
            .CountAsync(t => t.Status == "done" &&
                             t.CompletedAt.HasValue &&
                             DateOnly.FromDateTime(t.CompletedAt.Value) >= thisWeekStart);

        var projectsByStatus = await db.Projects
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            ActiveProjectCount     = activeProjects,
            ProjectsByStatus       = projectsByStatus,
            TicketStats            = ticketStats,
            OpenPrCount            = openPrs,
            CompletedTicketsThisWeek = completedThisWeek,
            MonthlyActualHours     = monthlyHours,
            MonthlyActualCost      = monthlyCost,
            TotalBudget            = totalBudget,
            TotalActualCost        = totalActualCost,
            BudgetConsumptionRate  = budgetConsumptionRate
        });
    }
}

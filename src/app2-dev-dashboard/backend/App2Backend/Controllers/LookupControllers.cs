using Microsoft.AspNetCore.Authorization;
using App2Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App2Backend.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class LookupControllers(AppDbContext db) : ControllerBase
{
    [HttpGet("business-units")]
    public async Task<IActionResult> GetBusinessUnits()
    {
        var units = await db.BusinessUnits
            .Select(b => new { b.Id, b.Name, b.Description })
            .ToListAsync();
        return Ok(units);
    }

    [HttpGet("business-units/{id:guid}/summary")]
    public async Task<IActionResult> GetBusinessUnitSummary(Guid id)
    {
        var unit = await db.BusinessUnits.FindAsync(id);
        if (unit is null) return NotFound();

        var serviceIds = await db.Services
            .Where(s => s.BusinessUnitId == id)
            .Select(s => s.Id)
            .ToListAsync();

        var projects = await db.Projects
            .Where(p => serviceIds.Contains(p.ServiceId))
            .Select(p => new
            {
                p.Id, p.Name, p.Status,
                ServiceName = p.Service.Name,
                TotalTickets    = p.Tickets.Count,
                CompletedTickets = p.Tickets.Count(t => t.Status == "done"),
                Budget = p.Budget
            })
            .ToListAsync();

        var monthlyStart = DateOnly.FromDateTime(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var projectIds = projects.Select(p => p.Id).ToList();

        var monthlyHours = await db.WorkLogs
            .Include(w => w.Ticket)
            .Where(w => projectIds.Contains(w.Ticket.ProjectId) &&
                        w.WorkDate >= monthlyStart && w.WorkDate <= today)
            .SumAsync(w => w.Hours);

        var monthlyCost = await db.WorkLogs
            .Include(w => w.Ticket)
            .Where(w => projectIds.Contains(w.Ticket.ProjectId) &&
                        w.WorkDate >= monthlyStart && w.WorkDate <= today)
            .SumAsync(w => w.Cost);

        return Ok(new
        {
            unit.Id, unit.Name, unit.Description,
            Projects         = projects,
            MonthlyHours     = monthlyHours,
            MonthlyCost      = monthlyCost,
            TotalBudget      = projects.Sum(p => p.Budget)
        });
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var depts = await db.Departments
            .Select(d => new { d.Id, d.Name, d.DefaultHourlyRate, d.Description })
            .ToListAsync();
        return Ok(depts);
    }

    [HttpGet("members")]
    public async Task<IActionResult> GetMembers([FromQuery] Guid? departmentId)
    {
        var query = db.Members.Include(m => m.Department).AsQueryable();
        if (departmentId.HasValue)
            query = query.Where(m => m.DepartmentId == departmentId.Value);

        var members = await query
            .Select(m => new
            {
                m.Id, m.Name, m.Status,
                Department = new { m.Department.Id, m.Department.Name }
            })
            .ToListAsync();
        return Ok(members);
    }

    [HttpGet("members/{id:guid}/work-logs")]
    public async Task<IActionResult> GetMemberWorkLogs(
        Guid id,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var dateFrom = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var dateTo   = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var logs = await db.WorkLogs
            .Where(w => w.MemberId == id && w.WorkDate >= dateFrom && w.WorkDate <= dateTo)
            .OrderByDescending(w => w.WorkDate)
            .Select(w => new
            {
                w.Id, w.WorkDate, w.Hours, w.Description, w.Cost,
                Ticket = new { w.Ticket.Id, w.Ticket.Title, ProjectId = w.Ticket.ProjectId }
            })
            .ToListAsync();

        return Ok(logs);
    }
}

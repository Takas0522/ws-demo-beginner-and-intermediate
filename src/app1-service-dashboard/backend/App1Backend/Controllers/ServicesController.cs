using App1Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App1Backend.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid[]? businessUnitIds,
        [FromQuery] Guid[]? categoryIds,
        [FromQuery] string? status)
    {
        var query = db.Services
            .Include(s => s.BusinessUnit)
            .Include(s => s.Category)
            .AsQueryable();

        if (businessUnitIds is { Length: > 0 })
            query = query.Where(s => businessUnitIds.Contains(s.BusinessUnitId));
        if (categoryIds is { Length: > 0 })
            query = query.Where(s => categoryIds.Contains(s.CategoryId));
        if (!string.IsNullOrEmpty(status))
            query = query.Where(s => s.Status == status);

        var services = await query
            .Select(s => new
            {
                s.Id, s.Name, s.Description, s.Status, s.LaunchedAt,
                BusinessUnit = new { s.BusinessUnit.Id, s.BusinessUnit.Name },
                Category     = new { s.Category.Id, s.Category.Name }
            })
            .ToListAsync();

        return Ok(services);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(
        Guid id,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var service = await db.Services
            .Include(s => s.BusinessUnit)
            .Include(s => s.Category)
            .Include(s => s.Plans)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (service is null) return NotFound();

        var dateFrom = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var dateTo   = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var metrics = await db.UserMetricDailies
            .Where(u => u.ServiceId == id && u.Date >= dateFrom && u.Date <= dateTo)
            .OrderBy(u => u.Date)
            .Select(u => new { u.Date, u.Mau, u.Dau, u.NewUsers, u.ChurnedUsers, u.TotalSubscriptions })
            .ToListAsync();

        var revenueBySub = await db.RevenueDailies
            .Where(r => r.ServiceId == id && r.Date >= dateFrom && r.Date <= dateTo)
            .GroupBy(r => new { r.Date, r.PlanId, PlanName = r.Plan.Name })
            .Select(g => new { g.Key.Date, g.Key.PlanId, g.Key.PlanName, Amount = g.Sum(r => r.Amount) })
            .OrderBy(r => r.Date)
            .ToListAsync();

        var totalRevenue = revenueBySub.Sum(r => r.Amount);

        var costByType = await db.CostDailies
            .Where(c => c.ServiceId == id && c.Date >= dateFrom && c.Date <= dateTo)
            .GroupBy(c => c.CostType)
            .Select(g => new { CostType = g.Key, Amount = g.Sum(c => c.Amount) })
            .ToListAsync();

        var totalCost = costByType.Sum(c => c.Amount);
        var grossProfit = totalRevenue - totalCost;
        var grossMargin = totalRevenue > 0 ? Math.Round(grossProfit / totalRevenue * 100, 2) : 0;

        var latestMetric = metrics.LastOrDefault();
        var arpu = latestMetric?.TotalSubscriptions > 0
            ? Math.Round(totalRevenue / latestMetric.TotalSubscriptions, 2)
            : 0;

        return Ok(new
        {
            service.Id,
            service.Name,
            service.Description,
            service.Status,
            service.LaunchedAt,
            BusinessUnit = new { service.BusinessUnit.Id, service.BusinessUnit.Name },
            Category     = new { service.Category.Id, service.Category.Name },
            Plans        = service.Plans.Select(p => new { p.Id, p.Name, p.Price, p.IsPaid }),
            Period       = new { From = dateFrom, To = dateTo },
            TotalRevenue = totalRevenue,
            TotalCost    = totalCost,
            GrossProfit  = grossProfit,
            GrossMargin  = grossMargin,
            Arpu         = arpu,
            UserMetrics  = metrics,
            RevenueByPlan = revenueBySub,
            CostBreakdown = costByType
        });
    }
}

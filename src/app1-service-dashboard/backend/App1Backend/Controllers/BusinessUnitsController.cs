using Microsoft.AspNetCore.Authorization;
using App1Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App1Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/business-units")]
public class BusinessUnitsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var units = await db.BusinessUnits
            .Select(b => new { b.Id, b.Name, b.Description })
            .ToListAsync();
        return Ok(units);
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetSummary(
        Guid id,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var dateFrom = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var dateTo   = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var unit = await db.BusinessUnits.FindAsync(id);
        if (unit is null) return NotFound();

        var serviceIds = await db.Services
            .Where(s => s.BusinessUnitId == id)
            .Select(s => s.Id)
            .ToListAsync();

        var revenue = await db.RevenueDailies
            .Where(r => serviceIds.Contains(r.ServiceId) && r.Date >= dateFrom && r.Date <= dateTo)
            .SumAsync(r => r.Amount);

        var cost = await db.CostDailies
            .Where(c => serviceIds.Contains(c.ServiceId) && c.Date >= dateFrom && c.Date <= dateTo)
            .SumAsync(c => c.Amount);

        var latestDate = await db.UserMetricDailies
            .Where(u => serviceIds.Contains(u.ServiceId))
            .MaxAsync(u => (DateOnly?)u.Date);

        var mau = latestDate.HasValue
            ? await db.UserMetricDailies
                .Where(u => serviceIds.Contains(u.ServiceId) && u.Date == latestDate)
                .SumAsync(u => u.Mau)
            : 0;

        var services = await db.Services
            .Where(s => s.BusinessUnitId == id)
            .Select(s => new { s.Id, s.Name, s.Status, s.LaunchedAt, CategoryName = s.Category.Name })
            .ToListAsync();

        var grossProfit = revenue - cost;
        var grossMargin = revenue > 0 ? Math.Round(grossProfit / revenue * 100, 2) : 0;

        return Ok(new
        {
            unit.Id,
            unit.Name,
            unit.Description,
            Period      = new { From = dateFrom, To = dateTo },
            Revenue     = revenue,
            Cost        = cost,
            GrossProfit = grossProfit,
            GrossMargin = grossMargin,
            TotalMau    = mau,
            Services    = services
        });
    }
}

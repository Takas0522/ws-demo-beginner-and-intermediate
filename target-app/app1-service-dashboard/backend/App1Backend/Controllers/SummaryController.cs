using Microsoft.AspNetCore.Authorization;
using App1Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App1Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/summary")]
public class SummaryController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var dateFrom = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var dateTo   = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var revenue = await db.RevenueDailies
            .Where(r => r.Date >= dateFrom && r.Date <= dateTo)
            .SumAsync(r => r.Amount);

        var cost = await db.CostDailies
            .Where(c => c.Date >= dateFrom && c.Date <= dateTo)
            .SumAsync(c => c.Amount);

        var grossProfit = revenue - cost;
        var grossMargin = revenue > 0 ? Math.Round(grossProfit / revenue * 100, 2) : 0;

        var latestMetrics = await db.UserMetricDailies
            .Where(u => u.Date == db.UserMetricDailies.Max(m => m.Date))
            .ToListAsync();

        var totalMau = latestMetrics.Sum(m => m.Mau);

        var serviceCount = await db.Services.CountAsync(s => s.Status == "active");

        var buRevenue = await db.RevenueDailies
            .Where(r => r.Date >= dateFrom && r.Date <= dateTo)
            .GroupBy(r => r.Service.BusinessUnitId)
            .Select(g => new
            {
                BusinessUnitId   = g.Key,
                BusinessUnitName = g.First().Service.BusinessUnit.Name,
                Revenue          = g.Sum(r => r.Amount)
            })
            .ToListAsync();

        return Ok(new
        {
            Period       = new { From = dateFrom, To = dateTo },
            TotalRevenue = revenue,
            TotalCost    = cost,
            GrossProfit  = grossProfit,
            GrossMargin  = grossMargin,
            TotalMau     = totalMau,
            ActiveServiceCount = serviceCount,
            BusinessUnitRevenue = buRevenue
        });
    }
}

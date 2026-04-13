using Microsoft.AspNetCore.Authorization;
using App1Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App1Backend.Controllers;

[Authorize]
[ApiController]
public class AbTestsController(AppDbContext db) : ControllerBase
{
    [HttpGet("api/services/{serviceId:guid}/ab-tests")]
    public async Task<IActionResult> GetByService(
        Guid serviceId,
        [FromQuery] string? status)
    {
        var query = db.AbTests
            .Where(a => a.ServiceId == serviceId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        var tests = await query
            .Select(a => new
            {
                a.Id, a.Name, a.PrimaryMetric, a.Status, a.StartedAt, a.EndedAt,
                a.WinnerVariantId,
                VariantCount = a.Variants.Count
            })
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();

        return Ok(tests);
    }

    [HttpGet("api/ab-tests/{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var test = await db.AbTests
            .Include(a => a.Service).ThenInclude(s => s.BusinessUnit)
            .Include(a => a.Variants).ThenInclude(v => v.Results)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (test is null) return NotFound();

        return Ok(new
        {
            test.Id,
            test.Name,
            test.Description,
            test.PrimaryMetric,
            test.Status,
            test.StartedAt,
            test.EndedAt,
            test.WinnerVariantId,
            Service = new
            {
                test.Service.Id,
                test.Service.Name,
                BusinessUnit = new { test.Service.BusinessUnit.Id, test.Service.BusinessUnit.Name }
            },
            Variants = test.Variants.Select(v => new
            {
                v.Id,
                v.Name,
                v.Description,
                v.TrafficAllocation,
                Results = v.Results.Select(r => new
                {
                    r.MetricName,
                    r.SampleSize,
                    r.MetricValue,
                    r.PValue,
                    r.ConfidenceIntervalLower,
                    r.ConfidenceIntervalUpper,
                    r.IsStatisticallySignificant
                })
            })
        });
    }
}

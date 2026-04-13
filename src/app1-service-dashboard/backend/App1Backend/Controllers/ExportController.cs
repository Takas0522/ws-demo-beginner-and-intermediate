using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using App1Backend.Data;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App1Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/export")]
public class ExportController(AppDbContext db) : ControllerBase
{
    private static readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true
    };

    [HttpGet("kpi-summary")]
    public async Task<IActionResult> ExportKpiSummary(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var dateFrom = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var dateTo   = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var metrics = await db.UserMetricDailies
            .Include(u => u.Service).ThenInclude(s => s.BusinessUnit)
            .Where(u => u.Date >= dateFrom && u.Date <= dateTo)
            .OrderBy(u => u.Date)
            .ToListAsync();

        var revenueMap = await db.RevenueDailies
            .Where(r => r.Date >= dateFrom && r.Date <= dateTo)
            .GroupBy(r => new { r.ServiceId, r.Date })
            .Select(g => new { g.Key.ServiceId, g.Key.Date, Total = g.Sum(r => r.Amount) })
            .ToDictionaryAsync(r => (r.ServiceId, r.Date), r => r.Total);

        var costMap = await db.CostDailies
            .Where(c => c.Date >= dateFrom && c.Date <= dateTo)
            .GroupBy(c => new { c.ServiceId, c.Date })
            .Select(g => new { g.Key.ServiceId, g.Key.Date, Total = g.Sum(c => c.Amount) })
            .ToDictionaryAsync(c => (c.ServiceId, c.Date), c => c.Total);

        var rows = metrics.Select(u =>
        {
            var rev  = revenueMap.GetValueOrDefault((u.ServiceId, u.Date), 0);
            var cost = costMap.GetValueOrDefault((u.ServiceId, u.Date), 0);
            var gp   = rev - cost;
            var arpu = u.TotalSubscriptions > 0 ? Math.Round(rev / u.TotalSubscriptions, 2) : 0;
            return new
            {
                Date            = u.Date.ToString("yyyy-MM-dd"),
                BusinessUnitId  = u.Service.BusinessUnitId,
                BusinessUnit    = u.Service.BusinessUnit.Name,
                ServiceId       = u.ServiceId,
                Service         = u.Service.Name,
                Mau             = u.Mau,
                Dau             = u.Dau,
                NewUsers        = u.NewUsers,
                ChurnedUsers    = u.ChurnedUsers,
                Revenue         = rev,
                Cost            = cost,
                GrossProfit     = gp,
                GrossMargin     = rev > 0 ? Math.Round(gp / rev * 100, 2) : 0,
                Arpu            = arpu
            };
        });

        return CsvResult(rows, $"kpi-summary_{dateFrom:yyyyMMdd}-{dateTo:yyyyMMdd}.csv");
    }

    [HttpGet("ab-tests")]
    public async Task<IActionResult> ExportAbTests(
        [FromQuery] Guid[]? serviceIds,
        [FromQuery] string? status)
    {
        var query = db.AbTests
            .Include(a => a.Service).ThenInclude(s => s.BusinessUnit)
            .Include(a => a.Variants).ThenInclude(v => v.Results)
            .AsQueryable();

        if (serviceIds is { Length: > 0 })
            query = query.Where(a => serviceIds.Contains(a.ServiceId));
        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        var tests = await query.ToListAsync();

        var rows = tests.SelectMany(t =>
            t.Variants.SelectMany(v =>
                v.Results.Select(r => new
                {
                    TestId          = t.Id,
                    TestName        = t.Name,
                    ServiceId       = t.ServiceId,
                    Service         = t.Service.Name,
                    BusinessUnit    = t.Service.BusinessUnit.Name,
                    StartedAt       = t.StartedAt.ToString("yyyy-MM-dd"),
                    EndedAt         = t.EndedAt?.ToString("yyyy-MM-dd") ?? "",
                    Status          = t.Status,
                    VariantId       = v.Id,
                    VariantName     = v.Name,
                    MetricName      = r.MetricName,
                    SampleSize      = r.SampleSize,
                    MetricValue     = r.MetricValue,
                    PValue          = r.PValue?.ToString() ?? "",
                    CiLower         = r.ConfidenceIntervalLower?.ToString() ?? "",
                    CiUpper         = r.ConfidenceIntervalUpper?.ToString() ?? "",
                    IsSignificant   = r.IsStatisticallySignificant,
                    IsWinner        = t.WinnerVariantId == v.Id
                })));

        return CsvResult(rows, $"ab-tests_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("user-metrics")]
    public async Task<IActionResult> ExportUserMetrics(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid[]? serviceIds)
    {
        var dateFrom = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6));
        var dateTo   = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var query = db.UserMetricDailies
            .Include(u => u.Service).ThenInclude(s => s.BusinessUnit)
            .Where(u => u.Date >= dateFrom && u.Date <= dateTo);

        if (serviceIds is { Length: > 0 })
            query = query.Where(u => serviceIds.Contains(u.ServiceId));

        var rows = await query
            .OrderBy(u => u.Date).ThenBy(u => u.ServiceId)
            .Select(u => new
            {
                Date            = u.Date.ToString(),
                ServiceId       = u.ServiceId,
                Service         = u.Service.Name,
                BusinessUnit    = u.Service.BusinessUnit.Name,
                Mau             = u.Mau,
                Dau             = u.Dau,
                NewUsers        = u.NewUsers,
                ChurnedUsers    = u.ChurnedUsers,
                TotalSubs       = u.TotalSubscriptions
            })
            .ToListAsync();

        return CsvResult(rows, $"user-metrics_{dateFrom:yyyyMMdd}-{dateTo:yyyyMMdd}.csv");
    }

    [HttpGet("stakeholders")]
    public async Task<IActionResult> ExportStakeholders(
        [FromQuery] Guid[]? serviceIds)
    {
        var query = db.ServiceStakeholders
            .Include(s => s.Service).ThenInclude(svc => svc.BusinessUnit)
            .AsQueryable();

        if (serviceIds is { Length: > 0 })
            query = query.Where(s => serviceIds.Contains(s.ServiceId));

        var rows = await query
            .OrderBy(s => s.Service.BusinessUnit.Name)
            .ThenBy(s => s.Service.Name)
            .ThenBy(s => s.Role)
            .Select(s => new
            {
                BusinessUnit           = s.Service.BusinessUnit.Name,
                ServiceId              = s.ServiceId,
                ServiceName            = s.Service.Name,
                StakeholderId          = s.Id,
                AuthUserId             = s.AuthUserId,
                DisplayName            = s.DisplayName,
                Role                   = s.Role,
                HourlyRate             = s.HourlyRate,
                AllocatedHoursMonthly  = s.AllocatedHoursMonthly,
                MonthlyCost            = s.HourlyRate * s.AllocatedHoursMonthly,
            })
            .ToListAsync();

        return CsvResult(rows, $"stakeholders_{DateTime.UtcNow:yyyyMMdd}.csv");
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

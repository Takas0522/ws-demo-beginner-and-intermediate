using App1Backend.Data;
using App1Backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App1Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/services/{serviceId:guid}/stakeholders")]
public class StakeholdersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid serviceId)
    {
        var rows = await db.ServiceStakeholders
            .Where(s => s.ServiceId == serviceId)
            .OrderBy(s => s.Role).ThenBy(s => s.DisplayName)
            .Select(s => new
            {
                s.Id,
                s.ServiceId,
                s.AuthUserId,
                s.DisplayName,
                s.Role,
                s.HourlyRate,
                s.AllocatedHoursMonthly,
                MonthlyCost = s.HourlyRate * s.AllocatedHoursMonthly,
                s.CreatedAt,
                s.UpdatedAt,
            })
            .ToListAsync();

        var summary = new
        {
            Stakeholders       = rows,
            TotalMonthlyCost   = rows.Sum(r => r.MonthlyCost),
            TotalAllocatedHours = rows.Sum(r => r.AllocatedHoursMonthly),
        };
        return Ok(summary);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid serviceId, [FromBody] StakeholderRequest req)
    {
        if (!await db.Services.AnyAsync(s => s.Id == serviceId))
            return NotFound();

        var exists = await db.ServiceStakeholders
            .AnyAsync(s => s.ServiceId == serviceId && s.AuthUserId == req.AuthUserId);
        if (exists)
            return Conflict(new { message = "このユーザーはすでに関係者として登録されています。" });

        var entity = new ServiceStakeholder
        {
            ServiceId              = serviceId,
            AuthUserId             = req.AuthUserId,
            DisplayName            = req.DisplayName,
            Role                   = req.Role,
            HourlyRate             = req.HourlyRate,
            AllocatedHoursMonthly  = req.AllocatedHoursMonthly,
            CreatedAt              = DateTime.UtcNow,
            UpdatedAt              = DateTime.UtcNow,
        };
        db.ServiceStakeholders.Add(entity);
        await db.SaveChangesAsync();
        return Created($"/api/services/{serviceId}/stakeholders/{entity.Id}", entity.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid serviceId, Guid id, [FromBody] StakeholderRequest req)
    {
        var entity = await db.ServiceStakeholders
            .FirstOrDefaultAsync(s => s.Id == id && s.ServiceId == serviceId);
        if (entity is null) return NotFound();

        entity.DisplayName           = req.DisplayName;
        entity.Role                  = req.Role;
        entity.HourlyRate            = req.HourlyRate;
        entity.AllocatedHoursMonthly = req.AllocatedHoursMonthly;
        entity.UpdatedAt             = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid serviceId, Guid id)
    {
        var entity = await db.ServiceStakeholders
            .FirstOrDefaultAsync(s => s.Id == id && s.ServiceId == serviceId);
        if (entity is null) return NotFound();

        db.ServiceStakeholders.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record StakeholderRequest(
    Guid    AuthUserId,
    string  DisplayName,
    string  Role,
    decimal HourlyRate,
    decimal AllocatedHoursMonthly);

using AuthService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentsController(AuthDbContext db) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var depts = await db.Departments
            .OrderBy(d => d.Name)
            .Select(d => new { d.Id, d.Name, d.Code, d.Description })
            .ToListAsync();
        return Ok(depts);
    }
}

[ApiController]
[Route("api/users")]
public class UsersController(AuthDbContext db) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await db.Users
            .Include(u => u.Department)
            .OrderBy(u => u.Username)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.DisplayName,
                u.Role,
                u.IsActive,
                DepartmentId   = u.DepartmentId,
                DepartmentName = u.Department != null ? u.Department.Name : null,
            })
            .ToListAsync();
        return Ok(users);
    }
}

using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthDbContext db, JwtService jwt) : ControllerBase
{
    public record LoginRequest(string Username, string Password);
    public record RegisterRequest(string Username, string Email, string Password,
        string? DisplayName, Guid? DepartmentId);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await db.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Username == req.Username && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "ユーザー名またはパスワードが正しくありません" });

        var token = jwt.Generate(user);
        return Ok(new
        {
            token,
            user = new
            {
                id           = user.Id,
                username     = user.Username,
                email        = user.Email,
                displayName  = user.DisplayName ?? user.Username,
                role         = user.Role,
                departmentId = user.DepartmentId,
                departmentName = user.Department?.Name,
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { message = "このユーザー名は既に使用されています" });
        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "このメールアドレスは既に使用されています" });

        var user = new AuthService.Entities.User
        {
            Id           = Guid.NewGuid(),
            Username     = req.Username,
            Email        = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            DisplayName  = req.DisplayName,
            DepartmentId = req.DepartmentId,
            Role         = "user",
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = jwt.Generate(user);
        return Created($"/api/users/{user.Id}", new { token, user = new { user.Id, user.Username, user.Email, user.Role } });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());

        var user = await db.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return NotFound();

        return Ok(new
        {
            id           = user.Id,
            username     = user.Username,
            email        = user.Email,
            displayName  = user.DisplayName ?? user.Username,
            role         = user.Role,
            departmentId = user.DepartmentId,
            departmentName = user.Department?.Name,
        });
    }
}

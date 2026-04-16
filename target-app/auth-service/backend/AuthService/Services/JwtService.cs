using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class JwtService(IConfiguration config)
{
    public string Generate(User user)
    {
        var secret  = config["Jwt:Secret"]!;
        var issuer  = config["Jwt:Issuer"]!;
        var audience = config["Jwt:Audience"]!;
        var expMinutes = int.Parse(config["Jwt:ExpiryMinutes"] ?? "480");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username",     user.Username),
            new Claim("displayName",  user.DisplayName ?? user.Username),
            new Claim("role",         user.Role),
            new Claim("departmentId", user.DepartmentId?.ToString() ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:   issuer,
            audience: audience,
            claims:   claims,
            expires:  DateTime.UtcNow.AddMinutes(expMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

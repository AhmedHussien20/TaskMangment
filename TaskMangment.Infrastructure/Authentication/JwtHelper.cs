using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Authentication
{
    public static class JwtHelper
    {
        public static string GenerateToken(
    Employee user,
    string key,
    string issuer,
    string audience,
    int expiryHours = 10)
        {
            var claims = new List<Claim>
    {
        new Claim("UserId", user.Id.ToString()),
        new Claim("CompanyId", user.CompanyId.ToString() ?? "0"),
        new Claim(ClaimTypes.Name, user.FullName ?? user.Email ?? "Unknown")
    };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

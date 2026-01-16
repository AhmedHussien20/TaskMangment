using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly IRoleAssignmentService _roleService;


        public JwtService(IConfiguration config, IRoleAssignmentService roleService)
        {
            _config = config;
            _roleService = roleService;
        }

        public async Task<string> GenerateTokenAsync(Employee user)
        {
            var roles = await _roleService.GetUserRolesAsync(user.Id);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("FullName", user.FullName ?? string.Empty),
                new Claim("Email", user.Email ?? string.Empty),
                new Claim("CompanyId", user.CompanyId.ToString()),
            };


            int roleLevelId = roles.Any()? roles.Max(r => r.Level) : (int)RoleLevelEnum.Employee;
            claims.Add(new Claim("RoleLevelId", roleLevelId.ToString()));


            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }


            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

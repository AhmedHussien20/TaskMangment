using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IEmployeePermissionService _permissionService;

        public JwtService(
            IConfiguration config,
            IRoleAssignmentService roleService,
            IEmployeePermissionService permissionService)
        {
            _config = config;
            _roleService = roleService;
            _permissionService = permissionService;
        }

        public async Task<string> GenerateTokenAsync(Employee user)
        {
            var roles = await _roleService.GetUserRolesAsync(user.Id);
            var permissions = await _permissionService.GetPermissionsAsync(user.Id);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("EmployeeId", user.Id.ToString()),
                new Claim("FullName", user.FullName ?? string.Empty),
                new Claim("Email", user.Email ?? string.Empty),
                new Claim("CompanyId", user.CompanyId.ToString()),
            };

            // Kept for display / temporary dual-read during migration — not for authorization.
            int roleLevelId = roles.Any() ? roles.Max(r => r.Level) : (int)RoleLevelEnum.Employee;
            claims.Add(new Claim("RoleLevelId", roleLevelId.ToString()));

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            foreach (var permission in permissions)
                claims.Add(new Claim("permission", permission));

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

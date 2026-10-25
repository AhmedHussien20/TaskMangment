using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly IRoleAssignmentService _roleService;
        private readonly IEmployeePermissionService _permissionService;
        private readonly TrainingDbContext _training;

        public JwtService(
            IConfiguration config,
            IRoleAssignmentService roleService,
            IEmployeePermissionService permissionService,
            TrainingDbContext training)
        {
            _config = config;
            _roleService = roleService;
            _permissionService = permissionService;
            _training = training;
        }

        public async Task<string> GenerateTokenAsync(Employee user, string schema)
        {
            IReadOnlyList<(string Name, int Level)> roles;
            IReadOnlyCollection<string> permissions;

            if (string.Equals(schema, OperationalSchema.Training, StringComparison.OrdinalIgnoreCase))
            {
                var access = await LoadTrainingAccessAsync(user.Id);
                roles = access.Roles;
                permissions = access.Permissions;
            }
            else
            {
                var userRoles = await _roleService.GetUserRolesAsync(user.Id);
                roles = userRoles.Select(r => (r.Name, r.Level)).ToList();
                permissions = await _permissionService.GetPermissionsAsync(user.Id);
            }

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
                new Claim("Schema", schema),
                new Claim("Actor", "Employee"),
            };

            // Kept for display / temporary dual-read during migration — not for authorization.
            int roleLevelId = roles.Count > 0 ? roles.Max(r => r.Level) : (int)RoleLevelEnum.Employee;
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

        public string GeneratePlatformToken(PlatformUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("FullName", user.FullName ?? string.Empty),
                new Claim("Email", user.Email ?? string.Empty),
                new Claim("Actor", "Platform"),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            if (user.OrganizationId.HasValue)
                claims.Add(new Claim("OrganizationId", user.OrganizationId.Value.ToString()));

            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<(List<(string Name, int Level)> Roles, List<string> Permissions)> LoadTrainingAccessAsync(int employeeId)
        {
            var roles = await _training.EmployeeRoles
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(er =>
                    er.EmployeeId == employeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.Role != null &&
                    !er.Role.IsDeleted)
                .Select(er => new { er.Role.Name, er.Role.Level })
                .ToListAsync();

            var permissions = await _training.EmployeeRoles
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(er =>
                    er.EmployeeId == employeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.Role != null &&
                    !er.Role.IsDeleted)
                .SelectMany(er => er.Role.RolePermissions)
                .Where(rp =>
                    rp.IsAssigned &&
                    !rp.IsDeleted &&
                    rp.Permission != null &&
                    !rp.Permission.IsDeleted)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();

            return (roles.Select(r => (r.Name, r.Level)).ToList(), permissions);
        }
    }
}

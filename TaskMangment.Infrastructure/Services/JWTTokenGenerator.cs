using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Infrastructure.Services
{
    public class JWTTokenGenerator : IJWTTokenGenerator
    {
        private readonly UserManager<SystemUser> _userManager;
        private readonly IConfiguration _config;

        public JWTTokenGenerator(UserManager<SystemUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _config = configuration;
        }



        public async Task<string> GenerateToken(SystemUser user)
        {
            var claims = new List<Claim>();
          /*  claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));*/

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(1);


            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        
    }
    }
}

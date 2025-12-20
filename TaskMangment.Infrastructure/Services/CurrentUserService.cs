using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;

            foreach (var claim in claims ?? Enumerable.Empty<Claim>())
            {
                Console.WriteLine($"CLAIM: {claim.Type} = {claim.Value}");
            }
        }

        public int? UserId =>
           int.TryParse(
               _httpContextAccessor.HttpContext?
                   .User?
                   .FindFirst("UserId")?.Value,
               out var id) ? id : null; 

        public int? CompanyId =>
              int.TryParse(
               _httpContextAccessor.HttpContext?
                   .User?
                   .FindFirst("CompanyId")?.Value,
               out var companyId) ? companyId : null;

        public string? UserName => throw new NotImplementedException();
    }
}

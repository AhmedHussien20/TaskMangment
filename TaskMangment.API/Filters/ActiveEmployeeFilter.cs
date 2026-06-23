using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.API.Filters
{
    public class ActiveEmployeeFilter : IAsyncAuthorizationFilter
    {
        private readonly AppDbContext _db;

        public ActiveEmployeeFilter(AppDbContext db)
        {
            _db = db;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
                return;

            var userIdClaim = context.HttpContext.User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return;

            var isActive = await _db.Employees
                .Where(e => e.Id == userId && !e.IsDeleted)
                .Select(e => e.IsActive)
                .FirstOrDefaultAsync();

            if (!isActive)
            {
                throw new AppException(
                    ErrorCodes.EmployeeInactive,
                    StatusCodes.Status403Forbidden);
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.DataContext
{
    /// <summary>
    /// Login resolves an email against dbo, then training, then the platform, so an email must be
    /// unique across all of them — not only inside the caller's schema or trainee company.
    /// </summary>
    public class OperationalEmailRegistry
    {
        private readonly DbContextOptions<AppDbContext> _dboOptions;
        private readonly TrainingDbContext _training;
        private readonly ICurrentUserService _currentUser;

        public OperationalEmailRegistry(
            DbContextOptions<AppDbContext> dboOptions,
            TrainingDbContext training,
            ICurrentUserService currentUser)
        {
            _dboOptions = dboOptions;
            _training = training;
            _currentUser = currentUser;
        }

        /// <param name="exceptEmployeeId">An employee in the caller's own schema to ignore (the one being edited or restored).</param>
        /// <param name="exceptPlatformUserId">Platform user to ignore when editing an admin / institute manager.</param>
        /// <param name="exceptTrainingEmployeeId">Training-schema employee to ignore (platform editing a trainee copy).</param>
        public async Task<bool> IsTakenAsync(
            string? email,
            int? exceptEmployeeId = null,
            int? exceptPlatformUserId = null,
            int? exceptTrainingEmployeeId = null)
        {
            var value = (email ?? string.Empty).Trim();
            if (value.Length == 0)
                return false;

            var callerIsTraining = string.Equals(
                _currentUser.Schema ?? OperationalSchemaAmbient.Current,
                OperationalSchema.Training,
                StringComparison.OrdinalIgnoreCase);

            int? exceptDbo = callerIsTraining ? null : exceptEmployeeId;
            int? exceptTraining = callerIsTraining
                ? exceptEmployeeId
                : exceptTrainingEmployeeId;

            await using var dbo = new AppDbContext(_dboOptions, _currentUser);

            if (await dbo.Employees.AnyAsync(e =>
                    e.Email == value && !e.IsDeleted && (exceptDbo == null || e.Id != exceptDbo)))
                return true;

            if (await dbo.PlatformUsers.AnyAsync(u =>
                    u.Email == value && !u.IsDeleted &&
                    (exceptPlatformUserId == null || u.Id != exceptPlatformUserId)))
                return true;

            return await _training.Employees
                .IgnoreQueryFilters()
                .AnyAsync(e =>
                    e.Email == value && !e.IsDeleted &&
                    (exceptTraining == null || e.Id != exceptTraining));
        }
    }
}

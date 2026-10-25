using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Hangfire.Jobs
{
    /// <summary>
    /// Runs an operational job once against dbo and once against the training schema.
    /// </summary>
    public class SchemaDispatchJob
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<SchemaDispatchJob> _logger;

        public SchemaDispatchJob(IServiceScopeFactory scopes, ILogger<SchemaDispatchJob> logger)
        {
            _scopes = scopes;
            _logger = logger;
        }

        public async Task ExecuteAsync(string jobName)
        {
            await RunAsync(OperationalSchema.Dbo, jobName);

            try
            {
                await RunAsync(OperationalSchema.Training, jobName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Training schema pass failed for job {JobName}", jobName);
            }
        }

        private async Task RunAsync(string schema, string jobName)
        {
            OperationalSchemaAmbient.Current = schema;
            try
            {
                using var scope = _scopes.CreateScope();
                var services = scope.ServiceProvider;
                switch (jobName)
                {
                    case "emails":
                        await services.GetRequiredService<ProcessPendingEmailsJob>().ExecuteAsync();
                        break;
                    case "penalty":
                        await services.GetRequiredService<PenaltyForMissingCommentsJob>().ExecuteAsync();
                        break;
                    case "archive":
                        await services.GetRequiredService<ArchiveOverdueTasksJob>().ExecuteAsync();
                        break;
                    case "due-today":
                        await services.GetRequiredService<TaskDueTodayEmailsProcessorJob>().ExecuteAsync();
                        break;
                    case "monthly-discounts":
                        await services.GetRequiredService<SendMonthlyEmployeeDiscountsJob>().ExecuteAsync();
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown schema job '{jobName}'.");
                }
            }
            finally
            {
                OperationalSchemaAmbient.Current = null;
            }
        }
    }
}

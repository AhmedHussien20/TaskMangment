using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Hangfire.Jobs
{
    /// <summary>
    /// Runs an operational job against one schema. Each schema is its own recurring job, so a failure in the
    /// training pass shows as failed in the dashboard and retries on its own without re-running dbo.
    /// </summary>
    public class SchemaDispatchJob
    {
        private readonly IServiceScopeFactory _scopes;

        public SchemaDispatchJob(IServiceScopeFactory scopes)
        {
            _scopes = scopes;
        }

        /// <summary>Kept so jobs enqueued before the per-schema split still deserialize; runs dbo only.</summary>
        public Task ExecuteAsync(string jobName) => ExecuteAsync(jobName, OperationalSchema.Dbo);

        public async Task ExecuteAsync(string jobName, string schema)
        {
            if (schema != OperationalSchema.Dbo && schema != OperationalSchema.Training)
                throw new InvalidOperationException($"Unknown schema '{schema}'.");

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

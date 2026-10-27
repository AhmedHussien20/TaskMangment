namespace TaskMangment.Infrastructure.DataContext
{
    public static class OperationalSchema
    {
        public const string Dbo = "dbo";
        public const string Training = "training";
    }

    /// <summary>
    /// Request-less schema selection used by Hangfire so a job can run once per schema.
    /// </summary>
    public static class OperationalSchemaAmbient
    {
        private static readonly AsyncLocal<string?> CurrentSchema = new();

        public static string? Current
        {
            get => CurrentSchema.Value;
            set => CurrentSchema.Value = value;
        }
    }

    /// <summary>
    /// Request-less company selection used by Hangfire when a job uploads blobs (or stamps data)
    /// for one trainee copy inside the shared training schema.
    /// </summary>
    public static class OperationalCompanyAmbient
    {
        private static readonly AsyncLocal<int?> CurrentCompany = new();

        public static int? Current
        {
            get => CurrentCompany.Value;
            set => CurrentCompany.Value = value;
        }
    }
}

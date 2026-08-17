using TaskMangment.Infrastructure;

namespace TaskMangment.Hangfire
{
    /// <summary>
    /// Saudi quiet rules: skip jobs 03:00–09:00, and all day Friday (configurable).
    /// </summary>
    public static class HangfireQuietHours
    {
        /// <summary>
        /// Every minute at hours 00 and 09–23, Sun–Thu + Sat (excludes Friday).
        /// Cron day-of-week: 0=Sunday … 5=Friday … 6=Saturday.
        /// </summary>
        public const string CronOutsideQuietHours = "* 0,9-23 * * 0-4,6";

        /// <summary>
        /// Every 5 minutes at hours 00 and 09–23, Sun–Thu + Sat (excludes Friday).
        /// </summary>
        public const string CronEvery5MinOutsideQuietHours = "*/5 0,9-23 * * 0-4,6";

        public const string CronDailyAfterQuietHours = "0 9 * * 0-4,6";
        public const string CronArchiveOutsideQuietHours = "1 0 * * 0-4,6";

        public static bool ShouldSkipNow(IConfiguration configuration)
        {
            if (!configuration.GetValue("HangfireQuietHours:Enabled", true))
                return false;

            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneHelper.GetSaudiArabia());

            if (configuration.GetValue("HangfireQuietHours:SkipFriday", true)
                && nowLocal.DayOfWeek == DayOfWeek.Friday)
                return true;

            var startHour = configuration.GetValue("HangfireQuietHours:StartHour", 3); // Changed to 3
            var endHour = configuration.GetValue("HangfireQuietHours:EndHour", 9);

            if (startHour == endHour)
                return false;

            if (startHour < endHour)
                return nowLocal.Hour >= startHour && nowLocal.Hour < endHour;

            return nowLocal.Hour >= startHour || nowLocal.Hour < endHour;
        }

        /// <summary>Backward-compatible alias.</summary>
        public static bool IsQuietNow(IConfiguration configuration) => ShouldSkipNow(configuration);
    }
}
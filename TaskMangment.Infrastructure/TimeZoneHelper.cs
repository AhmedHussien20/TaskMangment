namespace TaskMangment.Infrastructure
{
    public static class TimeZoneHelper
    {
        /// <summary>
        /// Windows uses "Arabian Standard Time"; Linux/Azure often uses "Asia/Riyadh".
        /// </summary>
        public static TimeZoneInfo GetSaudiArabia()
        {
            foreach (var id in new[] { "Arabian Standard Time", "Asia/Riyadh" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.CreateCustomTimeZone(
                "Saudi Arabia Standard Time",
                TimeSpan.FromHours(3),
                "Saudi Arabia Standard Time",
                "Saudi Arabia Standard Time");
        }
    }
}

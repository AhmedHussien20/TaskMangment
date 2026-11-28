using Microsoft.AspNetCore.Http;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;

namespace TaskMangment.Application.Common.Validation
{
    /// <summary>
    /// Saudi workweek: Sunday–Thursday. Due dates cannot fall on Friday/Saturday or in the past.
    /// </summary>
    public static class TaskDueDateRules
    {
        public static bool IsWeekend(DateTime date)
        {
            var day = date.Date.DayOfWeek;
            return day is DayOfWeek.Friday or DayOfWeek.Saturday;
        }

        public static DateTime MoveToNextWorkday(DateTime date)
        {
            var result = date;
            while (IsWeekend(result))
                result = result.AddDays(1);

            return result;
        }

        public static bool IsPast(DateTime date) =>
            date.Date < DateTime.UtcNow.Date;

        public static void EnsureNotWeekend(DateTime? dueDate)
        {
            if (!dueDate.HasValue)
                return;

            if (IsWeekend(dueDate.Value))
            {
                throw new AppException(
                    ErrorCodes.DueDateOnWeekend,
                    StatusCodes.Status400BadRequest);
            }
        }

        public static void EnsureNotPast(DateTime? dueDate)
        {
            if (!dueDate.HasValue)
                return;

            if (IsPast(dueDate.Value))
            {
                throw new AppException(
                    ErrorCodes.DueDateInPast,
                    StatusCodes.Status400BadRequest);
            }
        }

        public static void EnsureValidDueDate(DateTime? dueDate)
        {
            EnsureNotWeekend(dueDate);
            EnsureNotPast(dueDate);
        }
    }
}

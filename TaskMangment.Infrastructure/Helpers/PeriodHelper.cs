using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Helpers
{
    public static class PeriodHelper
    {
        public static (DateTime Start, DateTime End) GetRange(PeriodDto period)
        {
            var now = DateTime.UtcNow;

            return period.Type switch
            {
                DashboardPeriod.Day => (now.Date, now.Date.AddDays(1)),
                DashboardPeriod.Month => (new DateTime(now.Year, now.Month, 1), now),
                DashboardPeriod.Year => (new DateTime(now.Year, 1, 1), now),        
                DashboardPeriod.Custom => (period.StartDate!.Value, period.EndDate!.Value),
                _ => throw new ArgumentException("Invalid period type")
            };
        }
    }

}

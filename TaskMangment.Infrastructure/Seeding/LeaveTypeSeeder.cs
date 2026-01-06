using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Seeding
{
    public static class LeaveTypeSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (context.LeaveTypes.Any())
                return;

            context.LeaveTypes.AddRange(
                new LeaveType { NameAr = "إجازة سنوية", NameEn = "Annual Leave", IsPaid = true, MaxDaysPerYear = 21 },
                new LeaveType { NameAr = "إجازة مرضية", NameEn = "Sick Leave", IsPaid = true, MaxDaysPerYear = 30 },
                new LeaveType { NameAr = "إجازة طارئة", NameEn = "Emergency Leave", IsPaid = true, MaxDaysPerYear = 5 },
                new LeaveType { NameAr = "إجازة بدون أجر", NameEn = "Unpaid Leave", IsPaid = false, MaxDaysPerYear = null },
                new LeaveType { NameAr = "إجازة زواج", NameEn = "Marriage Leave", IsPaid = true, MaxDaysPerYear = 5 },
                new LeaveType { NameAr = "إجازة وفاة", NameEn = "Bereavement Leave", IsPaid = true, MaxDaysPerYear = 5 },
                new LeaveType { NameAr = "إجازة حج", NameEn = "Hajj Leave", IsPaid = true, MaxDaysPerYear = 10 },
                new LeaveType { NameAr = "إجازة وضع", NameEn = "Maternity Leave", IsPaid = true, MaxDaysPerYear = 90 },
                new LeaveType { NameAr = "إجازة أبوة", NameEn = "Paternity Leave", IsPaid = true, MaxDaysPerYear = 3 },
                new LeaveType { NameAr = "إجازة دراسة", NameEn = "Study Leave", IsPaid = false, MaxDaysPerYear = null }
            );

            context.SaveChanges();
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Common.Interfaces
{
    public interface ICacheInvalidator
    {
        Task InvalidateDashboardAsync(int companyId);
        Task InvalidateTasksAsync(int companyId);
        Task InvalidateEmployeesAsync(int companyId);
        Task InvalidateAreasAsync(int companyId);

        Task InvalidateBranchAsync(int companyId);
    }

}

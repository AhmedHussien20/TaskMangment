using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IPermissionChecker
    {
        Task<bool> HasPermissionAsync(int employeeId, string permissionCode);
    }

}

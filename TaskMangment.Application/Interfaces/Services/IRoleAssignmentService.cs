using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Employee;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IRoleAssignmentService
    {
        Task<ApiResponse<PagedResponse<AssignedEmployeeDto>>> GetAssignedEmployeesPagedAsync(int roleId, RoleAssignmentReguest request);
        Task<ApiResponse<bool>> AssignEmployeesToRoleAsync(int roleId,RoleWithManyEmployeeAssignDto dto);
        Task<List<string>> GetUserRolesAsync(int userId);



    }
}

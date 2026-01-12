using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Dashboards.Admin;
using TaskMangment.Application.Dashboards.Employee;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Services.TaskMangment.Infrastructure.Services.Dashboard;

namespace TaskMangment.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : BaseController
    {
        private readonly IAdminDashboardService _adminService;
        private readonly IEmployeeDashboardService _employeeService;

        public DashboardController(
            IAdminDashboardService adminService,
            IEmployeeDashboardService employeeService)
        {
            _adminService = adminService;
            _employeeService = employeeService;
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var result = await _adminService.GetDashboardAsync(CompanyId);
            return Success(result.Data);
        }

        [HttpGet("employee")]
        public async Task<IActionResult> GetEmployeeDashboard()
        {
            var result = await _employeeService.GetDashboardAsync(CurrentUserId);
            return Success(result.Data);
        }

        [HttpGet("admin/in-progress-updated-today")]
        public async Task<IActionResult> GetTodayUpdatedInProgressTasks()
        {
            var result = await _adminService.GetTodayUpdatedInProgressTasksAsync(this.CompanyId);
            return Success(result.Data);
        }

        [HttpGet("admin/completed-tasks-today")]
        public async Task<IActionResult> GetEmployeesCompletedTasksToday()
        {
            var result = await _adminService.GetEmployeesCompletedTasksTodayAsync(this.CompanyId);
                

            return Success(result.Data);
        }

        [HttpGet("admin/pending-close-requests")]
        public async Task<IActionResult> GetPendingCloseRequests()
        {
            var result = await _adminService .GetPendingCloseRequestsAsync(this.CompanyId);
            return Success(result.Data);
        }


    }

}

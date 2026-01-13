using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Dashboards.Admin;
using TaskMangment.Application.Dashboards.Employee;
using TaskMangment.Application.DTOs;
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
        public async Task<IActionResult> GetAdminDashboard([FromQuery] PeriodDto period)
        {
            var result = await _adminService.GetDashboardAsync(CompanyId, period);
            return Success(result.Data);
        }

        [HttpGet("employee")]
        public async Task<IActionResult> GetEmployeeDashboard()
        {
            var result = await _employeeService.GetDashboardAsync(CurrentUserId);
            return Success(result.Data);
        }

        [HttpGet("admin/in-progress-updated-today")]
        public async Task<IActionResult> GetTodayUpdatedInProgressTasks([FromQuery] PeriodDto period)
        {
            var result = await _adminService.GetTodayUpdatedInProgressTasksAsync(this.CompanyId,period);
            return Success(result.Data);
        }

        [HttpGet("admin/completed-tasks-today")]
        public async Task<IActionResult> GetEmployeesCompletedTasksToday([FromQuery] PeriodDto period)
        {
            var result = await _adminService.GetEmployeesCompletedTasksTodayAsync(this.CompanyId, period);
                

            return Success(result.Data);
        }

        [HttpGet("admin/pending-close-requests")]
        public async Task<IActionResult> GetPendingCloseRequests([FromQuery] PeriodDto period)
        {
            var result = await _adminService .GetPendingCloseRequestsAsync(this.CompanyId, period);
            return Success(result.Data);
        }

        [HttpGet("admin/tasks-by-status")]
        public async Task<IActionResult> GetAdminTasksByStatus([FromQuery] string status, [FromQuery] PeriodDto period)
        {
            var result = await _adminService.GetTasksByStatusAsync(this.CompanyId, status, period);
            return Success(result.Data);
        }

        [HttpGet("admin/kpis")]
        public async Task<IActionResult> GetAdminKpis([FromQuery] PeriodDto period)
        {
            var result = await _adminService.GetKpisAsync(this.CompanyId, period);
            return Success(result.Data);
        }

        [HttpGet("admin/discounts")]
        public async Task<IActionResult> GetDiscounts([FromQuery] PeriodDto period)
        {
            var result = await _adminService.GetDiscountsAsync(this.CompanyId, period);
            return Success(result.Data);
        }

        [HttpGet("admin/high-priority-tasks")]
        public async Task<IActionResult> GetHighPriorityTasks()
        {
            var result = await _adminService.GetHighPriorityTasksAsync(this.CompanyId);
            return Success(result.Data);
        }

        [HttpGet("admin/completed-tasks-details")]
        public async Task<IActionResult> GetCompletedTasksDetails([FromQuery] PeriodDto period)
        {
            var result = await _adminService.GetCompletedTasksDetailsAsync(this.CompanyId, period);
            return Success(result.Data);
        }
    }

}

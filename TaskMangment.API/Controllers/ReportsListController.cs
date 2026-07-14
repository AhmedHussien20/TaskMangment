using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsListController : BaseController
    {
        private readonly IReportService _reportService;

        public ReportsListController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("filter-roles")]
        public async Task<IActionResult> GetFilterRoles()
        {
            if (this.RoleLevel < 70)
                return Forbid();

            var data = await _reportService.GetReportFilterRolesAsync(this.CurrentUserId, this.RoleLevel);
            return Success(data);
        }

        [HttpGet("top-commenters")]
        public async Task<IActionResult> GetTopCommenters([FromQuery] DateRangeReportFilterDto filter)
        {
            var data = await _reportService.GetTopEmployeesByCommentsAsync(this.CurrentUserId,this.RoleLevel, filter);
            return Success(data); 
        }

        [HttpGet("most-assigned")]
        public async Task<IActionResult> GetMostAssignedEmployees([FromQuery] DateRangeReportFilterDto filter)
        {
            var data = await _reportService.GetMostAssignedEmployeesAsync(this.CurrentUserId, this.RoleLevel, filter);
            return Success(data);
        }

        [HttpGet("on-time-completion")]
        public async Task<IActionResult> GetOnTimeCompletion([FromQuery] DateRangeReportFilterDto filter)
        {
            var data = await _reportService.GetOnTimeCompletionReportAsync(this.CurrentUserId,this.RoleLevel, filter);
            return Success(data);
        }

        [HttpGet("archived-tasks")]
        public async Task<IActionResult> GetArchivedTasks([FromQuery] DateRangeReportFilterDto filter)
        {
            var data = await _reportService.GetMostArchivedEmployeesAsync(this.CurrentUserId, this.RoleLevel, filter);
            return Success(data);
        }


        [HttpGet("task-discounts")]
        public async Task<IActionResult> GetTaskDiscounts([FromQuery] TaskDiscountReportFilterDto filter) 
        {
            //filter.MovementType = movementType; 
            var data = await _reportService.GetTaskDiscountReportAsync(this.CurrentUserId, this.RoleLevel, filter);
            return Success(data);
        }

        [HttpGet("employee-total-discounts")]
        public async Task<IActionResult> GetEmployeeTotalDiscounts([FromQuery] EmployeeTotalDiscountReportFilterDto filter)
        {
            if (this.RoleLevel < 100)
                return Forbid();

            if (filter.ToDate.HasValue && !filter.FromDate.HasValue)
                return BadRequest("fromDate is required when toDate is selected.");

            var data = await _reportService.GetEmployeeTotalDiscountReportAsync(this.CurrentUserId, this.RoleLevel, filter);
            return Success(data);
        }


        [HttpGet("task-activities")]
        public async Task<IActionResult> GetTaskActivities([FromQuery] DateRangeReportFilterDto filter, ExportType exportType)
        {
            var data = await _reportService.GetTaskActivityReportAsync(this.CurrentUserId, this.RoleLevel, exportType, filter);
            return Success(data);
        }

        [HttpGet("task-movements")]
        public async Task<IActionResult> GetTaskMovements([FromQuery] TaskMovementReportFilterDto filter, ExportType exportType)
        {
            var data = await _reportService.GetTaskMovementReportAsync(this.CurrentUserId,this.RoleLevel, filter, exportType);
            return Success(data);
        }

        [HttpGet("closing-soon-tasks")]
        public async Task<IActionResult> GetClosingSoonTasks([FromQuery] int employeeId)
        {
            var now = DateTime.UtcNow;
            var next3Days = now.AddDays(3); 

            var tasks = await _reportService.GetTasksClosingSoonAsync(this.CurrentUserId, this.RoleLevel, employeeId, now, next3Days);

            return Success(tasks);
        }

        [HttpGet("employee-task-tracking")]
        public async Task<IActionResult> EmployeeTaskTracking(int? employeeId, DateTime fromDate, DateTime? toDate)
        {

            var tasks = await _reportService.GetEmployeeTaskTrackingAsync(this.CurrentUserId, this.RoleLevel,employeeId, fromDate, toDate);
            return Success(tasks);
        }

        [HttpGet("branch-tasks")]
        public async Task<IActionResult> GetBranchTasks(int branchId,DateTime fromDate,DateTime? toDate)
        {
            var filter = new BranchTasksReportFilterDto
            {
                BranchId = branchId,
                FromDate = fromDate,
                ToDate = toDate
            };

            var data = await _reportService.GetBranchTasksReportAsync(this.CurrentUserId,this.RoleLevel,filter);
            return Success(data);
        }

        [HttpGet("employee-assigned-tasks")]
        public async Task<IActionResult> GetEmployeeAssignedTasks([FromQuery] int employeeId)
        {
            var data = await _reportService.GetEmployeeAssignedTasksAsync(
                this.CurrentUserId,
                this.RoleLevel,
                employeeId);

            return Success(data);
        }

        [HttpGet("employee-task-comments")]
        public async Task<IActionResult> GetEmployeeTaskComments([FromQuery] int employeeId, [FromQuery] int taskId)
        {
            var data = await _reportService.GetEmployeeTaskCommentsAsync(
                this.CurrentUserId,
                this.RoleLevel,
                employeeId,
                taskId,
                ExportType.Excel);

            return Success(data);
        }


    }
}

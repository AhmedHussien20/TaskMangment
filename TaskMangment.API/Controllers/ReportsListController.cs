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

        [HttpGet("top-commenters")]
        public async Task<IActionResult> GetTopCommenters(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var data = await _reportService.GetTopEmployeesByCommentsAsync(fromDate, toDate);
            return Success(data); 
        }

        [HttpGet("most-assigned")]
        public async Task<IActionResult> GetMostAssignedEmployees(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var data = await _reportService.GetMostAssignedEmployeesAsync(fromDate, toDate);
            return Success(data);
        }

        [HttpGet("on-time-completion")]
        public async Task<IActionResult> GetOnTimeCompletion(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var data = await _reportService.GetOnTimeCompletionReportAsync(this.Role,this.CurrentUserId,fromDate, toDate);
            return Success(data);
        }

        [HttpGet("archived-tasks")]
        public async Task<IActionResult> GetArchivedTasks(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var data = await _reportService.GetMostArchivedEmployeesAsync(fromDate, toDate);
            return Success(data);
        }


        [HttpGet("task-discounts")]
        public async Task<IActionResult> GetTaskDiscounts([FromQuery] TaskDiscountReportFilterDto filter) 
        {
            //filter.MovementType = movementType; 
            var data = await _reportService.GetTaskDiscountReportAsync(this.RoleLevel,this.CurrentUserId,filter);
            return Success(data);
        }


        [HttpGet("task-activities")]
        public async Task<IActionResult> GetTaskActivities(DateTime? fromDate,DateTime? toDate)
        {
            var data = await _reportService.GetTaskActivityReportAsync(this.RoleLevel, this.CurrentUserId, fromDate, toDate);
            return Success(data);
        }

        [HttpGet("task-movements")]
        public async Task<IActionResult> GetTaskMovements([FromQuery] TaskMovementReportFilterDto filter)
        {
            var data = await _reportService.GetTaskMovementReportAsync(filter);
            return Success(data);
        }

        [HttpGet("closing-soon-tasks")]
        public async Task<IActionResult> GetClosingSoonTasks([FromQuery] int employeeId)
        {
            var now = DateTime.UtcNow;
            var next3Days = now.AddDays(3); 

            var tasks = await _reportService.GetTasksClosingSoonAsync(this.RoleLevel, this.CurrentUserId, employeeId, now, next3Days);

            return Success(tasks);
        }


    }
}

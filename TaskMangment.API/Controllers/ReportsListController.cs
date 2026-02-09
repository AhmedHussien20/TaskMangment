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
            var data = await _reportService.GetTopEmployeesByCommentsAsync(this.CurrentUserId, fromDate, toDate);
            return Success(data); 
        }

        [HttpGet("most-assigned")]
        public async Task<IActionResult> GetMostAssignedEmployees(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var data = await _reportService.GetMostAssignedEmployeesAsync(this.CurrentUserId, fromDate, toDate);
            return Success(data);
        }

        [HttpGet("on-time-completion")]
        public async Task<IActionResult> GetOnTimeCompletion(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var data = await _reportService.GetOnTimeCompletionReportAsync(this.CurrentUserId,fromDate, toDate);
            return Success(data);
        }

        [HttpGet("archived-tasks")]
        public async Task<IActionResult> GetArchivedTasks(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var data = await _reportService.GetMostArchivedEmployeesAsync(this.CurrentUserId, fromDate, toDate);
            return Success(data);
        }


        [HttpGet("task-discounts")]
        public async Task<IActionResult> GetTaskDiscounts([FromQuery] TaskDiscountReportFilterDto filter) 
        {
            //filter.MovementType = movementType; 
            var data = await _reportService.GetTaskDiscountReportAsync(this.CurrentUserId,filter);
            return Success(data);
        }


        [HttpGet("task-activities")]
        public async Task<IActionResult> GetTaskActivities(DateTime? fromDate,DateTime? toDate, ExportType exportType)
        {
            var data = await _reportService.GetTaskActivityReportAsync(this.CurrentUserId,exportType, fromDate, toDate);
            return Success(data);
        }

        [HttpGet("task-movements")]
        public async Task<IActionResult> GetTaskMovements([FromQuery] TaskMovementReportFilterDto filter, ExportType exportType)
        {
            var data = await _reportService.GetTaskMovementReportAsync(this.CurrentUserId,filter, exportType);
            return Success(data);
        }

        [HttpGet("closing-soon-tasks")]
        public async Task<IActionResult> GetClosingSoonTasks([FromQuery] int employeeId)
        {
            var now = DateTime.UtcNow;
            var next3Days = now.AddDays(3); 

            var tasks = await _reportService.GetTasksClosingSoonAsync(this.CurrentUserId, employeeId, now, next3Days);

            return Success(tasks);
        }


    }
}

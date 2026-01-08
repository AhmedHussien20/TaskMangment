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
            var data = await _reportService.GetOnTimeCompletionReportAsync(fromDate, toDate);
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
        public async Task<IActionResult> GetTaskDiscounts(
            [FromQuery] TaskDiscountReportFilterDto filter)
        {
            var data = await _reportService.GetTaskDiscountReportAsync(filter);
            return Success(data);
        }

    }
}

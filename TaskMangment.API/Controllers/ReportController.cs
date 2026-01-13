using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using TaskMangment.API.Reports.Task;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Services;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class ReportController : BaseController
    {

        private readonly ITaskService _taskService;
        private readonly IReportService _reportService;

        public ReportController(ITaskService taskService, IReportService reportService)
        {
            _taskService = taskService;
            _reportService = reportService;
        }

        [HttpGet("reports/tasks/pdf")]
        public async Task<IActionResult> GetTasksPdf()
        {
            var tasks = await _taskService.GetTasksForReportAsync();

            var report = new TasksPdfReport(tasks);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "tasks-report.pdf");
        }


        [HttpGet("top-commenters/pdf")]
        public async Task<IActionResult> GetTopCommentersPdf(DateTime? fromDate,DateTime? toDate)
        {
            var data = await _reportService.GetTopEmployeesByCommentsAsync(fromDate, toDate);

            var report = new EmployeeCommentsPdfReport(data);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "top-commenters-report.pdf");
        }


        [HttpGet("most-assigned/pdf")]
        public async Task<IActionResult> GetMostAssignedEmployeesPdf(DateTime? fromDate,DateTime? toDate)
        {
            var data = await _reportService.GetMostAssignedEmployeesAsync(fromDate, toDate);

            var report = new MostAssignedEmployeesPdfReport(data);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "most-assigned-employees.pdf");
        }

        [HttpGet("on-time-completion/pdf")]
        public async Task<IActionResult> GetOnTimeCompletionPdf(DateTime? fromDate,DateTime? toDate)
        {
            var data = await _reportService.GetOnTimeCompletionReportAsync(this.Role, this.CurrentUserId, fromDate, toDate);

            var report = new OnTimeCompletionPdfReport(data);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "on-time-completion-report.pdf");
        }

        [HttpGet("archived-tasks/pdf")]
        public async Task<IActionResult> GetArchivedTasksPdf(DateTime? fromDate,DateTime? toDate)
        {
            var data = await _reportService.GetMostArchivedEmployeesAsync(fromDate, toDate);
            var report = new ArchivedTasksPdfReport(data);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "archived-tasks-report.pdf");
        }


        [HttpGet("task-discounts/pdf")]
        public async Task<IActionResult> GetTaskDiscountsPdf([FromQuery] TaskDiscountReportFilterDto filter)
        {
            var data = await _reportService.GetTaskDiscountReportAsync(filter);


            var report = new TaskDiscountsMovementPdfReport(data, filter.MovementType);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "task-discounts-report.pdf");
        }

        [HttpGet("task-activities/pdf")]
        public async Task<IActionResult> GetTaskActivitiesPdf(DateTime? fromDate, DateTime? toDate)
        {
            var data = await _reportService.GetTaskActivityReportAsync(this.Role, this.CurrentUserId, fromDate, toDate);

            var report = new TaskActivityPdfReport(data); 
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "task-activities-report.pdf");
        }

        [HttpGet("task-movements/pdf")]
        public async Task<IActionResult> GetTaskMovementsPdf(
    [FromQuery] TaskMovementReportFilterDto filter)
        {
            var data = await _reportService.GetTaskMovementReportAsync(filter);

            

            var report = new TaskMovementPdfReport(data, filter.MovementType);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "task-movements-report.pdf");
        }

        [HttpGet("closing-soon-tasks/pdf")]
        public async Task<IActionResult> GetClosingSoonTasksPdf(int employeeId)
        {
            var now = DateTime.UtcNow;
            var next3Days = now.AddHours(72);

            var tasks = await _reportService.GetTasksClosingSoonAsync(employeeId, now, next3Days);

            var report = new ClosingSoonTasksPdfReport(tasks);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "closing-soon-tasks-report.pdf");
        }


    }
}

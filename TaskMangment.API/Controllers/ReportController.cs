using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using TaskMangment.API.Reports.Task;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Infrastructure.Services;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class ReportController : Controller
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
        public async Task<IActionResult> GetTopCommentersPdf(
    DateTime? fromDate,
    DateTime? toDate)
        {
            var data = await _reportService
                .GetTopEmployeesByCommentsAsync(fromDate, toDate);

            var report = new EmployeeCommentsPdfReport(data);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "top-commenters-report.pdf");
        }


        [HttpGet("most-assigned/pdf")]
        public async Task<IActionResult> GetMostAssignedEmployeesPdf(
    DateTime? fromDate,
    DateTime? toDate)
        {
            var data = await _reportService
                .GetMostAssignedEmployeesAsync(fromDate, toDate);

            var report = new MostAssignedEmployeesPdfReport(data);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "most-assigned-employees.pdf");
        }

        [HttpGet("on-time-completion/pdf")]
        public async Task<IActionResult> GetOnTimeCompletionPdf(
    DateTime? fromDate,
    DateTime? toDate)
        {
            var data = await _reportService
                .GetOnTimeCompletionReportAsync(fromDate, toDate);

            var report = new OnTimeCompletionPdfReport(data);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "on-time-completion-report.pdf");
        }

        [HttpGet("archived-tasks/pdf")]
        public async Task<IActionResult> GetArchivedTasksPdf(
    DateTime? fromDate,
    DateTime? toDate)
        {
            var data = await _reportService
                .GetMostArchivedEmployeesAsync(fromDate, toDate);

            var report = new ArchivedTasksPdfReport(data);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "archived-tasks-report.pdf");
        }



    }
}

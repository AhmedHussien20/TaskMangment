using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using System.Reflection.Metadata;
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
        public async Task<IActionResult> GetTasksPdf(ExportType exportType)
        {
            var tasks = await _taskService.GetTasksForReportAsync();
            if (exportType == ExportType.Pdf)
            {
                var report = new TasksPdfReport(tasks);
                var pdf = report.GeneratePdf();

                return File(pdf, "application/pdf", "tasks-report.pdf");
            }
            var xlsx = TasksExcelReport.Build(tasks);
            return File(
                xlsx,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "tasks-report.xlsx");
        }


        [HttpGet("top-commenters/pdf")]
        public async Task<IActionResult> GetTopCommentersPdf(ExportType exportType,DateTime? fromDate,DateTime? toDate)
        {
            var data = await _reportService.GetEmployeesCommentsActivityAsync(this.CurrentUserId, fromDate, toDate);

            if (exportType == ExportType.Pdf)
            {
                var report = new EmployeeCommentsActivityPdfReport(data, fromDate.Value, toDate);
                var pdf = report.GeneratePdf();

                return File(pdf, "application/pdf", "top-commenters-report.pdf");
            }
            var xlsxBytes = EmployeeCommentsActivityExcelReport.Build(data, fromDate.Value, toDate);
            return File(
                xlsxBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "employee-comments-activity.xlsx");

        }


        [HttpGet("most-assigned/pdf")]
        public async Task<IActionResult> GetMostAssignedEmployeesPdf(ExportType exportType, DateTime? fromDate,DateTime? toDate)
        {
            var data = await _reportService.GetMostAssignedEmployeesAsync(this.CurrentUserId, fromDate, toDate);

            if (exportType == ExportType.Pdf)
            {
                var report = new MostAssignedEmployeesPdfReport(data, fromDate.Value, toDate);
                var pdf = report.GeneratePdf();

                return File(pdf, "application/pdf", "most-assigned-employees.pdf");
            }
            var xlsx = MostAssignedEmployeesExcelReport.Build(data, fromDate.Value, toDate);
            return File(xlsx,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "most-assigned.xlsx");
        }

        [HttpGet("on-time-completion/pdf")]
        public async Task<IActionResult> GetOnTimeCompletionPdf(ExportType exportType,DateTime? fromDate,DateTime? toDate)
        {
            var data = await _reportService.GetOnTimeCompletionReportAsync( this.CurrentUserId, fromDate, toDate);

            if (exportType == ExportType.Pdf)
            {
                var report = new OnTimeCompletionPdfReport(data, fromDate.Value, toDate);
                var pdf = report.GeneratePdf();
                return File(pdf, "application/pdf", "on-time-completion-report.pdf");
            }
            var xlsx = OnTimeCompletionExcelReport.Build(data, fromDate.Value, toDate);
            return File(
                xlsx,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "on-time-completion.xlsx");
        }

        [HttpGet("archived-tasks/pdf")]
        public async Task<IActionResult> GetArchivedTasksPdf(ExportType exportType,DateTime? fromDate,DateTime? toDate)
        {
            var data = await _reportService.GetMostArchivedEmployeesAsync(this.CurrentUserId, fromDate, toDate);

            if (exportType == ExportType.Pdf)
            {
                var doc = new ArchivedTasksPdfReport(data);
                var pdfBytes = doc.GeneratePdf();
                return File(pdfBytes, "application/pdf", "archived-tasks.pdf");
            }

            var xlsxBytes = ArchivedTasksExcelReport.Build(data);
            return File(
                xlsxBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "archived-tasks.xlsx");

        }


        [HttpGet("task-discounts/pdf")]
        public async Task<IActionResult> GetTaskDiscountsPdf(ExportType exportType,[FromQuery] TaskDiscountReportFilterDto filter)
        {
            var data = await _reportService.GetTaskDiscountAuditReportAsync(this.CurrentUserId,filter);
            string title = filter.MovementType == TaskMovementType.Incoming
                ? "تقرير خصومات المهام الواردة"
                : "تقرير خصومات المهام الصادرة";

            if (exportType == ExportType.Pdf)
            {
                var report = new TaskDiscountAuditPdfReport(data, title);
                var pdf = report.GeneratePdf();

                return File(pdf, "application/pdf", "task-discounts-report.pdf");
            }
            var xlsx = TaskDiscountAuditExcelReport.Build(data, title);
            return File(
                xlsx,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "task-discount-audit.xlsx");
        }

        [HttpGet("task-activities/pdf")]
        public async Task<IActionResult> GetTaskActivitiesPdf(DateTime? fromDate, ExportType exportType, DateTime? toDate)
        {
            var data = await _reportService.GetTaskActivityReportAsync(this.CurrentUserId,exportType, fromDate, toDate);

            if (exportType == ExportType.Pdf)
            {
                var report = new TaskActivityPdfReport(data);
                var pdf = report.GeneratePdf();
                return File(pdf, "application/pdf", "task-activities-report.pdf");
            }
            else
            {
                var xlsxBytes = TaskActivityExcelReport.Build(data);
                return File(
                    xlsxBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "task-activities.xlsx");
            }
        }

        [HttpGet("task-movements/pdf")]
        public async Task<IActionResult> GetTaskMovementsPdf(
    [FromQuery] TaskMovementReportFilterDto filter, ExportType exportType)
        {
            var data = await _reportService.GetTaskMovementReportAsync(this.CurrentUserId, filter, exportType);
            if (exportType == ExportType.Pdf)
            {
                var report = new TaskMovementPdfReport(data, filter.MovementType);
                var pdf = report.GeneratePdf();
                return File(pdf, "application/pdf", "task-movements-report.pdf");
            }
            var xlsxBytes = TaskMovementExcelReport.Build(data, filter.MovementType);
            return File(
                xlsxBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "task-movements.xlsx");
        }

        [HttpGet("closing-soon-tasks/pdf")]
        public async Task<IActionResult> GetClosingSoonTasksPdf(ExportType exportType, int? employeeId)
        {
            var now = DateTime.UtcNow;
            var next3Days = now.AddDays(3);

            var tasks = await _reportService.GetTasksClosingSoonAsync(this.CurrentUserId,employeeId, now, next3Days);
            if (exportType == ExportType.Pdf)
            {
                var doc = new ClosingSoonTasksPdfReport(tasks);
                var pdfBytes = doc.GeneratePdf();
                return File(pdfBytes, "application/pdf", "closing-soon-tasks.pdf");
            }

            var xlsxBytes = ClosingSoonTasksExcelReport.Build(tasks);
            return File(
                xlsxBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "closing-soon-tasks.xlsx");

        }


    }
}

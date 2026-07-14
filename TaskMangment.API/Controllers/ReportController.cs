using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using System.Reflection.Metadata;
using TaskMangment.API.Reports.Task;
using TaskMangment.API.Reports.Excel;
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
        public async Task<IActionResult> GetTopCommentersPdf(ExportType exportType, [FromQuery] DateRangeReportFilterDto filter)
        {
            var data = await _reportService.GetEmployeesCommentsActivityAsync(this.CurrentUserId, this.RoleLevel, filter);
            var roleTitle = filter.RoleId.HasValue ? filter.RoleTitle : null;

            if (exportType == ExportType.Pdf)
            {
                var report = new EmployeeCommentsActivityPdfReport(data, filter.FromDate!.Value, filter.ToDate, roleTitle);
                var pdf = report.GeneratePdf();

                return File(pdf, "application/pdf", "top-commenters-report.pdf");
            }
            var xlsxBytes = EmployeeCommentsActivityExcelReport.Build(data, filter.FromDate!.Value, filter.ToDate, roleTitle);
            return File(
                xlsxBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "employee-comments-activity.xlsx");

        }


        [HttpGet("most-assigned/pdf")]
        public async Task<IActionResult> GetMostAssignedEmployeesPdf(ExportType exportType, [FromQuery] DateRangeReportFilterDto filter)
        {
            var data = await _reportService.GetMostAssignedEmployeesAsync(this.CurrentUserId, this.RoleLevel, filter);
            var roleTitle = filter.RoleId.HasValue ? filter.RoleTitle : null;

            if (exportType == ExportType.Pdf)
            {
                var report = new MostAssignedEmployeesPdfReport(data, filter.FromDate!.Value, filter.ToDate, roleTitle);
                var pdf = report.GeneratePdf();

                return File(pdf, "application/pdf", "most-assigned-employees.pdf");
            }
            var xlsx = MostAssignedEmployeesExcelReport.Build(data, filter.FromDate!.Value, filter.ToDate, roleTitle);
            return File(xlsx,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "most-assigned.xlsx");
        }

        [HttpGet("on-time-completion/pdf")]
        public async Task<IActionResult> GetOnTimeCompletionPdf(ExportType exportType, [FromQuery] DateRangeReportFilterDto filter)
        {
            var data = await _reportService.GetOnTimeCompletionReportAsync( this.CurrentUserId, this.RoleLevel, filter);
            var roleTitle = filter.RoleId.HasValue ? filter.RoleTitle : null;

            if (exportType == ExportType.Pdf)
            {
                var report = new OnTimeCompletionPdfReport(data, filter.FromDate!.Value, filter.ToDate, roleTitle);
                var pdf = report.GeneratePdf();
                return File(pdf, "application/pdf", "on-time-completion-report.pdf");
            }
            var xlsx = OnTimeCompletionExcelReport.Build(data, filter.FromDate!.Value, filter.ToDate, roleTitle);
            return File(
                xlsx,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "on-time-completion.xlsx");
        }

        [HttpGet("archived-tasks/pdf")]
        public async Task<IActionResult> GetArchivedTasksPdf(ExportType exportType, [FromQuery] DateRangeReportFilterDto filter)
        {
            var data = await _reportService.GetMostArchivedEmployeesAsync(this.CurrentUserId, this.RoleLevel, filter);
            var roleTitle = filter.RoleId.HasValue ? filter.RoleTitle : null;

            if (exportType == ExportType.Pdf)
            {
                var doc = new ArchivedTasksPdfReport(data, filter.FromDate, filter.ToDate, roleTitle);
                var pdfBytes = doc.GeneratePdf();
                return File(pdfBytes, "application/pdf", "archived-tasks.pdf");
            }

            var xlsxBytes = ArchivedTasksExcelReport.Build(data, filter.FromDate, filter.ToDate, roleTitle);
            return File(
                xlsxBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "archived-tasks.xlsx");

        }


        [HttpGet("task-discounts/pdf")]
        public async Task<IActionResult> GetTaskDiscountsPdf(ExportType exportType,[FromQuery] TaskDiscountReportFilterDto filter)
        {
            var data = await _reportService.GetTaskDiscountAuditReportAsync(this.CurrentUserId, this.RoleLevel, filter);
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

        [HttpGet("employee-total-discounts/pdf")]
        public async Task<IActionResult> GetEmployeeTotalDiscountsPdf(ExportType exportType, [FromQuery] EmployeeTotalDiscountReportFilterDto filter)
        {
            if (this.RoleLevel < 100)
                return Forbid();

            if (filter.ToDate.HasValue && !filter.FromDate.HasValue)
                return BadRequest("fromDate is required when toDate is selected.");

            var data = await _reportService.GetEmployeeTotalDiscountReportAsync(this.CurrentUserId, this.RoleLevel, filter);
            var roleTitle = filter.RoleId.HasValue ? filter.RoleTitle : null;

            if (exportType == ExportType.Pdf)
            {
                var report = new EmployeeTotalDiscountPdfReport(data, filter.FromDate, filter.ToDate, roleTitle);
                var pdf = report.GeneratePdf();
                return File(pdf, "application/pdf", "employee-total-discounts.pdf");
            }

            var xlsx = EmployeeTotalDiscountExcelReport.Build(data, filter.FromDate, filter.ToDate, roleTitle);
            return File(
                xlsx,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "employee-total-discounts.xlsx");
        }

        [HttpGet("task-activities/pdf")]
        public async Task<IActionResult> GetTaskActivitiesPdf([FromQuery] DateRangeReportFilterDto filter, ExportType exportType)
        {
            var data = await _reportService.GetTaskActivityReportAsync(this.CurrentUserId, this.RoleLevel, exportType, filter);
            var roleTitle = filter.RoleId.HasValue ? filter.RoleTitle : null;

            if (exportType == ExportType.Pdf)
            {
                var report = new TaskActivityPdfReport(data, filter.FromDate, filter.ToDate, roleTitle);
                var pdf = report.GeneratePdf();
                return File(pdf, "application/pdf", "task-activities-report.pdf");
            }
            else
            {
                var xlsxBytes = TaskActivityExcelReport.Build(data, filter.FromDate, filter.ToDate, roleTitle);
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
            var data = await _reportService.GetTaskMovementReportAsync(this.CurrentUserId, this.RoleLevel, filter, exportType);
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

            var tasks = await _reportService.GetTasksClosingSoonAsync(this.CurrentUserId,this.RoleLevel, employeeId, now, next3Days);
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
        [HttpGet("employee-task-tracking/pdf")]
        public async Task<IActionResult> EmployeeTaskTrackingPdf(ExportType exportType,int? employeeId,DateTime fromDate,DateTime? toDate)
        {
            var effectiveToDate = toDate ?? DateTime.UtcNow;
            var tasks = await _reportService.GetEmployeeTaskTrackingAsync(this.CurrentUserId,this.RoleLevel,employeeId,fromDate,effectiveToDate);

            if (exportType == ExportType.Pdf)
            {
                var doc = new EmployeeTaskTrackingPdfReport(tasks, fromDate, effectiveToDate);
                var pdfBytes = doc.GeneratePdf();
                return File(pdfBytes, "application/pdf", "employee-task-tracking.pdf");
            }
            var xlsxBytes = EmployeeTaskTrackingExcelReport.Build(tasks, fromDate, effectiveToDate);
            return File(
                xlsxBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "employee-task-tracking.xlsx");
        }


        [HttpGet("branch-tasks/pdf")]
        public async Task<IActionResult> GetBranchTasksReportPdf(ExportType exportType,int branchId,DateTime fromDate,DateTime? toDate)
        {
            var effectiveToDate = toDate ?? DateTime.UtcNow;
            string branchName = "-";
            var filter = new BranchTasksReportFilterDto
            {
                BranchId = branchId,
                FromDate = fromDate,
                ToDate = effectiveToDate
            };

            var data = await _reportService.GetBranchTasksReportAsync(
                this.CurrentUserId,
                this.RoleLevel,
                filter);

            if (exportType == ExportType.Pdf)
            {
                var doc = new BranchTasksPdfReport(data, branchName, fromDate, effectiveToDate);
                var pdfBytes = doc.GeneratePdf();
                return File(pdfBytes, "application/pdf", "branch-tasks-report.pdf");
            }

            var xlsxBytes = BranchTasksExcelReport.Build(data, branchName, fromDate, effectiveToDate);
            return File(
                xlsxBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "branch-tasks-report.xlsx");
        }

        [HttpGet("employee-task-comments/pdf")]
        public async Task<IActionResult> GetEmployeeTaskCommentsPdf(ExportType exportType, int employeeId, int taskId)
        {
            var data = await _reportService.GetEmployeeTaskCommentsAsync(
                this.CurrentUserId,
                this.RoleLevel,
                employeeId,
                taskId,
                exportType);

            if (exportType == ExportType.Pdf)
            {
                var report = new EmployeeTaskCommentsPdfReport(data);
                var pdf = report.GeneratePdf();
                return File(pdf, "application/pdf", "employee-task-comments.pdf");
            }

            var xlsxBytes = EmployeeTaskCommentsExcelReport.Build(data);
            return File(
                xlsxBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "employee-task-comments.xlsx");
        }



    }
}

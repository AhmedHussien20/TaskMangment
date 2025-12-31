using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.Services;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : Controller
    {

        private readonly ITaskService _taskService;

        public ReportController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("reports/tasks/pdf")]
        public async Task<IActionResult> GetTasksPdf()
        {
            var tasks = await _taskService.GetTasksForReportAsync();

            var report = new TasksPdfReport(tasks);
            var pdf = report.GeneratePdf();

            return File(pdf, "application/pdf", "tasks-report.pdf");
        }

    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.ApiRequests.Area;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : BaseController
    {
        private readonly IChartService _service;

        public ChartController(IChartService service)
        {
            _service = service;
        }

        [HttpGet("emp-tasks-chart")]
        public async Task<IActionResult> GetEmpTasksChart(int employeeId,DateTime from, DateTime? to = null)
        {

            var result = await _service.GetEmployeeTasksChartAsync(employeeId,from,to);
            return Success(result.Data);
        }
    }
}

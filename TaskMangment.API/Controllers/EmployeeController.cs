using Microsoft.AspNetCore.Mvc;
using TaskMangment.API.Reports.Excel;
using TaskMangment.Application.Common.ApiRequests.Employee;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Dashboards.Employee;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class EmployeeController : BaseController
    {
        private readonly IEmployeeService _service;
        private readonly IEmployee360Service _employee360Service;
        private readonly IAccessScopeResolver _accessScopeResolver;
        private readonly IEmployeeDashboardService _dashboardService;

        public EmployeeController(
            IEmployeeService service,
            IEmployee360Service employee360Service,
            IAccessScopeResolver accessScopeResolver,
            IEmployeeDashboardService dashboardService)
        {
            _service = service;
            _employee360Service = employee360Service;
            _accessScopeResolver = accessScopeResolver;
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] EmployeeRequest request)
        {
            var result = await _service.GetAllAsync(request, this.CurrentUserId, this.RoleLevel);

            if (!result.Success)
                return Fail(result.Message!);

            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!await _accessScopeResolver.CanViewEmployeeAsync(CurrentUserId, id))
                return Fail(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            var result = await _service.GetByIdAsync(id);
            if (!result.Success)
                return Fail(result.Message!, StatusCodes.Status404NotFound);

            return Success(result.Data);
        }

        [HttpGet("{id}/360")]
        public async Task<IActionResult> Get360(int id, [FromQuery] Employee360DateRangeRequest range)
        {
            var result = await _employee360Service.Get360Async(CurrentUserId, id, range);
            return Success(result.Data);
        }

        [HttpGet("{id}/timeline")]
        public async Task<IActionResult> GetTimeline(int id, [FromQuery] EmployeeTimelineRequest request)
        {
            var result = await _employee360Service.GetTimelineAsync(CurrentUserId, id, request);
            return Success(result.Data);
        }

        [HttpGet("{id}/access")]
        public async Task<IActionResult> GetAccess(int id)
        {
            var result = await _employee360Service.GetAccessAsync(CurrentUserId, id);
            return Success(result.Data);
        }

        [HttpGet("{id}/performance")]
        public async Task<IActionResult> GetPerformance(int id, [FromQuery] Employee360DateRangeRequest range)
        {
            var result = await _employee360Service.GetPerformanceAsync(CurrentUserId, id, range);
            return Success(result.Data);
        }

        [HttpGet("{id}/leave")]
        public async Task<IActionResult> GetLeave360(int id, [FromQuery] Employee360DateRangeRequest range)
        {
            var result = await _employee360Service.GetLeaveAsync(CurrentUserId, id, range);
            return Success(result.Data);
        }

        [HttpGet("{id}/emails")]
        public async Task<IActionResult> GetEmails(int id, [FromQuery] Employee360PagedRequest request)
        {
            var result = await _employee360Service.GetEmailsAsync(CurrentUserId, id, request);
            return Success(result.Data);
        }

        [HttpGet("{id}/notifications")]
        public async Task<IActionResult> GetNotifications(int id, [FromQuery] Employee360PagedRequest request)
        {
            var result = await _employee360Service.GetNotificationsAsync(CurrentUserId, id, request);
            return Success(result.Data);
        }

        [HttpGet("{id}/warnings")]
        public async Task<IActionResult> GetWarnings(int id, [FromQuery] PeriodDto? period)
        {
            if (!await _accessScopeResolver.CanViewEmployeeAsync(CurrentUserId, id))
                return Fail(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            var result = await _dashboardService.GetWarningsAsync(id, period);
            return Success(result.Data);
        }

        [HttpGet("{id}/deductions")]
        public async Task<IActionResult> GetDeductions(int id, [FromQuery] PeriodDto? period)
        {
            if (!await _accessScopeResolver.CanViewEmployeeAsync(CurrentUserId, id))
                return Fail(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            var result = await _dashboardService.GetDeductionsAsync(id, period);
            return Success(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] EmployeeAddEditDto dto)
        {
            var result = await _service.AddAsync(dto, this.CompanyId);
            return Success(result.Data, "Employee added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] EmployeeAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Success(result.Data, "Employee updated successfully");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true, "Employee deleted successfully");
        }

        [HttpGet("function-codes")]
        public async Task<ApiResponse<List<FunctionCodeEnumDto>>> GetFunctionCodes()
            => await _service.GetFunctionCodesAsync();

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] EmployeeRequest request)
        {
            var result = await _service.GetAllForExportAsync(request, this.CurrentUserId, this.RoleLevel);

            if (!result.Success)
                return Fail(result.Message!);

            var xlsxBytes = EmployeesExcelReport.Build(result.Data ?? new List<EmployeeGetDto>());
            return File(
                xlsxBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "employees.xlsx");
        }

    }
}

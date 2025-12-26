
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TaskMangment.Application.Authorization;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Services;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : BaseController
    {
        private readonly ITaskService _service;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hub;

        public TaskController(ITaskService service, IHubContext<NotificationHub> hub, INotificationService notificationService)
        {
            _service = service;
            _hub = hub;
            _notificationService= notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TaskRequest request)
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role))
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status400BadRequest);

            var result = await _service.GetAllAsync(request, CompanyId, role, this.CurrentUserId);

            if (!result.Success)
                return Fail(result.Message!);

            //SetCacheHeader(600);

            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result.Data);
        }
        [HttpPost]
        [HasRole("Manager")]
        public async Task<IActionResult> Add([FromBody] TaskAddEditDto dto)
        {
           
            var result = await _service.AddAsync(dto,this.CurrentUserId, this.CompanyId);
            return Success(result.Data, "Task added successfully");
        }

        [HttpPut("{id}")]
        [HasRole("Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto, this.CurrentUserId);
            return Success(result.Data, "Task updated successfully");
        }

        [HttpDelete("{id}")]
        [HasRole("Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true, "Task deleted successfully");
        }

        [HttpGet("{taskId}/assigned-employees")]
        [HasRole("Manager")]
        public async Task<IActionResult> GetAssignedEmployees(int taskId)
        {
            var response = await _service.GetAssignedEmployeesAsync(taskId);
            return Success(response.Data);
        }
    }
}

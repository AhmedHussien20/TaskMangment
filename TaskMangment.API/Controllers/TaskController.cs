
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
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
            var result = await _service.GetAllAsync(request);

            if (!result.Success)
                return Fail(result.Message!);

            SetCacheHeader(600);

            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (!result.Success)
                return Fail(result.Message!, 404);

            return Success(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TaskAddEditDto dto)
        {
            var result = await _service.AddAsync(dto);
            foreach(int empId in dto.AssignedEmployeeIds)
            {
                bool isOnline = NotificationHub.IsUserOnline(empId);
                if (isOnline)
                {
                    await _hub.Clients.Group($"user-{empId}").SendAsync("ReciveTask", dto.Description);
                }
                var notfi = new Notification
                {
                    UserId = empId,
                    Message = dto.Description,
                    Type = "Web",
                    IsRead = false
                };
                
            }
            
            if (!result.Success)
                return Fail(result.Message);

            return Success(true, "Task added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result.Success)
                return Fail(result.Message, 404);

            return Success(true, "Task updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result.Success)
                return Fail(result.Message, 404);

            return Success(true, "Task deleted successfully");
        }
    }
}

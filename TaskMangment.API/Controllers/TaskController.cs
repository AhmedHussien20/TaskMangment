
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; 
using TaskMangment.Application.Authorization;
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
        [HasMinRoleLevel(RoleLevelEnum.Employee)]
        public async Task<IActionResult> GetAll([FromQuery] TaskRequest request)
        {
            var result = await _service.GetAllAsync(request, CompanyId, this.Role, this.CurrentUserId);
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
        [HasMinRoleLevel(RoleLevelEnum.Manager)]
        public async Task<IActionResult> Add([FromBody] TaskAddEditDto dto)
        {
           
            var result = await _service.AddAsync(dto,this.CurrentUserId, this.CompanyId);
            return Success(result.Data, "Task added successfully");
        }

        [HttpPut("{id}")]
        [HasMinRoleLevel(RoleLevelEnum.Manager)]
        public async Task<IActionResult> Update(int id, [FromBody] TaskAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto, this.CurrentUserId);
            return Success(result.Data, "Task updated successfully");
        }

        [HttpDelete("{id}")]
        [HasMinRoleLevel(RoleLevelEnum.Manager)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true, "Task deleted successfully");
        }

        [HttpGet("{taskId}/assigned-employees")]
        [HasMinRoleLevel(RoleLevelEnum.Manager)]
        public async Task<IActionResult> GetAssignedEmployees(int taskId)
        {
            var response = await _service.GetAssignedEmployeesAsync(taskId);
            return Success(response.Data);
        }
    }
}

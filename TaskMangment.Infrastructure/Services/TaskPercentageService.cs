using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.Persistence.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TaskMangment.Infrastructure.Services
{
    public class TaskPercentageService : ITaskPercentageService
    {
        private readonly IRepository<TaskPercentage> _repo;
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepo;
        private readonly IGetHigherManager _getHigherManager;


        public TaskPercentageService(
            IRepository<TaskPercentage> repo,
            IRepository<WorkTask> taskRepo,
            IRepository<Employee> employeeRepo,
            IMapper mapper,
            ICachingService cache,
            IDomainEventDispatcher eventDispatcher,
            IRepository<TaskAssignment> taskAssignmentRepo,
            IGetHigherManager getHigherManager)
        {
            _repo = repo;
            _taskRepo = taskRepo;
            _employeeRepo = employeeRepo;
            _mapper = mapper;
            _cache = cache;
            _eventDispatcher = eventDispatcher;
            _taskAssignmentRepo = taskAssignmentRepo;
            _getHigherManager = getHigherManager;
        }

        public async Task<ApiResponse<PagedResponse<TaskPercentageGetDto>>> GetAllAsync(TaskPercentRequest request)
        {
            var query = _repo.GetAll()
                             .Include(tp => tp.Task)
                             .Include(tp => tp.Employee)
                             .ApplySearch(request.searchKey);

            var totalCount = await query.CountAsync();
            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<TaskPercentageGetDto>>(list);

            foreach (var dto in dtos)
            {
                var entity = list.First(t => t.Id == dto.Id);
                dto.TaskTitle = entity.Task?.Title;
                dto.EmployeeName = entity.Employee?.FullName;
            }

            return ApiResponse<PagedResponse<TaskPercentageGetDto>>.Ok(
                new PagedResponse<TaskPercentageGetDto>(dtos, totalCount, request.PageIndex, request.PageSize)
            );
        }

        public async Task<ApiResponse<TaskPercentageGetDto>> GetByIdAsync(int id)
        {
            var entity = await _repo.GetAll()
                .Include(tp => tp.Task)
                .Include(tp => tp.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(tp => tp.Id == id);

            if (entity == null)
                throw new AppException("TaskPercentage not found", StatusCodes.Status404NotFound);

            var dto = _mapper.Map<TaskPercentageGetDto>(entity);
            dto.TaskTitle = entity.Task?.Title;
            dto.EmployeeName = entity.Employee?.FullName;

            return ApiResponse<TaskPercentageGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<TaskPercentageGetDto>> AddAsync(int taskId, int employeeId,string role ,TaskPercentageAddEditDto dto)
        {
            var task = await _taskRepo.GetByIDAsync(taskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            if (task.Status == WorkTaskStatus.Closed || task.Status == WorkTaskStatus.Archived || task.Status == WorkTaskStatus.AutoClose)
                throw new AppException(ErrorCodes.TaskAlreadyClosed, StatusCodes.Status400BadRequest);


            if (role != "Manager" && task.CreatedByEmployeeId != employeeId)
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);




            var entity = _mapper.Map<TaskPercentage>(dto);
            entity.TaskId = taskId;
            entity.EmployeeId = employeeId;

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
            await _cache.RemoveAsync("taskPercentages:");

            var assignedEmployeeIds = await _taskAssignmentRepo
      .GetAll(a => a.TaskId == taskId && a.IsActive)
      .Where(a => a.EmployeeId != employeeId)
      .Select(a => a.EmployeeId)
      .ToListAsync();


            var employee = await _employeeRepo.GetByIDAsync(employeeId);
            string employeeName = employee?.FullName;



            if (task.AssignedByEmployeeId.HasValue && !assignedEmployeeIds.Contains(task.AssignedByEmployeeId.Value))
            {
                assignedEmployeeIds.Add(task.AssignedByEmployeeId.Value);
            }
            assignedEmployeeIds.Remove(employeeId);
            assignedEmployeeIds = await _employeeRepo.GetAll(e => assignedEmployeeIds.Contains(e.Id) && e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();

            foreach (var recipientId in assignedEmployeeIds.ToList())
            {
                var managerIds = await _getHigherManager.GetDirectHigherManagerIdsAsync(recipientId);
                assignedEmployeeIds.AddRange(managerIds.Where(id => !assignedEmployeeIds.Contains(id)));
            }

            assignedEmployeeIds = await _employeeRepo.GetAll(e => assignedEmployeeIds.Contains(e.Id) && e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();

            if (assignedEmployeeIds.Any())
                await _eventDispatcher.PublishAsync(new TaskAchievePercentEvent(entity.Id,dto.AchievementPercent, task.Id,task.Title, employeeName, assignedEmployeeIds));

            var saved = await _repo.GetAll()
                .Include(tp => tp.Task)
                .Include(tp => tp.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(tp => tp.Id == entity.Id);

            var resultDto = _mapper.Map<TaskPercentageGetDto>(saved);
            resultDto.TaskTitle = saved.Task?.Title;
            resultDto.EmployeeName = saved.Employee?.FullName;

            return ApiResponse<TaskPercentageGetDto>.Ok(resultDto, "Percentage added successfully");
        }

        public async Task<ApiResponse<TaskPercentageGetDto>> UpdateAsync(int id, TaskPercentageAddEditDto dto)
        {
            var entity = await _repo.GetByIDAsync(id);
            if (entity == null)
                throw new AppException("TaskPercentage not found", StatusCodes.Status404NotFound);

            entity.AchievementPercent = dto.AchievementPercent;
            entity.AchievementReason = dto.AchievementReason;

            await _repo.SaveChangesAsync();
            await _cache.RemoveAsync("taskPercentages:");

            var saved = await _repo.GetAll()
                .Include(tp => tp.Task)
                .Include(tp => tp.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(tp => tp.Id == entity.Id);

            var resultDto = _mapper.Map<TaskPercentageGetDto>(saved);
            resultDto.TaskTitle = saved.Task?.Title;
            resultDto.EmployeeName = saved.Employee?.FullName;

            return ApiResponse<TaskPercentageGetDto>.Ok(resultDto, "Percentage updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIDAsync(id);
            if (entity == null)
                throw new AppException("TaskPercentage not found", StatusCodes.Status404NotFound);

            _repo.SoftDelete(entity);
            await _repo.SaveChangesAsync();
            await _cache.RemoveAsync("taskPercentages:");

            return ApiResponse<bool>.Ok(true, "Percentage deleted successfully");
        }
    }
}

using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

namespace TaskMangment.Infrastructure.Services
{
    public class TaskWarningService : ITaskWarningService
    {
        private readonly IRepository<Warning> _warningRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepo;
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IDomainEventDispatcher _eventDispatcher;


        public TaskWarningService(
            IRepository<Warning> warningRepo,
            IRepository<Employee> employeeRepo,
            IRepository<TaskAssignment> taskAssignmentRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<WorkTask> taskRepo,
            IDomainEventDispatcher eventDispatcher)
        {
            _warningRepo = warningRepo;
            _employeeRepo = employeeRepo;
            _taskAssignmentRepo = taskAssignmentRepo;
            _mapper = mapper;
            _cache = cache;
            _taskRepo = taskRepo;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<ApiResponse<PagedResponse<WarningListDto>>> GetAllAsync(WarningRequest request)
        {
            string cacheKey = $"warnings:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}:{request.TaskId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<WarningListDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<WarningListDto>>.Ok(cached);
            }

            var task = await _taskRepo.GetByIDAsync(request.TaskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            var query = _warningRepo.GetAll(c => c.TaskId == request.TaskId)
                .Include(w => w.IssuedBy)
                .Include(w => w.Issued)
                .Include(c => c.Task)
                .ApplySearch(request.searchKey);



            var totalCount = await query.CountAsync();
            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<WarningListDto>>(list);

            var response = new PagedResponse<WarningListDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<WarningListDto>>.Ok(response);
        }

        public async Task<ApiResponse<WarningGetDto>> GetByIdAsync(int id)
        {
            var warning = await _warningRepo.GetAll(w => w.Id == id)
                .Include(w => w.IssuedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (warning == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var dto = _mapper.Map<WarningGetDto>(warning);
            return ApiResponse<WarningGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<WarningGetDto>> AddAsync(WarningAddEditDto dto,int taskId,int employeeId)
        {
            var task = await _taskRepo.GetByIDAsync(taskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            var assignment = await _taskAssignmentRepo.GetAll(a =>
                    a.TaskId == taskId &&
                    a.EmployeeId == dto.IssuedEmployeeId &&
                    a.IsActive)
                .FirstOrDefaultAsync();

            if (assignment == null)
                throw new AppException(ErrorCodes.NotAssigned, StatusCodes.Status400BadRequest);


            var warning = _mapper.Map<Warning>(dto);
            warning.TaskAssignmentId = assignment.Id;
            warning.TaskId = taskId;
            warning.IssuedByEmployeeId = employeeId;   // اللي بيعمل التحذير
            warning.IssuedEmployeeId = dto.IssuedEmployeeId; // اللي بيتحذر
            //warning.CreatedBy = employeeId;
            //warning.CreatedDate = DateTime.UtcNow;

            await _warningRepo.AddAsync(warning);
            await _warningRepo.SaveChangesAsync();

            await _cache.RemoveAsync("warnings:");


            var employeeName = await _employeeRepo.GetAll(e => e.Id == employeeId).Select(e => e.FullName).FirstOrDefaultAsync();

            await _eventDispatcher.PublishAsync(
                new TaskWarningEvent(warning.Id, taskId, employeeName, dto.IssuedEmployeeId, task.Title)
            );

            // تحميل البيانات للـ response
            var savedWarning = await _warningRepo.GetAll(w => w.Id == warning.Id)
                .Include(w => w.IssuedBy)
                .Include(w => w.Issued)
                .Include(w => w.TaskAssignment)
                    .ThenInclude(a => a.Task)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var warningDto = _mapper.Map<WarningGetDto>(savedWarning);
            warningDto.TaskTitle = savedWarning.TaskAssignment.Task?.Title;
            warningDto.IssuedEmployeeName = savedWarning.Issued?.FullName;
            warningDto.IssuedByName = savedWarning.IssuedBy?.FullName;

            return ApiResponse<WarningGetDto>.Ok(
                warningDto,
                "Warning added successfully"
            );
        }

        public async Task<ApiResponse<WarningGetDto>> UpdateAsync(int id, WarningAddEditDto dto)
        {
            var warning = await _warningRepo.GetAll(w => w.Id == id)
                .Include(w => w.TaskAssignment)
                .FirstOrDefaultAsync();

            if (warning == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);



            var taskAssignments = await _taskAssignmentRepo.GetAll(ta => ta.TaskId == warning.TaskAssignment.TaskId)
                .ToListAsync();

            var assignment = taskAssignments.FirstOrDefault(ta => ta.EmployeeId == dto.IssuedEmployeeId);

            if (assignment == null)
                throw new AppException(ErrorCodes.NotAssigned, StatusCodes.Status400BadRequest);

            warning = _mapper.Map<Warning>(dto);
            warning.TaskAssignmentId = assignment.Id;
           // warning.ModifiedDate = DateTime.UtcNow;
            warning.IssuedEmployeeId = dto.IssuedEmployeeId;

            await _warningRepo.SaveChangesAsync();
            await _cache.RemoveAsync("warnings:");


            var savedRequest = await _warningRepo.GetAll(r => r.Id == warning.Id)
                                                           .Include(r => r.IssuedBy)
                                                           .Include(r => r.Issued)
                                                           .Include(r => r.TaskAssignment)
                                                           .ThenInclude(a => a.Employee)
                                                           .Include(r => r.TaskAssignment)
                                                           .ThenInclude(a => a.Task)
                                                          .AsNoTracking()
                                                          .FirstOrDefaultAsync();

            var warningDto = _mapper.Map<WarningGetDto>(savedRequest);
            warningDto.TaskTitle = savedRequest.TaskAssignment.Task?.Title;
            warningDto.IssuedEmployeeName = savedRequest.Issued?.FullName;
            warningDto.IssuedByName = savedRequest.IssuedBy?.FullName;

            return ApiResponse<WarningGetDto>.Ok(warningDto, "Warning updated successfully");
        }


        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var warning = await _warningRepo.GetByIDAsync(id);
            if (warning == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            _warningRepo.SoftDelete(warning);
            await _warningRepo.SaveChangesAsync();
            await _cache.RemoveAsync("warnings:");

            return ApiResponse<bool>.Ok(true, "Warning deleted successfully");

        }
    }
}

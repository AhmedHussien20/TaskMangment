using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly IRepository<TaskAssignment> _assignmentRepo;
        private readonly IMapper _mapper;

        public TaskAssignmentService(IRepository<TaskAssignment> assignmentRepo, IMapper mapper)
        {
            _assignmentRepo = assignmentRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResponse<TaskAssignmentGetDto>>> GetAllAsync(TaskAssignmentRequest request)
        {
            var query = _assignmentRepo.GetAll()
                .Include(x => x.Employee)
                .Include(x => x.Task)
                .AsQueryable();

 
            // Sorting
            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            // Pagination
            int totalCount = await query.CountAsync();

            var data = await query
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

            var dtos = _mapper.Map<ICollection<TaskAssignmentGetDto>>(data);

            var result = new PagedResponse<TaskAssignmentGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            return ApiResponse<PagedResponse<TaskAssignmentGetDto>>.Ok(result);
        }

        public async Task<ApiResponse<TaskAssignmentGetDto>> GetByIdAsync(int id)
        {
            var item = await _assignmentRepo.GetAll(x => x.Id == id)
                .Include(x => x.Employee)
                .Include(x => x.Task)
                .FirstOrDefaultAsync();

            if (item == null)
                return ApiResponse<TaskAssignmentGetDto>.Fail("TaskAssignment not found");

            return ApiResponse<TaskAssignmentGetDto>.Ok(_mapper.Map<TaskAssignmentGetDto>(item));
        }

        public async Task<ApiResponse<bool>> AddAsync(TaskAssignmentAddEditDto dto)
        {
            var entity = _mapper.Map<TaskAssignment>(dto);

            await _assignmentRepo.AddAsync(entity);
            await _assignmentRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "TaskAssignment added");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, TaskAssignmentAddEditDto dto)
        {
            var entity = await _assignmentRepo.GetByIDAsync(id);

            if (entity == null)
                return ApiResponse<bool>.Fail("TaskAssignment not found");

            _mapper.Map(dto, entity);

            await _assignmentRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "TaskAssignment updated");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _assignmentRepo.GetByIDAsync(id);

            if (entity == null)
                return ApiResponse<bool>.Fail("TaskAssignment not found");

            _assignmentRepo.SoftDelete(entity);
            await _assignmentRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "TaskAssignment deleted");
        }
    }

}

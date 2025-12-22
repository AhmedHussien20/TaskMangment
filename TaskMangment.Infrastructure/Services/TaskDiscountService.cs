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
using TaskMangment.Infrastructure.Persistence.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TaskMangment.Infrastructure.Services
{
    public class TaskDiscountService : ITaskDiscountService
    {
        private readonly IRepository<Discount> _discountRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public TaskDiscountService(
            IRepository<Discount> discountRepo,
            IRepository<Employee> employeeRepo,
            IRepository<WorkTask> taskRepo,
            IMapper mapper,
            ICachingService cache)
        {
            _discountRepo = discountRepo;
            _employeeRepo = employeeRepo;
            _taskRepo = taskRepo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<DiscountListDto>>> GetAllAsync(TaskDiscountRequest request)
        {
            string cacheKey = $"discounts:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}:{request.TaskId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<DiscountListDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<DiscountListDto>>.Ok(cached);
            }

            var task = await _taskRepo.GetByIDAsync(request.TaskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            var query = _discountRepo.GetAll(c => c.TaskId == request.TaskId)
                .Include(d => d.Employee)
                .Include(d => d.Task)
                .Include(d => d.CreatedBy)
                .ApplySearch(request.searchKey);
            
            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<DiscountListDto>>(list);

            var response = new PagedResponse<DiscountListDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<DiscountListDto>>.Ok(response);
        }

        public async Task<ApiResponse<DiscountListDto>> GetByIdAsync(int id)
        {
            var discount = await _discountRepo.GetAll(x => x.Id == id)
                .Include(x => x.Employee)
                .Include(x => x.Task)
                .Include(x => x.CreatedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (discount == null)
                return ApiResponse<DiscountListDto>.Fail("Discount not found");

            var dto = _mapper.Map<DiscountListDto>(discount);

            return ApiResponse<DiscountListDto>.Ok(dto);
        }

        public async Task<ApiResponse<DiscountListDto>> AddAsync(int createdByEmployeeId, int TaskID, DiscountAddEditDto dto)
        {
            if (!await _employeeRepo.IsExistAsync(dto.EmployeeId))
                throw new AppException(ErrorCodes.NotAssigned, StatusCodes.Status404NotFound);

            if (!await _taskRepo.IsExistAsync(TaskID))
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            var discount = _mapper.Map<Discount>(dto);
            discount.TaskId = TaskID;
            discount.CreatedByEmployeeId = createdByEmployeeId;
            //discount.CreatedDate = DateTime.UtcNow;

            await _discountRepo.AddAsync(discount);
            await _discountRepo.SaveChangesAsync();
            await _cache.RemoveAsync("discounts:");


            var fullDiscount = await _discountRepo
      .GetAll(d => d.Id == discount.Id)
      .Include(d => d.Employee)
      .Include(d => d.Task)
      .FirstOrDefaultAsync();
            var discountdto = _mapper.Map<DiscountListDto>(fullDiscount);


            return ApiResponse<DiscountListDto>.Ok(discountdto, "Discount added successfully");
        }

        public async Task<ApiResponse<DiscountListDto>> UpdateAsync(int id,int TaskID, DiscountAddEditDto dto, int ModifiedByEmployeeId)
        {
            var discount = await _discountRepo.GetByIDAsync(id);
            if (discount == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (!await _employeeRepo.IsExistAsync(dto.EmployeeId))
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status404NotFound);

            if (!await _taskRepo.IsExistAsync(TaskID))
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            if (!await _employeeRepo.IsExistAsync(ModifiedByEmployeeId))
                return ApiResponse<DiscountListDto>.Fail("ModifiedByEmployee not found");


            _mapper.Map(dto, discount);
            discount.TaskId = TaskID;
            discount.ModifiedBy = ModifiedByEmployeeId;
            discount.ModifiedDate = DateTime.UtcNow;

            await _discountRepo.SaveChangesAsync();
            await _cache.RemoveAsync("discounts:");

            var fullDiscount = await _discountRepo
                .GetAll(d => d.Id == discount.Id)
                .Include(d => d.Employee)
                .Include(d => d.Task)
                .FirstOrDefaultAsync();
            var discountdto = _mapper.Map<DiscountListDto>(fullDiscount);

            return ApiResponse<DiscountListDto>.Ok(discountdto, "Discount updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var discount = await _discountRepo.GetByIDAsync(id);
            if (discount == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            _discountRepo.SoftDelete(discount);
            await _discountRepo.SaveChangesAsync();
            await _cache.RemoveAsync("discounts:");


            return ApiResponse<bool>.Ok(true, "Discount deleted successfully");
        }
    }
}

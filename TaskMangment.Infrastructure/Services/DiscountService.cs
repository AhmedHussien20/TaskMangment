using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Discount;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TaskMangment.Infrastructure.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IRepository<Discount> _discountRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public DiscountService(
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

        public async Task<ApiResponse<PagedResponse<DiscountListDto>>> GetAllAsync(DiscountRequest request)
        {
            string cacheKey =
                $"discounts-{request.PageIndex}-{request.PageSize}-{request.SortColumn}-{request.SortDirection}-{request.EmployeeId}-{request.TaskId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<DiscountListDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<DiscountListDto>>.Ok(cached);
            }

            var query = _discountRepo.GetAll()
                .Include(d => d.Employee)
                .Include(d => d.Task)
                .Include(d => d.CreatedBy)
                .AsQueryable();

            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);

            if (request.TaskId.HasValue)
                query = query.Where(x => x.TaskId == request.TaskId.Value);

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

        public async Task<ApiResponse<DiscountDetailsDto>> GetByIdAsync(int id)
        {
            var discount = await _discountRepo.GetAll(x => x.Id == id)
                .Include(x => x.Employee)
                .Include(x => x.Task)
                .Include(x => x.CreatedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (discount == null)
                return ApiResponse<DiscountDetailsDto>.Fail("Discount not found");

            var dto = _mapper.Map<DiscountDetailsDto>(discount);

            return ApiResponse<DiscountDetailsDto>.Ok(dto);
        }

        public async Task<ApiResponse<DiscountDetailsDto>> AddAsync(int createdByEmployeeId, int TaskID, DiscountAddEditDto dto)
        {
            if (!await _employeeRepo.IsExistAsync(dto.EmployeeId))
                return ApiResponse<DiscountDetailsDto>.Fail("Employee not found");

            if (!await _taskRepo.IsExistAsync(TaskID))
                return ApiResponse<DiscountDetailsDto>.Fail("Task not found");

            var discount = _mapper.Map<Discount>(dto);
            discount.TaskId = TaskID;
            discount.CreatedByEmployeeId = createdByEmployeeId;
            discount.CreatedDate = DateTime.UtcNow;

            await _discountRepo.AddAsync(discount);
            await _discountRepo.SaveChangesAsync();

            var fullDiscount = await _discountRepo
      .GetAll(d => d.Id == discount.Id)
      .Include(d => d.Employee)
      .Include(d => d.Task)
      .FirstOrDefaultAsync();
            var discountdto = _mapper.Map<DiscountDetailsDto>(fullDiscount);


            return ApiResponse<DiscountDetailsDto>.Ok(discountdto, "Discount added successfully");
        }

        public async Task<ApiResponse<DiscountDetailsDto>> UpdateAsync(int id,int TaskID, DiscountAddEditDto dto, int ModifiedByEmployeeId)
        {
            var discount = await _discountRepo.GetByIDAsync(id);
            if (discount == null)
                return ApiResponse<DiscountDetailsDto>.Fail("Discount not found");

            if (!await _employeeRepo.IsExistAsync(dto.EmployeeId))
                return ApiResponse<DiscountDetailsDto>.Fail("Employee not found");

            if (!await _employeeRepo.IsExistAsync(ModifiedByEmployeeId))
                return ApiResponse<DiscountDetailsDto>.Fail("ModifiedByEmployee not found");

            if (!await _taskRepo.IsExistAsync(TaskID))
                return ApiResponse<DiscountDetailsDto>.Fail("Task not found");

            _mapper.Map(dto, discount);
            discount.TaskId = TaskID;
            discount.ModifiedBy = ModifiedByEmployeeId;
            discount.ModifiedDate = DateTime.UtcNow;

            await _discountRepo.SaveChangesAsync();
            var fullDiscount = await _discountRepo
                .GetAll(d => d.Id == discount.Id)
                .Include(d => d.Employee)
                .Include(d => d.Task)
                .FirstOrDefaultAsync();
            var discountdto = _mapper.Map<DiscountDetailsDto>(fullDiscount);

            return ApiResponse<DiscountDetailsDto>.Ok(discountdto, "Discount updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var discount = await _discountRepo.GetByIDAsync(id);
            if (discount == null)
                return ApiResponse<bool>.Fail("Discount not found");

            _discountRepo.SoftDelete(discount);
            await _discountRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Discount deleted successfully");
        }
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Branch;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class BranchService : IBranchService
    {
        private readonly IRepository<Branch> _branchRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Area> _areaRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public BranchService(
            IRepository<Branch> branchRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Area> areaRepository,
            IRepository<Company> companyRepository,
            IMapper mapper,
            ICachingService cache)
        {
            _branchRepository = branchRepository;
            _employeeRepository = employeeRepository;
            _areaRepository = areaRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<BranchGetDto>>> GetAllAsync(BranchRequest request)
        {
            string safeName = request.Name ?? string.Empty;
            string safeCompanyId = request.CompanyId?.ToString() ?? "null";
            string safeAreaId = request.AreaId?.ToString() ?? "null";

            string cacheKey =
                $"branches-{request.PageIndex}-{request.PageSize}-{request.SortColumn}-{request.SortDirection}-{safeName}-{safeCompanyId}-{safeAreaId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<BranchGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<BranchGetDto>>.Ok(cached);
            }

            var query = _branchRepository.GetAll()
                .Include(b => b.Manager)
                .Include(b => b.Responsible)
                .Include(b => b.Area)

                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Name))
                query = query.Where(b => b.Name.Contains(request.Name));

            if (request.CompanyId.HasValue)
                query = query.Where(b => b.CompanyId == request.CompanyId.Value);

            if (request.AreaId.HasValue)
                query = query.Where(b => b.AreaId == request.AreaId.Value);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<BranchGetDto>>(list);

            var response = new PagedResponse<BranchGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<BranchGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<BranchGetDto>> GetByIdAsync(int id)
        {
            var branch = await _branchRepository.GetAll(b => b.Id == id && !b.IsDeleted)
                .Include(b => b.Manager)
                .Include(b => b.Responsible)
                .Include(b => b.Area)

                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (branch == null)
                return ApiResponse<BranchGetDto>.Fail("Branch not found", StatusCode.NotFound);

            var dto = _mapper.Map<BranchGetDto>(branch);
            return ApiResponse<BranchGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(BranchAddEditDto dto)
        {
            // تحقق من الـ Manager و Responsible
            if (dto.ManagerId.HasValue && !await _employeeRepository.IsExistAsync(dto.ManagerId.Value))
                return ApiResponse<bool>.Fail("Manager not found", StatusCode.NotFound);

            if (dto.ResponsibleId.HasValue && !await _employeeRepository.IsExistAsync(dto.ResponsibleId.Value))
                return ApiResponse<bool>.Fail("Responsible employee not found", StatusCode.NotFound);

            // تحقق من الـ Area
            if (dto.AreaId.HasValue && !await _areaRepository.IsExistAsync(dto.AreaId.Value))
                return ApiResponse<bool>.Fail("Area not found", StatusCode.NotFound);

            // تحقق من الـ Company
            if (!await _companyRepository.IsExistAsync(dto.CompanyId))
                return ApiResponse<bool>.Fail("Company not found", StatusCode.NotFound);

            var branch = _mapper.Map<Branch>(dto);

            await _branchRepository.AddAsync(branch);
            await _branchRepository.SaveChangesAsync();

            // TODO: Optional: Clear branch cache pattern
            // await _cache.RemoveByPatternAsync("branches-");

            return ApiResponse<bool>.Ok(true, "Branch added successfully");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, BranchAddEditDto dto)
        {
            var branch = await _branchRepository.GetByIDAsync(id);
            if (branch == null)
                return ApiResponse<bool>.Fail("Branch not found", StatusCode.NotFound);

            if (dto.ManagerId.HasValue && !await _employeeRepository.IsExistAsync(dto.ManagerId.Value))
                return ApiResponse<bool>.Fail("Manager not found", StatusCode.NotFound);

            if (dto.ResponsibleId.HasValue && !await _employeeRepository.IsExistAsync(dto.ResponsibleId.Value))
                return ApiResponse<bool>.Fail("Responsible employee not found", StatusCode.NotFound);

            if (dto.AreaId.HasValue && !await _areaRepository.IsExistAsync(dto.AreaId.Value))
                return ApiResponse<bool>.Fail("Area not found", StatusCode.NotFound);

            if (!await _companyRepository.IsExistAsync(dto.CompanyId))
                return ApiResponse<bool>.Fail("Company not found", StatusCode.NotFound);

            _mapper.Map(dto, branch);

            await _branchRepository.SaveChangesAsync();

            // TODO: Optional: Invalidate cache

            return ApiResponse<bool>.Ok(true, "Branch updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var branch = await _branchRepository.GetByIDAsync(id);
            if (branch == null)
                return ApiResponse<bool>.Fail("Branch not found", StatusCode.NotFound);

            _branchRepository.SoftDelete(branch);
            await _branchRepository.SaveChangesAsync();

            // TODO: Optional: Invalidate cache

            return ApiResponse<bool>.Ok(true, "Branch deleted successfully");
        }
    }
}


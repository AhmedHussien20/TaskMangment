using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskMangment.Application.ApiRequests.Area;
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
    public class AreaService : IAreaService
    {
        private readonly IRepository<Area> _areaRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Branch> _branchRepository;
        private readonly IRepository<Company> _companyRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public AreaService(
            IRepository<Area> areaRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Branch> branchRepository,
            IRepository<Company> companyRepo,
            IMapper mapper,
            ICachingService cache)
        {
            _areaRepository = areaRepository;
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
            _companyRepo = companyRepo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<AreaGetDto>>> GetAllAsync(AreaRequest request)
        {
            string safeName = request.Name ?? string.Empty;
            string safeCompanyId = request.CompanyId?.ToString() ?? "null";

            string cacheKey =
                $"areas-{request.PageIndex}-{request.PageSize}-{request.SortColumn}-{request.SortDirection}-{safeName}-{safeCompanyId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<AreaGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<AreaGetDto>>.Ok(cached);
            }


            var query = _areaRepository.GetAll()
                .Include(a => a.Manager)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Name))
                query = query.Where(a => a.Name.Contains(request.Name));

            if (request.CompanyId.HasValue)
                query = query.Where(a => a.CompanyId == request.CompanyId.Value);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            // Pagination
            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Map
            var dtos = _mapper.Map<ICollection<AreaGetDto>>(list);

            // Branch counts
            foreach (var dto in dtos)
            {
                dto.BranchCount = await _branchRepository.CountAsync(b => b.AreaId == dto.Id);
            }

            var response = new PagedResponse<AreaGetDto>(
                dtos, totalCount, request.PageIndex, request.PageSize);

            // Save to cache for 10 minutes
            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<AreaGetDto>>.Ok(response);
        }



        public async Task<ApiResponse<AreaGetDto>> GetByIdAsync(int id)
        {
            var area = await _areaRepository.GetAll(a => a.Id == id)
                .Include(a => a.Manager)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (area == null)
                return ApiResponse<AreaGetDto>.Fail("Area not found", StatusCode.NotFound);

            var dto = _mapper.Map<AreaGetDto>(area);
            dto.BranchCount = await _branchRepository.CountAsync(b => b.AreaId == id);

            return ApiResponse<AreaGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<AreaGetDto>> AddAsync(AreaAddEditDto dto, int CompanyId, int createdby)
        {
            if (!await _employeeRepository.IsExistAsync(dto.ManagerEmployeeId))
                return ApiResponse<AreaGetDto>.Fail("Manager not found", StatusCode.NotFound);

            if (!await _companyRepo.IsExistAsync(CompanyId))
                return ApiResponse<AreaGetDto>.Fail("Company not found", StatusCode.NotFound);

            var area = _mapper.Map<Area>(dto);
            area.CompanyId = CompanyId;
            area.CreatedBy = createdby;
            area.CreatedDate = DateTime.UtcNow;

            await _areaRepository.AddAsync(area);
            await _areaRepository.SaveChangesAsync();
            var areaDto = _mapper.Map<AreaGetDto>(area);


            // TODO: Optional: Clear area cache pattern
            // await _cache.RemoveByPatternAsync("areas-");

            return ApiResponse<AreaGetDto>.Ok(areaDto, "Area added successfully");
        }

        public async Task<ApiResponse<AreaGetDto>> UpdateAsync(int id, AreaAddEditDto dto)
        {
            var area = await _areaRepository.GetByIDAsync(id);
            if (area == null)
                return ApiResponse<AreaGetDto>.Fail("Area not found", StatusCode.NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ManagerEmployeeId))
                return ApiResponse<AreaGetDto>.Fail("Manager not found", StatusCode.NotFound);

            _mapper.Map(dto, area);

            await _areaRepository.SaveChangesAsync();
            var areaDto = _mapper.Map<AreaGetDto>(area);

            //  TODO: Invalidate cache later

            return ApiResponse<AreaGetDto>.Ok(areaDto, "Area updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var area = await _areaRepository.GetByIDAsync(id);
            if (area == null)
                return ApiResponse<bool>.Fail("Area not found", StatusCode.NotFound);

            _areaRepository.SoftDelete(area);
            await _areaRepository.SaveChangesAsync();

            // TODO: Invalidate cache later

            return ApiResponse<bool>.Ok(true, "Area deleted successfully");
        }
    }
}

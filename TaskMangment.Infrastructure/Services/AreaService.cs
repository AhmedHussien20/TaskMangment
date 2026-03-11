using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Text.Json;
using TaskMangment.Application.ApiRequests.Area;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Caching;
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
        private readonly ICacheInvalidator _cacheInvalidator;

        public AreaService(
            IRepository<Area> areaRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Branch> branchRepository,
            IRepository<Company> companyRepo,
            IMapper mapper,
            ICachingService cache,
            ICacheInvalidator cacheInvalidator)
        {
            _areaRepository = areaRepository;
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
            _companyRepo = companyRepo;
            _mapper = mapper;
            _cache = cache;
            _cacheInvalidator = cacheInvalidator;
        }

        private async Task<int> GetVersionAsync(string versionKey)
        {
            var v = await _cache.GetAsync<int>(versionKey);
            if (v <= 0)
            {
                await _cache.SetAsync(versionKey, 1, TimeSpan.FromDays(30));
                return 1;
            }
            return v;
        }
        public async Task<ApiResponse<PagedResponse<AreaGetDto>>> GetAllAsync(AreaRequest request, int CompanyId)
        {
            var version = await GetVersionAsync(CacheKeys.AreasVersion(CompanyId));
            var cacheKey = CacheKeys.AreasList(CompanyId, request, version);

            var response = await _cache.GetOrSetAsync<PagedResponse<AreaGetDto>>(
                cacheKey,
                async () =>
                {
                    var query = _areaRepository.GetAll()
                        .Include(a => a.Manager)
                        .ApplySearch(request.searchKey);

                    var totalCount = await query.CountAsync();

                    query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

                    var areas = await query
                        .Skip((request.PageIndex - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToListAsync();

                    var areaIds = areas.Select(a => a.Id).ToList();

                    var branchCounts = await _branchRepository
                        .GetAll(b => areaIds.Contains(b.AreaId ?? 0))
                        .GroupBy(b => b.AreaId)
                        .Select(g => new { AreaId = g.Key, Count = g.Count() })
                        .ToListAsync();

                    var dtoList = _mapper.Map<List<AreaGetDto>>(areas);

                    foreach (var dto in dtoList)
                    {
                        dto.BranchCount = branchCounts
                            .FirstOrDefault(x => x.AreaId == dto.Id)?.Count ?? 0;

                        dto.ManagerName = areas
                            .FirstOrDefault(a => a.Id == dto.Id)?.Manager?.FullName;
                    }

                    return new PagedResponse<AreaGetDto>(
                        dtoList, totalCount, request.PageIndex, request.PageSize);
                },
                TimeSpan.FromMinutes(2)
            );

            return ApiResponse<PagedResponse<AreaGetDto>>.Ok(response);
        }





        public async Task<ApiResponse<AreaGetDto>> GetByIdAsync(int id)
        {
            var area = await _areaRepository.GetAll(a => a.Id == id)
                .Include(a => a.Manager)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (area == null)
                throw new AppException(
                    ErrorCodes.AreaNotFound,
                    StatusCodes.Status404NotFound);

            var dto = _mapper.Map<AreaGetDto>(area);
            dto.BranchCount = await _branchRepository.CountAsync(b => b.AreaId == id);
            dto.ManagerName = area.Manager?.FullName;

            return ApiResponse<AreaGetDto>.Ok(dto);
        }


        public async Task<ApiResponse<AreaGetDto>> AddAsync( AreaAddEditDto dto, int companyId,int createdBy) 
        {
            if (!await _employeeRepository.IsExistAsync(dto.ManagerEmployeeId))
                throw new AppException(
                    ErrorCodes.ManagerNotFound,
                    StatusCodes.Status404NotFound);

            if (!await _companyRepo.IsExistAsync(companyId))
                throw new AppException(
                    ErrorCodes.CompanyNotFound,
                    StatusCodes.Status404NotFound);

            //var isManagerUsed = await _areaRepository
            //    .GetAll(a => a.ManagerEmployeeId == dto.ManagerEmployeeId)
            //    .AnyAsync();

            //if (isManagerUsed)
            //    throw new AppException(
            //        ErrorCodes.AlreadyAssigned,StatusCodes.Status400BadRequest);

            var area = _mapper.Map<Area>(dto);
            area.CompanyId = companyId;
             

            await _areaRepository.AddAsync(area);
            await _areaRepository.SaveChangesAsync();

            //await _cache.RemoveAsync("areas:");
            await _cacheInvalidator.InvalidateAreasAsync(companyId);

            var fullArea = await _areaRepository.GetAll()
                .Include(a => a.Manager)
                .FirstOrDefaultAsync(a => a.Id == area.Id);

            var areaDto = _mapper.Map<AreaGetDto>(fullArea);

            return ApiResponse<AreaGetDto>.Ok(areaDto, "Area added successfully");
        }



        public async Task<ApiResponse<AreaGetDto>> UpdateAsync(int id, AreaAddEditDto dto)
        {
            var area = await _areaRepository.GetByIDAsync(id);
            if (area == null)
                throw new AppException(
                    ErrorCodes.AreaNotFound,
                    StatusCodes.Status404NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ManagerEmployeeId))
                throw new AppException(
                    ErrorCodes.ManagerNotFound,
                    StatusCodes.Status404NotFound);

            //var isManagerUsed = await _areaRepository
            //    .GetAll(a => a.ManagerEmployeeId == dto.ManagerEmployeeId && a.Id != id)
            //    .AnyAsync();

            //if (isManagerUsed)
            //    throw new AppException(
            //        ErrorCodes.AlreadyAssigned,
            //        StatusCodes.Status400BadRequest);

            _mapper.Map(dto, area);

            await _areaRepository.SaveChangesAsync();
            //await _cache.RemoveAsync("areas:");

            var companyId = area.CompanyId;
            await _cacheInvalidator.InvalidateAreasAsync(companyId);

            var fullArea = await _areaRepository.GetAll()
                .Include(a => a.Manager)
                .FirstOrDefaultAsync(a => a.Id == area.Id);

            var areaDto = _mapper.Map<AreaGetDto>(fullArea);

            return ApiResponse<AreaGetDto>.Ok(areaDto, "Area updated successfully");
        }



        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var area = await _areaRepository.GetByIDAsync(id);
            if (area == null)
                throw new AppException(
                    ErrorCodes.AreaNotFound,
                    StatusCodes.Status404NotFound);

            _areaRepository.SoftDelete(area);
            await _areaRepository.SaveChangesAsync();

            var companyId = area.CompanyId;
            await _cacheInvalidator.InvalidateAreasAsync(companyId);
            return ApiResponse<bool>.Ok(true, "Area deleted successfully");
        }
    }
}

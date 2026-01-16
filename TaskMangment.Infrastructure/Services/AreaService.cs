using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ApiResponse<PagedResponse<AreaGetDto>>> GetAllAsync(AreaRequest request, int CompanyId)
        {
            //string cacheKey = $"areas:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}:{CompanyId}";

            //if (!request.BypassCache)
            //{
            //    var cached = await _cache.GetAsync<PagedResponse<AreaGetDto>>(cacheKey);
            //    if (cached != null)
            //        return ApiResponse<PagedResponse<AreaGetDto>>.Ok(cached);
            //}

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
             
            var response = new PagedResponse<AreaGetDto>(
                dtoList, totalCount, request.PageIndex, request.PageSize);
             
            //await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

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

            var isManagerUsed = await _areaRepository
                .GetAll(a => a.ManagerEmployeeId == dto.ManagerEmployeeId)
                .AnyAsync();

            if (isManagerUsed)
                throw new AppException(
                    ErrorCodes.AlreadyAssigned,StatusCodes.Status400BadRequest);

            var area = _mapper.Map<Area>(dto);
            area.CompanyId = companyId;
             

            await _areaRepository.AddAsync(area);
            await _areaRepository.SaveChangesAsync();

            await _cache.RemoveAsync("areas:");

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

            var isManagerUsed = await _areaRepository
                .GetAll(a => a.ManagerEmployeeId == dto.ManagerEmployeeId && a.Id != id)
                .AnyAsync();

            if (isManagerUsed)
                throw new AppException(
                    ErrorCodes.AlreadyAssigned,
                    StatusCodes.Status400BadRequest);

            _mapper.Map(dto, area);

            await _areaRepository.SaveChangesAsync();
            await _cache.RemoveAsync("areas:");

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
            await _cache.RemoveAsync("areas:");

            return ApiResponse<bool>.Ok(true, "Area deleted successfully");
        }
    }
}

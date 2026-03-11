using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Branch;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Caching;
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
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;
        private readonly IUserAccessContextProvider _accessProvider;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly ICacheInvalidator _cacheInvalidator;





        public BranchService(
            IRepository<Branch> branchRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Area> areaRepository,
            IRepository<Company> companyRepository,
            IMapper mapper,
            ICachingService cache,
            IRepository<ManagerBranches> managerBranchesRepo,
            IUserAccessContextProvider accessProvider,
            IRepository<Employee> employeeRepo,
            ICacheInvalidator cacheInvalidator
)
        {
            _branchRepository = branchRepository;
            _employeeRepository = employeeRepository;
            _areaRepository = areaRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
            _cache = cache;
            _managerBranchesRepo = managerBranchesRepo;
            _accessProvider = accessProvider;
            _employeeRepo = employeeRepo;
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

        public async Task<ApiResponse<PagedResponse<BranchGetDto>>> GetAllAsync( BranchRequest request,int employeeId,int roleLevel, int companyId)
        {
            var version = await GetVersionAsync(CacheKeys.BranchesVersion(companyId));
            var cacheKey = CacheKeys.BranchesList(companyId, employeeId, roleLevel, request, version);

            var response = await _cache.GetOrSetAsync<PagedResponse<BranchGetDto>>(
                cacheKey,
                async () =>
                {
                    var access = await _accessProvider.GetAsync(employeeId);

                    var query = _branchRepository
                        .GetAll(b => b.CompanyId == companyId && !b.IsDeleted)
                        .Include(b => b.Manager)
                        .Include(b => b.Responsible)
                        .Include(b => b.Area)
                        .ApplySearch(request.searchKey)
                        .ApplyAccessScope(access);

                    var totalCount = await query.CountAsync();

                    query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

                    var list = await query
                        .Skip((request.PageIndex - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToListAsync();

                    var dtos = _mapper.Map<ICollection<BranchGetDto>>(list);

                    return new PagedResponse<BranchGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);
                },
                TimeSpan.FromMinutes(2)
            );

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
                    throw new AppException(
                        ErrorCodes.BranchNotFound,
                        StatusCodes.Status404NotFound);

            var dto = _mapper.Map<BranchGetDto>(branch);
            return ApiResponse<BranchGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<BranchGetDto>> AddAsync(BranchAddEditDto dto, int CampanyId)
        {
            if (!await _employeeRepository.IsExistAsync(dto.ManagerId))
                throw new AppException(ErrorCodes.ManagerNotFound, StatusCodes.Status404NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ResponsibleId))
                throw new AppException(ErrorCodes.ManagerNotFound, StatusCodes.Status404NotFound);

            if (dto.AreaId.HasValue)
            {
                if (!await _areaRepository.IsExistAsync(dto.AreaId.Value))
                    throw new AppException(ErrorCodes.AreaNotFound, StatusCodes.Status404NotFound);
            }

            if (!await _companyRepository.IsExistAsync(CampanyId))
                throw new AppException(ErrorCodes.CompanyNotFound, StatusCodes.Status404NotFound);

            var existingManager = await _branchRepository.GetAll()
                .AnyAsync(b => b.ManagerID == dto.ManagerId || b.ResponsibleID == dto.ResponsibleId);

            if (existingManager)
                throw new AppException(ErrorCodes.AlreadyAssigned, StatusCodes.Status400BadRequest);

            var branch = _mapper.Map<Branch>(dto);
            branch.CompanyId = CampanyId;

            await _branchRepository.AddAsync(branch);
            await _branchRepository.SaveChangesAsync();

            await _cacheInvalidator.InvalidateBranchAsync(CampanyId);
            await _cacheInvalidator.InvalidateDashboardAsync(CampanyId);


            var managerBranch = new ManagerBranches
            {
                ManagerId = dto.ManagerId,
                BranchId = branch.Id,
                IsActive = true
            };

            await _managerBranchesRepo.AddAsync(managerBranch);
            await _managerBranchesRepo.SaveChangesAsync();

            //await _cache.RemoveAsync("branches:");

            var branchFull = await _branchRepository.GetAll()
                .Include(b => b.Manager)
                .Include(b => b.Responsible)
                .Include(b => b.Area)
                .FirstOrDefaultAsync(b => b.Id == branch.Id);

            var branchdto = _mapper.Map<BranchGetDto>(branchFull);

            return ApiResponse<BranchGetDto>.Ok(branchdto, "Branch added successfully");
        }


        public async Task<ApiResponse<BranchGetDto>> UpdateAsync(int id, BranchAddEditDto dto)
        {
            var branch = await _branchRepository.GetByIDAsync(id);
            if (branch == null)
                throw new AppException(ErrorCodes.BranchNotFound, StatusCodes.Status404NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ManagerId))
                throw new AppException(ErrorCodes.ManagerNotFound, StatusCodes.Status404NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ResponsibleId))
                throw new AppException(ErrorCodes.ManagerNotFound, StatusCodes.Status404NotFound);

            if (dto.AreaId.HasValue)
            {
                if (!await _areaRepository.IsExistAsync(dto.AreaId.Value))
                    throw new AppException(ErrorCodes.AreaNotFound, StatusCodes.Status404NotFound);
            }

            var existingManager = await _branchRepository.GetAll()
                .AnyAsync(b =>
                    b.Id != id &&
                    (b.ManagerID == dto.ManagerId || b.ResponsibleID == dto.ResponsibleId));

            if (existingManager)
                throw new AppException(ErrorCodes.AlreadyAssigned, StatusCodes.Status400BadRequest);

            var oldManagerId = branch.ManagerID;

            _mapper.Map(dto, branch);
            await _branchRepository.SaveChangesAsync();

            if (oldManagerId != dto.ManagerId)
            {
                var oldLink = await _managerBranchesRepo.GetAll()
                    .FirstOrDefaultAsync(x =>
                        x.BranchId == branch.Id &&
                        x.ManagerId == oldManagerId);

                if (oldLink != null)
                    oldLink.IsActive = false;

                var newLink = await _managerBranchesRepo.GetAll()
                    .FirstOrDefaultAsync(x =>
                        x.BranchId == branch.Id &&
                        x.ManagerId == dto.ManagerId);

                if (newLink == null)
                {
                    await _managerBranchesRepo.AddAsync(new ManagerBranches
                    {
                        BranchId = branch.Id,
                        ManagerId = dto.ManagerId,
                        IsActive = true
                    });
                }
                else
                {
                    newLink.IsActive = true;
                }

                var otherLinks = await _managerBranchesRepo.GetAll()
                    .Where(x => x.BranchId == branch.Id && x.ManagerId != dto.ManagerId)
                    .ToListAsync();

                foreach (var link in otherLinks)
                    link.IsActive = false;
            }

            await _managerBranchesRepo.SaveChangesAsync();

            //await _cache.RemoveAsync("branches:");
                await _cacheInvalidator.InvalidateBranchAsync(branch.CompanyId);

            var branchFull = await _branchRepository.GetAll()
                .Include(b => b.Manager)
                .Include(b => b.Responsible)
                .Include(b => b.Area)
                .FirstOrDefaultAsync(b => b.Id == branch.Id);

            var branchdto = _mapper.Map<BranchGetDto>(branchFull);

            return ApiResponse<BranchGetDto>.Ok(branchdto, "Branch updated successfully");
        }


        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var branch = await _branchRepository.GetByIDAsync(id);
            if (branch == null)
                throw new AppException(
                    ErrorCodes.BranchNotFound,
                    StatusCodes.Status404NotFound);

            _branchRepository.SoftDelete(branch);
            await _branchRepository.SaveChangesAsync();
            //await _cache.RemoveAsync("branches:");

                await _cacheInvalidator.InvalidateBranchAsync(branch.CompanyId);
    
                var managerBranches = await _managerBranchesRepo.GetAll()
                    .Where(x => x.BranchId == id)
                    .ToListAsync();
    
                foreach (var mb in managerBranches)
                {
                    mb.IsActive = false;

                }
    
                await _managerBranchesRepo.SaveChangesAsync();


            // TODO: Optional: Invalidate cache

            return ApiResponse<bool>.Ok(true, "Branch deleted successfully");
        }
    }
}


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
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;


        public BranchService(
            IRepository<Branch> branchRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Area> areaRepository,
            IRepository<Company> companyRepository,
            IMapper mapper,
            ICachingService cache,
            IRepository<ManagerBranches> managerBranchesRepo)
        {
            _branchRepository = branchRepository;
            _employeeRepository = employeeRepository;
            _areaRepository = areaRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
            _cache = cache;
            _managerBranchesRepo = managerBranchesRepo;
        }

        public async Task<ApiResponse<PagedResponse<BranchGetDto>>> GetAllAsync(
    BranchRequest request,
    int employeeId,
    int roleLevel,int companyId)
        {
            var query = _branchRepository.GetAll()
                .Include(b => b.Manager)
                .Include(b => b.Responsible)
                .Include(b => b.Area)
                .ApplySearch(request.searchKey);

            query = query.Where(b => b.CompanyId == companyId && !b.IsDeleted);


            if (roleLevel == 80)
            {
                var myBranchIds = await _managerBranchesRepo
                    .GetAll(x => x.ManagerId == employeeId && !x.IsDeleted)
                    .Select(x => x.BranchId)
                    .Distinct()
                    .ToListAsync();

                if (!myBranchIds.Any())
                {
                    query = query.Where(b => false);
                }
                else
                {
                    query = query.Where(b => myBranchIds.Contains(b.Id));
                }
            }

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<BranchGetDto>>(list);

            var response = new PagedResponse<BranchGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

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
                throw new AppException(
                    ErrorCodes.ManagerNotFound,
                    StatusCodes.Status404NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ResponsibleId))
                throw new AppException(
                    ErrorCodes.ManagerNotFound,
                    StatusCodes.Status404NotFound);

            if (dto.AreaId.HasValue)
            {
                if (!await _areaRepository.IsExistAsync(dto.AreaId.Value))
                    throw new AppException(
                        ErrorCodes.AreaNotFound,
                        StatusCodes.Status404NotFound);
            }


            if (!await _companyRepository.IsExistAsync(CampanyId))
                throw new AppException(
                    ErrorCodes.CompanyNotFound,
                    StatusCodes.Status404NotFound);

            var existingManager = await _branchRepository.GetAll().AnyAsync(b => b.ManagerID == dto.ManagerId || b.ResponsibleID == dto.ResponsibleId);
            if (existingManager)
                throw new AppException(ErrorCodes.AlreadyAssigned, StatusCodes.Status400BadRequest);


            var branch = _mapper.Map<Branch>(dto);
            branch.CompanyId = CampanyId;

            await _branchRepository.AddAsync(branch);
            await _branchRepository.SaveChangesAsync();
            await _cache.RemoveAsync("branches:");


            var branchFull = await _branchRepository.GetAll()
    .Include(b => b.Manager)
    .Include(b => b.Responsible)
    .Include(b => b.Area)
    .FirstOrDefaultAsync(b => b.Id == branch.Id);

            var branchdto = _mapper.Map<BranchGetDto>(branchFull);

            // TODO: Optional: Clear branch cache pattern
            // await _cache.RemoveByPatternAsync("branches-");

            return ApiResponse<BranchGetDto>.Ok(branchdto, "Branch added successfully");
        }

        public async Task<ApiResponse<BranchGetDto>> UpdateAsync(int id, BranchAddEditDto dto)
        {
            var branch = await _branchRepository.GetByIDAsync(id);
            if (branch == null)
                throw new AppException(
                    ErrorCodes.BranchNotFound,
                    StatusCodes.Status404NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ManagerId))
                throw new AppException(
                    ErrorCodes.ManagerNotFound,
                    StatusCodes.Status404NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ResponsibleId))
                throw new AppException(
                    ErrorCodes.ManagerNotFound,
                    StatusCodes.Status404NotFound);

            if (dto.AreaId.HasValue)
            {
                if (!await _areaRepository.IsExistAsync(dto.AreaId.Value))
                    throw new AppException(
                        ErrorCodes.AreaNotFound,
                        StatusCodes.Status404NotFound);
            }



            _mapper.Map(dto, branch);

            await _branchRepository.SaveChangesAsync();
            await _cache.RemoveAsync("branches:");

            var branchFull = await _branchRepository.GetAll()
   .Include(b => b.Manager)
   .Include(b => b.Responsible)
   .Include(b => b.Area)
   .FirstOrDefaultAsync(b => b.Id == branch.Id);
            var branchdto = _mapper.Map<BranchGetDto>(branch);

            // TODO: Optional: Invalidate cache

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
            await _cache.RemoveAsync("branches:");


            // TODO: Optional: Invalidate cache

            return ApiResponse<bool>.Ok(true, "Branch deleted successfully");
        }
    }
}


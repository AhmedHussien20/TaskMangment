using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class EmployeeTypeService : IEmployeeTypeService
    {
        private readonly IRepository<EmployeeType> _repo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<EmployeeFunctionalScope> _scopeRepo;
        private readonly IMapper _mapper;

        public EmployeeTypeService(
            IRepository<EmployeeType> repo,
            IRepository<Employee> employeeRepo,
            IRepository<EmployeeFunctionalScope> scopeRepo,
            IMapper mapper)
        {
            _repo = repo;
            _employeeRepo = employeeRepo;
            _scopeRepo = scopeRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResponse<EmployeeTypeGetDto>>> GetAllAsync(EmployeeTypeRequest request)
        {
            var query = _repo.GetAll().ApplySearch(request.searchKey);
            var total = await query.CountAsync();
            query = query.OrderByDynamicSafe(request.SortColumn ?? "Id", request.SortDirection ?? "ASC");

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var ids = list.Select(x => x.Id).ToList();
            var counts = await _employeeRepo.GetAll(e => ids.Contains(e.EmployeeTypeId))
                .GroupBy(e => e.EmployeeTypeId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();
            var dict = counts.ToDictionary(x => x.Key, x => x.Count);

            var dtos = list.Select(x =>
            {
                var dto = _mapper.Map<EmployeeTypeGetDto>(x);
                dto.EmployeeCount = dict.TryGetValue(x.Id, out var c) ? c : 0;
                return dto;
            }).ToList();

            return ApiResponse<PagedResponse<EmployeeTypeGetDto>>.Ok(
                new PagedResponse<EmployeeTypeGetDto>(dtos, total, request.PageIndex, request.PageSize));
        }

        public async Task<ApiResponse<List<FunctionCodeEnumDto>>> GetLookupAsync()
        {
            var list = await _repo.GetAll()
                .OrderBy(x => x.Id)
                .Select(x => new FunctionCodeEnumDto
                {
                    Id = x.Id,
                    Name = x.NameEn,
                    SeesAllTypesInBranchScope = x.SeesAllTypesInBranchScope
                })
                .ToListAsync();

            return ApiResponse<List<FunctionCodeEnumDto>>.Ok(list);
        }

        public async Task<ApiResponse<EmployeeTypeGetDto>> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIDAsync(id);
            if (entity == null || entity.IsDeleted)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var dto = _mapper.Map<EmployeeTypeGetDto>(entity);
            dto.EmployeeCount = await _employeeRepo.GetAll(e => e.EmployeeTypeId == id).CountAsync();
            return ApiResponse<EmployeeTypeGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<EmployeeTypeGetDto>> AddAsync(EmployeeTypeAddEditDto dto)
        {
            var code = dto.Code.Trim();
            if (await _repo.GetAll(x => x.Code == code).AnyAsync())
                throw new AppException(ErrorCodes.AlreadyAssigned, StatusCodes.Status409Conflict);

            var entity = _mapper.Map<EmployeeType>(dto);
            entity.Code = code;
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
            return ApiResponse<EmployeeTypeGetDto>.Ok(_mapper.Map<EmployeeTypeGetDto>(entity));
        }

        public async Task<ApiResponse<EmployeeTypeGetDto>> UpdateAsync(int id, EmployeeTypeAddEditDto dto)
        {
            var entity = await _repo.GetByIDAsync(id);
            if (entity == null || entity.IsDeleted)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var code = dto.Code.Trim();
            if (await _repo.GetAll(x => x.Code == code && x.Id != id).AnyAsync())
                throw new AppException(ErrorCodes.AlreadyAssigned, StatusCodes.Status409Conflict);

            entity.Code = code;
            entity.NameEn = dto.NameEn.Trim();
            entity.NameAr = dto.NameAr.Trim();
            entity.SeesAllTypesInBranchScope = dto.SeesAllTypesInBranchScope;
            await _repo.SaveChangesAsync();
            return ApiResponse<EmployeeTypeGetDto>.Ok(_mapper.Map<EmployeeTypeGetDto>(entity));
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIDAsync(id);
            if (entity == null || entity.IsDeleted)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (await _employeeRepo.GetAll(e => e.EmployeeTypeId == id).AnyAsync())
                throw new AppException(ErrorCodes.InvalidOperation, StatusCodes.Status400BadRequest,
                    "Cannot delete employee type while employees are assigned to it.");

            if (await _scopeRepo.GetAll(s => s.EmployeeTypeId == id && !s.IsDeleted).AnyAsync())
                throw new AppException(ErrorCodes.InvalidOperation, StatusCodes.Status400BadRequest,
                    "Cannot delete employee type while used in manager scopes.");

            _repo.SoftDelete(entity);
            await _repo.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true);
        }
    }
}

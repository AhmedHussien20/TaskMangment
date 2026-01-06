using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore; 
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class LeaveTypeService : ILeaveTypeService
    {
        private readonly IRepository<LeaveType> _repo;
        private readonly IRepository<Company> _companyRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public LeaveTypeService(
            IRepository<LeaveType> repo,
            IRepository<Company> companyRepo,
            IMapper mapper,
            ICachingService cache)
        {
            _repo = repo;
            _companyRepo = companyRepo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<List<LeaveTypeGetDto>>> GetAllAsync()
        {
            var list = await _repo.GetAll().OrderByDescending(x => x.Id).ToListAsync();

            var dtos = _mapper.Map<List<LeaveTypeGetDto>>(list);

            return ApiResponse<List<LeaveTypeGetDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<LeaveTypeGetDto>> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIDAsync(id);

            if (entity == null)
                throw new AppException(
                    ErrorCodes.NotFound,
                    StatusCodes.Status404NotFound);

            var dto = _mapper.Map<LeaveTypeGetDto>(entity);

            return ApiResponse<LeaveTypeGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<LeaveTypeGetDto>> AddAsync(LeaveTypeAddEditDto dto, int companyId, int createdBy)
        {
            if (!await _companyRepo.IsExistAsync(companyId))
                throw new AppException(
                    ErrorCodes.CompanyNotFound,
                    StatusCodes.Status404NotFound);

            var isExists = await _repo
                .GetAll(x => x.NameAr == dto.NameAr)
                .AnyAsync();

            if (isExists)
                throw new AppException(
                    ErrorCodes.AlreadyExists,
                    StatusCodes.Status400BadRequest);

            var entity = _mapper.Map<LeaveType>(dto);

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            await _cache.RemoveAsync("leavetypes:");

            var resultDto = _mapper.Map<LeaveTypeGetDto>(entity);

            return ApiResponse<LeaveTypeGetDto>.Ok(resultDto, "Leave type added successfully");
        }

        public async Task<ApiResponse<LeaveTypeGetDto>> UpdateAsync(int id, LeaveTypeAddEditDto dto)
        {
            var entity = await _repo.GetByIDAsync(id);

            if (entity == null)
                throw new AppException(
                    ErrorCodes.NotFound,
                    StatusCodes.Status404NotFound);

            _mapper.Map(dto, entity);

            await _repo.SaveChangesAsync();
            await _cache.RemoveAsync("leavetypes:");

            var resultDto = _mapper.Map<LeaveTypeGetDto>(entity);

            return ApiResponse<LeaveTypeGetDto>.Ok(resultDto, "Leave type updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIDAsync(id);

            if (entity == null)
                throw new AppException(
                    ErrorCodes.NotFound,
                    StatusCodes.Status404NotFound);

            _repo.SoftDelete(entity);
            await _repo.SaveChangesAsync();

            await _cache.RemoveAsync("leavetypes:");

            return ApiResponse<bool>.Ok(true, "Leave type deleted successfully");
        }
    }


}

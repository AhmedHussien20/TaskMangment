using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class AreaService: IAreaService
    {
        private readonly IRepository<Area> _areaRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Branch> _branchRepository;
        private readonly IMapper _mapper;

        public AreaService(
            IRepository<Area> areaRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Branch> branchRepository,
            IMapper mapper)
        {
            _areaRepository = areaRepository;
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ICollection<AreaGetDto>>> GetAllAsync()
        {
            var areas = await _areaRepository.GetAll()
                .Include(a => a.Manager)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<AreaGetDto>>(areas);

            foreach (var dto in dtos)
            {
                dto.BranchCount = await _branchRepository.CountAsync(b => b.AreaId == dto.Id);
            }

            return ApiResponse<ICollection<AreaGetDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<AreaGetDto>> GetByIdAsync(int id)
        {
            var area = await _areaRepository.GetAll(a => a.Id == id)
                .Include(a => a.Manager).AsNoTracking()
                .FirstOrDefaultAsync();

            if (area == null)
                return ApiResponse<AreaGetDto>.Fail("Area not found", StatusCode.NotFound);

            var dto = _mapper.Map<AreaGetDto>(area);
            dto.BranchCount = await _branchRepository.CountAsync(b => b.AreaId == id);

            return ApiResponse<AreaGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(AreaAddEditDto dto)
        {
            if (!await _employeeRepository.IsExistAsync(dto.ManagerID))
                return ApiResponse<bool>.Fail("Manager not found", StatusCode.NotFound);

            var area = _mapper.Map<Area>(dto);

            await _areaRepository.AddAsync(area);
            await _areaRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Area added successfully");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, AreaAddEditDto dto)
        {
            var area = await _areaRepository.GetByIDAsync(id);
            if (area == null)
                return ApiResponse<bool>.Fail("Area not found", StatusCode.NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ManagerID))
                return ApiResponse<bool>.Fail("Manager not found", StatusCode.NotFound);

            _mapper.Map(dto, area);

            await _areaRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Area updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var area = await _areaRepository.GetByIDAsync(id);
            if (area == null)
                return ApiResponse<bool>.Fail("Area not found", StatusCode.NotFound);

            _areaRepository.Delete(area);
            await _areaRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Area deleted successfully");
        }
    }
}


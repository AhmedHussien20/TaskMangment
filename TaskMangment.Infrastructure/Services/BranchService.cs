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
    public class BranchService : IBranchService
    {
        private readonly IRepository<Branch> _branchRepository;
        private readonly IRepository<Area> _areaRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IMapper _mapper;

        public BranchService(
            IRepository<Branch> branchRepository,
            IRepository<Area> areaRepository,
            IRepository<Employee> employeeRepository,
            IMapper mapper)
        {
            _branchRepository = branchRepository;
            _areaRepository = areaRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ICollection<BranchGetDto>>> GetAllAsync()
        {
            var branches = await _branchRepository.GetAll()
                  .Include(b => b.Area)
                  .Include(b => b.Manager)
                  .Include(b => b.Responsible)
                  .ToListAsync();

            var dtos = branches.Select(branch => new BranchGetDto
            {
                Id = branch.Id,
                Name = branch.Name,
                AreaName = branch.Area?.Name,
                ManagerID = branch.Manager?.Id ?? 0,
                ResponsibleID = branch.Responsible?.Id ?? 0
            }).ToList();

            return ApiResponse<ICollection<BranchGetDto>>.Ok(dtos);
        }
        public async Task<ApiResponse<BranchGetDto>> GetByIdAsync(int id)
        {
            var branch = await _branchRepository.GetByIDAsync(id);
            if (branch == null)
                return ApiResponse<BranchGetDto>.Fail("Branch not found", StatusCode.NotFound);

            var dto = _mapper.Map<BranchGetDto>(branch);

            var area = branch.AreaId.HasValue ? await _areaRepository.GetByIDAsync(branch.AreaId.Value) : null;
            var manager = await _employeeRepository.GetByIDAsync(branch.ManagerID);
            var responsible = await _employeeRepository.GetByIDAsync(branch.ResponsibleID);

            dto.AreaName = area?.Name;
            dto.ManagerID = manager?.Id ?? 0;
            dto.ResponsibleID = responsible?. Id ?? 0;

            return ApiResponse<BranchGetDto>.Ok(dto);
        }

        // ----------------- ADD -----------------
        public async Task<ApiResponse<bool>> AddAsync(BranchAddEditDto dto)
        {
            Area area = null;
            if (!string.IsNullOrEmpty(dto.Area))
            {
                var allAreas = await _areaRepository.GetAll().ToListAsync();
                area = allAreas.FirstOrDefault(a => a.Name == dto.Area);
                if (area == null)
                    return ApiResponse<bool>.Fail("Area not found", StatusCode.NotFound);
            }

            if (!await _employeeRepository.IsExistAsync(dto.ManagerID))
                return ApiResponse<bool>.Fail("Manager not found", StatusCode.NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ResponsibleID))
                return ApiResponse<bool>.Fail("Responsible employee not found", StatusCode.NotFound);

            var branch = _mapper.Map<Branch>(dto);
            branch.AreaId = area?.Id;

            await _branchRepository.AddAsync(branch);
            await _branchRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Branch added successfully");
        }

        // ----------------- UPDATE -----------------
        public async Task<ApiResponse<bool>> UpdateAsync(int id, BranchAddEditDto dto)
        {
            var branch = await _branchRepository.GetByIDAsync(id);
            if (branch == null)
                return ApiResponse<bool>.Fail("Branch not found", StatusCode.NotFound);

            Area area = null;
            if (!string.IsNullOrEmpty(dto.Area))
            {
                var allAreas = await _areaRepository.GetAll().ToListAsync();
                area = allAreas.FirstOrDefault(a => a.Name == dto.Area);
            }

            if (!await _employeeRepository.IsExistAsync(dto.ManagerID))
                return ApiResponse<bool>.Fail("Manager not found", StatusCode.NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.ResponsibleID))
                return ApiResponse<bool>.Fail("Responsible employee not found", StatusCode.NotFound);

            _mapper.Map(dto, branch);
            branch.AreaId = area?.Id;

            await _branchRepository.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Branch updated successfully");
        }

        // ----------------- DELETE -----------------
        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var branch = await _branchRepository.GetByIDAsync(id);
            if (branch == null)
                return ApiResponse<bool>.Fail("Branch not found", StatusCode.NotFound);

            _branchRepository.SoftDelete(branch);
            await _branchRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Branch deleted successfully");
        }

       
    }
}

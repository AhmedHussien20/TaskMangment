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
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Branch> _branchRepository;

        private readonly IMapper _mapper;

        public EmployeeService(IRepository<Employee> employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ICollection<EmployeeGetDto>>> GetAllAsync()
        {
            var employees = await _employeeRepository.GetAll().ToListAsync();
            var dtos = _mapper.Map<ICollection<EmployeeGetDto>>(employees);
            return ApiResponse<ICollection<EmployeeGetDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<EmployeeGetDto>> GetByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIDAsync(id);
            if (employee == null)
                return ApiResponse<EmployeeGetDto>.Fail("Employee not found", StatusCode.NotFound);

            var dto = _mapper.Map<EmployeeGetDto>(employee);
            return ApiResponse<EmployeeGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(EmployeeAddEditDto dto)
        {
            if (!await _branchRepository.IsExistAsync(dto.BranchId))
                return ApiResponse<bool>.Fail("Manager not found", StatusCode.NotFound);

            //department check can be added here dont forget

            var employee = _mapper.Map<Employee>(dto);
            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Employee added successfully");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, EmployeeAddEditDto dto)
        {
            if (!await _branchRepository.IsExistAsync(dto.BranchId))
                return ApiResponse<bool>.Fail("Manager not found", StatusCode.NotFound);

            //department check can be added here dont forget

            var employee = await _employeeRepository.GetByIDAsync(id);
            if (employee == null)
                return ApiResponse<bool>.Fail("Employee not found", StatusCode.NotFound);

            _mapper.Map(dto, employee);
            await _employeeRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Employee updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var employee = await _employeeRepository.GetByIDAsync(id);
            if (employee == null)
                return ApiResponse<bool>.Fail("Employee not found", StatusCode.NotFound);

            _employeeRepository.Delete(employee);
            await _employeeRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Employee deleted successfully");
        }
    }
}

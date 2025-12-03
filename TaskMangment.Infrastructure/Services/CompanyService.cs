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
using Task = System.Threading.Tasks.Task;

namespace TaskMangment.Infrastructure.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IMapper _mapper;

        public CompanyService(IRepository<Company> companyRepository, IMapper mapper, IRepository<Employee> employeeRepository)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
        }

        public async Task<ApiResponse<ICollection<CompanyGetDto>>> GetAllAsync()
        {
            var companies = await _companyRepository.GetAll()
                .Include(c => c.TechnicalManager)
                .Include(c => c.FinancialManager)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<CompanyGetDto>>(companies);
            return ApiResponse<ICollection<CompanyGetDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<CompanyGetDto>> GetByIdAsync(int id)
        {
            var company = await _companyRepository.GetAll()
                .Include(c => c.TechnicalManager)
                .Include(c => c.FinancialManager)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
                return ApiResponse<CompanyGetDto>.Fail("Company not found", StatusCode.NotFound);

            var dto = _mapper.Map<CompanyGetDto>(company);
            return ApiResponse<CompanyGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(CompanyAddEditDto dto)
        {
            var company = _mapper.Map<Company>(dto);
            if (!await _employeeRepository.IsExistAsync(dto.TechnicalManagerId))
                return ApiResponse<bool>.Fail("Technical Manager not found", StatusCode.NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.FinancialManagerId))
                return ApiResponse<bool>.Fail("Financial Manager not found", StatusCode.NotFound);

            await _companyRepository.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Company added successfully");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, CompanyAddEditDto dto)
        {
            var company = await _companyRepository.GetByIDAsync(id);
            if (company == null)
                return ApiResponse<bool>.Fail("Company not found", StatusCode.NotFound);

            _mapper.Map(dto, company);

            if (!await _employeeRepository.IsExistAsync(dto.TechnicalManagerId))
                return ApiResponse<bool>.Fail("Technical Manager not found", StatusCode.NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.FinancialManagerId))
                return ApiResponse<bool>.Fail("Financial Manager not found", StatusCode.NotFound);

            await _companyRepository.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Company updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var company = await _companyRepository.GetByIDAsync(id);
            if (company == null)
                return ApiResponse<bool>.Fail("Company not found", StatusCode.NotFound);

            _companyRepository.SoftDelete(company);
            await _companyRepository.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Company deleted successfully");
        }
    }
}

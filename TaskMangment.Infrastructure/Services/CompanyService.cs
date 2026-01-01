using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore; 
using TaskMangment.Application.Common.ApiRequests.Company;
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
    public class CompanyService : ICompanyService
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public CompanyService(
            IRepository<Company> companyRepository,
            IRepository<Employee> employeeRepository,
            IMapper mapper,
            ICachingService cache)
        {
            _companyRepository = companyRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<CompanyGetDto>>> GetAllAsync(CompanyRequest request)
        {
            //string cacheKey = $"companies:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            //if (!request.BypassCache)
            //{
            //    var cached = await _cache.GetAsync<PagedResponse<CompanyGetDto>>(cacheKey);
            //    if (cached != null)
            //        return ApiResponse<PagedResponse<CompanyGetDto>>.Ok(cached);
            //}

            var query = _companyRepository.GetAll()
                .Include(c => c.TechnicalManager)
                .Include(c => c.FinancialManager)
                .ApplySearch(request.searchKey);
            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<CompanyGetDto>>(list);

            var response = new PagedResponse<CompanyGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            //await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<CompanyGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<CompanyGetDto>> GetByIdAsync(int id)
        {
            var company = await _companyRepository.GetAll(c => c.Id == id)
                .Include(c => c.TechnicalManager)
                .Include(c => c.FinancialManager)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (company == null)
                throw new AppException(
                                      ErrorCodes.CompanyNotFound,
                                      StatusCodes.Status404NotFound);
            var dto = _mapper.Map<CompanyGetDto>(company);
            return ApiResponse<CompanyGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<CompanyGetDto>> AddAsync(CompanyAddEditDto dto)
        {
            if (dto.TechnicalManagerId.HasValue && !await _employeeRepository.IsExistAsync(dto.TechnicalManagerId.Value))
                throw new AppException(ErrorCodes.ManagerNotFound, StatusCodes.Status404NotFound);

            if (dto.FinancialManagerId.HasValue && !await _employeeRepository.IsExistAsync(dto.FinancialManagerId.Value))
                throw new AppException(ErrorCodes.ManagerNotFound, StatusCodes.Status404NotFound);

            var company = _mapper.Map<Company>(dto);

            await _companyRepository.AddAsync(company);
            await _companyRepository.SaveChangesAsync();
            await _cache.RemoveAsync("companies:");

            var campanydto = _mapper.Map<CompanyGetDto>(company);

            return ApiResponse<CompanyGetDto>.Ok(campanydto, "Company added successfully");
        }

        public async Task<ApiResponse<CompanyGetDto>> UpdateAsync(int id, CompanyAddEditDto dto)
        {
            var company = await _companyRepository.GetByIDAsync(id);
            if (company == null)
                throw new AppException(ErrorCodes.CompanyNotFound, StatusCodes.Status404NotFound);

            if (dto.TechnicalManagerId.HasValue && !await _employeeRepository.IsExistAsync(dto.TechnicalManagerId.Value))
                throw new AppException(ErrorCodes.ManagerNotFound, StatusCodes.Status404NotFound);

            if (dto.FinancialManagerId.HasValue && !await _employeeRepository.IsExistAsync(dto.FinancialManagerId.Value))
                throw new AppException(ErrorCodes.ManagerNotFound, StatusCodes.Status404NotFound);

            _mapper.Map(dto, company);

            await _companyRepository.SaveChangesAsync();
            await _cache.RemoveAsync("companies:");

            var campanydto = _mapper.Map<CompanyGetDto>(company);
            return ApiResponse<CompanyGetDto>.Ok(campanydto, "Company updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var company = await _companyRepository.GetByIDAsync(id);
            if (company == null)
                throw new AppException(ErrorCodes.CompanyNotFound, StatusCodes.Status404NotFound);

            _companyRepository.SoftDelete(company);
            await _companyRepository.SaveChangesAsync();
            await _cache.RemoveAsync("companies:");


            return ApiResponse<bool>.Ok(true, "Company deleted successfully");
        }
    }
}

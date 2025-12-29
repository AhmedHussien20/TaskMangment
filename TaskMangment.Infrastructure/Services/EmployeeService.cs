using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Employee;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IRepository<Branch> _branchRepo;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Attachment> _attachmentRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly AppDbContext _db;

        public EmployeeService(
            IRepository<Employee> employeeRepo,
            IRepository<Role> roleRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            IRepository<Branch> branchRepo,
            IRepository<Company> companyRepository,
            IMapper mapper,
            ICachingService cache,
            IRepository<Attachment> attachmentRepo,
            AppDbContext db)
        {
            _employeeRepo = employeeRepo;
            _roleRepo = roleRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _branchRepo = branchRepo;
            _companyRepository = companyRepository;

            _mapper = mapper;
            _cache = cache;
            _attachmentRepo = attachmentRepo;
            _db = db;
        }

        public async Task<ApiResponse<PagedResponse<EmployeeGetDto>>> GetAllAsync(EmployeeRequest request)
        {
            string cacheKey =
                $"employees:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<EmployeeGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<EmployeeGetDto>>.Ok(cached);
            }

            // 1) Base query (Employees)
            var empQuery = _employeeRepo.GetAll()
                .Include(e => e.Branch)
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .ApplySearch(request.searchKey)
                .AsNoTracking();

            var totalCount = await empQuery.CountAsync();

            empQuery = empQuery.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var employees = await empQuery
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var employeeIds = employees.Select(e => e.Id).ToList();

            var images = await _db.Attachments
                .Where(a =>
                    a.AttachmentType == AttachmentType.Employee &&
                    !a.IsDeleted && 
                    employeeIds.Contains(a.ReferenceId))
                .GroupBy(a => a.ReferenceId)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    ImageUrl = g.OrderByDescending(x => x.CreatedDate)
                                .Select(x => x.FilePath)
                                .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.EmployeeId, x => x.ImageUrl);

            var dtos = _mapper.Map<List<EmployeeGetDto>>(employees);

            foreach (var dto in dtos)
            {
                dto.ImageUrl = images.TryGetValue(dto.Id, out var url) ? url : null;
            }

            var response = new PagedResponse<EmployeeGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<EmployeeGetDto>>.Ok(response);
        }


        public async Task<ApiResponse<EmployeeGetDto>> GetByIdAsync(int id)
        {
            var employee = await _employeeRepo.GetAll(e => e.Id == id)
                .Include(e => e.Branch)
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (employee == null)
                return ApiResponse<EmployeeGetDto>.Fail("Employee not found");

            
            var dto = _mapper.Map<EmployeeGetDto>(employee);

            return ApiResponse<EmployeeGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<EmployeeGetDto>> AddAsync(EmployeeAddEditDto dto, int CampanyId)
        {
            if (dto.BranchId.HasValue && !await _branchRepo.IsExistAsync(dto.BranchId.Value))
                throw new AppException(
                                    ErrorCodes.BranchNotFound,
                                    StatusCodes.Status400BadRequest);

            if (!await _companyRepository.IsExistAsync(CampanyId))
                throw new AppException(ErrorCodes.CompanyNotFound, StatusCodes.Status400BadRequest);

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new AppException(ErrorCodes.Invalid, StatusCodes.Status400BadRequest);


            var employee = _mapper.Map<Employee>(dto);
            employee.CompanyId = CampanyId;
            employee.CreatedDate = DateTime.UtcNow;


            var hasher = new PasswordHasher<Employee>();
            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _employeeRepo.AddAsync(employee);
            await _employeeRepo.SaveChangesAsync();

            if (dto.Attachments != null)
            {
                var uploadsRoot = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "comments");

                Directory.CreateDirectory(uploadsRoot);

                var fileName = $"{Guid.NewGuid()}_{dto.Attachments.FileName}";
                var filePath = Path.Combine(uploadsRoot, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Attachments.CopyToAsync(stream);
                }


                var  attachment = new Attachment
                {
                    FileName = dto.Attachments.FileName,
                    FilePath = $"uploads/comments/{fileName}",
                    Size = dto.Attachments.Length,
                    ContentType = dto.Attachments.ContentType,
                    UploadedBy = employee.Id,
                    UploadedAt = DateTime.UtcNow,
                    ReferenceId = employee.Id,
                    AttachmentType = AttachmentType.Employee
                   
                };

                await _attachmentRepo.AddAsync(attachment);
                await _attachmentRepo.SaveChangesAsync();
            }

            await _employeeRepo.SaveChangesAsync();


            // Assign roles
            //foreach (var roleId in dto.RoleIds)
            //{
            //    if (!await _roleRepo.IsExistAsync(roleId))
            //        return ApiResponse<EmployeeGetDto>.Fail($"Role with ID {roleId} not found");

            //    await _employeeRoleRepo.AddAsync(new EmployeeRole
            //    {
            //        EmployeeId = employee.Id,
            //        RoleId = roleId
            //    });
            //}

            //await _employeeRoleRepo.SaveChangesAsync();
            await _cache.RemoveAsync("employees:");

            var fullEmployee = await _employeeRepo.GetAll(e => e.Id == employee.Id)
                                                  .Include(e => e.Branch)
                                                  .Include(e => e.EmployeeRoles)
                                                      .ThenInclude(er => er.Role)
                                                  .FirstOrDefaultAsync();

            var employeeDto = _mapper.Map<EmployeeGetDto>(fullEmployee);

            return ApiResponse<EmployeeGetDto>.Ok(employeeDto, "Employee added successfully");
        }

        public async Task<ApiResponse<EmployeeGetDto>> UpdateAsync(int id, EmployeeAddEditDto dto)
        {
            if (dto.BranchId.HasValue && !await _branchRepo.IsExistAsync(dto.BranchId.Value))
                throw new AppException(
                    ErrorCodes.BranchNotFound,
                    StatusCodes.Status400BadRequest);

            var employee = await _employeeRepo.GetByIDAsync(id);
            if (employee == null)
                throw new AppException(
                    ErrorCodes.EmployeeNotFound,
                    StatusCodes.Status400BadRequest);

            _mapper.Map(dto, employee);

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            if (dto.Attachments != null)
            {
                var oldAttachment = await _attachmentRepo
                    .GetAll(a => a.ReferenceId == employee.Id && a.AttachmentType == AttachmentType.Employee)
                    .FirstOrDefaultAsync();

                if (oldAttachment != null)
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldAttachment.FilePath);
                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);

                    _attachmentRepo.SoftDelete(oldAttachment);
                    await _attachmentRepo.SaveChangesAsync();
                }

                var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "employees");
                Directory.CreateDirectory(uploadsRoot);

                var fileName = $"{Guid.NewGuid()}_{dto.Attachments.FileName}";
                var filePath = Path.Combine(uploadsRoot, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Attachments.CopyToAsync(stream);
                }

                var attachment = new Attachment
                {
                    FileName = dto.Attachments.FileName,
                    FilePath = $"uploads/employees/{fileName}",
                    Size = dto.Attachments.Length,
                    ContentType = dto.Attachments.ContentType,
                    UploadedBy = employee.Id,
                    UploadedAt = DateTime.UtcNow,
                    ReferenceId = employee.Id,
                    AttachmentType = AttachmentType.Employee
                };

                await _attachmentRepo.AddAsync(attachment);
            }
            // -------------------------------------------

            await _employeeRepo.SaveChangesAsync();
            await _cache.RemoveAsync("employees:");

            var fullEmployee = await _employeeRepo.GetAll(e => e.Id == employee.Id)
                .Include(e => e.Branch)
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .FirstOrDefaultAsync();

            var employeeDto = _mapper.Map<EmployeeGetDto>(fullEmployee);
            return ApiResponse<EmployeeGetDto>.Ok(employeeDto, "Employee updated successfully");
        }


        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var employee = await _employeeRepo.GetByIDAsync(id);
            if (employee == null)
                throw new AppException(
                    ErrorCodes.EmployeeNotFound,
                    StatusCodes.Status400BadRequest);
            _employeeRepo.SoftDelete(employee);
            await _employeeRepo.SaveChangesAsync();
            await _cache.RemoveAsync("employees:");


            return ApiResponse<bool>.Ok(true, "Employee deleted successfully");
        }
    }
}

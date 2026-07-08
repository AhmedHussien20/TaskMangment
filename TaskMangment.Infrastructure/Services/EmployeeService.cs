using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Employee;
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
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Persistence.Extensions;
using Attachment = TaskMangment.Domain.Entities.Attachment;

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
        private readonly IWebHostEnvironment _env;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IAppUnitOfWork _uow;
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;
        private readonly IUserAccessContextProvider _accessProvider;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepo;
        private readonly IRepository<WorkTask> _taskRepo;



        public EmployeeService(
            IRepository<Employee> employeeRepo,
            IRepository<Role> roleRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            IRepository<Branch> branchRepo,
            IRepository<Company> companyRepository,
            IMapper mapper,
            ICachingService cache,
            IRepository<Attachment> attachmentRepo,
            IBlobStorageService blobStorageService,
            AppDbContext db,
            IWebHostEnvironment env,
            IAppUnitOfWork uow,
            IRepository<ManagerBranches> managerBranchesRepo,
            IUserAccessContextProvider accessProvider,
            IRepository<TaskAssignment> taskAssignmentRepo,
            IRepository<WorkTask> taskRepo
            )
        {
            _employeeRepo = employeeRepo;
            _roleRepo = roleRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _branchRepo = branchRepo;
            _companyRepository = companyRepository;
            _blobStorageService = blobStorageService;
            _mapper = mapper;
            _cache = cache;
            _attachmentRepo = attachmentRepo;
            _db = db;
            _uow = uow;
            _managerBranchesRepo = managerBranchesRepo;
            _accessProvider = accessProvider;
            _env = env;
            _taskAssignmentRepo = taskAssignmentRepo;
            _taskRepo = taskRepo;
        }

        public async Task<ApiResponse<PagedResponse<EmployeeGetDto>>> GetAllAsync(EmployeeRequest request,int employeeId,int roleLevel) 
        {
            var empQuery = await BuildEmployeeQueryAsync(request, employeeId, roleLevel);

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
                if (images.TryGetValue(dto.Id, out var blobName) &&
                    !string.IsNullOrWhiteSpace(blobName))
                    dto.ImageUrl = _blobStorageService.WithSas(blobName);
                else
                    dto.ImageUrl = null;
            }

            var response = new PagedResponse<EmployeeGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);
            return ApiResponse<PagedResponse<EmployeeGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<List<EmployeeGetDto>>> GetAllForExportAsync(
            EmployeeRequest request,
            int employeeId,
            int roleLevel)
        {
            var empQuery = await BuildEmployeeQueryAsync(request, employeeId, roleLevel);

            empQuery = empQuery.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var employees = await empQuery.ToListAsync();
            var dtos = _mapper.Map<List<EmployeeGetDto>>(employees);

            return ApiResponse<List<EmployeeGetDto>>.Ok(dtos);
        }

        private async Task<IQueryable<Employee>> BuildEmployeeQueryAsync(
            EmployeeRequest request,
            int employeeId,
            int roleLevel)
        {
            var access = await _accessProvider.GetAsync(employeeId);

            var myBranchId = await _employeeRepo.GetAll(e => e.Id == employeeId)
                .Select(e => e.BranchId)
                .FirstOrDefaultAsync();

            if (myBranchId == 0)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var empQuery = _employeeRepo.GetAll()
                .Include(e => e.Branch)
                .Include(e => e.Department)
                .Include(e => e.Job)
                .Include(e => e.EmployeeRoles.Where(er => er.IsAssigned && !er.IsDeleted))
                    .ThenInclude(er => er.Role)
                .ApplySearch(request.searchKey)
                .ApplyAccessScope(access)
                .AsNoTracking();

            if (request.BranchId.HasValue)
                empQuery = empQuery.Where(e => e.BranchId == request.BranchId.Value);

            if (request.IsActive.HasValue)
                empQuery = empQuery.Where(e => e.IsActive == request.IsActive.Value);

            if (!access.BranchIds.Any() && !access.FunctionCodes.Any() && roleLevel != 100 && string.IsNullOrWhiteSpace(request.PermissionCode))
                empQuery = empQuery.Where(e => e.Id == employeeId);

            if (!access.BranchIds.Any() && !access.FunctionCodes.Any() && roleLevel >= 70 && roleLevel < 100)
                empQuery = empQuery.Where(e => e.Id == employeeId);

            if (!string.IsNullOrWhiteSpace(request.PermissionCode) && request.PermissionCode == "CREATE_TASK" && roleLevel < 60)
            {
                empQuery = empQuery
                    .Where(e => e.IsActive)
                    .Where(e => e.BranchId == myBranchId)
                    .Where(e => e.EmployeeRoles.Any(er =>
                        er.IsAssigned &&
                        !er.IsDeleted &&
                        er.Role != null &&
                        er.Role.Level < 60));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(request.PermissionCode) && request.PermissionCode == "CREATE_TASK")
                    empQuery = empQuery.Where(e => e.IsActive);

                if (roleLevel != 100)
                    empQuery = empQuery.ApplyRoleHierarchy(roleLevel);
            }

            return empQuery;
        }



        public async Task<ApiResponse<EmployeeGetDto>> GetByIdAsync(int id)
        {
            var employee = await _employeeRepo.GetAll(e => e.Id == id)
                .Include(e => e.Branch)
                .Include(e => e.Department)
                .Include(e => e.Job)
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (employee == null)
                return ApiResponse<EmployeeGetDto>.Fail("Employee not found");

            
            var dto = _mapper.Map<EmployeeGetDto>(employee);
           // var x = employee.Department.Name;

            return ApiResponse<EmployeeGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<EmployeeGetDto>> AddAsync(EmployeeAddEditDto dto, int CampanyId)
        {
            if (dto.BranchId.HasValue && !await _branchRepo.IsExistAsync(dto.BranchId.Value))
                throw new AppException(ErrorCodes.BranchNotFound, StatusCodes.Status400BadRequest);

            if (!await _companyRepository.IsExistAsync(CampanyId))
                throw new AppException(ErrorCodes.CompanyNotFound, StatusCodes.Status400BadRequest);

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new AppException(ErrorCodes.Invalid, StatusCodes.Status400BadRequest);

            var existingByEmail = await _db.Employees
                .FirstOrDefaultAsync(e => e.Email == dto.Email);

            if (existingByEmail != null && !existingByEmail.IsDeleted)
                throw new AppException(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest);

            await _uow.BeginTransactionAsync();
            string? blobUrl = null;

            try
            {
                var employee = existingByEmail ?? _mapper.Map<Employee>(dto);

                if (existingByEmail != null)
                {
                    _mapper.Map(dto, employee);
                    employee.IsDeleted = false;
                    employee.DeletedDate = null;
                    employee.DeletedBy = null;
                    employee.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    employee.CreatedDate = DateTime.UtcNow;
                    await _employeeRepo.AddAsync(employee);
                }

                employee.CompanyId = CampanyId;
                employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                await _employeeRepo.SaveChangesAsync();

                if (dto.Attachments != null)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.Attachments.FileName)}";

                    await using var stream = dto.Attachments.OpenReadStream();

                    blobUrl = await _blobStorageService.UploadAsync(
                        stream,
                        uniqueFileName,
                        dto.Attachments.ContentType,
                        folder: "attachments"
                    );


                    var attachment = new Attachment
                    {
                        FileName = dto.Attachments.FileName,  
                        FilePath = blobUrl,
                        Size = dto.Attachments.Length,
                        UploadedBy = employee.Id,
                        ContentType = dto.Attachments.ContentType,
                        UploadedAt = DateTime.UtcNow,
                        AttachmentType = AttachmentType.Employee,
                        ReferenceId = employee.Id,
                        BlobUrl = blobUrl,
                        BlobUploadedAt = DateTime.UtcNow,
                        IsUploadedToBlob = true
                    };


                    await _attachmentRepo.AddAsync(attachment);
                    await _attachmentRepo.SaveChangesAsync();

                    
                }

                await _cache.RemoveAsync("employees:");

                await _uow.CommitAsync();

                var fullEmployee = await _employeeRepo.GetAll(e => e.Id == employee.Id)
                                                      .Include(e => e.Branch)
                                                      .Include(e => e.Job)
                                                      .Include(e => e.Department)
                                                      .Include(e => e.EmployeeRoles)
                                                          .ThenInclude(er => er.Role)
                                                      .FirstOrDefaultAsync();

                var employeeDto = _mapper.Map<EmployeeGetDto>(fullEmployee);

                return ApiResponse<EmployeeGetDto>.Ok(employeeDto, "Employee added successfully");
            }
            catch
            {
                await _uow.RollbackAsync();

                if (!string.IsNullOrWhiteSpace(blobUrl))
                {
                    try
                    {
                        await _blobStorageService.DeleteAsync(blobUrl);
                    }
                    catch
                    {
                    }
                }

                throw;
            }
        }

        public async Task<ApiResponse<EmployeeGetDto>> UpdateAsync(int id, EmployeeAddEditDto dto)
        {
            if (dto.BranchId.HasValue && !await _branchRepo.IsExistAsync(dto.BranchId.Value))
                throw new AppException(ErrorCodes.BranchNotFound, StatusCodes.Status400BadRequest);

            var employee = await _employeeRepo.GetByIDAsync(id);
            if (employee == null)
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status400BadRequest);

            if (await _employeeRepo.GetAll(e => e.Email == dto.Email && e.Id != id).AnyAsync())
                throw new AppException(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest);

            await _uow.BeginTransactionAsync();

            string? newBlobUrl = null;
            string? oldBlobUrl = null;   

            try
            {
                _mapper.Map(dto, employee);

                if (!string.IsNullOrWhiteSpace(dto.Password))
                    employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                if (dto.Attachments != null)
                {
                    var oldAttachment = await _attachmentRepo
                        .GetAll(a => a.ReferenceId == employee.Id && a.AttachmentType == AttachmentType.Employee)
                        .FirstOrDefaultAsync();

                    if (oldAttachment != null)
                        oldBlobUrl = oldAttachment.BlobUrl;

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.Attachments.FileName)}";
                    await using var stream = dto.Attachments.OpenReadStream();

                    newBlobUrl = await _blobStorageService.UploadAsync(
                        stream,
                        uniqueFileName,
                        dto.Attachments.ContentType,
                        folder: "attachments"
                    );

                    if (oldAttachment != null)
                    {
                        oldAttachment.FileName = dto.Attachments.FileName; 
                        oldAttachment.FilePath = newBlobUrl;
                        oldAttachment.Size = dto.Attachments.Length;
                        oldAttachment.ContentType = dto.Attachments.ContentType;
                        oldAttachment.UploadedAt = DateTime.UtcNow;

                        oldAttachment.BlobUrl = newBlobUrl;
                        oldAttachment.BlobUploadedAt = DateTime.UtcNow;
                        oldAttachment.IsUploadedToBlob = true;

                        await _attachmentRepo.SaveChangesAsync();
                    }
                    else
                    {
                        var attachment = new Attachment
                        {
                            FileName = dto.Attachments.FileName,
                            FilePath = newBlobUrl,
                            Size = dto.Attachments.Length,
                            UploadedBy = employee.Id,
                            ContentType = dto.Attachments.ContentType,
                            UploadedAt = DateTime.UtcNow,
                            AttachmentType = AttachmentType.Employee,
                            ReferenceId = employee.Id,
                            BlobUrl = newBlobUrl,
                            BlobUploadedAt = DateTime.UtcNow,
                            IsUploadedToBlob = true
                        };

                        await _attachmentRepo.AddAsync(attachment);
                        await _attachmentRepo.SaveChangesAsync();
                    }
                }

                await _employeeRepo.SaveChangesAsync();
                await _cache.RemoveAsync("employees:");

                await _uow.CommitAsync();

                if (!string.IsNullOrWhiteSpace(oldBlobUrl) &&
                    !string.IsNullOrWhiteSpace(newBlobUrl) &&
                    !string.Equals(oldBlobUrl, newBlobUrl, StringComparison.OrdinalIgnoreCase))
                {
                    try { await _blobStorageService.DeleteAsync(oldBlobUrl); } catch { }
                }

                var fullEmployee = await _employeeRepo.GetAll(e => e.Id == employee.Id)
                    .Include(e => e.Branch)
                    .Include(e => e.Job)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeeRoles)
                        .ThenInclude(er => er.Role)
                    .FirstOrDefaultAsync();

                var employeeDto = _mapper.Map<EmployeeGetDto>(fullEmployee);
                return ApiResponse<EmployeeGetDto>.Ok(employeeDto, "Employee updated successfully");
            }
            catch
            {
                await _uow.RollbackAsync();

                if (!string.IsNullOrWhiteSpace(newBlobUrl))
                {
                    try { await _blobStorageService.DeleteAsync(newBlobUrl); } catch { }
                }

                throw;
            }
        }



        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var employee = await _employeeRepo.GetByIDAsync(id);
            if (employee == null)
                throw new AppException(
                    ErrorCodes.EmployeeNotFound,
                    StatusCodes.Status400BadRequest);

            var hasActiveAssignments = await _taskAssignmentRepo
                .GetAll(a => a.EmployeeId == id && a.IsActive)
                .AnyAsync();
            var hasTasks= await _taskRepo.GetAll(a => a.CreatedByEmployeeId == id).AnyAsync();



            if (hasActiveAssignments || hasTasks)
                throw new AppException(
                    ErrorCodes.EmployeeHasActiveTasks,
                    StatusCodes.Status409Conflict);


            _employeeRepo.SoftDelete(employee);
            await _employeeRepo.SaveChangesAsync();
            await _cache.RemoveAsync("employees:");


            return ApiResponse<bool>.Ok(true, "Employee deleted successfully");
        }

        public Task<ApiResponse<List<FunctionCodeEnumDto>>> GetFunctionCodesAsync()
        {
            var values = Enum.GetValues<FunctionCode>()
                .Select(x => new FunctionCodeEnumDto { Id = (int)x, Name = x.ToString() })
                .ToList();

            return Task.FromResult(ApiResponse<List<FunctionCodeEnumDto>>.Ok(values));
        }



    }
}

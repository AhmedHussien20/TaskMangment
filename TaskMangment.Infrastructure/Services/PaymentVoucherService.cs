using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.PaymentVoucher;
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
    public class PaymentVoucherService : IPaymentVoucherService
    {
        private readonly IRepository<PaymentVoucher> _voucherRepo;
        private readonly IRepository<Attachment> _attachmentRepo;
        private readonly IRepository<Company> _companyRepo;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Branch> _branchRepository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IMapper _mapper;

        private readonly ICachingService _cache;

        public PaymentVoucherService(
            IRepository<PaymentVoucher> voucherRepo,
            IRepository<Attachment> attachmentRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<Company> companyRepo,
            IRepository<Employee> employeeRepository,
            IBlobStorageService blobStorageService,
            IRepository<Branch> branchRepository)
        {
            _voucherRepo = voucherRepo;
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
            _cache = cache;
            _companyRepo = companyRepo;
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
            _blobStorageService = blobStorageService;
        }

        public async Task<ApiResponse<PagedResponse<PaymentVoucherGetDto>>> GetAllAsync(PaymentVoucherRequest request)
        {
            string cacheKey = $"vouchers:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<PaymentVoucherGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<PaymentVoucherGetDto>>.Ok(cached);
            }

            var query = _voucherRepo.GetAll()
                .Include(v => v.Company)
                .Include(v => v.Branch)
                .Include(v => v.CreatedBy)
                .ApplySearch(request.searchKey);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<PaymentVoucherGetDto>>(list);

            foreach (var dto in dtos)
            {
                var voucher = list.First(v => v.Id == dto.Id);
                dto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.ReferenceId == voucher.Id && a.AttachmentType==AttachmentType.Voucher);
            }

            var response = new PagedResponse<PaymentVoucherGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<PaymentVoucherGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<PaymentVoucherGetDto>> GetByIdAsync(int id)
        {
            var voucher = await _voucherRepo.GetAll(v => v.Id == id)
                .Include(v => v.Company)
                .Include(v => v.Branch)
                .Include(v => v.CreatedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (voucher == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var dto = _mapper.Map<PaymentVoucherGetDto>(voucher);
            dto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.ReferenceId == id && a.AttachmentType==AttachmentType.Voucher);

            return ApiResponse<PaymentVoucherGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<PaymentVoucherGetDto>> AddAsync(
            PaymentVoucherAddEditDto dto,
            int CompanyId,
            int CreatedBy)
        {
            if (dto.BranchId.HasValue && !await _branchRepository.IsExistAsync(dto.BranchId.Value))
                throw new AppException(ErrorCodes.BranchNotFound, StatusCodes.Status404NotFound);

            if (!await _employeeRepository.IsExistAsync(CreatedBy))
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status404NotFound);

            if (!await _companyRepo.IsExistAsync(CompanyId))
                throw new AppException(ErrorCodes.CompanyNotFound, StatusCodes.Status404NotFound);

            var voucher = _mapper.Map<PaymentVoucher>(dto);
            voucher.CompanyId = CompanyId;
            voucher.CreatedByEmployeeId = CreatedBy;


            if (dto.File != null)
            {
                using var stream = dto.File.OpenReadStream();
                var blobUrl = await _blobStorageService.UploadAsync(
                    stream,
                    dto.File.FileName,
                    dto.File.ContentType,
                    folder: "attachments"
                );
                var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
                var attachment = new Attachment
                {
                    FileName = dto.File.FileName,
                    FilePath = blobUrl,
                    Size = dto.File.Length,
                    UploadedBy = CreatedBy,
                    ContentType = dto.File.ContentType,
                    UploadedAt = DateTime.UtcNow,
                    AttachmentType = AttachmentType.Voucher,
                    ReferenceId = voucher.Id,
                    BlobUrl = blobUrl,
                    BlobUploadedAt = DateTime.UtcNow,
                    IsUploadedToBlob = true
                };
                await _attachmentRepo.AddAsync(attachment);
                await _attachmentRepo.SaveChangesAsync();
                 
            }

            await _voucherRepo.AddAsync(voucher);
            await _voucherRepo.SaveChangesAsync();
            await _cache.RemoveAsync("vouchers:");

            var fullVoucher = await _voucherRepo.GetAll(v => v.Id == voucher.Id)
                .Include(v => v.Company)
                .Include(v => v.Branch)
                .Include(v => v.CreatedBy)
               // .Include(c => c.Attachments)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var voucherDto = _mapper.Map<PaymentVoucherGetDto>(fullVoucher);
            voucherDto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.ReferenceId == voucher.Id && a.AttachmentType == AttachmentType.Comment);



            return ApiResponse<PaymentVoucherGetDto>
                .Ok(voucherDto, "Voucher added successfully");
        }

        public async Task<ApiResponse<PaymentVoucherGetDto>> UpdateAsync(int id, PaymentVoucherAddEditDto dto)
        {
            var voucher = await _voucherRepo.GetByIDAsync(id);
            if (voucher == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (dto.BranchId.HasValue && !await _branchRepository.IsExistAsync(dto.BranchId.Value))
                throw new AppException(ErrorCodes.BranchNotFound, StatusCodes.Status404NotFound);

            _mapper.Map(dto, voucher);
            //voucher.ModifiedDate = DateTime.UtcNow;

            await _voucherRepo.SaveChangesAsync();
            await _cache.RemoveAsync("vouchers:");

            // Optional: clear cache pattern
            // await _cache.RemoveByPatternAsync("vouchers-");
            var fullVoucher = await _voucherRepo.GetAll(v => v.Id == voucher.Id)
       .Include(v => v.Company)
       .Include(v => v.Branch)
       .Include(v => v.CreatedBy)
      // .Include(v => v.Attachments)
       .FirstOrDefaultAsync();

            var voucherDto = _mapper.Map<PaymentVoucherGetDto>(fullVoucher);
            return ApiResponse<PaymentVoucherGetDto>.Ok(voucherDto, "Voucher updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var voucher = await _voucherRepo.GetByIDAsync(id);
            if (voucher == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            _voucherRepo.SoftDelete(voucher);
            await _voucherRepo.SaveChangesAsync();
            await _cache.RemoveAsync("vouchers:");

            // Optional: clear cache pattern
            // await _cache.RemoveByPatternAsync("vouchers-");

            return ApiResponse<bool>.Ok(true, "Voucher deleted successfully");
        }
    }
}

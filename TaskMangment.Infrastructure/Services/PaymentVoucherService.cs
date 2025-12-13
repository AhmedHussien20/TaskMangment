using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.PaymentVoucher;
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

        private readonly IMapper _mapper;

        private readonly ICachingService _cache;

        public PaymentVoucherService(
            IRepository<PaymentVoucher> voucherRepo,
            IRepository<Attachment> attachmentRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<Company> companyRepo,
            IRepository<Employee> employeeRepository,
            IRepository<Branch> branchRepository)
        {
            _voucherRepo = voucherRepo;
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
            _cache = cache;
            _companyRepo = companyRepo;
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
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
                dto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.VoucherId == voucher.Id);
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
                return ApiResponse<PaymentVoucherGetDto>.Fail("Voucher not found");

            var dto = _mapper.Map<PaymentVoucherGetDto>(voucher);
            dto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.VoucherId == id);

            return ApiResponse<PaymentVoucherGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<PaymentVoucherGetDto>> AddAsync(PaymentVoucherAddEditDto dto, int CompanyId, int CreatedBy)
        {
            if (dto.BranchId.HasValue && !await _branchRepository.IsExistAsync(dto.BranchId.Value))
                return ApiResponse<PaymentVoucherGetDto>.Fail("Branch not found", StatusCode.NotFound);

            if (!await _employeeRepository.IsExistAsync(CreatedBy))
                return ApiResponse<PaymentVoucherGetDto>.Fail("Employee not found", StatusCode.NotFound);

            if (!await _companyRepo.IsExistAsync(CompanyId))
                return ApiResponse<PaymentVoucherGetDto>.Fail("Company not found", StatusCode.NotFound);
            var voucher = _mapper.Map<PaymentVoucher>(dto);
            voucher.CompanyId = CompanyId;
            voucher.CreatedByEmployeeId = CreatedBy;
            voucher.CreatedDate= DateTime.UtcNow;


            await _voucherRepo.AddAsync(voucher);
            await _voucherRepo.SaveChangesAsync();
            await _cache.RemoveAsync("vouchers:");

            // Optional: clear cache pattern
            // await _cache.RemoveByPatternAsync("vouchers-");
            var fullVoucher = await _voucherRepo.GetAll(v => v.Id == voucher.Id)
                       .Include(v => v.Company)
       .Include(v => v.Branch)
       .Include(v => v.CreatedBy)
       .Include(v => v.Attachments)
       .FirstOrDefaultAsync();

            var voucherDto = _mapper.Map<PaymentVoucherGetDto>(fullVoucher);

            return ApiResponse<PaymentVoucherGetDto>.Ok(voucherDto, "Voucher added successfully");
        }

        public async Task<ApiResponse<PaymentVoucherGetDto>> UpdateAsync(int id, PaymentVoucherAddEditDto dto)
        {
            var voucher = await _voucherRepo.GetByIDAsync(id);
            if (voucher == null)
                return ApiResponse<PaymentVoucherGetDto>.Fail("Voucher not found");


            _mapper.Map(dto, voucher);
            voucher.ModifiedDate = DateTime.UtcNow;

            await _voucherRepo.SaveChangesAsync();
            await _cache.RemoveAsync("vouchers:");

            // Optional: clear cache pattern
            // await _cache.RemoveByPatternAsync("vouchers-");
            var fullVoucher = await _voucherRepo.GetAll(v => v.Id == voucher.Id)
       .Include(v => v.Company)
       .Include(v => v.Branch)
       .Include(v => v.CreatedBy)
       .Include(v => v.Attachments)
       .FirstOrDefaultAsync();

            var voucherDto = _mapper.Map<PaymentVoucherGetDto>(fullVoucher);
            return ApiResponse<PaymentVoucherGetDto>.Ok(voucherDto, "Voucher updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var voucher = await _voucherRepo.GetByIDAsync(id);
            if (voucher == null)
                return ApiResponse<bool>.Fail("Voucher not found");

            _voucherRepo.SoftDelete(voucher);
            await _voucherRepo.SaveChangesAsync();
            await _cache.RemoveAsync("vouchers:");

            // Optional: clear cache pattern
            // await _cache.RemoveByPatternAsync("vouchers-");

            return ApiResponse<bool>.Ok(true, "Voucher deleted successfully");
        }
    }
}

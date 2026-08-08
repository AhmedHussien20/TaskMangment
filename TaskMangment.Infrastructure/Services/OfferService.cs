using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Offer;
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
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class OfferService : IOfferService
    {
        private readonly IRepository<Offer> _offerRepo;
        private readonly IRepository<OfferAssignment> _offerAssignmentRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<CourseSubject> _subjectRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly IUserAccessContextProvider _accessProvider;



        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public OfferService(IRepository<Offer> offerRepo, IRepository<OfferAssignment> offerAssignmentRepo, IRepository<Student> studentRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<CourseSubject> subjectRepo,
            IRepository<Course> courseRepo, IDomainEventDispatcher eventDispatcher,
            IRepository<Employee> employeeRepo, IUserAccessContextProvider accessProvider)
        {
            _offerRepo = offerRepo;
            _offerAssignmentRepo = offerAssignmentRepo;
            _studentRepo = studentRepo;
            _mapper = mapper;
            _cache = cache;
            _subjectRepo = subjectRepo;
            _courseRepo = courseRepo;
            _eventDispatcher = eventDispatcher;
            _employeeRepo = employeeRepo;
            _accessProvider = accessProvider;
        }

        public async Task<ApiResponse<PagedResponse<OfferGetDto>>> GetAllAsync(
            OfferRequest request,
            int employeeId,
            int roleLevel)
        {
            var access = await _accessProvider.GetAsync(employeeId);

            var myBranchId = await _employeeRepo.GetAll(e => e.Id == employeeId)
                .Select(e => e.BranchId)
                .FirstOrDefaultAsync();

            var query = _offerRepo.GetAll()
                .Include(o => o.Course)
                .Include(o => o.Subject)
                .Include(o => o.Assignments).ThenInclude(a => a.Student)
                .ApplySearch(request.searchKey);

            if (access.BranchIds.Any())
            {
                query = query.Where(o => o.BranchId.HasValue && access.BranchIds.Contains(o.BranchId.Value));
            }
            else if (roleLevel != 100)
            {
                query = query.Where(o => o.BranchId == myBranchId);
            }

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<OfferGetDto>>(list);

            foreach (var dto in dtos)
            {
                var offer = list.First(o => o.Id == dto.Id);

                dto.AssignedStudents = offer.Assignments
                    .Select(a => a.Student.FullName)
                    .ToList();

                dto.CourseTitle = offer.Course?.Title;
                dto.SubjectTitle = offer.Subject?.Title;

                if (!string.IsNullOrWhiteSpace(offer.Body))
                {
                    var body = JsonSerializer.Deserialize<Dictionary<string, string>>(offer.Body);

                    if (body != null)
                    {
                        dto.PaymentMethod = body.GetValueOrDefault("PaymentMethod");
                        dto.Price = body.GetValueOrDefault("Price");
                        dto.InterestRate = body.GetValueOrDefault("InterestRate");
                        dto.DiscountRate = body.GetValueOrDefault("DiscountRate");
                        dto.InstallmentValue = body.GetValueOrDefault("InstallmentValue");
                        dto.NetAmount = body.GetValueOrDefault("NetAmount");
                        dto.OfferOwner = body.GetValueOrDefault("OfferOwner");
                        dto.Specialization = body.GetValueOrDefault("Specialization");
                    }
                }
            }

            // 9. Response
            var response = new PagedResponse<OfferGetDto>(
                dtos,
                totalCount,
                request.PageIndex,
                request.PageSize
            );

            return ApiResponse<PagedResponse<OfferGetDto>>.Ok(response);
        }
        public async Task<ApiResponse<OfferGetDto>> GetByIdAsync(int id)
        {
            var offer = await _offerRepo.GetAll(o => o.Id == id)
                .Include(o => o.Course)
                .Include(o => o.Subject)
                .Include(o => o.Assignments).ThenInclude(a => a.Student)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (offer == null)
                throw new AppException(ErrorCodes.OfferNotFound, StatusCodes.Status400BadRequest);

            var dto = _mapper.Map<OfferGetDto>(offer);
            dto.AssignedStudents = offer.Assignments.Select(a => a.Student.FullName).ToList();
            dto.AssignedStudentsIds = offer.Assignments.Select(a => a.StudentId).ToList();
            dto.CourseId = offer.Course?.Id;
            dto.SubjectId = offer.Subject?.Id;

            if (!string.IsNullOrWhiteSpace(offer.Body))
            {
                var body = JsonSerializer.Deserialize<Dictionary<string, string>>(offer.Body);
                if (body != null)
                {
                    dto.PaymentMethod = body.GetValueOrDefault("PaymentMethod");
                    dto.Price = body.GetValueOrDefault("Price");
                    dto.InterestRate = body.GetValueOrDefault("InterestRate");
                    dto.DiscountRate = body.GetValueOrDefault("DiscountRate");
                    dto.InstallmentValue = body.GetValueOrDefault("InstallmentValue");
                    dto.NetAmount = body.GetValueOrDefault("NetAmount");
                    dto.OfferOwner = body.GetValueOrDefault("OfferOwner");
                    dto.Specialization = body.GetValueOrDefault("Specialization");
                }
            }

            return ApiResponse<OfferGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<OfferGetDto>> AddAsync(OfferAddEditDto dto, int CreatedById)
         {

            if (dto.AssignedStudentIds != null && dto.AssignedStudentIds.Any())
            {
                var allExist = await _studentRepo
                    .GetAll(s => dto.AssignedStudentIds.Contains(s.Id))
                    .CountAsync() == dto.AssignedStudentIds.Count;

                if (!allExist)
                    throw new AppException(
                        ErrorCodes.StudentNotFound,
                        StatusCodes.Status404NotFound
                    );
            }
            if (dto.CourseId.HasValue)
            {
                if (!await _courseRepo.IsExistAsync(dto.CourseId.Value))
                    throw new AppException(
                        ErrorCodes.CourseNotFound,
                        StatusCodes.Status404NotFound);
            }
            if (dto.SubjectId.HasValue)
            {
                if (!await _subjectRepo.IsExistAsync(dto.SubjectId.Value))
                    throw new AppException(
                        ErrorCodes.SubjectNotFound,
                        StatusCodes.Status404NotFound);
            }

            var employee = await _employeeRepo.GetByIDAsync(CreatedById);
            var offer = _mapper.Map<Offer>(dto);
            offer.BranchId = employee.BranchId;

            var bodyObj = new
            {
                dto.PaymentMethod,
                dto.Price,
                dto.InterestRate,
                dto.DiscountRate,
                dto.InstallmentValue,
                dto.NetAmount,
                dto.OfferOwner,
                dto.Specialization,
            };
            offer.Body = JsonSerializer.Serialize(bodyObj);


            foreach (var studentId in dto.AssignedStudentIds)
            {
                if (await _studentRepo.IsExistAsync(studentId))
                {
                    offer.Assignments.Add(new OfferAssignment
                    {
                        StudentId = studentId
                    });
                }

            }

            await _offerRepo.AddAsync(offer);
            await _offerRepo.SaveChangesAsync();
            await _cache.RemoveAsync("offers:");

            await _eventDispatcher.PublishAsync(new OfferSentEvent(offer.Id, offer.Title, dto.AssignedStudentIds));


            var fullOffer = await _offerRepo.GetAll(o => o.Id == offer.Id)
      .Include(o => o.Course)
      .Include(o => o.Subject)
      .Include(o => o.Assignments)
      .ThenInclude(a => a.Student) 
      .FirstOrDefaultAsync();

            var offerDto = _mapper.Map<OfferGetDto>(fullOffer);

            if (!string.IsNullOrWhiteSpace(fullOffer.Body))
            {
                var body = JsonSerializer.Deserialize<Dictionary<string, string>>(fullOffer.Body);
                if (body != null)
                {
                    offerDto.PaymentMethod = body.GetValueOrDefault("PaymentMethod");
                    offerDto.Price = body.GetValueOrDefault("Price");
                    offerDto.InterestRate = body.GetValueOrDefault("InterestRate");
                    offerDto.DiscountRate = body.GetValueOrDefault("DiscountRate");
                    offerDto.InstallmentValue = body.GetValueOrDefault("InstallmentValue");
                    offerDto.NetAmount = body.GetValueOrDefault("NetAmount");
                    offerDto.OfferOwner = body.GetValueOrDefault("OfferOwner");
                    offerDto.Specialization = body.GetValueOrDefault("Specialization");
                }
            }

            return ApiResponse<OfferGetDto>.Ok(offerDto, "Offer added successfully");
        }

        public async Task<ApiResponse<OfferGetDto>> UpdateAsync(int id, OfferAddEditDto dto)
        {
            var offer = await _offerRepo.GetByIDAsync(id);
            if (offer == null)
                throw new AppException(ErrorCodes.OfferNotFound, StatusCodes.Status400BadRequest);

            if (dto.AssignedStudentIds != null && dto.AssignedStudentIds.Any())
            {
                var allExist = await _studentRepo
                    .GetAll(s => dto.AssignedStudentIds.Contains(s.Id))
                    .CountAsync() == dto.AssignedStudentIds.Count;

                if (!allExist)
                    throw new AppException(
                        ErrorCodes.StudentNotFound,
                        StatusCodes.Status404NotFound
                    );
            }

            if (dto.CourseId.HasValue)
            {
                if (!await _courseRepo.IsExistAsync(dto.CourseId.Value))
                    throw new AppException(
                        ErrorCodes.CourseNotFound,
                        StatusCodes.Status404NotFound
                    );
            }

            if (dto.SubjectId.HasValue)
            {
                if (!await _subjectRepo.IsExistAsync(dto.SubjectId.Value))
                    throw new AppException(
                        ErrorCodes.SubjectNotFound,
                        StatusCodes.Status404NotFound
                    );
            }

            _mapper.Map(dto, offer);

            var bodyObj = new
            {
                dto.PaymentMethod,
                dto.Price,
                dto.InterestRate,
                dto.DiscountRate,
                dto.InstallmentValue,
                dto.NetAmount,
                dto.OfferOwner,
                dto.Specialization,
            };
            offer.Body = JsonSerializer.Serialize(bodyObj);

            var existingAssignments = await _offerRepo.GetAll(o => o.Id == id)
                .Include(o => o.Assignments)
                .SelectMany(o => o.Assignments)
                .ToListAsync();

            foreach (var assignment in existingAssignments)
            {
                if (!dto.AssignedStudentIds.Contains(assignment.StudentId))
                    assignment.IsAccepted = false;
            }

            foreach (var studentId in dto.AssignedStudentIds)
            {
                var exists = existingAssignments.Any(a => a.StudentId == studentId);
                if (!exists)
                {
                    offer.Assignments.Add(new OfferAssignment { StudentId = studentId });
                }
            }

            await _offerRepo.SaveChangesAsync();
            await _cache.RemoveAsync("offers:");

            var fullOffer = await _offerRepo.GetAll(o => o.Id == offer.Id)
                .Include(o => o.Course)
                .Include(o => o.Subject)
                .Include(o => o.Assignments)
                .ThenInclude(a => a.Student)
                .FirstOrDefaultAsync();

            var offerDto = _mapper.Map<OfferGetDto>(fullOffer);

            if (!string.IsNullOrWhiteSpace(fullOffer.Body))
            {
                var body = JsonSerializer.Deserialize<Dictionary<string, string>>(fullOffer.Body);
                if (body != null)
                {
                    offerDto.PaymentMethod = body.GetValueOrDefault("PaymentMethod");
                    offerDto.Price = body.GetValueOrDefault("Price");
                    offerDto.InterestRate = body.GetValueOrDefault("InterestRate");
                    offerDto.DiscountRate = body.GetValueOrDefault("DiscountRate");
                    offerDto.InstallmentValue = body.GetValueOrDefault("InstallmentValue");
                    offerDto.NetAmount = body.GetValueOrDefault("NetAmount");
                    offerDto.OfferOwner = body.GetValueOrDefault("OfferOwner");
                    offerDto.Specialization = body.GetValueOrDefault("Specialization");
                }
            }

            return ApiResponse<OfferGetDto>.Ok(offerDto, "Offer updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var offer = await _offerRepo.GetByIDAsync(id);
            if (offer == null)
                throw new AppException(ErrorCodes.OfferNotFound, StatusCodes.Status400BadRequest);

            if (await _offerAssignmentRepo.GetAll(a => a.OfferId == id).AnyAsync())
                throw new AppException(ErrorCodes.OfferHasAssignments, StatusCodes.Status400BadRequest);

            _offerRepo.SoftDelete(offer);
            await _offerRepo.SaveChangesAsync();
            await _cache.RemoveAsync("offers:");

            return ApiResponse<bool>.Ok(true, "Offer deleted successfully");
        }
    }
}

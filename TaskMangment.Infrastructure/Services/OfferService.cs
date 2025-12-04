using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Offer;
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
    public class OfferService : IOfferService
    {
        private readonly IRepository<Offer> _offerRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public OfferService(IRepository<Offer> offerRepo, IRepository<Student> studentRepo, IMapper mapper, ICachingService cache)
        {
            _offerRepo = offerRepo;
            _studentRepo = studentRepo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<OfferGetDto>>> GetAllAsync(OfferRequest request)
        {
            string safeTitle = request.Title ?? string.Empty;
            string safeCourseId = request.CourseId?.ToString() ?? "null";
            string safeSubjectId = request.SubjectId?.ToString() ?? "null";

            string cacheKey = $"offers-{request.PageIndex}-{request.PageSize}-{request.SortColumn}-{request.SortDirection}-{safeTitle}-{safeCourseId}-{safeSubjectId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<OfferGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<OfferGetDto>>.Ok(cached);
            }

            var query = _offerRepo.GetAll()
                .Include(o => o.Course)
                .Include(o => o.Subject)
                .Include(o => o.Assignments).ThenInclude(a => a.Student)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
                query = query.Where(o => o.Title.Contains(request.Title));

            if (request.CourseId.HasValue)
                query = query.Where(o => o.CourseId == request.CourseId.Value);

            if (request.SubjectId.HasValue)
                query = query.Where(o => o.SubjectId == request.SubjectId.Value);

            var totalCount = await query.CountAsync();
            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<OfferGetDto>>(list);

            foreach (var dto in dtos)
            {
                var offer = list.FirstOrDefault(o => o.Id == dto.Id);
                dto.AssignedStudents = offer.Assignments.Select(a => a.Student.FullName).ToList();
                dto.CourseTitle = offer.Course?.Title;
                dto.SubjectTitle = offer.Subject?.Title;
            }

            var response = new PagedResponse<OfferGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);
            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

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
                return ApiResponse<OfferGetDto>.Fail("Offer not found", StatusCode.NotFound);

            var dto = _mapper.Map<OfferGetDto>(offer);
            dto.AssignedStudents = offer.Assignments.Select(a => a.Student.FullName).ToList();
            dto.CourseTitle = offer.Course?.Title;
            dto.SubjectTitle = offer.Subject?.Title;

            return ApiResponse<OfferGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(OfferAddEditDto dto)
        {
            var offer = _mapper.Map<Offer>(dto);

            // Assign students
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

            return ApiResponse<bool>.Ok(true, "Offer added successfully");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, OfferAddEditDto dto)
        {
            var offer = await _offerRepo.GetByIDAsync(id);
            if (offer == null)
                return ApiResponse<bool>.Fail("Offer not found", StatusCode.NotFound);

            _mapper.Map(dto, offer);

            var existingAssignments = await _offerRepo.GetAll(o => o.Id == id)
                .Include(o => o.Assignments)
                .SelectMany(o => o.Assignments)
                .ToListAsync();

            // Deactivate old assignments
            foreach (var assignment in existingAssignments)
            {
                if (!dto.AssignedStudentIds.Contains(assignment.StudentId))
                    assignment.IsAccepted = false; // Or any flag you want
            }

            // Add new assignments
            foreach (var studentId in dto.AssignedStudentIds)
            {
                var exists = existingAssignments.Any(a => a.StudentId == studentId);
                if (!exists)
                {
                    offer.Assignments.Add(new OfferAssignment { StudentId = studentId });
                }
            }

            await _offerRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Offer updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var offer = await _offerRepo.GetByIDAsync(id);
            if (offer == null)
                return ApiResponse<bool>.Fail("Offer not found", StatusCode.NotFound);

            _offerRepo.SoftDelete(offer);
            await _offerRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Offer deleted successfully");
        }
    }
}

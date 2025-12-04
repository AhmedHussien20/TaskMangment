using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Course;
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
    public class CourseService : ICourseService
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<CourseSubject> _subjectRepository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public CourseService(
            IRepository<Course> courseRepository,
            IRepository<CourseSubject> subjectRepository,
            IMapper mapper,
            ICachingService cache)
        {
            _courseRepository = courseRepository;
            _subjectRepository = subjectRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<CourseGetDto>>> GetAllAsync(CourseRequest request)
        {
            string safeTitle = request.Title ?? string.Empty;

            string cacheKey = $"courses-{request.PageIndex}-{request.PageSize}-{request.SortColumn}-{request.SortDirection}-{safeTitle}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<CourseGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<CourseGetDto>>.Ok(cached);
            }

            var query = _courseRepository.GetAll()
                .Include(c => c.Subjects)
                .Include(c => c.Offers)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
                query = query.Where(c => c.Title.Contains(request.Title));

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<CourseGetDto>>(list);

            // Fill subjects as List<string>
            //foreach (var dto in dtos)
            //{
            //    var course = list.First(c => c.Id == dto.Id);
            //    dto.Subjects = course.Subjects.Select(s => s.Title).ToList();
            //    dto.OfferCount = course.Offers.Count;
            //}

            var response = new PagedResponse<CourseGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<CourseGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<CourseGetDto>> GetByIdAsync(int id)
        {
            var course = await _courseRepository.GetAll(c => c.Id == id)
                .Include(c => c.Subjects)
                .Include(c => c.Offers)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (course == null)
                return ApiResponse<CourseGetDto>.Fail("Course not found", StatusCode.NotFound);

            var dto = _mapper.Map<CourseGetDto>(course);
            //dto.Subjects = course.Subjects.Select(s => s.Title).ToList();
            //dto.OfferCount = course.Offers.Count;

            return ApiResponse<CourseGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(CourseAddEditDto dto)
        {
            var course = _mapper.Map<Course>(dto);


            // Add Subjects
            foreach (var subjectName in dto.Subjects)
            {

                course.Subjects.Add(new CourseSubject { Title = subjectName });
            }

            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Course added successfully");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, CourseAddEditDto dto)
        {
            var course = await _courseRepository.GetAll(c => c.Id == id)
                .Include(c => c.Subjects)
                .FirstOrDefaultAsync();

            if (course == null)
                return ApiResponse<bool>.Fail("Course not found", StatusCode.NotFound);

            // Update main properties
            _mapper.Map(dto, course);

            // Remove subjects that are not in dto
            //var toRemove = course.Subjects.Where(s => !dto.Subjects.Contains(s.Title)).ToList();
            //foreach (var s in toRemove)
            //{
            //    course.Subjects.Remove(s);
            //}



            //have an issue with subjectid remain 0
            foreach (var subjectName in dto.Subjects)
            {
                if (!course.Subjects.Any(s => s.Title == subjectName))
                    course.Subjects.Add(new CourseSubject { Title = subjectName });
            }

            await _courseRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Course updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var course = await _courseRepository.GetByIDAsync(id);
            if (course == null)
                return ApiResponse<bool>.Fail("Course not found", StatusCode.NotFound);

            _courseRepository.SoftDelete(course);
            await _courseRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Course deleted successfully");
        }
    }
}

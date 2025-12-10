using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Student;
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
    public class StudentService : IStudentService
    {
        private readonly IRepository<Student> _studentRepository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public StudentService(IRepository<Student> studentRepository, IMapper mapper, ICachingService cache)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<StudentGetDto>>> GetAllAsync(StudentRequest request)
        {
            string cacheKey = $"students{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<StudentGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<StudentGetDto>>.Ok(cached);
            }

            var query = _studentRepository.GetAll()
                .Include(s => s.OfferAssignments)
                .ApplySearch(request.searchKey);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<StudentGetDto>>(list);

            foreach (var dto in dtos)
            {
                var student = list.FirstOrDefault(s => s.Id == dto.Id);
                dto.OfferCount = student.OfferAssignments.Count;
            }

            var response = new PagedResponse<StudentGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<StudentGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<StudentGetDto>> GetByIdAsync(int id)
        {
            var student = await _studentRepository.GetAll(s => s.Id == id)
                .Include(s => s.OfferAssignments)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (student == null)
                return ApiResponse<StudentGetDto>.Fail("Student not found", StatusCode.NotFound);

            var dto = _mapper.Map<StudentGetDto>(student);
            dto.OfferCount = student.OfferAssignments.Count;

            return ApiResponse<StudentGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(StudentAddEditDto dto)
        {
            var student = _mapper.Map<Student>(dto);

            await _studentRepository.AddAsync(student);
            await _studentRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Student added successfully");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, StudentAddEditDto dto)
        {
            var student = await _studentRepository.GetByIDAsync(id);
            if (student == null)
                return ApiResponse<bool>.Fail("Student not found", StatusCode.NotFound);

            _mapper.Map(dto, student);
            await _studentRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Student updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var student = await _studentRepository.GetByIDAsync(id);
            if (student == null)
                return ApiResponse<bool>.Fail("Student not found", StatusCode.NotFound);

            _studentRepository.SoftDelete(student);
            await _studentRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Student deleted successfully");
        }
    }
}

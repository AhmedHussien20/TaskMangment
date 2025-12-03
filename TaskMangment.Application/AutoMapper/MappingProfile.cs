using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs;
using TaskMangment.Domain.Entities;
using AutoMapper;
using Task = TaskMangment.Domain.Entities.Task;

namespace TaskMangment.Application.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyGetDto>();
            CreateMap<CompanyAddEditDto, Company>();
            CreateMap<Area, AreaGetDto>();
            CreateMap<AreaAddEditDto, Area>();


            CreateMap<Branch, BranchGetDto>()
    .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager != null ? src.Manager.FullName : string.Empty))
    .ForMember(dest => dest.ResponsibleName, opt => opt.MapFrom(src => src.Responsible != null ? src.Responsible.FullName : string.Empty))
    .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Area != null ? src.Area.Name : string.Empty));

            CreateMap<BranchAddEditDto, Branch>();
            CreateMap<Employee, EmployeeGetDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.EmployeeRoles.Select(er => er.Role.Name).ToList()));
            CreateMap<EmployeeAddEditDto, Employee>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());


            CreateMap<Role, RoleGetDto>();
            CreateMap<RoleAddDto, Role>();
            CreateMap<Role, RoleWithPermissionsDto>();

            CreateMap<Permission, PermissionGetDto>();
            CreateMap<PermissionAddDto, Permission>();
            CreateMap<Task, TaskGetDto>();
            CreateMap<TaskAddEditDto, Task>();

            CreateMap<EmployeeRole, EmployeeRoleGetDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

            CreateMap<Department, DepartmentGetDto>()
              .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
              .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Branch.Area != null ? src.Branch.Area.Name : string.Empty))
              .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager != null ? src.Manager.FullName : string.Empty))
              .ForMember(dest => dest.EmployeeCount, opt => opt.Ignore());

            CreateMap<DepartmentAddEditDto, Department>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.Jobs, opt => opt.Ignore()); 

            CreateMap<Department, DepartmentAddEditDto>()
                .ForMember(dest => dest.ManagerEmployeeId, opt => opt.MapFrom(src => src.ManagerEmployeeId))
                .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId));

            CreateMap<Course, CourseGetDto>()
           .ForMember(dest => dest.Title,
                      opt => opt.MapFrom(src => src.Subjects.Select(s => s.Title).ToList()));

            CreateMap<CourseAddEditDto, Course>()
                .ForMember(dest => dest.Subjects,
                           opt => opt.Ignore());


            CreateMap<JobAddEditDto, Job>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())      
            .ForMember(dest => dest.Employees, opt => opt.Ignore())  
            .ForMember(dest => dest.Department, opt => opt.Ignore());

            // Map Job entity to Get DTO
            CreateMap<Job, JobGetDto>()
                .ForMember(dest => dest.EmployeeName, opt =>
                    opt.MapFrom(src => src.Employees.FirstOrDefault().FullName))
                .ForMember(dest => dest.DepartmentName, opt =>
                    opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.BranchName, opt =>
                    opt.MapFrom(src => src.Department.Branch.Name));

        }
    }
}

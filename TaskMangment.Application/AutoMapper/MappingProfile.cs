using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs;
using TaskMangment.Domain.Entities;
using AutoMapper;

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
            CreateMap<Branch, BranchGetDto>();
            CreateMap<BranchAddEditDto, Branch>();
            CreateMap<Employee, EmployeeGetDto>();
            CreateMap<EmployeeAddEditDto, Employee>();
            CreateMap<Role, RoleGetDto>();
            CreateMap<RoleAddDto, Role>();
            CreateMap<Role, RoleWithPermissionsDto>();

            CreateMap<Permission, PermissionGetDto>();
            CreateMap<PermissionAddDto, Permission>();

            CreateMap<EmployeeRole, EmployeeRoleGetDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

        }
    }
}

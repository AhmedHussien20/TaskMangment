
using TaskMangment.Application.DTOs;
using TaskMangment.Domain.Entities;
using AutoMapper;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Domain.Entities.Enum;

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
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                    src.EmployeeRoles
                        .Where(er => er.Role != null)
                        .Select(er => er.Role.Name)
                        .ToList()))
                .ForMember(dest => dest.JobName, opt => opt.MapFrom(src => src.Job != null ? src.Job.Title : null))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null));



            CreateMap<EmployeeAddEditDto, Employee>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());


            CreateMap<Role, RoleGetDto>()
     .ForMember(
         d => d.LevelName,
         o => o.MapFrom(s =>
             Enum.IsDefined(typeof(RoleLevelEnum), s.Level)
                 ? Enum.GetName(typeof(RoleLevelEnum), s.Level)
                 : "Custom"
         )
     );

            CreateMap<RoleAddEditDto, Role>();
            //CreateMap<Role, RoleWithPermissionsDto>();

            CreateMap<Permission, PermissionGetDto>();
            CreateMap<PermissionAddDto, Permission>();

            CreateMap<WorkTask, TaskGetDto>()
            .ForMember(dest => dest.AssignedByName, opt => opt.MapFrom(src => src.AssignedBy != null ? src.AssignedBy.FullName: string.Empty))
               
                   
                       
                        

            .ForMember(dest => dest.AssignEmployee, opt => opt.MapFrom(src =>  src.Assignments != null? src.Assignments.Select(a => new TaskEmployeeAssignmentDto
                        {
                            Id = a.Employee.Id,
                            Name = a.Employee.FullName
                        }).ToList()
                        : new List<TaskEmployeeAssignmentDto>()))

           .ForMember(dest => dest.AssignedByName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.FullName : string.Empty))
           .ForMember(dest => dest.PriorityText, opt => opt.MapFrom(src => src.Priority))
           .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status));

            CreateMap<TaskAddEditDto, WorkTask>()
                .ForMember(dest => dest.Assignments, opt => opt.Ignore()); 
            //remember for discussion Ignore null values during mapping for not have to send the whole object
            //.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

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
    .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects.Select(s => s.Title).ToList()))
    .ForMember(dest => dest.OfferCount, opt => opt.MapFrom(src => src.Offers.Count));

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


            CreateMap<TaskAssignment, TaskAssignmentGetDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task.Title));
            CreateMap<TaskAssignmentAddEditDto, TaskAssignment>();


            CreateMap<Student, StudentGetDto>();
            CreateMap<StudentAddEditDto, Student>();

            CreateMap<OfferAddEditDto, Offer>()
               .ForMember(dest => dest.Assignments, opt => opt.Ignore());

            CreateMap<Offer, OfferGetDto>()
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course != null ? src.Course.Title : string.Empty))
                .ForMember(dest => dest.SubjectTitle, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Title : string.Empty))
                .ForMember(dest => dest.AssignedStudentsIds, opt => opt.MapFrom(src => src.Assignments.Select(a => a.Student.Id)))
                 .ForMember(dest => dest.AssignedStudents, opt => opt.MapFrom(src => src.Assignments.Select(a => a.Student.FullName)));


            CreateMap<AttachmentAddDto, Attachment>()
           .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.File.FileName))
           .ForMember(dest => dest.FilePath, opt => opt.MapFrom(src => "/uploads/" + src.File.FileName))
           .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.File.ContentType))
           .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.File.Length));
            CreateMap<Attachment, AttachmentGetDto>();

            CreateMap<PaymentVoucher, PaymentVoucherGetDto>()
    .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Company.Name))
    .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch.Name))
    .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy.FullName));

            CreateMap<PaymentVoucherAddEditDto, PaymentVoucher>();

            CreateMap<TaskCommentAddEditDto, TaskComment>();

            CreateMap<TaskComment, TaskCommentGetDto>()
          .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee.FullName));


            CreateMap<TaskPercentage, TaskPercentageGetDto>()
               .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task.Title))
               .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee.FullName));

            CreateMap<TaskPercentageAddEditDto, TaskPercentage>();
             


            CreateMap<TaskExtensionRequest, TaskExtensionRequestListDto>()
                .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.RequestedBy.FullName))
                .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task.Title))
                .ForMember(dest => dest.ReviewedByName, opt => opt.MapFrom(src => src.ReviewedBy.FullName))
                .ForMember(dest => dest.ExtendRequestText, opt => opt.MapFrom(src => src.Status));




            CreateMap<TaskExtensionRequest, TaskExtensionRequestDetailsDto>()
                .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.RequestedBy.FullName))
                .ForMember(dest => dest.ReviewedByName, opt => opt.MapFrom(src => src.ReviewedBy.FullName))
                            .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task.Title))
                            .ForMember(dest => dest.ExtendRequestText, opt => opt.MapFrom(src => src.Status));


            CreateMap<TaskExtensionRequestAddDto, TaskExtensionRequest>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedAt, opt => opt.Ignore());



            CreateMap<TaskCloseRequest, TaskCloseRequestListDto>()
           .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.RequestedBy.FullName))
            .ForMember(dest => dest.CloseRequestText, opt => opt.MapFrom(src => src.Status));


            CreateMap<TaskCloseRequest, TaskCloseRequestDetailsDto>()
                .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.RequestedBy.FullName))
                .ForMember(dest => dest.ReviewedByName, opt => opt.MapFrom(src => src.ReviewedBy.FullName))
                 .ForMember(dest => dest.CloseRequestText, opt => opt.MapFrom(src => src.Status));


            CreateMap<TaskCloseRequestAddDto, TaskCloseRequest>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedAt, opt => opt.Ignore());


            CreateMap<Warning, WarningGetDto>()
               .ForMember(dest => dest.IssuedByName, opt => opt.MapFrom(src => src.IssuedBy.FullName))
                .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.TaskAssignment.Task.Title))
                .ForMember(dest => dest.IssuedEmployeeName, opt => opt.MapFrom(src => src.Issued.FullName));


            CreateMap<Warning, WarningGetDto>()
                .ForMember(dest => dest.IssuedByName, opt => opt.MapFrom(src => src.IssuedBy.FullName))
                 .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.TaskAssignment.Task.Title))
                .ForMember(dest => dest.IssuedEmployeeName, opt => opt.MapFrom(src => src.Issued.FullName));

            CreateMap<WarningAddEditDto, Warning>()
                .ForMember(dest => dest.IssuedAt, opt => opt.Ignore());



            CreateMap<CalendarEventAddEditDto, CalendarEvent>()
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => (CalendarEventType)src.EventType));

            CreateMap<CalendarEvent, CalendarEventGetDto>()
                .ForMember(d => d.RelatedTaskTitle,
                    opt => opt.MapFrom(s => s.RelatedTask != null ? s.RelatedTask.Title : null))
                .ForMember(dest => dest.EventTypeText, opt => opt.MapFrom(src => src.EventType));


            CreateMap<DiscountAddEditDto, Discount>();
            CreateMap<Discount, DiscountGetDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task != null ? src.Task.Title : null));

            CreateMap<Discount, DiscountGetDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task != null ? src.Task.Title : null));

            CreateMap<AuditLog,AuditLogDTO>()
                .ForMember(dest => dest.ChangedBy, opt => opt.MapFrom(src => src.ChangedBy != null ? src.ChangedBy : "System"));
             
            CreateMap<RoleAddEditDto, Role>();
            //CreateMap<Role, RoleWithPermissionsDto>()
            //    .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.RolePermissions.Select(rp => rp.Permission)));

            CreateMap<PermissionAddDto, Permission>();
            CreateMap<Permission, PermissionGetDto>();

            CreateMap<LeaveType, LeaveTypeGetDto>().ReverseMap();
            CreateMap<LeaveTypeAddEditDto, LeaveType>();

            CreateMap<Leave, LeaveGetDto>()
               .ForMember(dest => dest.EmployeeName,
                   opt => opt.MapFrom(src => src.Employee.FullName))

               .ForMember(dest => dest.LeaveTypeName,
                   opt => opt.MapFrom(src => src.LeaveType.NameAr))

               .ForMember(dest => dest.LeaveTypeId,
                   opt => opt.MapFrom(src => src.LeaveType.Id))

               .ForMember(dest => dest.Status,
                   opt => opt.MapFrom(src => src.Status))

               .ForMember(dest => dest.RejectionReason,
                   opt => opt.MapFrom(src => src.RejectionReason));



        }
    }
}

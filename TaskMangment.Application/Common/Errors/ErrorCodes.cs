using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Common.Errors
{
    public static class ErrorCodes
    {
        public const string NotFound = "NOT_FOUND";
        public const string EmailNotFound = "EMAIL_NOT_FOUND";
        public const string Invalid = "INVALID_PASS_OR_EMAIL";
        public const string InvalidToken = "INVALID_TOKEN";
        public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
        public const string EmployeeHasActiveTasks = "EMPLOYEE_HAS_ACTIVE_TASKS";


        public const string TaskNotFound = "TASK_NOT_FOUND";
        public const string CannotDeleteTask = "CANNOT_DELETE_TASK";
        public const string NotAssigned = "NOT_ASSIGNED";
        public const string AlreadyReviewed = "ALREADY_REVIEWED";
        public const string TaskAlreadyClosed = "TASK_ALREADY_CLOSED";
        public const string TaskMustBeClosedBeforeArchive = "TASK_MUST_BE_CLOSED_BEFORE_ARCHIVE";
        public const string TaskHasNoPercentage = "TASK_HAS_NO_PERCENTAGE";


        public const string NotAuthorized = "NOT_AUTHORIZED";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string SaveFailed = "SAVE_FAILED";
        public const string WhatsAppNotConfigured = "WHATSAPP_NOT_CONFIGURED";
        public const string WhatsAppSendFailed = "WHATSAPP_SEND_FAILED";
        public const string NotificationSendFailed = "NOTIFICATION_SEND_FAILED";


        public const string AreaNotFound = "AREA_NOT_FOUND";
        public const string AreaHasBranches = "AREA_HAS_BRANCHES";
        public const string ManagerNotFound = "MANAGER_NOT_FOUND";
        public const string CompanyNotFound = "COMPANY_NOT_FOUND";
        public const string CompanyHasEmployees = "COMPANY_HAS_EMPLOYEES";
        public const string CompanyHasAreas = "COMPANY_HAS_AREAS";
        public const string CompanyHasBranches = "COMPANY_HAS_BRANCHES";
        public const string CompanyHasRoles = "COMPANY_HAS_ROLES";

        public const string BranchNotFound = "BRANCH_NOT_FOUND";
        public const string BranchHasEmployees = "BRANCH_HAS_EMPLOYEES";
        public const string BranchHasDepartments = "BRANCH_HAS_DEPARTMENTS";
        public const string DepartmentNotFound = "DEPARTMENT_NOT_FOUND";
        public const string DepartmentHasJobs = "DEPARTMENT_HAS_JOBS";
        public const string DepartmentHasEmployees = "DEPARTMENT_HAS_EMPLOYEES";
        public  const string JobNotFound = "JOB_NOT_FOUND";
        public const string JobHasEmployees = "JOB_HAS_EMPLOYEES";

        public const string CourseNotFound = "COURSE_NOT_FOUND";
        public const string CourseHasSubjects = "COURSE_HAS_SUBJECTS";
        public const string CourseHasOffers = "COURSE_HAS_OFFERS";
        public const string OfferNotFound = "OFFER_NOT_FOUND";
        public const string OfferHasAssignments = "OFFER_HAS_ASSIGNMENTS";
        public const string OfferExpired = "OFFER_EXPIRED";
        public const string StudentNotFound = "STUDENT_NOT_FOUND";
        public const string StudentHasOfferAssignments = "STUDENT_HAS_OFFER_ASSIGNMENTS";
        public const string SubjectNotFound = "SUBJECT_NOT_FOUND";


        public const string EmployeeNotFound = "EMPLOYEE_NOT_FOUND";
        public const string EmployeeInactive = "EMPLOYEE_INACTIVE";

        public const string RoleNotFound = "ROLE_NOT_FOUND";
        public const string RoleHasEmployees = "ROLE_HAS_EMPLOYEES";
        public const string EmployeeMaxRolesExceeded = "EMPLOYEE_MAX_ROLES_EXCEEDED";
        public const string PermissionNotFound = "PERMISSION_NOT_FOUND";
        public const string PermissionHasRoles = "PERMISSION_HAS_ROLES";
        public const string AlreadyExists = "ALREADY_EXISTS";
        public const string AlreadyAssigned = "ALREADY_ASSIGNED";

        public const string InvalidDate = "INVALID_DATE";
        public const string DueDateOnWeekend = "DUE_DATE_ON_WEEKEND";
        public const string DueDateInPast = "DUE_DATE_IN_PAST";

        public const string Unauthorized = "Unauthorized";

        public const string InvalidOperation = "INVALID_OPERATION";
        public const string CloseRequestAlreadyPending = "CLOSEREQUESTALREADYREQUESTED";
        public const string InvalidManagerRoleLevel = "INVALID_MANAGER_ROLE_LEVEL";
        public const string InvalidBranchManagerRole = "INVALID_BRANCH_MANAGER_ROLE";
        public const string InvalidAreaManagerRole = "INVALID_AREA_MANAGER_ROLE";
        public const string UseAreaForBranchScope = "USE_AREA_FOR_BRANCH_SCOPE";
        public const string InvalidNotificationScope = "INVALID_NOTIFICATION_SCOPE";
        public const string LeaveTypeHasLeaves = "LEAVE_TYPE_HAS_LEAVES";
        public const string RoleEmployeeTypeRequired = "ROLE_EMPLOYEE_TYPE_REQUIRED";
        public const string EmployeeTypeMismatch = "EMPLOYEE_TYPE_MISMATCH";
    }

}

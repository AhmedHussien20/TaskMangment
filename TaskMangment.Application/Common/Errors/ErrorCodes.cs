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


        public const string AreaNotFound = "AREA_NOT_FOUND";
        public const string ManagerNotFound = "MANAGER_NOT_FOUND";
        public const string CompanyNotFound = "COMPANY_NOT_FOUND";

        public const string BranchNotFound = "BRANCH_NOT_FOUND";
        public const string DepartmentNotFound = "DEPARTMENT_NOT_FOUND";
        public  const string JobNotFound = "JOB_NOT_FOUND";

        public const string CourseNotFound = "COURSE_NOT_FOUND";
        public const string OfferNotFound = "OFFER_NOT_FOUND";
        public const string OfferExpired = "OFFER_EXPIRED";
        public const string StudentNotFound = "STUDENT_NOT_FOUND";
        public const string SubjectNotFound = "SUBJECT_NOT_FOUND";


        public const string EmployeeNotFound = "EMPLOYEE_NOT_FOUND";

        public const string RoleNotFound = "ROLE_NOT_FOUND";
        public const string PermissionNotFound = "PERMISSION_NOT_FOUND";
        public const string AlreadyExists = "ALREADY_EXISTS";
        public const string AlreadyAssigned = "ALREADY_ASSIGNED";

        public const string InvalidDate = "INVALID_DATE";

        public const string Unauthorized = "Unauthorized";

        public const string InvalidOperation = "INVALID_OPERATION";
        public const string CloseRequestAlreadyPending = "CLOSEREQUESTALREADYREQUESTED";
        public const string InvalidManagerRoleLevel = "INVALID_MANAGER_ROLE_LEVEL";
    }

}

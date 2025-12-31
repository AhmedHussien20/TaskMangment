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

        public const string TaskNotFound = "TASK_NOT_FOUND";
        public const string NotAssigned = "NOT_ASSIGNED";
        public const string AlreadyReviewed = "ALREADY_REVIEWED";


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
    }

}

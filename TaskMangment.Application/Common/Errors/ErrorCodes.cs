using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Common.Errors
{
    public static class ErrorCodes
    {
        public const string TaskNotFound = "TAKS_NOT_FOUND";
        public const string NotAuthorized = "NOT_AUTHORIZED";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string SaveFailed = "SAVE_FAILED";


        public const string AreaNotFound = "AREA_NOT_FOUND";
        public const string ManagerNotFound = "MANAGER_NOT_FOUND";
        public const string CompanyNotFound = "COMPANY_NOT_FOUND";
        public const string ManagerAlreadyAssigned = "MANAGER_ALREADY_ASSIGNED";
    }

}

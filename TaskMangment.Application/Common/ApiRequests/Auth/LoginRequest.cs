using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Common.ApiRequests.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }


    public class LoginResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int? CompanyId { get; set; }
        public string? campanyName { get; set; }
        public int? BranchId { get; set; }
        public string? branchName { get; set; }

        public int? DepartmentId { get; set; }
        public string? deptName { get; set; }

        public int? JobId { get; set; }

        public string Title { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string Qualification { get; set; }

        public bool IsActive { get; set; }

        // Roles
        public List<string> Roles { get; set; } = new();

        // Permissions
        public List<string> Permissions { get; set; } = new();

        public int RoleLevel { get; set; }
        public string RoleLevelName { get; set; } = string.Empty;

        // JWT Token
        public string Token { get; set; }
        public string? ProfileImage { get; set; }
        public FunctionCode FunctionCode { get; set; } = FunctionCode.Operations;


    }


    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }


    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }  
        public string NewPassword { get; set; }
    }
    public class VerifyResetCodeRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
    public class UpdatePasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int? BranchId { get; set; }
        public int? DepartmentId { get; set; }
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

        // JWT Token
        public string Token { get; set; }
        public string? ProfileImage { get; set; } 

    }


    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }


    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }   // reset code sent by email
        public string NewPassword { get; set; }
    }


}

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
        public string Token { get; set; }
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

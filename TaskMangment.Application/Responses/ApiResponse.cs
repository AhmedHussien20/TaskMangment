using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Responses
{
    public class ApiResponse <T>
    {
        public bool Success { get; set; } = true;
        public StatusCode Status_Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success", StatusCode code = StatusCode.Success)
            => new() { Success = true, Status_Code = code, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message, StatusCode code = StatusCode.Updated)
            => new() { Success = false, Status_Code = code, Message = message, Data = default };
    }
}

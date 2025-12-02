using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class EmployeeController : BaseController<EmployeeGetDto, EmployeeAddEditDto>
    {
        public EmployeeController(IEmployeeService service) : base(service)
        {
        }
    
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class CompanyController : BaseController<CompanyGetDto, CompanyAddEditDto>
    {
        public CompanyController(ICompanyService service) : base(service)
        {
        }
    }
}

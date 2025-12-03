using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    
    [Route("api/[controller]")]
    public class BranchController  
    {
        public BranchController(IBranchService service)  
        {
        }
    }
}

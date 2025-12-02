using TaskMangment.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController :BaseController
    {
        [HttpGet("exception")]
        public IActionResult ThrowException()
        {
            throw new Exception("This is a test exception!");
        }

        [HttpGet("app-exception")]
        public IActionResult ThrowAppException()
        {
            throw new AppException("This is a test AppException", TaskMangment.Application.Responses.StatusCode.BadRequest);
        }

        [HttpGet("unauthorized")]
        public IActionResult ThrowUnauthorized()
        {
            throw new UnauthorizedAccessException("You are not allowed!");
        }
    }
}

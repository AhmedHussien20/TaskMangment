using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.API.Middlewares;
using TaskMangment.Application.Common.ApiRequests.Branch;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{

    [Route("api/[controller]")]
    public class BranchController : BaseController
    {
        private readonly IBranchService _service;

        public BranchController(IBranchService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BranchRequest request)
        {
            var result = await _service.GetAllAsync(request,this.CurrentUserId,this.RoleLevel,this.CompanyId);

            if (!result.Success)
                return Fail(result.Message!);

            //SetCacheHeader(600);

            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result.Data);
        }

        [HttpPost]
        [PermissionAuthorize(PermissionCodes.CreateBranch)]
        public async Task<IActionResult> Add([FromBody] BranchAddEditDto dto)
        {
            var result = await _service.AddAsync(dto, this.CompanyId);
            return Success(result.Data, "Branch added successfully");
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(PermissionCodes.UpdateBranch)]
        public async Task<IActionResult> Update(int id, [FromBody] BranchAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Success(result.Data, "Branch updated successfully");
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(PermissionCodes.DeleteBranch)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true, "Branch deleted successfully");
        }
    }
}

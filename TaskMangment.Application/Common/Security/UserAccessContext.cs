using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Common.Security
{
    public class UserAccessContext
    {
        public int EmployeeId { get; set; }
        public IReadOnlyList<int> BranchIds { get; set; } = [];
        public IReadOnlyList<FunctionCode> FunctionCodes { get; set; } = [];
    }

}

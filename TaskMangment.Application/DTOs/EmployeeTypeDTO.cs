using System.ComponentModel.DataAnnotations;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.DTOs
{
    public class EmployeeTypeAddEditDto
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        public bool SeesAllTypesInBranchScope { get; set; }
    }

    public class EmployeeTypeGetDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public bool SeesAllTypesInBranchScope { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class EmployeeTypeRequest : BaseApiRequest
    {
    }
}

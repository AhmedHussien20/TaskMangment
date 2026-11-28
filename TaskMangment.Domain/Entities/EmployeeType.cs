using System.ComponentModel.DataAnnotations;

namespace TaskMangment.Domain.Entities
{
    /// <summary>
    /// Configurable employee type (replaces FunctionCode enum). Seeded: Operations, Accounting, HR, GeneralAffairs.
    /// </summary>
    public class EmployeeType : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        /// <summary>
        /// When true (Operations): coverage uses branches and can see all employee types in those branches.
        /// When false: coverage is all employees of this type (branch vs company is decided by the role flags).
        /// </summary>
        public bool SeesAllTypesInBranchScope { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<EmployeeFunctionalScope> FunctionalScopes { get; set; } = new List<EmployeeFunctionalScope>();
    }
}

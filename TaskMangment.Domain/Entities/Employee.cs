using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Employee : BaseEntity
    {
        [Required]
        public int CompanyId { get; set; }

        public int? BranchId { get; set; }
        public int? DepartmentId { get; set; }
        public int? JobId { get; set; }

        [MaxLength(50)]
        public string Title { get; set; }

        [Required, MaxLength(250)]
        public string FullName { get; set; }

        [MaxLength(100)]
        public string Nationality { get; set; }

        [MaxLength(100)]
        public string IdentityNumber { get; set; }

        [MaxLength(50)]
        public string Mobile { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(200)]
        public string Qualification { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        [MaxLength(500)]
        public string PasswordHash { get; set; }

        public bool IsActive { get; set; } = true; 


        // Navigation
        public Company Company { get; set; }
        public Branch Branch { get; set; }
        public Department Department { get; set; }
        public Job Job { get; set; }

        public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();

    }

}

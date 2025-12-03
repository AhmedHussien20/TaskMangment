using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class EmployeeAddEditDto
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }

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

        public List<int> RoleIds { get; set; } = new(); 


        [MaxLength(200)]
        public string Email { get; set; }

        [MaxLength(500)]
        public string Password { get; set; } 
    }

    public class EmployeeGetDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string BranchName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs
{
    public class EmployeeAddEditDto
    {
        public int? BranchId { get; set; }
        public int? JobId { get; set; }
        public int? DepartmentId { get; set; }

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
        public string? Password { get; set; }

        public IFormFile? Attachments { get; set; }
        public FunctionCode? FunctionCode { get; set; }

        public bool IsActive { get; set; } = true;


    }

    public class EmployeeGetDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? Title { get; set; }
        public string BranchName { get; set; }
        public string BranchId { get; set; }
        public string? JobName { get; set; }
        public int? JobId { get; set; }
        public string? DepartmentName { get; set; }
        public int? DepartmentId { get; set; }



        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Qualification { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; }

        public FunctionCode? FunctionCode { get; set; }


    }
    public class FunctionCodeEnumDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

}

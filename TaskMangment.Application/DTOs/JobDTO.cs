using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class JobAddEditDto
    {
        [Required]
        public int DepartmentId { get; set; } 

        [Required]
        public int EmployeeId { get; set; }   

        [Required, MaxLength(200)]
        public string Title { get; set; }
        [Required, MaxLength(1000)]
        public string Description { get; set; }
    }
    public class JobGetDto
    {
        public int Id { get; set; }          
        public string EmployeeName { get; set; }
        public string Title { get; set; }      
        public string DepartmentName { get; set; } 
        public string BranchName { get; set; }  
    }
}

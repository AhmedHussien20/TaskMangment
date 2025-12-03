using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class DepartmentAddEditDto
    {
        [Required]
        public int BranchId { get; set; } 

        [Required, MaxLength(200)]
        public string Name { get; set; } 

        public int? ManagerEmployeeId { get; set; } 
    }
    public class DepartmentGetDto
    {
        public int Id { get; set; }              
        public string Name { get; set; }       
        public string BranchName { get; set; }  
        public string AreaName { get; set; }     
        public string ManagerName { get; set; }  
        public int EmployeeCount { get; set; }  
    }
}

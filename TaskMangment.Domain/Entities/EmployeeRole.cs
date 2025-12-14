using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class EmployeeRole : BaseEntity
    {
        public int EmployeeId { get; set; }
        public int RoleId { get; set; }
        public bool IsAssigned { get; set; } = true; 

        public Employee Employee { get; set; }
        public Role Role { get; set; }
    }

}

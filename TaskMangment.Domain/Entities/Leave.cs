using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Leave : BaseEntity
    {
        
        [Required] 
        public int EmployeeId { get; set; }
        [Required] 
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Notes { get; set; }

        [ForeignKey(nameof(EmployeeId))] 
        public Employee Employee { get; set; }
        [ForeignKey(nameof(LeaveTypeId))] 
        public LeaveType LeaveType { get; set; }
    }

}

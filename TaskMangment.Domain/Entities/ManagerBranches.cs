using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class ManagerBranches : BaseEntity
    {
        public int ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))] public Employee? Manager { get; set; }  

        public bool IsActive { get; set; } = false;


        public int BranchId { get; set; }
        [ForeignKey(nameof(BranchId))] public Branch? Branch { get; set; } 
    }

}

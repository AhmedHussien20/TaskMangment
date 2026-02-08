using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class EmployeeFunctionalScope: BaseEntity
    {
        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))] public Employee? Employee { get; set; }
        public FunctionCode FunctionCode { get; set; }
    }
}

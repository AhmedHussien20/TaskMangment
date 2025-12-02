using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Area : BaseEntity
{
    [Required]
    public int CompanyId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string Address { get; set; }

    public int? ManagerEmployeeId { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; }

    [ForeignKey(nameof(ManagerEmployeeId))]
    public Employee Manager { get; set; }
}

}

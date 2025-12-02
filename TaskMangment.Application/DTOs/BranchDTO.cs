using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class BranchAddEditDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        [MaxLength(50)]
        public string Mobile { get; set; }

        [MaxLength(50)]
        public string Fax { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Website { get; set; }

        [MaxLength(100)]

        public string Password { get; set; }
        public string CommercialRecord { get; set; }

        public string Area { get; set; }
        public int ManagerID { get; set; }
        public int ResponsibleID { get; set; }
    }

    public class BranchGetDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AreaName { get; set; }
        public int ManagerID { get; set; }
        public int ResponsibleID { get; set; }
       
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class AreaAddEditDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        public int ManagerID { get; set; }
    }

    public class AreaGetDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ManagerID { get; set; }
        public int BranchCount { get; set; }
    }
}

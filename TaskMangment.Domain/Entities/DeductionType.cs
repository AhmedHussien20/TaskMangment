using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class DeductionType
    {
        [Key] public int DeductionTypeId { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; }
        public string Description { get; set; }
    }
}

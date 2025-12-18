using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class AttachmentType : BaseEntity
    {
        [Required]
        public int RefId { get; set; }

        [Required, MaxLength(50)]
        public string TypeName { get; set; } 
    }

}

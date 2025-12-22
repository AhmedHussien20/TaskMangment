using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class EmailTemplate:BaseEntity
    {

        public string Key { get; set; } = null!;
        public string SubjectTemplate { get; set; } = null!;
        public string BodyTemplate { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Student : BaseEntity
    {
        [MaxLength(250)] public string FullName { get; set; }
        [MaxLength(200)] public string Email { get; set; }
        [MaxLength(50)] public string Mobile { get; set; }
       // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<OfferAssignment> OfferAssignments { get; set; }= new List<OfferAssignment>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace TaskMangment.Domain.Entities
{
    public class Organization : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<PlatformUser> Admins { get; set; } = new List<PlatformUser>();
    }
}

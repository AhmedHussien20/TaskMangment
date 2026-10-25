using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMangment.Domain.Entities
{
    public class PlatformUser : BaseEntity
    {
        [Required, MaxLength(250)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        public PlatformRole Role { get; set; }

        public int? OrganizationId { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(OrganizationId))]
        public Organization? Organization { get; set; }
    }
}

namespace TaskMangment.Application.DTOs
{
    public class PlatformLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class PlatformLoginResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class CreateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
    }

    public class UpdateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        /// <summary>Optional. Empty keeps the current password.</summary>
        public string? AdminPassword { get; set; }
    }

    public class OrganizationListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? AdminId { get; set; }
        public string? AdminEmail { get; set; }
        public string? AdminFullName { get; set; }
    }

    public class CreateTraineeRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
        /// <summary>Required for SuperAdmin / PlatformAdmin. Ignored for OrganizationAdmin (uses their own org).</summary>
        public int? OrganizationId { get; set; }
    }

    public class UpdateTraineeRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        /// <summary>Optional. Empty keeps the current password.</summary>
        public string? AdminPassword { get; set; }
    }

    public class TraineeCopyDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
    }

    public class SetTraineeActiveRequest
    {
        public bool IsActive { get; set; }
    }

    public class CreatePlatformAdminRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdatePlatformAdminRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        /// <summary>Optional. Empty keeps the current password.</summary>
        public string? Password { get; set; }
    }

    public class PlatformAdminListItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

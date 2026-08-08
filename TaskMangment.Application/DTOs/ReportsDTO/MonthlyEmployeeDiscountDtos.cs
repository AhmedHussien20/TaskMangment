namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class BranchDiscountRowDto
    {
        public int BranchId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string RoleTitle { get; set; } = string.Empty;
        public decimal TotalDiscount { get; set; }
    }

    public class AccountantRecipientDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Mobile { get; set; }
    }
}

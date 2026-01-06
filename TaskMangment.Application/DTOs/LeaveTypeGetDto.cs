namespace TaskMangment.Application.DTOs
{
    public class LeaveTypeGetDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool IsPaid { get; set; }
        public int? MaxDaysPerYear { get; set; }
    }

    public class LeaveTypeAddEditDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool IsPaid { get; set; }
        public int? MaxDaysPerYear { get; set; }
    }

}

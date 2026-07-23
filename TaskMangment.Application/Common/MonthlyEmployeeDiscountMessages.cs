namespace TaskMangment.Application.Common
{
    public static class MonthlyEmployeeDiscountMessages
    {
        public const string ReportTitle = "خصومات الموظفين طوال الشهر";
        public const string EmailTemplateKey = "MonthlyEmployeeDiscounts";

        public static string BuildWhatsAppMessage(string branchName, DateTime fromDate, DateTime toDate)
        {
            return $"{ReportTitle}\nفرع: {branchName}\nالفترة: من {fromDate:yyyy/MM/dd} إلى {toDate:yyyy/MM/dd}\nمرفق تقرير PDF بإجمالي خصومات الموظفين.";
        }

        public static Dictionary<string, string> BuildEmailTokens(
            string userName,
            string branchName,
            DateTime fromDate,
            DateTime toDate)
        {
            return new Dictionary<string, string>
            {
                ["UserName"] = string.IsNullOrWhiteSpace(userName) ? "مستخدم" : userName,
                ["BranchName"] = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName,
                ["FromDate"] = fromDate.ToString("yyyy/MM/dd"),
                ["ToDate"] = toDate.ToString("yyyy/MM/dd"),
                ["ReportTitle"] = ReportTitle
            };
        }
    }
}

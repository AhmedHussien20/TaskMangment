using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class EmployeeCommentsReportDto
    {
        public string EmployeeName { get; set; }
        public int CommentsCount { get; set; }
    }

    public class EmployeeCommentsActivityReportDto
    {
        public string EmployeeName { get; set; }

        public int TotalComments { get; set; }
        public int DistinctTasksCount { get; set; }

        public decimal AvgCommentsPerTask { get; set; }

        public DateTime? LastCommentDate { get; set; }
    }

}

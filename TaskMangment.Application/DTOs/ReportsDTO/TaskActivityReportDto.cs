using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class TaskActivityReportDto
    {
        public string TaskTitleWithId { get; set; } 
        public string AssignedBy { get; set; }  
        public string? Comment { get; set; }      
        public DateTime CommentDate { get; set; } 
        public string CommentedBy { get; set; }
    }
    public enum ExportType
    {
        Pdf = 1,
        Excel = 2
    }

}

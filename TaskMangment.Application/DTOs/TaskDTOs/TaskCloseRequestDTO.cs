using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class TaskCloseRequestAddDto
    {
        [Required, MaxLength(1000)]
        public string Message { get; set; }
    }
    public class TaskCloseRequestListDto
    {
        public string Message { get; set; }
        public CloseRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public string RequestedByName { get; set; }

    }
    public class TaskCloseRequestDetailsDto
    {
        public string Message { get; set; }
        public CloseRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewedByName { get; set; }
        public string? RequestedByName { get; set; }

    }
}

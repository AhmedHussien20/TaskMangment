using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class TaskExtensionRequestAddDto
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; }
    }
    public class TaskExtensionRequestListDto
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public string TaskTitle { get; set; }


        public string Reason { get; set; }
        public ExtensionRequestStatus Status { get; set; }
        public string RequestedByName { get; set; }
        public DateTime RequestedAt { get; set; }
    }
    public class TaskExtensionRequestDetailsDto
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public string TaskTitle { get; set; }

        public string Reason { get; set; }
        public ExtensionRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? RequestedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewedByName { get; set; }
    }
}

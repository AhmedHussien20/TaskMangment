using System;
using System.Collections.Generic;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs
{
    public class TaskNotificationDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public NotificationType NotificationType { get; set; }
        public int ReferenceId { get; set; }
        public int? TaskId { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<AttachmentVm> Attachments { get; set; } = new();
    }
}

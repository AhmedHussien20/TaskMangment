using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public enum TaskPriority : byte { Low = 1, Medium = 2, High = 3 }
    public enum WorkTaskStatus : byte { New = 1, InProgress = 2, Closed = 3, Archived = 4 }
    public enum ExtensionRequestStatus : byte { Pending = 1, Approved = 2, Rejected = 3 }
    public enum CloseRequestStatus : byte { Pending = 1, Approved = 2, Rejected = 3 }

    public enum NotificationChannel
    {
        Web,
        Email,
        WhatsApp
    }

    public enum AttachmentType
    {
        Task = 1,
        Comment = 2,
        Voucher = 3,
        Employee = 4,
    }

    public enum EmailStatus
    {
        Pending = 1,
        Processing = 2,
        Sent = 3,
        Failed = 4
    }

    public enum ReferenceType
    {
        Task = 1,
        Event = 2,

        TaskComment = 3,
        TaskExtensionRequest = 4,
        TaskCloseRequest = 5,

        EmployeeWarning = 6,
        EmployeeDeduction = 7,
        TaskDueTodayReminder = 8,
        CourseOffer = 9,

        TaskExtensionRequestApproved = 10,
        TaskCloseRequestApproved = 11,
        TaskAchieve = 12,

    }
    public enum RecipientType
    {
        Employee,
        Student
    }

    public enum RoleLevelEnum
    {
        Employee = 10,
        TeamLead = 50,
        Manager = 70,
        Admin = 100
    }

    public enum CommentAllowPeriod
    {
        Daily = 1,      
        Weekly = 7,    
        Monthly = 30  
    }


}

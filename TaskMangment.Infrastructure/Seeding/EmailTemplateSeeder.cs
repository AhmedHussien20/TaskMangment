using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Seeding
{
    public static class EmailTemplateSeeder
    {
        const string LayoutHeader = @"
<div dir='rtl' style='background-color:#f4f6f8;padding:20px;font-family:Tahoma,Arial'>
  <div style='max-width:600px;margin:auto;background:#ffffff;border-radius:8px;overflow:hidden'>
    <div style='background:#0d6efd;color:#ffffff;padding:15px;text-align:center;font-size:18px;font-weight:bold'>
      نظام إدارة المهام | Task Management System
    </div>
    <div style='padding:20px;color:#333;font-size:14px;line-height:1.8'>
";

        const string LayoutFooter = @"
    </div>
    <div style='background:#f1f1f1;padding:10px;text-align:center;font-size:12px;color:#777'>
      هذا البريد مرسل تلقائيًا – يرجى عدم الرد<br/>
      This is an automated email – please do not reply
    </div>
  </div>
</div>";

        public static void Seed(AppDbContext context)
        {
            if (context.EmailTemplates.Any())
                return;

            context.EmailTemplates.AddRange(

                // 🟢 Task Assigned
                new EmailTemplate
                {
                    Key = "TaskAssigned",
                    SubjectTemplate = "تم إسناد مهمة جديدة | New Task Assigned",
                    BodyTemplate = LayoutHeader + @"
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
تم إسناد مهمة جديدة إليك.<br/><br/>

<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
<strong>تاريخ الاستحقاق:</strong> {{DueDate}}<br/><br/>

يرجى الدخول إلى النظام لمتابعة تفاصيل المهمة.<br/><br/>

<hr/>

<strong>Hello {{UserName}},</strong><br/><br/>
A new task has been assigned to you.<br/><br/>

<strong>Task Title:</strong> {{TaskTitle}}<br/>
<strong>Due Date:</strong> {{DueDate}}<br/><br/>

Please log in to the system to view task details.
" + LayoutFooter
                },

                // 💬 Task Comment Added
                new EmailTemplate
                {
                    Key = "TaskCommentAdded",
                    SubjectTemplate = "تعليق جديد على المهمة | New Task Comment",
                    BodyTemplate = LayoutHeader + @"
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
تمت إضافة تعليق جديد على المهمة:<br/><br/>

<strong>{{TaskTitle}}</strong><br/>
<div style='background:#f8f9fa;padding:10px;border-right:4px solid #0d6efd'>
{{CommentText}}
</div><br/>

<hr/>

<strong>Hello {{UserName}},</strong><br/><br/>
A new comment has been added to the following task:<br/><br/>

<strong>{{TaskTitle}}</strong><br/>
<div style='background:#f8f9fa;padding:10px;border-left:4px solid #0d6efd'>
{{CommentText}}
</div>
" + LayoutFooter
                },

                // 📅 Event Reminder
                new EmailTemplate
                {
                    Key = "EventReminder",
                    SubjectTemplate = "تذكير بموعد حدث | Event Reminder",
                    BodyTemplate = LayoutHeader + @"
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
هذا تذكير بموعد الحدث التالي:<br/><br/>

<strong>اسم الحدث:</strong> {{EventTitle}}<br/>
<strong>التاريخ والوقت:</strong> {{EventDate}}<br/><br/>

<hr/>

<strong>Hello {{UserName}},</strong><br/><br/>
This is a reminder for the following event:<br/><br/>

<strong>Event Title:</strong> {{EventTitle}}<br/>
<strong>Date & Time:</strong> {{EventDate}}
" + LayoutFooter
                },

                // ⏳ Task Extension Request
                new EmailTemplate
                {
                    Key = "TaskExtensionRequest",
                    SubjectTemplate = "طلب تمديد موعد المهمة | Task Extension Request",
                    BodyTemplate = LayoutHeader + @"
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
تم تقديم طلب لتمديد موعد المهمة التالية:<br/><br/>

<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
<strong>سبب الطلب:</strong> {{ExtensionReason}}<br/><br/>

<hr/>

<strong>Hello {{UserName}},</strong><br/><br/>
A request has been submitted to extend the deadline of the following task:<br/><br/>

<strong>Task Title:</strong> {{TaskTitle}}<br/>
<strong>Reason:</strong> {{ExtensionReason}}
" + LayoutFooter
                },

                // ✅ Task Close Request
                new EmailTemplate
                {
                    Key = "TaskCloseRequest",
                    SubjectTemplate = "طلب إغلاق المهمة | Task Close Request",
                    BodyTemplate = LayoutHeader + @"
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
تم تقديم طلب لإغلاق المهمة التالية:<br/><br/>

<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
<strong>ملاحظات الإغلاق:</strong> {{CloseNotes}}<br/><br/>

<hr/>

<strong>Hello {{UserName}},</strong><br/><br/>
A request has been submitted to close the following task:<br/><br/>

<strong>Task Title:</strong> {{TaskTitle}}<br/>
<strong>Close Notes:</strong> {{CloseNotes}}
" + LayoutFooter
                },

                // ⚠️ Warning
                new EmailTemplate
                {
                    Key = "EmployeeWarning",
                    SubjectTemplate = "تحذير إداري | Administrative Warning",
                    BodyTemplate = LayoutHeader + @"
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
نود تنبيهكم بوجود ملاحظة تتعلق بالأداء الوظيفي:<br/><br/>

<div style='background:#fff3cd;padding:10px;border-right:4px solid #ffc107'>
{{WarningReason}}
</div><br/>

<hr/>

<strong>Hello {{UserName}},</strong><br/><br/>
This is an administrative warning regarding the following issue:<br/><br/>

<div style='background:#fff3cd;padding:10px;border-left:4px solid #ffc107'>
{{WarningReason}}
</div>
" + LayoutFooter
                },

                // 💸 Employee Deduction (عدم الإنجاز)
                new EmailTemplate
                {
                    Key = "EmployeeDeduction",
                    SubjectTemplate = "إشعار بخصم إداري تلقائي | Automated Deduction Notice",
                    BodyTemplate = LayoutHeader + @"
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
نحيطكم علمًا بأنه قد تم <strong>تطبيق خصم إداري تلقائي</strong>، وذلك لعدم إنجاز المهمة في الموعد المحدد، وفقًا للتفاصيل التالية:<br/><br/>

<strong>رقم المهمة:</strong> {{TaskNumber}}<br/>
<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
<strong>قيمة الخصم:</strong> {{DeductionAmount}}<br/><br/>

يرجى العلم أن هذا الإجراء تم <strong>آليًا من خلال النظام</strong> ولا يمكن الرد على هذا البريد الإلكتروني.<br/><br/>

<hr/>

<strong>Hello {{UserName}},</strong><br/><br/>
An <strong>automatic administrative deduction</strong> has been applied due to failure to complete the assigned task within the specified deadline:<br/><br/>

<strong>Task Number:</strong> {{TaskNumber}}<br/>
<strong>Task Title:</strong> {{TaskTitle}}<br/>
<strong>Deduction Amount:</strong> {{DeductionAmount}}<br/><br/>

This action was automatically generated by the system and this email is not monitored for replies.
" + LayoutFooter
                },
                new EmailTemplate
                {
                    Key = "TaskDueTodayReminder",
                    SubjectTemplate = "تذكير هام: موعد انتهاء المهمة اليوم | Task Due Today",
                    BodyTemplate = LayoutHeader + @"
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

نود تذكيرك بأن موعد انتهاء المهمة التالية هو <strong>اليوم</strong>.<br/><br/>

<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
<strong>تاريخ الاستحقاق:</strong> {{DueDate}}<br/><br/>

يرجى التأكد من إنهاء المهمة في الموعد المحدد أو اتخاذ الإجراء اللازم.<br/><br/>

<hr/>

<strong>Hello {{UserName}},</strong><br/><br/>

This is a reminder that the due date for the following task is <strong>today</strong>.<br/><br/>

<strong>Task Title:</strong> {{TaskTitle}}<br/>
<strong>Due Date:</strong> {{DueDate}}<br/><br/>

Please ensure the task is completed on time or take the necessary action.
" + LayoutFooter
                },
                new EmailTemplate
                {
                    Key = "TaskAssignedToExistingTask",
                    SubjectTemplate = "تم إسنادك إلى مهمة | Assigned to Existing Task",
                    BodyTemplate = LayoutHeader + @"
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

نود إعلامك بأنه تم <strong>إسنادك</strong> إلى مهمة موجودة بالفعل ضمن النظام.<br/><br/>

<strong>تفاصيل المهمة:</strong><br/>
<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
<strong>الوصف:</strong> {{TaskDescription}}<br/>
<strong>تاريخ الاستحقاق:</strong> {{DueDate}}<br/><br/>

يرجى مراجعة تفاصيل المهمة والبدء في تنفيذها في أقرب وقت ممكن.<br/><br/>

<hr/>

<strong>Hello {{UserName}},</strong>،<br/><br/>

You have been <strong>assigned</strong> to an existing task in the system.<br/><br/>

<strong>Task Details:</strong><br/>
<strong>Task Title:</strong> {{TaskTitle}}<br/>
<strong>Description:</strong> {{TaskDescription}}<br/>
<strong>Due Date:</strong> {{DueDate}}<br/><br/>

Please review the task details and start working on it as soon as possible.
" + LayoutFooter
                },
                new EmailTemplate
                {
                    Key = "TaskUnAssignedFromExistingTask",
                    SubjectTemplate = "تم إلغاء إسنادك من المهمة | Unassigned from Task",
                    BodyTemplate = LayoutHeader + @"
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

نود إعلامك بأنه تم <strong>إلغاء إسنادك</strong> من المهمة التالية:<br/><br/>

<strong>تفاصيل المهمة:</strong><br/>
<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
<strong>الوصف:</strong> {{TaskDescription}}<br/><br/>

لم يعد مطلوبًا منك العمل على هذه المهمة حاليًا.<br/><br/>

<hr/>

<strong>Hello {{UserName}},</strong><br/><br/>

You have been <strong>unassigned</strong> from the following task:<br/><br/>

<strong>Task Details:</strong><br/>
<strong>Task Title:</strong> {{TaskTitle}}<br/>
<strong>Description:</strong> {{TaskDescription}}<br/><br/>

You are no longer required to work on this task at this time.
" + LayoutFooter
                }

            );

            context.SaveChanges();
        }
    }


}

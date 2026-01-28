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
//            if (context.EmailTemplates.Any())
//                return;

            context.EmailTemplates.AddRange(

                           //                // 🟢 Task Assigned
                           //                new EmailTemplate
                           //                {
                           //                    Key = "TaskAssigned",
                           //                    SubjectTemplate = "تم إسناد مهمة جديدة | New Task Assigned",
                           //                    BodyTemplate = LayoutHeader + @"
                           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                           //تم إسناد مهمة جديدة إليك.<br/><br/>

                           //<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
                           //<strong>تاريخ الاستحقاق:</strong> {{DueDate}}<br/><br/>

                           //يرجى الدخول إلى النظام لمتابعة تفاصيل المهمة.<br/><br/>

                           //<hr/>

                           //<strong>Hello {{UserName}},</strong><br/><br/>
                           //A new task has been assigned to you.<br/><br/>

                           //<strong>Task Title:</strong> {{TaskTitle}}<br/>
                           //<strong>Due Date:</strong> {{DueDate}}<br/><br/>

                           //Please log in to the system to view task details.
                           //" + LayoutFooter
                           //                },

                           //                // 💬 Task Comment Added
                           //                new EmailTemplate
                           //                {
                           //                    Key = "TaskCommentAdded",
                           //                    SubjectTemplate = "تعليق جديد على المهمة | New Task Comment",
                           //                    BodyTemplate = LayoutHeader + @"
                           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                           //تمت إضافة تعليق جديد على المهمة:<br/><br/>

                           //<strong>{{TaskTitle}}</strong><br/>
                           //<div style='background:#f8f9fa;padding:10px;border-right:4px solid #0d6efd'>
                           //{{CommentText}}
                           //</div><br/>

                           //<hr/>

                           //<strong>Hello {{UserName}},</strong><br/><br/>
                           //A new comment has been added to the following task:<br/><br/>

                           //<strong>{{TaskTitle}}</strong><br/>
                           //<div style='background:#f8f9fa;padding:10px;border-left:4px solid #0d6efd'>
                           //{{CommentText}}
                           //</div>
                           //" + LayoutFooter
                           //                },

                           //                // 📅 Event Reminder
                           //                new EmailTemplate
                           //                {
                           //                    Key = "EventReminder",
                           //                    SubjectTemplate = "تذكير بموعد حدث | Event Reminder",
                           //                    BodyTemplate = LayoutHeader + @"
                           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                           //هذا تذكير بموعد الحدث التالي:<br/><br/>

                           //<strong>اسم الحدث:</strong> {{EventTitle}}<br/>
                           //<strong>التاريخ والوقت:</strong> {{EventDate}}<br/><br/>

                           //<hr/>

                           //<strong>Hello {{UserName}},</strong><br/><br/>
                           //This is a reminder for the following event:<br/><br/>

                           //<strong>Event Title:</strong> {{EventTitle}}<br/>
                           //<strong>Date & Time:</strong> {{EventDate}}
                           //" + LayoutFooter
                           //                },

                           //                // ⏳ Task Extension Request
                           //                new EmailTemplate
                           //                {
                           //                    Key = "TaskExtensionRequest",
                           //                    SubjectTemplate = "طلب تمديد موعد المهمة | Task Extension Request",
                           //                    BodyTemplate = LayoutHeader + @"
                           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                           //تم تقديم طلب لتمديد موعد المهمة التالية:<br/><br/>

                           //<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
                           //<strong>سبب الطلب:</strong> {{ExtensionReason}}<br/><br/>

                           //<hr/>

                           //<strong>Hello {{UserName}},</strong><br/><br/>
                           //A request has been submitted to extend the deadline of the following task:<br/><br/>

                           //<strong>Task Title:</strong> {{TaskTitle}}<br/>
                           //<strong>Reason:</strong> {{ExtensionReason}}
                           //" + LayoutFooter
                           //                },

                           //                // ✅ Task Close Request
                           //                new EmailTemplate
                           //                {
                           //                    Key = "TaskCloseRequest",
                           //                    SubjectTemplate = "طلب إغلاق المهمة | Task Close Request",
                           //                    BodyTemplate = LayoutHeader + @"
                           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                           //تم تقديم طلب لإغلاق المهمة التالية:<br/><br/>

                           //<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
                           //<strong>ملاحظات الإغلاق:</strong> {{CloseNotes}}<br/><br/>

                           //<hr/>

                           //<strong>Hello {{UserName}},</strong><br/><br/>
                           //A request has been submitted to close the following task:<br/><br/>

                           //<strong>Task Title:</strong> {{TaskTitle}}<br/>
                           //<strong>Close Notes:</strong> {{CloseNotes}}
                           //" + LayoutFooter
                           //                },

                           //                // ⚠️ Warning
                           //                new EmailTemplate
                           //                {
                           //                    Key = "EmployeeWarning",
                           //                    SubjectTemplate = "تحذير إداري | Administrative Warning",
                           //                    BodyTemplate = LayoutHeader + @"
                           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                           //نود تنبيهكم بوجود ملاحظة تتعلق بالأداء الوظيفي:<br/><br/>

                           //<div style='background:#fff3cd;padding:10px;border-right:4px solid #ffc107'>
                           //{{WarningReason}}
                           //</div><br/>

                           //<hr/>

                           //<strong>Hello {{UserName}},</strong><br/><br/>
                           //This is an administrative warning regarding the following issue:<br/><br/>

                           //<div style='background:#fff3cd;padding:10px;border-left:4px solid #ffc107'>
                           //{{WarningReason}}
                           //</div>
                           //" + LayoutFooter
                           //                },

                           //                // 💸 Employee Deduction (عدم الإنجاز)
                           new EmailTemplate
                           {
                               Key = "EmployeeDeduction",
                               SubjectTemplate = "إشعار بخصم إداري تلقائي | Automated Deduction Notice",
                               BodyTemplate = LayoutHeader + @"
           مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

نود إحاطتكم علمًا بأنه تم <strong>تطبيق خصم إداري تلقائي</strong> على إحدى المهام. تفاصيل المهمة كالتالي:<br/><br/>

<strong>اسم الموظف المجذي:</strong> {{EmployeeName}}<br/>
<strong>رقم المهمة:</strong> {{TaskNumber}}<br/>
<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
<strong>قيمة الخصم:</strong> {{DeductionAmount}}<br/><br/>
<strong>سبب الخصم:</strong> {{DeductionReason}}<br/><br/>


يرجى العلم أن هذا الإجراء تم <strong>آليًا بالكامل من خلال النظام</strong>، وذلك لأغراض التوثيق والمتابعة الإدارية.<br/><br/>

<hr/>

<strong>Hello {{UserName}},</strong><br/><br/>

This email is to inform you that an <strong>automatic administrative deduction</strong> has been applied to a task. Task details are as follows:<br/><br/>

<strong>Deducted Employee Name:</strong> {{EmployeeName}}<br/>
<strong>Task Number:</strong> {{TaskNumber}}<br/>
<strong>Task Title:</strong> {{TaskTitle}}<br/>
<strong>Deduction Amount:</strong> {{DeductionAmount}}<br/><br/>
<strong>Deduction Reason:</strong> {{DeductionReason}}<br/><br/>


Please note that this action was <strong>automatically generated by the system</strong> for documentation and administrative tracking purposes. This email is not monitored for replies.

           " + LayoutFooter
                           }
           //                new EmailTemplate
           //                {
           //                    Key = "TaskDueTodayReminder",
           //                    SubjectTemplate = "تذكير هام: موعد انتهاء المهمة اليوم | Task Due Today",
           //                    BodyTemplate = LayoutHeader + @"
           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

           //نود تذكيرك بأن موعد انتهاء المهمة التالية هو <strong>اليوم</strong>.<br/><br/>

           //<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
           //<strong>تاريخ الاستحقاق:</strong> {{DueDate}}<br/><br/>

           //يرجى التأكد من إنهاء المهمة في الموعد المحدد أو اتخاذ الإجراء اللازم.<br/><br/>

           //<hr/>

           //<strong>Hello {{UserName}},</strong><br/><br/>

           //This is a reminder that the due date for the following task is <strong>today</strong>.<br/><br/>

           //<strong>Task Title:</strong> {{TaskTitle}}<br/>
           //<strong>Due Date:</strong> {{DueDate}}<br/><br/>

           //Please ensure the task is completed on time or take the necessary action.
           //" + LayoutFooter
           //                },
           //                new EmailTemplate
           //                {
           //                    Key = "TaskAssignedToExistingTask",
           //                    SubjectTemplate = "تم إسنادك إلى مهمة | Assigned to Existing Task",
           //                    BodyTemplate = LayoutHeader + @"
           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

           //نود إعلامك بأنه تم <strong>إسنادك</strong> إلى مهمة موجودة بالفعل ضمن النظام.<br/><br/>

           //<strong>تفاصيل المهمة:</strong><br/>
           //<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
           //<strong>الوصف:</strong> {{TaskDescription}}<br/>
           //<strong>تاريخ الاستحقاق:</strong> {{DueDate}}<br/><br/>

           //يرجى مراجعة تفاصيل المهمة والبدء في تنفيذها في أقرب وقت ممكن.<br/><br/>

           //<hr/>

           //<strong>Hello {{UserName}},</strong>،<br/><br/>

           //You have been <strong>assigned</strong> to an existing task in the system.<br/><br/>

           //<strong>Task Details:</strong><br/>
           //<strong>Task Title:</strong> {{TaskTitle}}<br/>
           //<strong>Description:</strong> {{TaskDescription}}<br/>
           //<strong>Due Date:</strong> {{DueDate}}<br/><br/>

           //Please review the task details and start working on it as soon as possible.
           //" + LayoutFooter
           //                },
           //                new EmailTemplate
           //                {
           //                    Key = "TaskUnAssignedFromExistingTask",
           //                    SubjectTemplate = "تم إلغاء إسنادك من المهمة | Unassigned from Task",
           //                    BodyTemplate = LayoutHeader + @"
           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

           //نود إعلامك بأنه تم <strong>إلغاء إسنادك</strong> من المهمة التالية:<br/><br/>

           //<strong>تفاصيل المهمة:</strong><br/>
           //<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
           //<strong>الوصف:</strong> {{TaskDescription}}<br/><br/>

           //لم يعد مطلوبًا منك العمل على هذه المهمة حاليًا.<br/><br/>

           //<hr/>

           //<strong>Hello {{UserName}},</strong><br/><br/>

           //You have been <strong>unassigned</strong> from the following task:<br/><br/>

           //<strong>Task Details:</strong><br/>
           //<strong>Task Title:</strong> {{TaskTitle}}<br/>
           //<strong>Description:</strong> {{TaskDescription}}<br/><br/>

           //You are no longer required to work on this task at this time.
           //" + LayoutFooter
           //                },





           ////////////Extension request approved/////
           //                new EmailTemplate
           //                {
           //                    Key = "TaskExtensionApproved",
           //                    SubjectTemplate = "تمت الموافقة على تمديد المهمة | Task Extension Approved",
           //                    BodyTemplate = LayoutHeader + @"
           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

           //نود إعلامك بأنه تمت الموافقة على طلب تمديد المهمة.<br/><br/>

           //<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
           //<strong>تاريخ الاستحقاق السابق:</strong> {{OldDueDate}}<br/>
           //<strong>تاريخ الاستحقاق الجديد:</strong> {{NewDueDate}}<br/><br/>

           //يرجى متابعة المهمة وفقًا للتاريخ الجديد.<br/><br/>

           //<hr/>

           //<strong>Hello {{UserName}},</strong><br/><br/>

           //We would like to inform you that your task extension request has been approved.<br/><br/>

           //<strong>Task Title:</strong> {{TaskTitle}}<br/>
           //<strong>Previous Due Date:</strong> {{OldDueDate}}<br/>
           //<strong>New Due Date:</strong> {{NewDueDate}}<br/><br/>

           //Please proceed with the task according to the updated due date.
           //" + LayoutFooter
           //                },



           //                new EmailTemplate
           //                {
           //                    Key = "TaskCloseApproved",
           //                    SubjectTemplate = "تمت الموافقة على إغلاق المهمة | Task Closure Approved",
           //                    BodyTemplate = LayoutHeader + @"
           //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

           //نود إعلامك بأنه تمت الموافقة على طلب إغلاق المهمة.<br/><br/>

           //<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>

           //شكرًا لالتزامك وإنجازك للمهمة.<br/><br/>

           //<hr/>

           //<strong>Hello {{UserName}},</strong><br/><br/>

           //We are pleased to inform you that your task closure request has been approved.<br/><br/>

           //<strong>Task Title:</strong> {{TaskTitle}}<br/>

           //Thank you for completing the task successfully.
           //" + LayoutFooter
           //                }



           //                new EmailTemplate
           //                {
           //                    Key = "LeaveRequestCreated",
           //                    SubjectTemplate = "طلب إجازة جديد | New Leave Request",
           //                    BodyTemplate = LayoutHeader + @"
           //مرحبًا،<br/><br/>

           //تم تقديم <strong>طلب إجازة جديد</strong> من الموظف:<br/><br/>

           //<strong>اسم الموظف:</strong> {{EmployeeName}}<br/>
           //<strong>نوع الإجازة:</strong> {{LeaveType}}<br/>
           //<strong>من:</strong> {{StartDate}}<br/>
           //<strong>إلى:</strong> {{EndDate}}<br/><br/>

           //يرجى مراجعة الطلب من النظام.<br/><br/>

           //<hr/>

           //<strong>Hello,</strong><br/><br/>

           //A <strong>new leave request</strong> has been submitted:<br/><br/>

           //<strong>Employee:</strong> {{EmployeeName}}<br/>
           //<strong>Leave Type:</strong> {{LeaveType}}<br/>
           //<strong>From:</strong> {{StartDate}}<br/>
           //<strong>To:</strong> {{EndDate}}<br/>
           //" + LayoutFooter
           //                }

//           new EmailTemplate
//           {
//               Key = "LeaveApproved",
//               SubjectTemplate = "تمت الموافقة على طلب الإجازة | Leave Approved",
//               BodyTemplate = LayoutHeader + @"
//مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

//نود إعلامك بأنه تم <strong>الموافقة</strong> على طلب الإجازة الخاص بك.<br/><br/>

//<strong>نوع الإجازة:</strong> {{LeaveType}}<br/>
//<strong>من:</strong> {{StartDate}}<br/>
//<strong>إلى:</strong> {{EndDate}}<br/>
//<strong>تمت الموافقة بواسطة:</strong> {{ApprovedBy}}<br/><br/>

//<hr/>

//<strong>Hello {{UserName}},</strong><br/><br/>

//We are pleased to inform you that your <strong>leave request has been approved</strong>.<br/><br/>

//<strong>Leave Type:</strong> {{LeaveType}}<br/>
//<strong>From:</strong> {{StartDate}}<br/>
//<strong>To:</strong> {{EndDate}}<br/>
//<strong>Approved By:</strong> {{ApprovedBy}}<br/><br/>

//" + LayoutFooter
//           },

//           new EmailTemplate
//           {
//               Key = "LeaveRejected",
//               SubjectTemplate = "تم رفض طلب الإجازة | Leave Rejected",
//               BodyTemplate = LayoutHeader + @"
//مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

//نأسف لإبلاغك بأنه تم <strong>رفض</strong> طلب الإجازة الخاص بك.<br/><br/>

//<strong>نوع الإجازة:</strong> {{LeaveType}}<br/>
//<strong>من:</strong> {{StartDate}}<br/>
//<strong>إلى:</strong> {{EndDate}}<br/>
//<strong>سبب الرفض:</strong> {{RejectReason}}<br/>
//<strong>تم الرفض بواسطة:</strong> {{RejectedBy}}<br/><br/>

//<hr/>

//<strong>Hello {{UserName}},</strong><br/><br/>

//We regret to inform you that your <strong>leave request has been rejected</strong>.<br/><br/>

//<strong>Leave Type:</strong> {{LeaveType}}<br/>
//<strong>From:</strong> {{StartDate}}<br/>
//<strong>To:</strong> {{EndDate}}<br/>
//<strong>Reject Reason:</strong> {{RejectReason}}<br/>
//<strong>Rejected By:</strong> {{RejectedBy}}<br/><br/>

//" + LayoutFooter
//           }





            //Task Achievement
            //                          new EmailTemplate
            //                          {
            //                              Key = "TaskAchievement",
            //                              SubjectTemplate = "إضافة نسبة إنجاز على المهمة | Task Achievement Added",
            //                              BodyTemplate = LayoutHeader + @"
            //مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

            //قام الموظف <strong>{{EmployeeName}}</strong> بإضافة نسبة إنجاز جديدة على المهمة التالية:<br/><br/>

            //<strong>المهمة:</strong> {{TaskTitle}}<br/>
            //<strong>نسبة الإنجاز:</strong> {{Percent}}<br/><br/>

            //<hr/>

            //<strong>Hello {{UserName}},</strong><br/><br/>

            //Employee <strong>{{EmployeeName}}</strong> has added a new achievement rate to the following task:<br/><br/>

            //<strong>Task:</strong> {{TaskTitle}}<br/>
            //<strong>Achievement Rate:</strong> {{Percent}}<br/><br/>
            //" + LayoutFooter
            //                          }








            // 💼 Offer Sent To Student
            //new EmailTemplate
            //{
            //    Key = "OfferSent",
            //    SubjectTemplate = "عرض سعر جديد | New Price Offer",
            //    BodyTemplate = LayoutHeader + @"
            //مرحبًا <strong>{{StudentName}}</strong>،<br/><br/>

            //نود إبلاغكم بأنه تم إرسال <strong>عرض سعر جديد</strong> لكم، وفق التفاصيل التالية:<br/><br/>

            //<table style='width:100%;border-collapse:collapse;font-size:14px'>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>عنوان العرض</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{OfferTitle}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>الوصف</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{OfferDescription}}</td>
            //  </tr>
            //<tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>الكورس</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{CourseName}}</td>
            //  </tr>
            //<tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>مادة الكورس</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{SubjectName}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>تاريخ البداية</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{StartDate}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>تاريخ الانتهاء</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{EndDate}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>طريقة الدفع</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{PaymentMethod}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>السعر</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{Price}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>معدل الفائدة</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{InterestRate}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>نسبة الخصم</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{DiscountRate}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>قيمة القسط</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{InstallmentValue}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>المبلغ الصافي</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{NetAmount}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>مالك العرض</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{OfferOwner}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>التخصص</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{Specialization}}</td>
            //  </tr>

            //</table>

            //<br/>

            //<div style='margin-top:15px;padding:10px;background:#fff3cd;border-right:4px solid #ffc107;color:#856404'>
            //<strong>تنويه:</strong> هذا العرض ساري لمدة <strong>أسبوع واحد فقط</strong> من تاريخ الإرسال.
            //</div>

            //<br/>

            //يرجى مراجعة العرض والتواصل معنا في حال وجود أي استفسارات.<br/><br/>

            //<hr/>

            //<strong>Hello {{StudentName}},</strong><br/><br/>

            //We are pleased to inform you that a <strong>new price offer</strong> has been sent to you with the following details:<br/><br/>

            //<table style='width:100%;border-collapse:collapse;font-size:14px'>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Offer Title</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{OfferTitle}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Description</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{OfferDescription}}</td>
            //  </tr>
            //<tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Course</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{CourseName}}</td>
            //  </tr>
            //<tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Course subject</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{SubjectName}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Start Date</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{StartDate}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>End Date</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{EndDate}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Payment Method</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{PaymentMethod}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Price</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{Price}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Interest Rate</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{InterestRate}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Discount Rate</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{DiscountRate}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Installment Value</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{InstallmentValue}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Net Amount</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{NetAmount}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Offer Owner</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{OfferOwner}}</td>
            //  </tr>
            //  <tr>
            //    <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Specialization</strong></td>
            //    <td style='padding:6px;border:1px solid #ddd'>{{Specialization}}</td>
            //  </tr>

            //</table>

            //<br/>

            //<div style='margin-top:15px;padding:10px;background:#fff3cd;border-left:4px solid #ffc107;color:#856404'>
            //<strong>Note:</strong> This offer is valid for <strong>one week only</strong> from the sending date.
            //</div>

            //<br/>

            //Please review the offer and contact us if you have any questions.
            //" + LayoutFooter

            //}


            );

            context.SaveChanges();
        }
    }


}

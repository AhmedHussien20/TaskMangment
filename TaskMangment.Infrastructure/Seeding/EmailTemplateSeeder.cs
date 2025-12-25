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
      نظام إدارة المهام
    </div>
    <div style='padding:20px;color:#333;font-size:14px;line-height:1.8'>
";

        const string LayoutFooter = @"
    </div>
    <div style='background:#f1f1f1;padding:10px;text-align:center;font-size:12px;color:#777'>
      هذا البريد مرسل تلقائيًا – يرجى عدم الرد
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
                    SubjectTemplate = "تم إسناد مهمة جديدة: {{TaskTitle}}",
                    BodyTemplate = LayoutHeader + @"
                                            مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                                            تم إسناد مهمة جديدة إليك.<br/><br/>

                                            <strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
                                            <strong>تاريخ الاستحقاق:</strong> {{DueDate}}<br/><br/>

                                            يرجى الدخول إلى النظام لمتابعة تفاصيل المهمة.
" + LayoutFooter
                },

                // 💬 Task Comment Added
                new EmailTemplate
                {
                    Key = "TaskCommentAdded",
                    SubjectTemplate = "تعليق جديد على المهمة: {{TaskTitle}}",
                    BodyTemplate = LayoutHeader + @"
                        مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                        تم إضافة تعليق جديد على المهمة:<br/><br/>

                        <strong>{{TaskTitle}}</strong><br/><br/>
                        <div style='background:#f8f9fa;padding:10px;border-right:4px solid #0d6efd'>
                        {{CommentText}}
                        </div>
" + LayoutFooter
                },

                // 📅 Event Reminder
                new EmailTemplate
                {
                    Key = "EventReminder",
                    SubjectTemplate = "تذكير بموعد الحدث: {{EventTitle}}",
                    BodyTemplate = LayoutHeader + @"
                        مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                        هذا تذكير بموعد الحدث التالي:<br/><br/>

                        <strong>اسم الحدث:</strong> {{EventTitle}}<br/>
                        <strong>التاريخ والوقت:</strong> {{EventDate}}<br/><br/>
                        نرجو الالتزام بالموعد المحدد.
" + LayoutFooter
                },

                // ⏳ Task Extension Request
                new EmailTemplate
                {
                    Key = "TaskExtensionRequest",
                    SubjectTemplate = "طلب تمديد موعد المهمة: {{TaskTitle}}",
                    BodyTemplate = LayoutHeader + @" 
مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                                تم تقديم طلب لتمديد موعد المهمة التالية:<br/><br/>

                                <strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
                                <strong>السبب:</strong> {{ExtensionReason}}<br/><br/>

                                يرجى مراجعة الطلب واتخاذ الإجراء المناسب.
" + LayoutFooter
                },

                // ✅ Task Close Request
                new EmailTemplate
                {
                    Key = "TaskCloseRequest",
                    SubjectTemplate = "طلب إغلاق المهمة: {{TaskTitle}}",
                    BodyTemplate = LayoutHeader + @"
                                       مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                                        تم تقديم طلب لإغلاق المهمة التالية:<br/><br/>

                                        <strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
                                        <strong>ملاحظات الإغلاق:</strong> {{CloseNotes}}<br/><br/>

                                        يرجى مراجعة الطلب.
" + LayoutFooter
                },

                // ⚠️ Warning
                new EmailTemplate
                {
                    Key = "EmployeeWarning",
                    SubjectTemplate = "تنبيه وتحذير بخصوص الأداء الوظيفي",
                    BodyTemplate = LayoutHeader + @"
                                مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                                نود تنبيهكم بوجود ملاحظة تتعلق بالأداء الوظيفي:<br/><br/>

                                <div style='background:#fff3cd;padding:10px;border-right:4px solid #ffc107'>
                                {{WarningReason}}
                                </div>
                                <br/>
                                يرجى الالتزام بالتعليمات لتجنب أي إجراءات لاحقة.
                                " + LayoutFooter
                },

                // 💸 Deduction
                new EmailTemplate
                {
                    Key = "EmployeeDeduction",
                    SubjectTemplate = "إشعار بخصم إداري",
                    BodyTemplate = LayoutHeader + @"
                                                مرحبًا <strong>{{UserName}}</strong>،<br/><br/>
                                                نحيطكم علمًا بأنه قد تم تطبيق خصم إداري وفقًا للتفاصيل التالية:<br/><br/>

                                                <strong>سبب الخصم:</strong> {{DeductionReason}}<br/>
                                                <strong>قيمة الخصم:</strong> {{DeductionAmount}}<br/><br/>

                                                في حال وجود استفسار يرجى التواصل مع الإدارة.
                                                " + LayoutFooter
                },
                // ⏰ Task Due Today Reminder
                new EmailTemplate
                {
                    Key = "TaskDueTodayReminder",
                    SubjectTemplate = "تذكير هام: موعد انتهاء المهمة اليوم - {{TaskTitle}}",
                    BodyTemplate = LayoutHeader + @"
                مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

                نود تذكيرك بأن موعد انتهاء المهمة التالية هو <strong>اليوم</strong>.<br/><br/>

                <strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
                <strong>تاريخ الاستحقاق:</strong> {{DueDate}}<br/><br/>

                يرجى التأكد من إنهاء المهمة في الموعد المحدد أو اتخاذ الإجراء اللازم.
                " + LayoutFooter
                },
                new EmailTemplate
                {
                    Key = "TaskAssignedToExistingTask",
                    SubjectTemplate = "تم إسناد مهمة إليك: {{TaskTitle}}",
                    BodyTemplate = LayoutHeader + @"
                    مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

                    نود إعلامك بأنه تم <strong>إسنادك</strong> إلى مهمة موجودة بالفعل ضمن النظام.<br/><br/>

                    <strong>تفاصيل المهمة:</strong><br/>
                    <strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
                    <strong>الوصف:</strong> {{TaskDescription}}<br/>
                    <strong>تاريخ الاستحقاق:</strong> {{DueDate}}<br/><br/>

                    يرجى مراجعة تفاصيل المهمة والبدء في تنفيذها في أقرب وقت ممكن.<br/><br/>

                    في حال وجود أي استفسار، يرجى التواصل مع مديرك المباشر.
                    " + LayoutFooter
                },
                new EmailTemplate
                {
                    Key = "TaskUnAssignedFromExistingTask",
                    SubjectTemplate = "تم إلغاء إسنادك من المهمة: {{TaskTitle}}",
                    BodyTemplate = LayoutHeader + @"
                    مرحبًا <strong>{{UserName}}</strong>،<br/><br/>

                    نود إعلامك بأنه تم <strong>إلغاء إسنادك</strong> من المهمة التالية:<br/><br/>

                    <strong>تفاصيل المهمة:</strong><br/>
                    <strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
                    <strong>الوصف:</strong> {{TaskDescription}}<br/><br/>

                    لم يعد مطلوبًا منك العمل على هذه المهمة حاليًا.<br/><br/>

                    في حال كان لديك أي استفسار بخصوص هذا التغيير، يرجى التواصل مع مديرك المباشر.
                    " + LayoutFooter
                }
            );

            context.SaveChanges();
        }

    }

}

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
        public static void Seed(AppDbContext context)
        {
            if (context.EmailTemplates.Any())
                return;

            context.EmailTemplates.AddRange(

                new EmailTemplate
                {
                    Key = "TaskAssigned",
                    SubjectTemplate = "تم إسناد مهمة جديدة: {{TaskTitle}}",
                    BodyTemplate = @"
<div dir='rtl' style='font-family: Tahoma, Arial; font-size:14px'>
مرحبًا {{UserName}}،<br/><br/>

تم إسناد مهمة جديدة إليك.<br/><br/>

<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
<strong>تاريخ الاستحقاق:</strong> {{DueDate}}<br/><br/>

يرجى الدخول على النظام لمتابعة تفاصيل المهمة.<br/><br/>

مع تحياتنا،<br/>
<strong>فريق العمل</strong>
</div>"
                },

                new EmailTemplate
                {
                    Key = "TaskCommentAdded",
                    SubjectTemplate = "تعليق جديد على المهمة: {{TaskTitle}}",
                    BodyTemplate = @"
<div dir='rtl' style='font-family: Tahoma, Arial; font-size:14px'>
مرحبًا {{UserName}}،<br/><br/>

تم إضافة تعليق جديد على المهمة التالية:<br/><br/>

<strong>عنوان المهمة:</strong> {{TaskTitle}}<br/>
<strong>التعليق:</strong><br/>
{{CommentText}}<br/><br/>

يرجى مراجعة المهمة للاطلاع على التفاصيل.<br/><br/>

مع تحياتنا،<br/>
<strong>فريق العمل</strong>
</div>"
                },

                new EmailTemplate
                {
                    Key = "EventReminder",
                    SubjectTemplate = "تذكير بموعد الحدث: {{EventTitle}}",
                    BodyTemplate = @"
<div dir='rtl' style='font-family: Tahoma, Arial; font-size:14px'>
مرحبًا {{UserName}}،<br/><br/>

هذا تذكير بموعد الحدث التالي:<br/><br/>

<strong>اسم الحدث:</strong> {{EventTitle}}<br/>
<strong>تاريخ ووقت الحدث:</strong> {{EventDate}}<br/><br/>

نرجو الالتزام بالموعد المحدد.<br/><br/>

مع تحياتنا،<br/>
<strong>فريق العمل</strong>
</div>"
                }
            );

            context.SaveChanges();
        }
    }

}

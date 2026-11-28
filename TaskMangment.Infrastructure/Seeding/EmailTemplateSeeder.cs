using System.Linq;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Seeding
{
    public static class EmailTemplateSeeder
    {
        const string LayoutHeader = @"
<div dir='rtl' style='background-color:#eef2f6;padding:24px 12px;font-family:Tahoma,Arial,sans-serif'>
  <div style='max-width:600px;margin:0 auto;background:#ffffff;border-radius:10px;overflow:hidden;border:1px solid #e3e8ef'>
    <div style='background:#0d6efd;color:#ffffff;padding:18px 20px;text-align:center'>
      <div style='font-size:17px;font-weight:bold;letter-spacing:0.2px'>نظام إدارة المهام</div>
      <div style='font-size:12px;opacity:0.9;margin-top:4px'>Task Management System</div>
    </div>
    <div style='padding:24px 22px;color:#1f2937;font-size:14px;line-height:1.9'>
";

        const string LayoutFooter = @"
    </div>
    <div style='background:#f8fafc;padding:14px 20px;text-align:center;font-size:11px;color:#6b7280;border-top:1px solid #e5e7eb;line-height:1.6'>
      تم إرسال هذه الرسالة تلقائيًا من النظام لإشعارك بالمهمة أو التنبيه. يُرجى عدم الرد على هذه الرسالة، واستخدام النظام لمتابعة المهمة أو إضافة أي تعليق. شكرًا لك.<br/>
      This message was sent automatically by the system. Please do not reply; use the system to follow up on the task or add any comment. Thank you.
    </div>
  </div>
</div>";

        // Inline clickable task number — filled by EmailTemplateRenderer as {{TaskNumberLink}}.
        // CTA button — filled by EmailTemplateRenderer as {{TaskDetailsLink}} when a task URL exists.
        const string TaskDetailsLinkPlaceholder = "{{TaskDetailsLink}}";

        public static void Seed(AppDbContext context)
        {
            // 🟢 Task Assigned (legacy key) — keep neutral {{EmployeeName}} wording from current branch.
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "TaskAssigned",
                SubjectTemplate = "إسناد مهمة | Task Assigned",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>تم إسناد المهمة لـ <strong>{{EmployeeName}}</strong>: مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>تاريخ الاستحقاق:</strong> {{DueDate}}</div>
              <div><strong>الحد الأدنى للتعليقات:</strong> {{MinCommentsPerPeriod}} تعليق خلال كل {{CommentAllowPeriodDays}} يوم</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>Task assigned to <strong>{{EmployeeName}}</strong>: task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>Due Date:</strong> {{DueDate}}</div>
              <div><strong>Minimum comments:</strong> {{MinCommentsPerPeriod}} comment(s) every {{CommentAllowPeriodDays}} day(s)</div>
            </div>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // Assigned to existing task (used by TaskAssignedEmailHandler)
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "TaskAssignedToExistingTask",
                SubjectTemplate = "إسناد مهمة | Task Assigned",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>تم إسناد المهمة لـ <strong>{{EmployeeName}}</strong>: مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>الوصف:</strong> {{TaskDescription}}</div>
              <div style='margin-bottom:6px'><strong>تاريخ الاستحقاق:</strong> {{DueDate}}</div>
              <div><strong>الحد الأدنى للتعليقات:</strong> {{MinCommentsPerPeriod}} تعليق خلال كل {{CommentAllowPeriodDays}} يوم</div>
            </div>
            <p style='margin:0 0 18px'>يرجى مراجعة تفاصيل المهمة وإضافة التعليقات المطلوبة في الفترة المحددة.</p>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>Task assigned to <strong>{{EmployeeName}}</strong>: task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>Description:</strong> {{TaskDescription}}</div>
              <div style='margin-bottom:6px'><strong>Due Date:</strong> {{DueDate}}</div>
              <div><strong>Minimum comments:</strong> {{MinCommentsPerPeriod}} comment(s) every {{CommentAllowPeriodDays}} day(s)</div>
            </div>
            <p style='margin:0 0 18px'>Please review the task details and add the required comments within the period.</p>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // Unassigned — keep neutral {{EmployeeName}} wording from current branch.
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "TaskUnAssignedFromExistingTask",
                SubjectTemplate = "إلغاء إسناد مهمة | Task Unassigned",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>تم إلغاء إسناد <strong>{{EmployeeName}}</strong> من مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div><strong>الوصف:</strong> {{TaskDescription}}</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'><strong>{{EmployeeName}}</strong> was removed from task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div><strong>Description:</strong> {{TaskDescription}}</div>
            </div>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // 💬 Task Comment Added
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "TaskCommentAdded",
                SubjectTemplate = "تعليق جديد على المهمة | New Task Comment",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>تعليق جديد على مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong> من <strong>{{EmployeeName}}</strong> - فرع <strong>{{BranchName}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-right:4px solid #0d6efd;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='font-size:12px;color:#6b7280;margin-bottom:6px'><strong>التعليق</strong></div>
              <div>{{CommentText}}</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>A new comment on task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong> from <strong>{{EmployeeName}}</strong> - branch <strong>{{BranchName}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-left:4px solid #0d6efd;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='font-size:12px;color:#6b7280;margin-bottom:6px'><strong>Comment</strong></div>
              <div>{{CommentText}}</div>
            </div>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });
            // 📅 Event Reminder
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "EventReminder",
                SubjectTemplate = "تذكير بموعد حدث | Event Reminder",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>هذا تذكير بموعد الحدث التالي:</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>اسم الحدث:</strong> {{EventTitle}}</div>
              <div><strong>التاريخ والوقت:</strong> {{EventDate}}</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>This is a reminder for the following event:</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0'>
              <div style='margin-bottom:6px'><strong>Event Title:</strong> {{EventTitle}}</div>
              <div><strong>Date &amp; Time:</strong> {{EventDate}}</div>
            </div>
            " + LayoutFooter
            });

            // ⏳ Task Extension Request
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "TaskExtensionRequest",
                SubjectTemplate = "طلب تمديد موعد المهمة | Task Extension Request",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>طلب تمديد جديد على مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong> من <strong>{{EmployeeName}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div><strong>سبب الطلب:</strong> {{ExtensionReason}}</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>A new extension request on task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong> from <strong>{{EmployeeName}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div><strong>Reason:</strong> {{ExtensionReason}}</div>
            </div>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // ✅ Task Close Request
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "TaskCloseRequest",
                SubjectTemplate = "طلب إغلاق المهمة | Task Close Request",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>طلب غلق جديد على مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong> من <strong>{{EmployeeName}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div><strong>ملاحظات الإغلاق:</strong> {{CloseNotes}}</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>A new close request on task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong> from <strong>{{EmployeeName}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div><strong>Close Notes:</strong> {{CloseNotes}}</div>
            </div>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // ⚠️ Warning
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "EmployeeWarning",
                SubjectTemplate = "تحذير إداري | Administrative Warning",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>تحذير على مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong> من <strong>{{IssuedByName}}</strong>.</p>
            <div style='background:#fff8e6;border:1px solid #fde68a;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>اسم الموظف:</strong> {{EmployeeName}}</div>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>اليوم المتعلق بالمخالفة:</strong> {{ViolationDate}}</div>
              <div style='margin-bottom:6px'><strong>تاريخ إرسال الإشعار:</strong> {{IssuedAt}}</div>
              <div><strong>سبب التحذير:</strong> {{WarningReason}}</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>Warning on task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong> from <strong>{{IssuedByName}}</strong>.</p>
            <div style='background:#fff8e6;border:1px solid #fde68a;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>Employee Name:</strong> {{EmployeeName}}</div>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>Related Violation Day:</strong> {{ViolationDate}}</div>
              <div style='margin-bottom:6px'><strong>Notification Sent Date:</strong> {{IssuedAt}}</div>
              <div><strong>Warning Reason:</strong> {{WarningReason}}</div>
            </div>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // 💸 Employee Deduction
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "EmployeeDeduction",
                SubjectTemplate = "إشعار بخصم إداري | Deduction Notice",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>جزاء جديد بقيمة <strong>{{DeductionAmount}}</strong> على مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong> من <strong>{{IssuedByName}}</strong>.</p>
            <div style='background:#fef2f2;border:1px solid #fecaca;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>اسم الموظف:</strong> {{EmployeeName}}</div>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>اليوم المتعلق بالمخالفة:</strong> {{ViolationDate}}</div>
              <div style='margin-bottom:6px'><strong>تاريخ إرسال الإشعار:</strong> {{IssuedAt}}</div>
              <div><strong>سبب الخصم:</strong> {{DeductionReason}}</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>A new penalty of <strong>{{DeductionAmount}}</strong> on task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong> from <strong>{{IssuedByName}}</strong>.</p>
            <div style='background:#fef2f2;border:1px solid #fecaca;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>Employee Name:</strong> {{EmployeeName}}</div>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>Related Violation Day:</strong> {{ViolationDate}}</div>
              <div style='margin-bottom:6px'><strong>Notification Sent Date:</strong> {{IssuedAt}}</div>
              <div><strong>Deduction Reason:</strong> {{DeductionReason}}</div>
            </div>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // Task due today
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "TaskDueTodayReminder",
                SubjectTemplate = "تذكير هام: موعد انتهاء المهمة اليوم | Task Due Today",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>نود تذكيرك بأن موعد انتهاء مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong> هو <strong>اليوم</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div><strong>تاريخ الاستحقاق:</strong> {{DueDate}}</div>
            </div>
            <p style='margin:0 0 18px'>يرجى التأكد من إنهاء المهمة في الموعد المحدد أو اتخاذ الإجراء اللازم.</p>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>This is a reminder that the due date for task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong> is <strong>today</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div><strong>Due Date:</strong> {{DueDate}}</div>
            </div>
            <p style='margin:0 0 18px'>Please ensure the task is completed on time or take the necessary action.</p>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // Extension approved
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "TaskExtensionApproved",
                SubjectTemplate = "تمت الموافقة على تمديد المهمة | Task Extension Approved",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>تم تمديد مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>من تاريخ:</strong> {{OldDueDate}}</div>
              <div><strong>إلى تاريخ:</strong> {{NewDueDate}}</div>
            </div>
            <p style='margin:0 0 18px'>يرجى متابعة المهمة وفقًا للتاريخ الجديد.</p>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>Task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong> has been extended.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>From:</strong> {{OldDueDate}}</div>
              <div><strong>To:</strong> {{NewDueDate}}</div>
            </div>
            <p style='margin:0 0 18px'>Please proceed with the task according to the updated due date.</p>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // Close approved
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "TaskCloseApproved",
                SubjectTemplate = "تمت الموافقة على إغلاق المهمة | Task Closure Approved",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>تم غلق مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong> وفقًا للطلب.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div><strong>الفرع:</strong> {{BranchName}}</div>
            </div>
            <p style='margin:0 0 18px'>شكرًا لالتزامك وإنجازك للمهمة.</p>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>Task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong> has been closed according to the request.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div><strong>Branch:</strong> {{BranchName}}</div>
            </div>
            <p style='margin:0 0 18px'>Thank you for completing the task successfully.</p>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // Leave request
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "LeaveRequestCreated",
                SubjectTemplate = "طلب إجازة جديد | New Leave Request",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا،</p>
            <p style='margin:0 0 14px'>طلب <strong>{{LeaveType}}</strong> من <strong>{{EmployeeName}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>من:</strong> {{StartDate}}</div>
              <div><strong>إلى:</strong> {{EndDate}}</div>
            </div>
            <p style='margin:0 0 18px'>يرجى مراجعة الطلب من النظام.</p>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello,</strong></p>
            <p style='margin:0 0 14px'>New leave request <strong>{{LeaveType}}</strong> from <strong>{{EmployeeName}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0'>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>From:</strong> {{StartDate}}</div>
              <div><strong>To:</strong> {{EndDate}}</div>
            </div>
            " + LayoutFooter
            });

            // Leave approved
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "LeaveApproved",
                SubjectTemplate = "تمت الموافقة على طلب الإجازة | Leave Approved",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>تم الموافقة على <strong>{{LeaveType}}</strong> بواسطة <strong>{{ApprovedBy}}</strong>.</p>
            <div style='background:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>من:</strong> {{StartDate}}</div>
              <div><strong>إلى:</strong> {{EndDate}}</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>The leave request <strong>{{LeaveType}}</strong> has been approved by <strong>{{ApprovedBy}}</strong>.</p>
            <div style='background:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;padding:12px 14px;margin:0'>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>From:</strong> {{StartDate}}</div>
              <div><strong>To:</strong> {{EndDate}}</div>
            </div>
            " + LayoutFooter
            });

            // Leave rejected
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "LeaveRejected",
                SubjectTemplate = "تم رفض طلب الإجازة | Leave Rejected",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>تم رفض طلب <strong>{{LeaveType}}</strong> بواسطة <strong>{{RejectedBy}}</strong>.</p>
            <div style='background:#fef2f2;border:1px solid #fecaca;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>الفرع:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>السبب:</strong> {{RejectReason}}</div>
              <div style='margin-bottom:6px'><strong>من:</strong> {{StartDate}}</div>
              <div><strong>إلى:</strong> {{EndDate}}</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>Request <strong>{{LeaveType}}</strong> has been rejected by <strong>{{RejectedBy}}</strong>.</p>
            <div style='background:#fef2f2;border:1px solid #fecaca;border-radius:8px;padding:12px 14px;margin:0'>
              <div style='margin-bottom:6px'><strong>Branch:</strong> {{BranchName}}</div>
              <div style='margin-bottom:6px'><strong>Reason:</strong> {{RejectReason}}</div>
              <div style='margin-bottom:6px'><strong>From:</strong> {{StartDate}}</div>
              <div><strong>To:</strong> {{EndDate}}</div>
            </div>
            " + LayoutFooter
            });

            // Task Achievement
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "TaskAchievement",
                SubjectTemplate = "إضافة نسبة إنجاز على المهمة | Task Achievement Added",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>قام <strong>{{EmployeeName}}</strong> بإنجاز <strong>{{Percent}}</strong> من مهمة رقم {{TaskNumberLink}} بعنوان <strong>{{TaskTitle}}</strong> - فرع <strong>{{BranchName}}</strong>.</p>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 18px'><strong>{{EmployeeName}}</strong> achieved <strong>{{Percent}}</strong> from task number {{TaskNumberLink}} titled <strong>{{TaskTitle}}</strong> - branch <strong>{{BranchName}}</strong>.</p>
            " + TaskDetailsLinkPlaceholder + LayoutFooter
            });

            // 💼 Offer Sent To Student
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "OfferSent",
                SubjectTemplate = "عرض سعر جديد | New Price Offer",
                            BodyTemplate = LayoutHeader + @"
            مرحبًا <strong>{{StudentName}}</strong>،<br/><br/>

            نود إبلاغكم بأنه تم إرسال <strong>عرض سعر جديد</strong> لكم، وفق التفاصيل التالية:<br/><br/>

            <table style='width:100%;border-collapse:collapse;font-size:14px'>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>عنوان العرض</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{OfferTitle}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>الفرع</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{BranchName}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>الوصف</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{OfferDescription}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>الكورس</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{CourseName}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>مادة الكورس</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{SubjectName}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>تاريخ البداية</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{StartDate}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>تاريخ الانتهاء</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{EndDate}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>طريقة الدفع</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{PaymentMethod}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>السعر</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{Price}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>معدل الفائدة</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{InterestRate}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>نسبة الخصم</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{DiscountRate}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>قيمة القسط</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{InstallmentValue}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>المبلغ الصافي</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{NetAmount}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>مالك العرض</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{OfferOwner}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>التخصص</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{Specialization}}</td>
              </tr>
            </table>

            <br/>

            <div style='margin-top:15px;padding:10px;background:#fff3cd;border-right:4px solid #ffc107;color:#856404'>
            <strong>تنويه:</strong> هذا العرض ساري لمدة <strong>أسبوع واحد فقط</strong> من تاريخ الإرسال.
            </div>

            <br/>

            يرجى مراجعة العرض والتواصل معنا في حال وجود أي استفسارات.<br/><br/>

            <hr/>

            <strong>Hello {{StudentName}},</strong><br/><br/>

            We are pleased to inform you that a <strong>new price offer</strong> has been sent to you with the following details:<br/><br/>

            <table style='width:100%;border-collapse:collapse;font-size:14px'>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Offer Title</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{OfferTitle}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Branch</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{BranchName}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Description</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{OfferDescription}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Course</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{CourseName}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Course subject</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{SubjectName}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Start Date</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{StartDate}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>End Date</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{EndDate}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Payment Method</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{PaymentMethod}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Price</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{Price}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Interest Rate</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{InterestRate}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Discount Rate</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{DiscountRate}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Installment Value</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{InstallmentValue}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Net Amount</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{NetAmount}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Offer Owner</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{OfferOwner}}</td>
              </tr>
              <tr>
                <td style='padding:6px;border:1px solid #ddd;background:#f8f9fa'><strong>Specialization</strong></td>
                <td style='padding:6px;border:1px solid #ddd'>{{Specialization}}</td>
              </tr>
            </table>

            <br/>

            <div style='margin-top:15px;padding:10px;background:#fff3cd;border-left:4px solid #ffc107;color:#856404'>
            <strong>Note:</strong> This offer is valid for <strong>one week only</strong> from the sending date.
            </div>

            <br/>

            Please review the offer and contact us if you have any questions.
            " + LayoutFooter
                        });

            // Official Holiday
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "OfficialHoliday",
                SubjectTemplate = "إجازة رسمية | Official Holiday",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا،</p>
            <p style='margin:0 0 14px'>نود إعلامكم بوجود إجازة رسمية:</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div style='margin-bottom:6px'><strong>العنوان:</strong> {{HolidayTitle}}</div>
              <div style='margin-bottom:6px'><strong>من:</strong> {{StartDate}}</div>
              <div style='margin-bottom:6px'><strong>إلى:</strong> {{EndDate}}</div>
              <div><strong>ملاحظات:</strong> {{HolidayNotes}}</div>
            </div>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello,</strong></p>
            <p style='margin:0 0 14px'>Please be informed of an official holiday:</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0'>
              <div style='margin-bottom:6px'><strong>Title:</strong> {{HolidayTitle}}</div>
              <div style='margin-bottom:6px'><strong>From:</strong> {{StartDate}}</div>
              <div style='margin-bottom:6px'><strong>To:</strong> {{EndDate}}</div>
              <div><strong>Notes:</strong> {{HolidayNotes}}</div>
            </div>
            " + LayoutFooter
            });

            // Monthly employee discounts (Hangfire job)
            UpsertTemplate(context, new EmailTemplate
            {
                Key = "MonthlyEmployeeDiscounts",
                SubjectTemplate = "خصومات الموظفين طوال الشهر - {{BranchName}} | Monthly Employee Discounts",
                BodyTemplate = LayoutHeader + @"
            <p style='margin:0 0 14px'>مرحبًا <strong>{{UserName}}</strong>،</p>
            <p style='margin:0 0 14px'>مرفق لكم تقرير <strong>{{ReportTitle}}</strong> لفرع <strong>{{BranchName}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div><strong>الفترة:</strong> من {{FromDate}} إلى {{ToDate}}</div>
            </div>
            <p style='margin:0 0 18px'>يحتوي التقرير على اسم كل موظف وإجمالي الخصومات خلال الفترة المحددة.</p>
            <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
            <p style='margin:0 0 14px'><strong>Hello {{UserName}},</strong></p>
            <p style='margin:0 0 14px'>Please find attached the report <strong>{{ReportTitle}}</strong> for branch <strong>{{BranchName}}</strong>.</p>
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 14px;margin:0 0 18px'>
              <div><strong>Period:</strong> from {{FromDate}} to {{ToDate}}</div>
            </div>
            <p style='margin:0'>The report includes each employee name and their total discounts for the specified period.</p>
            " + LayoutFooter
            });

            context.SaveChanges();
        }

        private static void UpsertTemplate(AppDbContext context, EmailTemplate template)
        {
            var existing = context.EmailTemplates.FirstOrDefault(x => x.Key == template.Key);
            if (existing == null)
            {
                context.EmailTemplates.Add(template);
                return;
            }

            existing.SubjectTemplate = template.SubjectTemplate;
            existing.BodyTemplate = template.BodyTemplate;
            existing.IsActive = true;
        }
    }
}

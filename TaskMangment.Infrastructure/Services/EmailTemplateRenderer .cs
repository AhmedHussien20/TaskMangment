using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Services
{
    public class EmailTemplateRenderer : IEmailTemplateRenderer
    {
        private readonly AppDbContext _db;
        private readonly WhatsAppSettings _frontendSettings;

        public EmailTemplateRenderer(AppDbContext db, IOptions<WhatsAppSettings> frontendSettings)
        {
            _db = db;
            _frontendSettings = frontendSettings.Value;
        }

        public async Task<RenderedEmail> RenderAsync(string templateKey, ReferenceType referenceType, int referenceId, int? userId)
            => await RenderAsync(templateKey, referenceType, referenceId, userId, null);

        public async Task<RenderedEmail> RenderAsync(
            string templateKey,
            ReferenceType referenceType,
            int referenceId,
            int? userId,
            IReadOnlyDictionary<string, string>? extraTokens)
        {
            var template = await _db.EmailTemplates
                .FirstOrDefaultAsync(x => x.Key == templateKey && x.IsActive);

            if (template == null)
                throw new Exception($"Email template '{templateKey}' not found.");


            //course offer is the only key that need student name so if the key is anything else use employee normaly
            var data = await LoadDataAsync(referenceType, referenceId);
            await EnsureTaskNumberAsync(data, referenceType, referenceId);
            EnsureTaskLink(data);

            if (extraTokens != null)
            {
                foreach (var kv in extraTokens)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Key))
                        data[kv.Key] = kv.Value ?? string.Empty;
                }
            }

            if (userId.HasValue)
            {
                if (referenceType == ReferenceType.CourseOffer)
                {
                    var studentName = await _db.OfferAssignments
                        .Where(a => a.OfferId == referenceId && a.StudentId == userId.Value)
                        .Select(a => a.Student.FullName)
                        .FirstOrDefaultAsync();

                    data["StudentName"] = string.IsNullOrWhiteSpace(studentName) ? "طالب" : studentName;
                }
                else 
                {
                    var user = await _db.Employees
                        .Where(e => e.Id == userId.Value)
                        .Select(e => new
                        {
                            e.FullName,
                            BranchName = e.Branch != null ? e.Branch.Name : null
                        })
                        .FirstOrDefaultAsync();

                    data["UserName"] = string.IsNullOrWhiteSpace(user?.FullName) ? "مستخدم" : user!.FullName;

                    // Fallback for templates without a subject-employee branch (e.g. due-today).
                    if (!data.ContainsKey("BranchName") || string.IsNullOrWhiteSpace(data["BranchName"]))
                        data["BranchName"] = string.IsNullOrWhiteSpace(user?.BranchName) ? "-" : user!.BranchName!;
                }
            }
            else
            {
                if (referenceType == ReferenceType.CourseOffer)
                    data["StudentName"] = "طالب";
                else
                    data["UserName"] = "مستخدم";

                if (!data.ContainsKey("BranchName") || string.IsNullOrWhiteSpace(data["BranchName"]))
                    data["BranchName"] = "-";
            }

            return new RenderedEmail
            {
                Subject = Replace(template.SubjectTemplate, data),
                Body = AppendTaskLinkFallback(Replace(template.BodyTemplate, data), data)
            };
        }

        public async Task<RenderedEmail> RenderWithTokensAsync(string templateKey, Dictionary<string, string> tokens)
        {
            var template = await _db.EmailTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Key == templateKey && x.IsActive);

            if (template == null)
                throw new Exception($"Email template '{templateKey}' not found.");

            tokens ??= new Dictionary<string, string>();
            EnsureTaskLink(tokens);

            return new RenderedEmail
            {
                Subject = Replace(template.SubjectTemplate, tokens),
                Body = AppendTaskLinkFallback(Replace(template.BodyTemplate, tokens), tokens)
            };
        }
        private static string Replace(string template, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(template) || data == null || data.Count == 0)
                return template;

            // Longer keys first so TaskNumber is not affected by shorter overlapping keys.
            foreach (var item in data.OrderByDescending(x => x.Key.Length))
            {
                if (string.IsNullOrWhiteSpace(item.Key))
                    continue;

                template = template.Replace(
                    "{{" + item.Key + "}}",
                    item.Value ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase);
            }

            return template;
        }

        private void EnsureTaskLink(Dictionary<string, string> data)
        {
            if (data == null)
                return;

            if (!data.ContainsKey("TaskLink"))
                data["TaskLink"] = string.Empty;
            if (!data.ContainsKey("TaskLinkBlock"))
                data["TaskLinkBlock"] = string.Empty;
            if (!data.ContainsKey("TaskDetailsLink"))
                data["TaskDetailsLink"] = string.Empty;

            var taskNumberLabel = data.TryGetValue("TaskNumber", out var rawNumber) && !string.IsNullOrWhiteSpace(rawNumber)
                ? rawNumber.Trim()
                : null;

            // Default: plain number (no link) — used when FrontendUrl/task id is missing.
            if (!data.ContainsKey("TaskNumberLink"))
            {
                data["TaskNumberLink"] = string.IsNullOrWhiteSpace(taskNumberLabel)
                    ? string.Empty
                    : $"<strong>{taskNumberLabel}</strong>";
            }

            if (!TryGetTaskId(data, out var taskId))
                return;

            var frontendUrl = _frontendSettings?.FrontendUrl;
            if (string.IsNullOrWhiteSpace(frontendUrl))
                return;

            var taskUrl = $"{frontendUrl.TrimEnd('/')}/task/task-list?taskId={taskId}";
            var display = taskNumberLabel ?? taskId.ToString();
            data["TaskLink"] = taskUrl;
            data["TaskNumberLink"] =
                $"<a href='{taskUrl}' style='color:#0d6efd;text-decoration:underline;font-weight:bold'>{display}</a>";
            data["TaskDetailsLink"] = BuildTaskDetailsLinkHtml(taskUrl);
            data["TaskLinkBlock"] = data["TaskDetailsLink"];
        }

        private static string BuildTaskDetailsLinkHtml(string taskUrl)
        {
            return
                "<div style='margin:18px 0 0;text-align:center'>" +
                $"<a href='{taskUrl}' style='display:inline-block;background:#0d6efd;color:#ffffff;text-decoration:none;font-weight:bold;padding:10px 18px;border-radius:8px'>" +
                "اضغط هنا لعرض التفاصيل | Click here to see the details" +
                "</a>" +
                "</div>";
        }

        private static bool TryGetTaskId(Dictionary<string, string> data, out int taskId)
        {
            taskId = 0;
            if (!data.TryGetValue("TaskNumber", out var raw) || string.IsNullOrWhiteSpace(raw) || raw == "-")
                return false;

            return int.TryParse(raw.Trim(), out taskId) && taskId > 0;
        }

        private static string AppendTaskLinkFallback(string body, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(body) || data == null)
                return body;

            if (!data.TryGetValue("TaskDetailsLink", out var detailsLink) || string.IsNullOrWhiteSpace(detailsLink))
                return body;

            // Templates already include {{TaskDetailsLink}}; only inject if the placeholder was missing.
            if (body.Contains("task/task-list?taskId=", StringComparison.OrdinalIgnoreCase) ||
                body.Contains("اضغط هنا لعرض التفاصيل", StringComparison.OrdinalIgnoreCase))
                return body;

            return body + detailsLink;
        }

        private async Task EnsureTaskNumberAsync(
            Dictionary<string, string> data,
            ReferenceType referenceType,
            int referenceId)
        {
            if (data == null || referenceId <= 0)
                return;

            if (data.TryGetValue("TaskNumber", out var existing) &&
                !string.IsNullOrWhiteSpace(existing) &&
                existing != "-")
            {
                return;
            }

            int? taskId = referenceType switch
            {
                ReferenceType.Task => referenceId,
                ReferenceType.TaskDueTodayReminder => referenceId,

                ReferenceType.TaskComment => await _db.TaskComments
                    .Where(c => c.Id == referenceId)
                    .Select(c => (int?)c.TaskId)
                    .FirstOrDefaultAsync(),

                ReferenceType.TaskAchieve => await _db.TaskPercentages
                    .Where(p => p.Id == referenceId)
                    .Select(p => (int?)p.TaskId)
                    .FirstOrDefaultAsync(),

                ReferenceType.TaskExtensionRequest => await _db.TaskExtensionRequests
                    .Where(r => r.Id == referenceId)
                    .Select(r => (int?)r.TaskId)
                    .FirstOrDefaultAsync(),

                ReferenceType.TaskExtensionRequestApproved => await _db.TaskExtensionRequests
                    .Where(r => r.Id == referenceId)
                    .Select(r => (int?)r.TaskId)
                    .FirstOrDefaultAsync(),

                ReferenceType.TaskCloseRequest => await _db.TaskCloseRequests
                    .Where(r => r.Id == referenceId)
                    .Select(r => (int?)r.TaskId)
                    .FirstOrDefaultAsync(),

                ReferenceType.TaskCloseRequestApproved => await _db.TaskCloseRequests
                    .Where(r => r.Id == referenceId)
                    .Select(r => (int?)r.TaskId)
                    .FirstOrDefaultAsync(),

                ReferenceType.EmployeeWarning => await _db.Warnings
                    .Where(w => w.Id == referenceId)
                    .Select(w => (int?)w.TaskId)
                    .FirstOrDefaultAsync(),

                ReferenceType.EmployeeDeduction => await _db.Discounts
                    .Where(d => d.Id == referenceId)
                    .Select(d => d.TaskId)
                    .FirstOrDefaultAsync(),

                _ => null
            };

            if (taskId.HasValue && taskId.Value > 0)
                data["TaskNumber"] = taskId.Value.ToString();
        }

        private async Task<Dictionary<string, string>> LoadDataAsync(
            ReferenceType referenceType,
            int referenceId)
        {
            switch (referenceType)
            {
                // =========================
                //  Task
                // =========================
                case ReferenceType.Task:
                    {
                        var task = await _db.Tasks
                            .Where(t => t.Id == referenceId)
                            .Select(t => new
                            {
                                TaskNumber = t.Id,
                                t.Title,
                                t.DueDate,
                                t.Description,
                                PeriodDays = t.CommentAllowPeriodDays.HasValue ? (int?)t.CommentAllowPeriodDays.Value : null,
                                t.MinCommentsPerPeriod
                            })
                            .FirstOrDefaultAsync();

                        if (task == null)
                            throw new Exception($"Task with Id {referenceId} not found.");

                        var minComments = task.MinCommentsPerPeriod < 1 ? 1 : task.MinCommentsPerPeriod;
                        var periodDays = task.PeriodDays.HasValue && task.PeriodDays.Value > 0
                            ? task.PeriodDays.Value.ToString()
                            : "-";

                        return new Dictionary<string, string>
                        {
                            // Prefer queued referenceId so TaskNumber never comes back empty.
                            ["TaskNumber"] = (referenceId > 0 ? referenceId : task.TaskNumber).ToString(),
                            ["TaskTitle"] = task.Title ?? "-",
                            ["DueDate"] = task.DueDate?.ToString("yyyy-MM-dd") ?? "-",
                            ["TaskDescription"] = task.Description ?? "-",
                            ["MinCommentsPerPeriod"] = minComments.ToString(),
                            ["CommentAllowPeriodDays"] = periodDays
                        };
                    }

                // =========================
                //  Event
                // =========================
                case ReferenceType.Event:
                    {
                        var ev = await _db.CalendarEvents
                            .Where(e => e.Id == referenceId)
                            .Select(e => new
                            {
                                e.Title,
                                e.StartDate
                            })
                            .FirstOrDefaultAsync();

                        if (ev == null)
                            throw new Exception($"Event with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["EventTitle"] = ev.Title,
                            ["EventDate"] = ev.StartDate.ToString("yyyy-MM-dd HH:mm")
                        };
                    }

                // =========================
                //  Task Comment
                // =========================
                case ReferenceType.TaskComment:
                    {
                        var comment = await _db.TaskComments
                            .Where(c => c.Id == referenceId)
                            .Select(c => new
                            {
                                TaskNumber = c.TaskId,
                                TaskTitle = c.Task.Title,
                                EmployeeName = c.Employee != null ? c.Employee.FullName : "موظف",
                                BranchName = c.Employee != null && c.Employee.Branch != null
                                    ? c.Employee.Branch.Name
                                    : null,
                                c.CommentText
                            })
                            .FirstOrDefaultAsync();

                        if (comment == null)
                            throw new Exception($"Task comment with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskNumber"] = comment.TaskNumber.ToString(),
                            ["TaskTitle"] = comment.TaskTitle,
                            ["EmployeeName"] = comment.EmployeeName ?? "موظف",
                            ["BranchName"] = string.IsNullOrWhiteSpace(comment.BranchName) ? "-" : comment.BranchName,
                            ["CommentText"] = string.IsNullOrWhiteSpace(comment.CommentText)
                                ? "رفع ملف"
                                : comment.CommentText
                        };
                    }

                // =========================
                //  Task Achievement
                // =========================
                case ReferenceType.TaskAchieve:
                    {
                        var percent = await _db.TaskPercentages
                            .Where(c => c.Id == referenceId)
                            .Select(c => new
                            {
                                TaskNumber = c.TaskId,
                                TaskTitle = c.Task.Title,
                                EmployeeName = c.Employee.FullName,
                                BranchName = c.Employee.Branch != null ? c.Employee.Branch.Name : null,
                                Percent = c.AchievementPercent
                            })
                            .FirstOrDefaultAsync();

                        if (percent == null)
                            throw new Exception($"Task Percent with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskNumber"] = percent.TaskNumber.ToString(),
                            ["TaskTitle"] = percent.TaskTitle,
                            ["EmployeeName"] = percent.EmployeeName,
                            ["BranchName"] = string.IsNullOrWhiteSpace(percent.BranchName) ? "-" : percent.BranchName,
                            ["Percent"] = percent.Percent
                        };
                    }

                // =========================
                //  Task Extension Request
                // =========================
                case ReferenceType.TaskExtensionRequest:
                    {
                        var request = await _db.TaskExtensionRequests
                            .Where(r => r.Id == referenceId)
                            .Select(r => new
                            {
                                TaskNumber = r.TaskId,
                                TaskTitle = r.Task.Title,
                                EmployeeName = r.RequestedBy != null ? r.RequestedBy.FullName : "موظف",
                                BranchName = r.RequestedBy != null && r.RequestedBy.Branch != null
                                    ? r.RequestedBy.Branch.Name
                                    : null,
                                r.Reason
                            })
                            .FirstOrDefaultAsync();

                        if (request == null)
                            throw new Exception($"Task extension request with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskNumber"] = request.TaskNumber.ToString(),
                            ["TaskTitle"] = request.TaskTitle,
                            ["EmployeeName"] = request.EmployeeName ?? "موظف",
                            ["BranchName"] = string.IsNullOrWhiteSpace(request.BranchName) ? "-" : request.BranchName,
                            ["ExtensionReason"] = request.Reason ?? "-"
                        };
                    }

                // =========================
                // ✅ Task Close Request
                // =========================
                case ReferenceType.TaskCloseRequest:
                    {
                        var close = await _db.TaskCloseRequests
                            .Where(r => r.Id == referenceId)
                            .Select(r => new
                            {
                                TaskNumber = r.TaskId,
                                TaskTitle = r.Task.Title,
                                EmployeeName = r.RequestedBy != null ? r.RequestedBy.FullName : "موظف",
                                BranchName = r.RequestedBy != null && r.RequestedBy.Branch != null
                                    ? r.RequestedBy.Branch.Name
                                    : null,
                                r.Message
                            })
                            .FirstOrDefaultAsync();

                        if (close == null)
                            throw new Exception($"Task close request with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskNumber"] = close.TaskNumber.ToString(),
                            ["TaskTitle"] = close.TaskTitle,
                            ["EmployeeName"] = close.EmployeeName ?? "موظف",
                            ["BranchName"] = string.IsNullOrWhiteSpace(close.BranchName) ? "-" : close.BranchName,
                            ["CloseNotes"] = close.Message ?? "-"
                        };
                    }

                // =========================
                // ⚠️ Employee Warning
                // =========================
                case ReferenceType.EmployeeWarning:
                    {
                        var warning = await _db.Warnings
                            .Where(w => w.Id == referenceId)
                            .Select(w => new
                            {
                                TaskNumber = w.TaskId,
                                TaskTitle = w.Task.Title,
                                EmployeeName = w.Issued != null
                                    ? w.Issued.FullName
                                    : (w.TaskAssignment != null && w.TaskAssignment.Employee != null
                                        ? w.TaskAssignment.Employee.FullName
                                        : null),
                                BranchName = w.Issued != null && w.Issued.Branch != null
                                    ? w.Issued.Branch.Name
                                    : (w.TaskAssignment != null
                                       && w.TaskAssignment.Employee != null
                                       && w.TaskAssignment.Employee.Branch != null
                                        ? w.TaskAssignment.Employee.Branch.Name
                                        : null),
                                IssuedByName = w.IssuedBy != null ? w.IssuedBy.FullName : "النظام",
                                w.Reason,
                                w.ViolationDate,
                                w.IssuedAt
                            })
                            .FirstOrDefaultAsync();

                        if (warning == null)
                            throw new Exception($"Employee warning with Id {referenceId} not found.");

                        // Related day of the violation (e.g. missing comment yesterday), not the send date.
                        var relatedDate = warning.ViolationDate ?? warning.IssuedAt;

                        return new Dictionary<string, string>
                        {
                            ["TaskNumber"] = warning.TaskNumber.ToString(),
                            ["TaskTitle"] = warning.TaskTitle ?? "-",
                            ["EmployeeName"] = string.IsNullOrWhiteSpace(warning.EmployeeName) ? "-" : warning.EmployeeName,
                            ["BranchName"] = string.IsNullOrWhiteSpace(warning.BranchName) ? "-" : warning.BranchName,
                            ["IssuedByName"] = string.IsNullOrWhiteSpace(warning.IssuedByName) ? "النظام" : warning.IssuedByName,
                            ["WarningReason"] = string.IsNullOrWhiteSpace(warning.Reason) ? "-" : warning.Reason,
                            ["ViolationDate"] = relatedDate.ToString("yyyy-MM-dd"),
                            ["IssuedAt"] = warning.IssuedAt.ToString("yyyy-MM-dd")
                        };
                    }

                // =========================
                // 💸 Employee Deduction
                // =========================
                case ReferenceType.EmployeeDeduction:
                    {
                        var deduction = await _db.Discounts
                            .Where(d => d.Id == referenceId)
                            .Select(d => new
                            {
                                EmployeeName = d.Employee.FullName,
                                BranchName = d.Employee.Branch != null ? d.Employee.Branch.Name : null,
                                TaskNumber = d.TaskId.HasValue ? d.TaskId.Value.ToString() : "-",
                                TaskTitle = d.Task != null ? d.Task.Title : "-",
                                IssuedByName = d.CreatedBy != null ? d.CreatedBy.FullName : "النظام",
                                d.Reason,
                                d.Amount,
                                d.ViolationDate,
                                CreatedDate = d.CreatedDate
                            })
                            .FirstOrDefaultAsync();

                        if (deduction == null)
                            throw new Exception($"Employee deduction with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["EmployeeName"] = deduction.EmployeeName,
                            ["BranchName"] = string.IsNullOrWhiteSpace(deduction.BranchName) ? "-" : deduction.BranchName,
                            ["TaskNumber"] = deduction.TaskNumber,
                            ["TaskTitle"] = deduction.TaskTitle,
                            ["IssuedByName"] = string.IsNullOrWhiteSpace(deduction.IssuedByName) ? "النظام" : deduction.IssuedByName,
                            ["DeductionReason"] = deduction.Reason ?? "-",
                            ["DeductionAmount"] = deduction.Amount.ToString("N2"),
                            // Related day of the violation (e.g. missing comment yesterday), not the send date.
                            ["ViolationDate"] = deduction.ViolationDate.ToString("yyyy-MM-dd"),
                            ["IssuedAt"] = deduction.CreatedDate.ToString("yyyy-MM-dd")
                        };
                    }

                // =========================
                // Task Due Today Reminder
                // =========================
                case ReferenceType.TaskDueTodayReminder:
                    {
                        var taskDue = await _db.Tasks
                            .Where(t => t.Id == referenceId)
                            .Select(t => new { t.Id, t.Title, t.DueDate })
                            .FirstOrDefaultAsync();

                        if (taskDue == null)
                            throw new Exception($"Task with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskNumber"] = (referenceId > 0 ? referenceId : taskDue.Id).ToString(),
                            ["TaskTitle"] = taskDue.Title,
                            ["DueDate"] = taskDue.DueDate?.ToString("yyyy-MM-dd") ?? "-"
                        };
                    }

                // =========================
                //  Offer Assignment
                // =========================
                case ReferenceType.CourseOffer:
                    {
                        var offer = await _db.Offers
                            .Where(a => a.Id == referenceId)
                            .Select(a => new
                            {
                                a.Title,
                                a.Description,
                                a.StartDate,
                                a.EndDate,
                                a.Body,
                                SubjectName = a.Subject.Title,
                                CourseName = a.Course.Title,
                                BranchName = a.Branch != null ? a.Branch.Name : null
                            })
                            .FirstOrDefaultAsync();

                        if (offer == null)
                            throw new Exception($"Offer with Id {referenceId} not found.");

                        var bodyDict = string.IsNullOrEmpty(offer.Body)
                            ? new Dictionary<string, string>()
                            : JsonSerializer.Deserialize<Dictionary<string, string>>(offer.Body);

                        return new Dictionary<string, string>
                        {
                            ["OfferTitle"] = offer.Title,
                            ["OfferDescription"] = offer.Description,
                            ["BranchName"] = string.IsNullOrWhiteSpace(offer.BranchName) ? "-" : offer.BranchName,
                            ["SubjectName"] = offer.SubjectName ?? "-",
                            ["CourseName"] = offer.CourseName ?? "-",
                            ["StartDate"] = offer.StartDate?.ToString("yyyy-MM-dd") ?? "-",
                            ["EndDate"] = offer.EndDate?.ToString("yyyy-MM-dd") ?? "-",
                            ["PaymentMethod"] = bodyDict?.GetValueOrDefault("PaymentMethod") ?? "-",
                            ["Price"] = bodyDict?.GetValueOrDefault("Price") ?? "-",
                            ["InterestRate"] = bodyDict?.GetValueOrDefault("InterestRate") ?? "-",
                            ["DiscountRate"] = bodyDict?.GetValueOrDefault("DiscountRate") ?? "-",
                            ["InstallmentValue"] = bodyDict?.GetValueOrDefault("InstallmentValue") ?? "-",
                            ["NetAmount"] = bodyDict?.GetValueOrDefault("NetAmount") ?? "-",
                            ["OfferOwner"] = bodyDict?.GetValueOrDefault("OfferOwner") ?? "-",
                            ["Specialization"] = bodyDict?.GetValueOrDefault("Specialization") ?? "-",
                        };
                    }

                // =========================
                // ✅ Task Extension Approved
                // =========================
                case ReferenceType.TaskExtensionRequestApproved:
                    {
                        var request = await _db.TaskExtensionRequests
                            .Where(r => r.Id == referenceId)
                            .Select(r => new
                            {
                                TaskNumber = r.TaskId,
                                TaskTitle = r.Task.Title,
                                OldDueDate = r.Task.DueDate,
                                NewDueDate = r.NewDueDate,
                                BranchName = r.RequestedBy != null && r.RequestedBy.Branch != null
                                    ? r.RequestedBy.Branch.Name
                                    : null
                            })
                            .FirstOrDefaultAsync();

                        if (request == null)
                            throw new Exception($"Task extension request with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskNumber"] = request.TaskNumber.ToString(),
                            ["TaskTitle"] = request.TaskTitle,
                            ["BranchName"] = string.IsNullOrWhiteSpace(request.BranchName) ? "-" : request.BranchName,
                            ["OldDueDate"] = request.OldDueDate?.ToString("yyyy-MM-dd") ?? "-",
                            ["NewDueDate"] = request.NewDueDate.ToString("yyyy-MM-dd")
                        };
                    }

                // =========================
                // ✅ Task Close Approved
                // =========================
                case ReferenceType.TaskCloseRequestApproved:
                    {
                        var request = await _db.TaskCloseRequests
                            .Where(r => r.Id == referenceId)
                            .Select(r => new
                            {
                                TaskNumber = r.TaskId,
                                TaskTitle = r.Task.Title,
                                BranchName = r.RequestedBy != null && r.RequestedBy.Branch != null
                                    ? r.RequestedBy.Branch.Name
                                    : null
                            })
                            .FirstOrDefaultAsync();

                        if (request == null)
                            throw new Exception($"Task close request with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskNumber"] = request.TaskNumber.ToString(),
                            ["TaskTitle"] = request.TaskTitle,
                            ["BranchName"] = string.IsNullOrWhiteSpace(request.BranchName) ? "-" : request.BranchName
                        };
                    }

                // =========================
                // Leave
                // =========================
                case ReferenceType.Leave:
                    {
                        var leave = await _db.Leaves
                            .Where(l => l.Id == referenceId)
                            .Select(l => new
                            {
                                EmployeeName = l.Employee.FullName,
                                BranchName = l.Employee.Branch != null ? l.Employee.Branch.Name : null,
                                LeaveType = l.LeaveType.NameAr,
                                l.StartDate,
                                l.EndDate,
                                ApprovedBy = l.ApprovedBy != null ? l.ApprovedBy.FullName : "-",
                                RejectReason = l.RejectionReason
                            })
                            .FirstOrDefaultAsync();

                        if (leave == null)
                            throw new Exception($"Leave with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["EmployeeName"] = leave.EmployeeName,
                            ["BranchName"] = string.IsNullOrWhiteSpace(leave.BranchName) ? "-" : leave.BranchName,
                            ["LeaveType"] = leave.LeaveType ?? "-",
                            ["StartDate"] = leave.StartDate.ToString("yyyy-MM-dd"),
                            ["EndDate"] = leave.EndDate.ToString("yyyy-MM-dd"),
                            ["ApprovedBy"] = leave.ApprovedBy ?? "-",
                            ["RejectedBy"] = leave.ApprovedBy ?? "-",
                            ["RejectReason"] = leave.RejectReason ?? "-"
                        };
                    }

                case ReferenceType.OfficialHoliday:
                    {
                        var holiday = await _db.CalendarEvents
                            .Where(e => e.Id == referenceId)
                            .Select(e => new
                            {
                                e.Title,
                                e.StartDate,
                                e.EndDate,
                                e.Description
                            })
                            .FirstOrDefaultAsync();

                        if (holiday == null)
                            throw new Exception($"Official holiday with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["HolidayTitle"] = holiday.Title ?? "-",
                            ["HolidayDate"] = holiday.StartDate.ToString("yyyy-MM-dd"),
                            ["StartDate"] = holiday.StartDate.ToString("yyyy-MM-dd"),
                            ["EndDate"] = holiday.EndDate?.ToString("yyyy-MM-dd") ?? "-",
                            ["HolidayNotes"] = holiday.Description ?? "-"
                        };
                    }

                // =========================
                default:
                    return new Dictionary<string, string>();
            }
        }
    }
}

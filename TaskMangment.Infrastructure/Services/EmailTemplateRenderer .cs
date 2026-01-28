using Microsoft.EntityFrameworkCore;
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

        public EmailTemplateRenderer(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RenderedEmail> RenderAsync(string templateKey, ReferenceType referenceType, int referenceId, int? userId)
        {
            var template = await _db.EmailTemplates
                .FirstOrDefaultAsync(x => x.Key == templateKey && x.IsActive);

            if (template == null)
                throw new Exception($"Email template '{templateKey}' not found.");


            //course offer is the only key that need student name so if the key is anything else use employee normaly
            var data = await LoadDataAsync(referenceType, referenceId);
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
                    var userName = await _db.Employees
                        .Where(e => e.Id == userId.Value)
                        .Select(e => e.FullName)
                        .FirstOrDefaultAsync();

                    data["UserName"] = string.IsNullOrWhiteSpace(userName) ? "مستخدم" : userName;
                }
            }
            else
            {
                if (referenceType == ReferenceType.CourseOffer)
                    data["StudentName"] = "طالب";
                else
                    data["UserName"] = "مستخدم";
            }

            return new RenderedEmail
            {
                Subject = Replace(template.SubjectTemplate, data),
                Body = Replace(template.BodyTemplate, data)
            };
        }
        private static string Replace(string template, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(template))
                return template;
            foreach (var item in data)
            {
                template = template.Replace(
                    $"{{{{{item.Key}}}}}",   // {{Key}}
                    item.Value ?? string.Empty
                );
            }

            return template;
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
                                t.Title,
                                t.DueDate,
                                t.Description

                            })
                            .FirstOrDefaultAsync();

                        if (task == null)
                            throw new Exception($"Task with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskTitle"] = task.Title,
                            ["DueDate"] = task.DueDate?.ToString("yyyy-MM-dd") ?? "-",
                            ["TaskDescription"] = task.Description

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
                                TaskTitle = c.Task.Title,
                                c.CommentText
                            })
                            .FirstOrDefaultAsync();

                        if (comment == null)
                            throw new Exception($"Task comment with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskTitle"] = comment.TaskTitle,
                            ["CommentText"] = comment.CommentText
                        };
                    }



                // =========================
                //  Task Achievement
                // =========================
                case ReferenceType.TaskAchieve:
                    {
                        var Percent = await _db.TaskPercentages
                            .Where(c => c.Id == referenceId)
                            .Select(c => new
                            {
                                TaskTitle = c.Task.Title,
                                EmployeeName = c.Employee.FullName,
                                Percent = c.AchievementPercent
                            })
                            .FirstOrDefaultAsync();

                        if (Percent == null)
                            throw new Exception($"Task Percent with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskTitle"] = Percent.TaskTitle,
                            ["EmployeeName"] = Percent.EmployeeName,
                            ["Percent"] = Percent.Percent
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
                                TaskTitle = r.Task.Title,
                                r.Reason
                            })
                            .FirstOrDefaultAsync();

                        if (request == null)
                            throw new Exception($"Task extension request with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskTitle"] = request.TaskTitle,
                            ["ExtensionReason"] = request.Reason
                        };
                    }

                // =========================
                // ✅Task Close Request
                // =========================
                case ReferenceType.TaskCloseRequest:
                    {
                        var close = await _db.TaskCloseRequests
                            .Where(r => r.Id == referenceId)
                            .Select(r => new
                            {
                                TaskTitle = r.Task.Title,
                                r.Message
                            })
                            .FirstOrDefaultAsync();

                        if (close == null)
                            throw new Exception($"Task close request with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskTitle"] = close.TaskTitle,
                            ["CloseNotes"] = close.Message
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
                                w.Reason
                            })
                            .FirstOrDefaultAsync();

                        if (warning == null)
                            throw new Exception($"Employee warning with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["WarningReason"] = warning.Reason
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
                                TaskNumber = d.Task.Id.ToString(),
                                TaskTitle = d.Task.Title,
                                d.Reason,
                                d.Amount
                            })
                            .FirstOrDefaultAsync();

                        if (deduction == null)
                            throw new Exception($"Employee deduction with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["EmployeeName"] = deduction.EmployeeName,
                            ["TaskNumber"] = deduction.TaskNumber,
                            ["TaskTitle"] = deduction.TaskTitle,
                            ["DeductionReason"] = deduction.Reason,
                            ["DeductionAmount"] = deduction.Amount.ToString("N2"),

                        };
                    }


                // =========================
                // Task Due Today Reminder
                // =========================
                case ReferenceType.TaskDueTodayReminder:
                    { 
                    var taskDue = await _db.Tasks
                        .Where(t => t.Id == referenceId)
                        .Select(t => new { t.Title, t.DueDate })
                        .FirstOrDefaultAsync();

                    if (taskDue == null)
                        throw new Exception($"Task with Id {referenceId} not found.");

                    return new Dictionary<string, string>
                    {
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
                                CourseName = a.Course.Title
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
                            //.Include(r => r.Task)
                            .Where(r => r.Id == referenceId)
                            .Select(r => new
                            {
                                TaskTitle = r.Task.Title,
                                OldDueDate = r.Task.DueDate,
                                NewDueDate = r.NewDueDate
                            })
                            .FirstOrDefaultAsync();


                        if (request == null)
                            throw new Exception($"Task extension request with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskTitle"] = request.TaskTitle,
                            ["OldDueDate"] = request.OldDueDate?.ToString("yyyy-MM-dd") ?? "-",
                            ["NewDueDate"] = request.NewDueDate.ToString("yyyy-MM-dd") ?? "-"
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
                                TaskTitle = r.Task.Title,
                            })
                            .FirstOrDefaultAsync();

                        if (request == null)
                            throw new Exception($"Task close request with Id {referenceId} not found.");

                        return new Dictionary<string, string>
                        {
                            ["TaskTitle"] = request.TaskTitle,
                        };
                    }


                // =========================
                default:
                    return new Dictionary<string, string>();
            }
        }

    }
    }

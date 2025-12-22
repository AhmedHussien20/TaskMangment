using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<RenderedEmail> RenderAsync(string templateKey, ReferenceType referenceType,int referenceId, int? userId)
        {
            var template = await _db.EmailTemplates
                .FirstOrDefaultAsync(x => x.Key == templateKey && x.IsActive);

            if (template == null)
                throw new Exception($"Email template '{templateKey}' not found.");

            var data = await LoadDataAsync(referenceType, referenceId);
            if (userId.HasValue)
            {
                var userName = await _db.Employees
                    .Where(e => e.Id == userId.Value)
                    .Select(e => e.FullName) 
                    .FirstOrDefaultAsync();

                data["UserName"] = string.IsNullOrWhiteSpace(userName) ? "مستخدم" : userName;
            }
            else
            {
                data["UserName"] = "مستخدم";
            }

            return new RenderedEmail
            {
                Subject = Replace(template.SubjectTemplate, data),
                Body = Replace(template.BodyTemplate, data)
            };
        }

        private async Task<Dictionary<string, string>> LoadDataAsync( ReferenceType referenceType,int referenceId)
        {
            if (referenceType == ReferenceType.Task)
            {
                var task = await _db.Tasks
                    .Where(t => t.Id == referenceId)
                    .Select(t => new
                    {
                        t.Title,
                        t.DueDate
                    })
                    .FirstAsync();

                return new Dictionary<string, string>
                {
                    ["TaskTitle"] = task.Title,
                    ["DueDate"] = task.DueDate?.ToString("yyyy-MM-dd") ?? "-"
                };
            }

            if (referenceType == ReferenceType.Event)
            {
                var ev = await _db.CalendarEvents
                    .Where(e => e.Id == referenceId)
                    .Select(e => new
                    {
                        e.Title,
                        e.StartDate
                    })
                    .FirstAsync();

                return new Dictionary<string, string>
                {
                    ["EventTitle"] = ev.Title,
                    ["EventDate"] = ev.StartDate.ToString("yyyy-MM-dd HH:mm")
                };
            }

            return new();
        }

        private static string Replace( string template, Dictionary<string, string> data)
        {
            foreach (var item in data)
            {
                template = template.Replace(
                    $"{{{{{item.Key}}}}}",
                    item.Value);
            }

            return template;
        }
    }

}

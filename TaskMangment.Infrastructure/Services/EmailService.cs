using TaskMangment.Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Exceptions;

namespace TaskMangment.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string subject, string body)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSSL,
                Credentials = new NetworkCredential(_settings.From, _settings.Password)
            };

            var mail = new MailMessage(_settings.From, _settings.To, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(_settings.From))
            {
                throw new AppException("Email Setting Not Found",500);
            }
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSSL,
                Credentials = new NetworkCredential(_settings.From, _settings.Password)
            };

            var mail = new MailMessage(_settings.From, to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }

    }

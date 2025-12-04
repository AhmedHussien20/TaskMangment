using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        public Task SendMessageAsync(string phoneNumber, string message)
        {
            // TODO: integrate with WhatsApp Cloud API
            Console.WriteLine($"WhatsApp → {phoneNumber}: {message}");
            return Task.CompletedTask;
        }
    }
}


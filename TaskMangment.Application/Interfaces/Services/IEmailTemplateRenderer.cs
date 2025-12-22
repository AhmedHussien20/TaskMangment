using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IEmailTemplateRenderer
    {
        Task<RenderedEmail> RenderAsync(string templateKey,ReferenceType referenceType,int referenceId, int? userId);
             
            
           
    }

}

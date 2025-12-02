using TaskMangment.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IJWTTokenGenerator
    {
        Task<string> GenerateToken(SystemUser user);

    }
}

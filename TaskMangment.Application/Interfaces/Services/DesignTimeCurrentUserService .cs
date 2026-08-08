using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Interfaces.Services
{
    public class DesignTimeCurrentUserService : ICurrentUserService
    {
        public int? UserId => null;
        public int? CompanyId => null;
        public string? UserName => "DesignTime";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TaskMangment.Domain.Entities
{
    public class SystemUser: IdentityUser
    {
        public string FullName { get; set; }

    }
}

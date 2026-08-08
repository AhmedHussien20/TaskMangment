using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using TaskMangment.Application.Interfaces.Services;

public class HangfireCurrentUserService : ICurrentUserService
{
    public int? UserId => null;
    public string? UserName => "Hangfire";
    public bool IsAuthenticated => false;

    public int? CompanyId => throw new NotImplementedException();
}


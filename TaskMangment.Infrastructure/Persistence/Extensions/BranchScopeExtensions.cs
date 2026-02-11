using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class BranchScopeExtensions
{
    public static IQueryable<Branch> ApplyAccessScope(
        this IQueryable<Branch> query,
        UserAccessContext access)
    {
        if (access.BranchIds != null && access.BranchIds.Any())
            query = query.Where(b => access.BranchIds.Contains(b.Id));

        return query;
    }
}

}

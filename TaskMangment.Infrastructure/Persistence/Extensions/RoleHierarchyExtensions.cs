using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class RoleHierarchyExtensions
    {
        public static IQueryable<Employee> ApplyRoleHierarchy(this IQueryable<Employee> query , int maxRoleLevel)
        {
            return query.Where(e => !e.EmployeeRoles.Any(er => er.IsAssigned &&
            !er.IsDeleted &&
            er.Role != null &&
            er.Role.Level > maxRoleLevel));
        }
    }
}

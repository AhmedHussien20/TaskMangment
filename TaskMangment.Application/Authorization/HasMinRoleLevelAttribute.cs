using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Authorization
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HasMinRoleLevelAttribute : Attribute
    {
        public RoleLevelEnum MinLevel { get; }

        public HasMinRoleLevelAttribute(RoleLevelEnum minLevel)
        {
            MinLevel = minLevel;
        }
    }

}

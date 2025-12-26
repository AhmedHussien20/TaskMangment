 

namespace TaskMangment.Application.Authorization
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HasRoleAttribute : Attribute
    {
        public string [] Roles { get; }

        public HasRoleAttribute(params string[] roles)
        {
            Roles = roles;
        }
    }
}


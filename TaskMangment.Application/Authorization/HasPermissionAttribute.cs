 

namespace TaskMangment.Application.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HasPermissionAttribute : Attribute
    {
        public string PermissionCode { get; }

        public HasPermissionAttribute(string permissionCode)
        {
            PermissionCode = permissionCode;
        }
    }
}


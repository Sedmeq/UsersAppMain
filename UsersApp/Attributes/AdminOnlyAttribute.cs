using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace UsersApp.Attributes
{
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        public AdminOnlyAttribute()
        {
            Roles = "Admin";
        }
    }
}

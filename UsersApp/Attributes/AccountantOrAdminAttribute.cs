using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace UsersApp.Attributes
{
    public class AccountantOrAdminAttribute : AuthorizeAttribute
    {
        public AccountantOrAdminAttribute()
        {
            Roles = "Admin,Accountant";
        }
    }
}

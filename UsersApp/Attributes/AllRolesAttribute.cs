using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace UsersApp.Attributes
{
    public class AllRolesAttribute : AuthorizeAttribute
    {
        public AllRolesAttribute()
        {
            Roles = "Admin,Accountant,Cashier";
        }
    }
}

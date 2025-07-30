using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace UsersApp.Attributes
{
    public class CashierOrAdminAttribute : AuthorizeAttribute
    {
        public CashierOrAdminAttribute()
        {
            Roles = "Admin,Cashier";
        }
    }
}

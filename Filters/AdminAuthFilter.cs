using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PrintMarket.Filters
{
    public class AdminAuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Session'da yetki kontrolü
            var adminUser = context.HttpContext.Session.GetString("AdminUser");
            var userRole = context.HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(adminUser) && userRole != "Business")
            {
                // Giriş yapılmamışsa veya yetkisizse Login sayfasına yönlendir
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}

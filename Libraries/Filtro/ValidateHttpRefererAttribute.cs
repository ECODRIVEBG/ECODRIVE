using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EcoDriveTcc.Libraries.Filtro
{
    public class ValidateHttpRefererAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            string referer = context.HttpContext.Request.Headers["Referer"].ToString();

            if (string.IsNullOrEmpty(referer))
            {
                context.Result = new ContentResult { Content = "Acesso negado!" };
                return;
            }

            var uri = new Uri(referer);
            string hostReferer = uri.Host;
            string hostServidor = context.HttpContext.Request.Host.Host;

            if (hostReferer != hostServidor)
            {
                context.Result = new ContentResult { Content = "Acesso negado!" };
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
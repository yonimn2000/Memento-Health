using MementoHealth.Attributes;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MementoHealth.Filters
{
    public class PinLockFilter : ActionFilterAttribute
    {
        public const string SessionKey = "PinLockOn";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated
                && filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowThroughPinLockAttribute), false).Length == 0)
            {

                if (Enabled)
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "PinUnlock" },
                        { "returnUrl", filterContext.RequestContext.HttpContext.Request.Path }
                    });
            }
        }

        public static bool Enabled
        {
            get
            {
                object pinLockOnObj = HttpContext.Current.Session[SessionKey];
                return pinLockOnObj != null && (bool)pinLockOnObj;
            }
            set => HttpContext.Current.Session[SessionKey] = value;
        }
    }
}
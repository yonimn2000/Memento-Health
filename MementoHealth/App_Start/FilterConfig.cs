using MementoHealth.Filters;
using System.Web.Mvc;

namespace MementoHealth
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new PinLockFilter());
        }
    }
}
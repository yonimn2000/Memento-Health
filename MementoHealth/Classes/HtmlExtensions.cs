using System.Web;
using System.Web.Mvc;

namespace MementoHealth.Classes
{
    public static class HtmlExtensions
    {
        public static IHtmlString Phone(this HtmlHelper helper, string phone)
        {
            string encoded = HttpUtility.HtmlEncode(phone);
            return new HtmlString(string.IsNullOrWhiteSpace(phone) ? "<span>None</span>" : $"<a href='tel:{encoded}'>{encoded}</a>");
        }

        public static IHtmlString Email(this HtmlHelper helper, string email)
        {
            string encoded = HttpUtility.HtmlEncode(email);
            return new HtmlString(string.IsNullOrWhiteSpace(email) ? "<span>None</span>" : $"<a href='mailto:{encoded}' target='_blank' rel='noopener noreferrer'>{encoded}</a>");
        }
    }
}
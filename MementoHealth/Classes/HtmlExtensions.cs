using System.Web;
using System.Web.Mvc;

namespace MementoHealth.Classes
{
    public static class HtmlExtensions
    {
        public static IHtmlString Phone(this HtmlHelper helper, string phone)
        {
            return new HtmlString(string.IsNullOrWhiteSpace(phone) ? "<span>None</span>" : $"<a href='tel:{phone}'>{phone}</a>");
        }

        public static IHtmlString Email(this HtmlHelper helper, string email)
        {
            return new HtmlString(string.IsNullOrWhiteSpace(email) ? "<span>None</span>" : $"<a href='mailto:{email}' target='_blank' rel='noopener noreferrer'>{email}</a>");
        }
    }
}
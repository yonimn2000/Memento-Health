using System.Web;
using System.Web.Mvc;

namespace MementoHealth.Classes
{
    public static class HtmlExtensions
    {
        public static IHtmlString Phone(this HtmlHelper helper, string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return new HtmlString("<span>None</span>");

            string encodedPhone = HttpUtility.HtmlEncode(phone);
            return new HtmlString($"<a href='tel:{encodedPhone}'>{encodedPhone}</a>");
        }

        public static IHtmlString Email(this HtmlHelper helper, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new HtmlString("<span>None</span>");

            string encodedEmail = HttpUtility.HtmlEncode(email);
            return new HtmlString($"<a href='mailto:{encodedEmail}' target='_blank' rel='noopener noreferrer'>{encodedEmail}</a>");
        }
    }
}
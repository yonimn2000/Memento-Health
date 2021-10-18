using System.Web.Mvc;

namespace MementoHealth.Classes
{
    public static class HtmlExtensions
    {
        public static string Phone(this HtmlHelper helper, string phone)
        {
            return $"<a href='tel:{phone}'>{phone}</a>";
        }

        public static string Email(this HtmlHelper helper, string email)
        {
            return $"<a href='mailto:{email}'>{email}</a>";
        }
    }
}
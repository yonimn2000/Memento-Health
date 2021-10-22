using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Security.Principal;

namespace MementoHealth.Classes
{
    public static class UserExtensions
    {
        public static string GetFullName(this IPrincipal user)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
                return db.Users.Find(user.Identity.GetUserId())?.FullName;
        }

        public static string GetRole(this IPrincipal user)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
                return db.Users.Find(user.Identity.GetUserId())?.Roles.First().Role.Name;
        }
    }
}
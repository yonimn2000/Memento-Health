using MementoHealth.Migrations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MementoHealth.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string PinHash { get; set; }
        public int PinAccessFailedCount { get; set; }

        [Required]
        [DisplayName("Full Name")]
        public string FullName { get; set; }

        [NotMapped]
        [DisplayName("Is Locked Out")]
        public bool LockedOut => LockoutEndDateUtc != default;

        [NotMapped]
        [DisplayName("Lock Out Status")]
        public string LockOutStatus => LockedOut ? "Locked out" : "Unlocked";

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
        }

        public static ApplicationDbContext Create() => new ApplicationDbContext();
    }
}
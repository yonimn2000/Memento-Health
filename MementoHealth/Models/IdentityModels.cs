using MementoHealth.Entities;
using MementoHealth.Migrations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MementoHealth.Models
{
    public class ApplicationUser : IdentityUser<string, IdentityUserLogin, ApplicationUserRole, IdentityUserClaim>
    {
        public string PinHash { get; set; }
        public int PinAccessFailedCount { get; set; }
        public virtual new ICollection<ApplicationUserRole> Roles { get; set; }

        [ForeignKey("Provider")]
        public int? ProviderId { get; set; }
        public virtual Provider Provider { get; set; }

        [Required]
        [DisplayName("Full Name")]
        public string FullName { get; set; }

        [NotMapped]
        [DisplayName("Is Locked Out")]
        public bool LockedOut => LockoutEndDateUtc != default;

        [NotMapped]
        [DisplayName("Lock Out Status")]
        public string LockOutStatus => LockedOut ? "Locked out" : "Unlocked";

        public ApplicationUser()
        {
            Id = Guid.NewGuid().ToString();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, string> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }

    public class ApplicationRole : IdentityRole<string, ApplicationUserRole>
    {
        public virtual new ICollection<ApplicationUserRole> Users { get; set; }

        public ApplicationRole() { } // Needed to work correctly.
        public ApplicationRole(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserLogin, ApplicationUserRole, IdentityUserClaim>
    {
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<FormSubmission> FormSubmissions { get; set; }
        public DbSet<FormQuestion> FormQuestions { get; set; }
        public DbSet<FormQuestionAnswer> FormQuestionsAnswers { get; set; }
        public DbSet<FormQuestionCondition> FormQuestionConditions { get; set; }

        public ApplicationDbContext() : base("DefaultConnection")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
        }

        public static ApplicationDbContext Create() => new ApplicationDbContext();

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUserRole>().HasRequired(s => s.Role).WithMany(g => g.Users).HasForeignKey(s => s.RoleId);
            modelBuilder.Entity<ApplicationUserRole>().HasRequired(s => s.User).WithMany(g => g.Roles).HasForeignKey(s => s.UserId);
            modelBuilder.Entity<ApplicationUser>().HasOptional(u => u.Provider).WithMany(u => u.Staff).WillCascadeOnDelete(true);
        }
    }
}
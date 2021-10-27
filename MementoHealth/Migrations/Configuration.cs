namespace MementoHealth.Migrations
{
    using MementoHealth.Classes;
    using MementoHealth.Entities;
    using MementoHealth.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "MementoHealth.Models.ApplicationDbContext";
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.Roles.AddOrUpdate(r => r.Name,
                new ApplicationRole(Role.SysAdmin),
                new ApplicationRole(Role.ProviderAdmin),
                new ApplicationRole(Role.Doctor),
                new ApplicationRole(Role.Assistant));

#if DEBUG
            string email = Environment.GetEnvironmentVariable("MEMENTO_ADMIN_EMAIL");
            if (!string.IsNullOrWhiteSpace(email) && new EmailAddressAttribute().IsValid(email))
            {
                string providerAdminEmail = email;
                string systemAdminEmail = email.Replace("@", "+admin@");
                string providerEmail = email.Replace("@", "+provider@");
                string doctorEmail = email.Replace("@", "+doctor@");
                string assistantEmail = email.Replace("@", "+assistant@");
                const string initialPassword = "P@ssw0rd";

                using (DbContextTransaction transaction = context.Database.BeginTransaction())
                {
                    ApplicationUserManager userManager = new ApplicationUserManager(new UserStore<ApplicationUser, ApplicationRole, string, IdentityUserLogin, ApplicationUserRole, IdentityUserClaim>(context));
                    userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                    if (!context.Users.Any(u => u.Email == systemAdminEmail))
                    {
                        ApplicationUser sysAdmin = new ApplicationUser
                        {
                            FullName = "System Admin",
                            Email = systemAdminEmail,
                            UserName = systemAdminEmail,
                            EmailConfirmed = true,
                            SecurityStamp = Guid.NewGuid().ToString("D"),
                            PasswordHash = userManager.PasswordHasher.HashPassword(initialPassword),
                            LockoutEnabled = true
                        };
                        userManager.Create(sysAdmin);
                        userManager.AddToRole(sysAdmin.Id, Role.SysAdmin);
                    }

                    Provider provider = context.Providers.SingleOrDefault(p => p.Email.Equals(providerEmail));
                    if (provider == null)
                    {
                        provider = new Provider
                        {
                            Email = providerEmail,
                            Address = "115 Library Dr, Rochester, MI 48309",
                            Name = "My Provider",
                            Phone = "(999) 555-1234",
                        };
                    }

                    if (!context.Users.Any(u => u.Email == providerAdminEmail))
                    {
                        ApplicationUser providerAdmin = new ApplicationUser
                        {
                            FullName = "Provider Admin",
                            Email = providerAdminEmail,
                            UserName = providerAdminEmail,
                            EmailConfirmed = true,
                            SecurityStamp = Guid.NewGuid().ToString("D"),
                            PasswordHash = userManager.PasswordHasher.HashPassword(initialPassword),
                            LockoutEnabled = true,
                            Provider = provider
                        };
                        userManager.Create(providerAdmin);
                        userManager.AddToRole(providerAdmin.Id, Role.ProviderAdmin);
                    }

                    if (!context.Users.Any(u => u.Email == doctorEmail))
                    {
                        ApplicationUser doctor = new ApplicationUser
                        {
                            FullName = "Doctor",
                            Email = doctorEmail,
                            UserName = doctorEmail,
                            EmailConfirmed = true,
                            SecurityStamp = Guid.NewGuid().ToString("D"),
                            PasswordHash = userManager.PasswordHasher.HashPassword(initialPassword),
                            LockoutEnabled = true,
                            Provider = provider
                        };
                        userManager.Create(doctor);
                        userManager.AddToRole(doctor.Id, Role.Doctor);
                    }

                    if (!context.Users.Any(u => u.Email == assistantEmail))
                    {
                        ApplicationUser assistant = new ApplicationUser
                        {
                            FullName = "Assistant",
                            Email = assistantEmail,
                            UserName = assistantEmail,
                            EmailConfirmed = true,
                            SecurityStamp = Guid.NewGuid().ToString("D"),
                            PasswordHash = userManager.PasswordHasher.HashPassword(initialPassword),
                            LockoutEnabled = true,
                            Provider = provider
                        };
                        userManager.Create(assistant);
                        userManager.AddToRole(assistant.Id, Role.Assistant);
                    }

                    transaction.Commit();
                }
            }
#endif
        }
    }
}
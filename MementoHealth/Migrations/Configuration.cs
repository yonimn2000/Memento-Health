namespace MementoHealth.Migrations
{
    using MementoHealth.Models;
    using Microsoft.AspNet.Identity.EntityFramework;
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

            if(context.Roles.Any(r => r.Name.Equals("SiteAdmin")))
                context.Roles.Remove(context.Roles.Where(r => r.Name.Equals("SiteAdmin")).Single());

            context.Roles.AddOrUpdate(r => r.Name,
                new IdentityRole("SysAdmin"),
                new IdentityRole("ProviderAdmin"),
                new IdentityRole("Doctor"),
                new IdentityRole("Assistant"));
        }
    }
}
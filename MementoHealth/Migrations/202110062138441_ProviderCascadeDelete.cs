namespace MementoHealth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProviderCascadeDelete : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "ProviderId", "dbo.Providers");
            AddForeignKey("dbo.AspNetUsers", "ProviderId", "dbo.Providers", "ProviderId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "ProviderId", "dbo.Providers");
            AddForeignKey("dbo.AspNetUsers", "ProviderId", "dbo.Providers", "ProviderId");
        }
    }
}

namespace MementoHealth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class User : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "PinHash", c => c.String());
            AddColumn("dbo.AspNetUsers", "PinAccessFailedCount", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "FullName", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "FullName");
            DropColumn("dbo.AspNetUsers", "PinAccessFailedCount");
            DropColumn("dbo.AspNetUsers", "PinHash");
        }
    }
}

namespace MementoHealth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FormV2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "IsPublished", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forms", "IsPublished");
        }
    }
}

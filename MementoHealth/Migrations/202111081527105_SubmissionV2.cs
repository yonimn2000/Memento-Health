namespace MementoHealth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SubmissionV2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FormSubmissions", "SubmissionStartDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.FormSubmissions", "SubmissionEndDate", c => c.DateTime());
            DropColumn("dbo.FormSubmissions", "SubmissionDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FormSubmissions", "SubmissionDate", c => c.DateTime());
            DropColumn("dbo.FormSubmissions", "SubmissionEndDate");
            DropColumn("dbo.FormSubmissions", "SubmissionStartDate");
        }
    }
}

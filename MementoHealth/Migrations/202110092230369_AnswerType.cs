namespace MementoHealth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AnswerType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FormQuestions", "Type", c => c.String(nullable: false));
            AlterColumn("dbo.FormQuestions", "JsonData", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.FormQuestions", "JsonData", c => c.String(nullable: false));
            DropColumn("dbo.FormQuestions", "Type");
        }
    }
}

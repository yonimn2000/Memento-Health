namespace MementoHealth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AnswerV2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FormQuestionAnswers", "JsonData", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FormQuestionAnswers", "JsonData");
        }
    }
}

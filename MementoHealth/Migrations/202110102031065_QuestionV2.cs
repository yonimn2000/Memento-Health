namespace MementoHealth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestionV2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FormQuestions", "NextQuestionId", "dbo.FormQuestions");
            DropIndex("dbo.FormQuestions", new[] { "NextQuestionId" });
            AddColumn("dbo.FormQuestions", "Number", c => c.Int(nullable: false));
            AddColumn("dbo.FormQuestions", "Type", c => c.String(nullable: false));
            AlterColumn("dbo.FormQuestions", "JsonData", c => c.String());
            DropColumn("dbo.FormQuestions", "NextQuestionId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FormQuestions", "NextQuestionId", c => c.Int());
            AlterColumn("dbo.FormQuestions", "JsonData", c => c.String(nullable: false));
            DropColumn("dbo.FormQuestions", "Type");
            DropColumn("dbo.FormQuestions", "Number");
            CreateIndex("dbo.FormQuestions", "NextQuestionId");
            AddForeignKey("dbo.FormQuestions", "NextQuestionId", "dbo.FormQuestions", "QuestionId");
        }
    }
}

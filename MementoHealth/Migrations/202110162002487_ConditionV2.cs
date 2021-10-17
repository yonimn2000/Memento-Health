namespace MementoHealth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConditionV2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FormQuestionConditions", "Number", c => c.Int(nullable: false));
            AddColumn("dbo.FormQuestionConditions", "JsonData", c => c.String(nullable: false));
            AddColumn("dbo.FormQuestionConditions", "GoToQuestionId", c => c.Int());
            CreateIndex("dbo.FormQuestionConditions", "GoToQuestionId");
            AddForeignKey("dbo.FormQuestionConditions", "GoToQuestionId", "dbo.FormQuestions", "QuestionId");
            DropColumn("dbo.FormQuestionConditions", "JsonCondition");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FormQuestionConditions", "JsonCondition", c => c.String(nullable: false));
            DropForeignKey("dbo.FormQuestionConditions", "GoToQuestionId", "dbo.FormQuestions");
            DropIndex("dbo.FormQuestionConditions", new[] { "GoToQuestionId" });
            DropColumn("dbo.FormQuestionConditions", "GoToQuestionId");
            DropColumn("dbo.FormQuestionConditions", "JsonData");
            DropColumn("dbo.FormQuestionConditions", "Number");
        }
    }
}

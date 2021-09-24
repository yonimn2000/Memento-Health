namespace MementoHealth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Db0 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FormQuestionConditions",
                c => new
                    {
                        ConditionId = c.Int(nullable: false, identity: true),
                        JsonCondition = c.String(nullable: false),
                        QuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ConditionId)
                .ForeignKey("dbo.FormQuestions", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.FormQuestions",
                c => new
                    {
                        QuestionId = c.Int(nullable: false, identity: true),
                        Question = c.String(nullable: false),
                        JsonData = c.String(nullable: false),
                        IsRequired = c.Boolean(nullable: false),
                        FormId = c.Int(nullable: false),
                        NextQuestionId = c.Int(),
                    })
                .PrimaryKey(t => t.QuestionId)
                .ForeignKey("dbo.Forms", t => t.FormId, cascadeDelete: true)
                .ForeignKey("dbo.FormQuestions", t => t.NextQuestionId)
                .Index(t => t.FormId)
                .Index(t => t.NextQuestionId);
            
            CreateTable(
                "dbo.FormQuestionAnswers",
                c => new
                    {
                        AnswerId = c.Int(nullable: false, identity: true),
                        SubmissionId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AnswerId)
                .ForeignKey("dbo.FormSubmissions", t => t.SubmissionId, cascadeDelete: true)
                .ForeignKey("dbo.FormQuestions", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.SubmissionId)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.FormSubmissions",
                c => new
                    {
                        SubmissionId = c.Int(nullable: false, identity: true),
                        SubmissionDate = c.DateTime(),
                        PatientId = c.Int(),
                        FormId = c.Int(),
                    })
                .PrimaryKey(t => t.SubmissionId)
                .ForeignKey("dbo.Forms", t => t.FormId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .Index(t => t.PatientId)
                .Index(t => t.FormId);
            
            CreateTable(
                "dbo.Forms",
                c => new
                    {
                        FormId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        ProviderId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FormId)
                .ForeignKey("dbo.Providers", t => t.ProviderId, cascadeDelete: true)
                .Index(t => t.ProviderId);
            
            CreateTable(
                "dbo.Providers",
                c => new
                    {
                        ProviderId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Phone = c.String(nullable: false),
                        Address = c.String(nullable: false),
                        Email = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ProviderId);
            
            CreateTable(
                "dbo.Patients",
                c => new
                    {
                        PatientId = c.Int(nullable: false, identity: true),
                        ExternalPatientId = c.String(),
                        FullName = c.String(nullable: false),
                        Birthday = c.DateTime(nullable: false),
                        ProviderId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PatientId)
                .ForeignKey("dbo.Providers", t => t.ProviderId, cascadeDelete: true)
                .Index(t => t.ProviderId);
            
            AddColumn("dbo.AspNetUsers", "ProviderId", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "ProviderId");
            AddForeignKey("dbo.AspNetUsers", "ProviderId", "dbo.Providers", "ProviderId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FormQuestionConditions", "QuestionId", "dbo.FormQuestions");
            DropForeignKey("dbo.FormQuestions", "NextQuestionId", "dbo.FormQuestions");
            DropForeignKey("dbo.FormQuestions", "FormId", "dbo.Forms");
            DropForeignKey("dbo.FormQuestionAnswers", "QuestionId", "dbo.FormQuestions");
            DropForeignKey("dbo.FormQuestionAnswers", "SubmissionId", "dbo.FormSubmissions");
            DropForeignKey("dbo.FormSubmissions", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.FormSubmissions", "FormId", "dbo.Forms");
            DropForeignKey("dbo.Forms", "ProviderId", "dbo.Providers");
            DropForeignKey("dbo.AspNetUsers", "ProviderId", "dbo.Providers");
            DropForeignKey("dbo.Patients", "ProviderId", "dbo.Providers");
            DropIndex("dbo.AspNetUsers", new[] { "ProviderId" });
            DropIndex("dbo.Patients", new[] { "ProviderId" });
            DropIndex("dbo.Forms", new[] { "ProviderId" });
            DropIndex("dbo.FormSubmissions", new[] { "FormId" });
            DropIndex("dbo.FormSubmissions", new[] { "PatientId" });
            DropIndex("dbo.FormQuestionAnswers", new[] { "QuestionId" });
            DropIndex("dbo.FormQuestionAnswers", new[] { "SubmissionId" });
            DropIndex("dbo.FormQuestions", new[] { "NextQuestionId" });
            DropIndex("dbo.FormQuestions", new[] { "FormId" });
            DropIndex("dbo.FormQuestionConditions", new[] { "QuestionId" });
            DropColumn("dbo.AspNetUsers", "ProviderId");
            DropTable("dbo.Patients");
            DropTable("dbo.Providers");
            DropTable("dbo.Forms");
            DropTable("dbo.FormSubmissions");
            DropTable("dbo.FormQuestionAnswers");
            DropTable("dbo.FormQuestions");
            DropTable("dbo.FormQuestionConditions");
        }
    }
}

namespace EmployabilityWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingFeedback : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SaFeedback",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Rating = c.Int(nullable: false),
                        Comment = c.String(),
                        BasicUser_Id = c.String(nullable: false, maxLength: 128),
                        selfAssessment_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BasicUser", t => t.BasicUser_Id)
                .ForeignKey("dbo.SelfAssessment", t => t.selfAssessment_Id, cascadeDelete: true)
                .Index(t => t.BasicUser_Id)
                .Index(t => t.selfAssessment_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SaFeedback", "selfAssessment_Id", "dbo.SelfAssessment");
            DropForeignKey("dbo.SaFeedback", "BasicUser_Id", "dbo.BasicUser");
            DropIndex("dbo.SaFeedback", new[] { "selfAssessment_Id" });
            DropIndex("dbo.SaFeedback", new[] { "BasicUser_Id" });
            DropTable("dbo.SaFeedback");
        }
    }
}

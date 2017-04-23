namespace EmployabilityWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSurveyData : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SubAnswer", "Answer_Id", "dbo.Answer");
            DropIndex("dbo.SubAnswer", new[] { "Answer_Id" });
            RenameColumn(table: "dbo.SimplifiedAnswer", name: "Element_Id", newName: "SimplifiedElement_Id");
            RenameIndex(table: "dbo.SimplifiedAnswer", name: "IX_Element_Id", newName: "IX_SimplifiedElement_Id");
            AddColumn("dbo.Answer", "BasicUser_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.CoreSkillElement", "Position", c => c.Int(nullable: false));
            AddColumn("dbo.DetailedOption", "Position", c => c.Int(nullable: false));
            AlterColumn("dbo.SubAnswer", "Answer_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.Answer", "BasicUser_Id");
            CreateIndex("dbo.SubAnswer", "Answer_Id");
            AddForeignKey("dbo.Answer", "BasicUser_Id", "dbo.BasicUser", "Id");
            AddForeignKey("dbo.SubAnswer", "Answer_Id", "dbo.Answer", "Id", cascadeDelete: true);
            DropColumn("dbo.Answer", "User_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Answer", "User_Id", c => c.String(nullable: false, maxLength: 128));
            DropForeignKey("dbo.SubAnswer", "Answer_Id", "dbo.Answer");
            DropForeignKey("dbo.Answer", "BasicUser_Id", "dbo.BasicUser");
            DropIndex("dbo.SubAnswer", new[] { "Answer_Id" });
            DropIndex("dbo.Answer", new[] { "BasicUser_Id" });
            AlterColumn("dbo.SubAnswer", "Answer_Id", c => c.Int());
            DropColumn("dbo.DetailedOption", "Position");
            DropColumn("dbo.CoreSkillElement", "Position");
            DropColumn("dbo.Answer", "BasicUser_Id");
            RenameIndex(table: "dbo.SimplifiedAnswer", name: "IX_SimplifiedElement_Id", newName: "IX_Element_Id");
            RenameColumn(table: "dbo.SimplifiedAnswer", name: "SimplifiedElement_Id", newName: "Element_Id");
            CreateIndex("dbo.SubAnswer", "Answer_Id");
            AddForeignKey("dbo.SubAnswer", "Answer_Id", "dbo.Answer", "Id");
        }
    }
}

namespace EmployabilityWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingBasicUserIdInAnswer : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Answer", new[] { "BasicUser_Id" });
            AlterColumn("dbo.Answer", "BasicUser_Id", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Answer", "BasicUser_Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Answer", new[] { "BasicUser_Id" });
            AlterColumn("dbo.Answer", "BasicUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Answer", "BasicUser_Id");
        }
    }
}

namespace EmployabilityWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Answer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TimeStamp = c.DateTime(nullable: false, defaultValueSql: "GETDATE()"),
                        SelfAssessment_Id = c.Int(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SelfAssessment", t => t.SelfAssessment_Id, cascadeDelete: true)
                .Index(t => t.SelfAssessment_Id);
            
            CreateTable(
                "dbo.SelfAssessment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        TimeStamp = c.DateTime(nullable: false, defaultValueSql: "GETDATE()"),
                        Organisation_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organisation", t => t.Organisation_Id)
                .Index(t => t.Organisation_Id);
            
            CreateTable(
                "dbo.CoreSkill",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        SelfAssessment_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SelfAssessment", t => t.SelfAssessment_Id, cascadeDelete: true)
                .Index(t => t.SelfAssessment_Id);
            
            CreateTable(
                "dbo.CoreSkillElement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        CoreSkill_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CoreSkill", t => t.CoreSkill_Id)
                .Index(t => t.CoreSkill_Id);
            
            CreateTable(
                "dbo.DetailedOption",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(nullable: false),
                        DetailedElement_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CoreSkillElement", t => t.DetailedElement_Id)
                .Index(t => t.DetailedElement_Id);
            
            CreateTable(
                "dbo.SubAnswer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Answer_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Answer", t => t.Answer_Id)
                .Index(t => t.Answer_Id);
            
            CreateTable(
                "dbo.ApplicationUser",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.IdentityUserClaim",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.IdentityUserLogin",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(),
                        ProviderKey = c.String(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.IdentityUserRole",
                c => new
                    {
                        RoleId = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        IdentityRole_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.RoleId, t.UserId })
                .ForeignKey("dbo.IdentityRole", t => t.IdentityRole_Id)
                .Index(t => t.IdentityRole_Id);
            
            CreateTable(
                "dbo.Organisation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrganisationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OrganisationUser", t => t.OrganisationUser_Id)
                .Index(t => t.OrganisationUser_Id);
            
            CreateTable(
                "dbo.IdentityRole",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OrganisationUser",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(),
                        StartTime = c.DateTime(),
                        EndTime = c.DateTime(),
                        OrganisationName = c.String(nullable: false),
                        Domain = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AdminUser",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DataUser",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BasicUser",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Organisation_Id = c.Int(),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(),
                        GivenName = c.String(),
                        FamilyName = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organisation", t => t.Organisation_Id)
                .Index(t => t.Organisation_Id);
            
            CreateTable(
                "dbo.DetailedAnswer",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Choice_Id = c.Int(nullable: false),
                        DetailedElement_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SubAnswer", t => t.Id)
                .ForeignKey("dbo.DetailedOption", t => t.Choice_Id, cascadeDelete: true)
                .ForeignKey("dbo.CoreSkillElement", t => t.DetailedElement_Id, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.Choice_Id)
                .Index(t => t.DetailedElement_Id);
            
            CreateTable(
                "dbo.SimplifiedAnswer",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Element_Id = c.Int(nullable: false),
                        Choice = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SubAnswer", t => t.Id)
                .ForeignKey("dbo.CoreSkillElement", t => t.Element_Id, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.Element_Id);
            
            CreateTable(
                "dbo.TextAnswer",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        TextElement_Id = c.Int(nullable: false),
                        Text = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SubAnswer", t => t.Id)
                .ForeignKey("dbo.CoreSkillElement", t => t.TextElement_Id, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.TextElement_Id);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TextAnswer", "TextElement_Id", "dbo.CoreSkillElement");
            DropForeignKey("dbo.TextAnswer", "Id", "dbo.SubAnswer");
            DropForeignKey("dbo.SimplifiedAnswer", "Element_Id", "dbo.CoreSkillElement");
            DropForeignKey("dbo.SimplifiedAnswer", "Id", "dbo.SubAnswer");
            DropForeignKey("dbo.DetailedAnswer", "DetailedElement_Id", "dbo.CoreSkillElement");
            DropForeignKey("dbo.DetailedAnswer", "Choice_Id", "dbo.DetailedOption");
            DropForeignKey("dbo.DetailedAnswer", "Id", "dbo.SubAnswer");
            DropForeignKey("dbo.BasicUser", "Organisation_Id", "dbo.Organisation");
            DropForeignKey("dbo.IdentityUserRole", "IdentityRole_Id", "dbo.IdentityRole");
            DropForeignKey("dbo.SelfAssessment", "Organisation_Id", "dbo.Organisation");
            DropForeignKey("dbo.Organisation", "OrganisationUser_Id", "dbo.OrganisationUser");
            DropForeignKey("dbo.SubAnswer", "Answer_Id", "dbo.Answer");
            DropForeignKey("dbo.Answer", "SelfAssessment_Id", "dbo.SelfAssessment");
            DropForeignKey("dbo.CoreSkill", "SelfAssessment_Id", "dbo.SelfAssessment");
            DropForeignKey("dbo.CoreSkillElement", "CoreSkill_Id", "dbo.CoreSkill");
            DropForeignKey("dbo.DetailedOption", "DetailedElement_Id", "dbo.CoreSkillElement");
            DropIndex("dbo.TextAnswer", new[] { "TextElement_Id" });
            DropIndex("dbo.TextAnswer", new[] { "Id" });
            DropIndex("dbo.SimplifiedAnswer", new[] { "Element_Id" });
            DropIndex("dbo.SimplifiedAnswer", new[] { "Id" });
            DropIndex("dbo.DetailedAnswer", new[] { "DetailedElement_Id" });
            DropIndex("dbo.DetailedAnswer", new[] { "Choice_Id" });
            DropIndex("dbo.DetailedAnswer", new[] { "Id" });
            DropIndex("dbo.BasicUser", new[] { "Organisation_Id" });
            DropIndex("dbo.Organisation", new[] { "OrganisationUser_Id" });
            DropIndex("dbo.IdentityUserRole", new[] { "IdentityRole_Id" });
            DropIndex("dbo.SubAnswer", new[] { "Answer_Id" });
            DropIndex("dbo.DetailedOption", new[] { "DetailedElement_Id" });
            DropIndex("dbo.CoreSkillElement", new[] { "CoreSkill_Id" });
            DropIndex("dbo.CoreSkill", new[] { "SelfAssessment_Id" });
            DropIndex("dbo.SelfAssessment", new[] { "Organisation_Id" });
            DropIndex("dbo.Answer", new[] { "SelfAssessment_Id" });
            DropTable("dbo.TextAnswer");
            DropTable("dbo.SimplifiedAnswer");
            DropTable("dbo.DetailedAnswer");
            DropTable("dbo.BasicUser");
            DropTable("dbo.DataUser");
            DropTable("dbo.AdminUser");
            DropTable("dbo.OrganisationUser");
            DropTable("dbo.IdentityRole");
            DropTable("dbo.Organisation");
            DropTable("dbo.IdentityUserRole");
            DropTable("dbo.IdentityUserLogin");
            DropTable("dbo.IdentityUserClaim");
            DropTable("dbo.ApplicationUser");
            DropTable("dbo.SubAnswer");
            DropTable("dbo.DetailedOption");
            DropTable("dbo.CoreSkillElement");
            DropTable("dbo.CoreSkill");
            DropTable("dbo.SelfAssessment");
            DropTable("dbo.Answer");
        }
    }
}

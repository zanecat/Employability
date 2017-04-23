using System.Data.Entity.ModelConfiguration.Conventions;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace EmployabilityWebApp.Models
{
    public class EmployabilityDbContext : IdentityDbContext<ApplicationUser>
    {
        public EmployabilityDbContext()
            : base("DefaultConnection", false)
        {
        }

        public static EmployabilityDbContext Create()
        {
            return new EmployabilityDbContext();
        }

        // Use virtual properties to allow them to be stubbed over for testing.
        public virtual DbSet<SubAnswer> SubAnswers { get; set; }
        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<DetailedOption> DetailedOptions { get; set; }
        public virtual DbSet<CoreSkillElement> CoreSkillElements { get; set; }
        public virtual DbSet<CoreSkill> CoreSkills { get; set; }
        public virtual DbSet<SelfAssessment> SelfAssessments { get; set; }
        public virtual DbSet<Organisation> Organisations { get; set; }
        public virtual DbSet<OrganisationUser> OrganisationUsers { get; set; }
        public virtual DbSet<SaFeedback> SaFeedbacks { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<IdentityUserLogin>().HasKey(l => l.UserId);
            modelBuilder.Entity<IdentityRole>().HasKey(r => r.Id);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });

            // Here it is telling Entity framework to create a table for each concrete subclass.
            // This is so called, table per concrete class.
            modelBuilder.Entity<AdminUser>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("AdminUser");
            });

            modelBuilder.Entity<OrganisationUser>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("OrganisationUser");
            });

            modelBuilder.Entity<DataUser>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("DataUser");
            });

            modelBuilder.Entity<BasicUser>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("BasicUser");
            });

        }
    }
}

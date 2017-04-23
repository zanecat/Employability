
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using EmployabilityWebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EmployabilityWebApp.Migrations
{

    internal sealed class Configuration : DbMigrationsConfiguration<EmployabilityDbContext>
    {
        private const string AdminEmail = "admin@example.com";
        private const string AdminPassword = "Admin@123456";

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "EmployabilityWebApp.Models.DefaultConnection";
        }

        // We can assume all the data doesn't already exist as part of the
        // contract of this method.
        public static void InitializeIdentityForEF(EmployabilityDbContext db)
        {
            var userManager = new UserManager<AdminUser>(new UserStore<AdminUser>(db));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            // Create the four roles.
            roleManager.Create(new IdentityRole(AdminUser.Role));
            roleManager.Create(new IdentityRole(OrganisationUser.Role));
            roleManager.Create(new IdentityRole(BasicUser.Role));
            roleManager.Create(new IdentityRole(DataUser.Role));

            // Create Admin user and make them an admin.
            var user = new AdminUser(AdminEmail);
            userManager.Create(user, AdminPassword);
            userManager.SetLockoutEnabled(user.Id, false);
            userManager.AddToRole(user.Id, AdminUser.Role);
        }

        protected override void Seed(EmployabilityDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            // Do nothing if we're already populated.
            if (context.SelfAssessments.Count() != 0)
            {
                return;
            }

            InitializeIdentityForEF(context);

            List<CoreSkill> coreSkills = new List<CoreSkill>
            {
                        new CoreSkill
                        (
                            "Support",
                            "How well you operate in the workplace is shaped by your family, health and other personal circumstances, as well as the attributes of your role and work. Before completing My Employability please reflect on where you are at this time.",
                            new List<CoreSkillElement>
                            {
                                new SimplifiedElement
                                (
                                    "Personal Support \nI am well  and supported  in my personal life",
                                    1
                                ),
                                new SimplifiedElement
                                (
                                    "Professional Support \nI am supported and challenged in my professional life",
                                    2
                                ),
                                new TextElement
                                (
                                    "Comments:",
                                    3
                                )
                            }
                        ),
                        new CoreSkill
                        (
                            "Attributes",
                            "How you manage yourself on a day-to-day basis and invest in your long-term career plays a significant role in your work life.",
                            new List<CoreSkillElement>
                            {
                                ///need to discuss about this part
                                new SimplifiedElement
                                (
                                    "Professional Attitude\nI am usually prepared and ready for a changing work day",
                                    1
                                ),
                                new SimplifiedElement
                                (
                                    "Cultural Fit\nI understand the different ways varying enterprises operate and how that could impact on me",
                                    2
                                ),
                                new SimplifiedElement
                                (
                                    "Commercial Awareness\nI keep up-to-date on social and economic developments locally and globally",
                                    3
                                ),
                                new SimplifiedElement
                                (
                                    "Social Media\nI make good use of social and professional media platforms",
                                    4
                                ),
                                new SimplifiedElement
                                (
                                    "Work Ethic\nI have a strong work ethic, and try to deliver more than required",
                                    5
                                ),
                                new SimplifiedElement
                                (
                                    "Networking\nI maintain a healthy personal and professional network",
                                    6
                                ),
                                new SimplifiedElement
                                (
                                    "Life-long learning\nI am committed to on-going life-long learning to function effectively in a changing place.",
                                    7
                                ),
                                new SimplifiedElement
                                (
                                    "Professional Demeanor\nI maintain a professional demeanor at work",
                                    8
                                ),
                                new SimplifiedElement
                                (
                                    "Dependable\nI am consistent, predictable and reliable",
                                    9
                                ),
                                new TextElement
                                (
                                    "Provide evidence of your claims",
                                    10
                                ),
                                new DetailedElement(
                                    "Understanding your strengths and weakness, knowing the opportunities and threats to your work effort builds your capacity to survive in a constantly changing workplace and job landscape. ",
                                    new List<DetailedOption>{
                                        new DetailedOption(
                                            "I do not know much about who I am, or the work environment or what I can do to improve my work situation",
                                            1
                                            ),
                                        new DetailedOption(
                                            "I have some idea of who I am, what work is, what I want to do but no idea how to go about building my career options",
                                            2
                                            ),
                                        new DetailedOption(
                                            "I know myself well enough to  have identified a few options and starting to explore building my own capacities",
                                            3
                                            ),
                                        new DetailedOption(
                                            "I am gaining confidence and understanding of my capacities as a professional and have  identified clear options",
                                            4
                                            ),
                                        new DetailedOption(
                                            "I know myself very well, I know the opportunities available to me and feel confident I can progress my career",
                                            5
                                            )
                                    },
                                    11
                               ),
                                ///Need to discuss with this part
                                new TextElement
                                (
                                    "Provide evidence of your claims",
                                    12
                                ),

                            }
                        ),
                        new CoreSkill
                        (
                            "Learned Skills",
                            "Understanding your own skill set, what you naturally do well, what you need to work on, allows you to build on your personal strengths and adapt your behaviour to maximise professional work satisfaction and output.",
                            new List<CoreSkillElement>
                            {
                                new SimplifiedElement
                                (
                                    "Verbal Communication\n My verbal communication is very clear and targeted",
                                    1
                                ),
                                new SimplifiedElement
                                (
                                    "Written Communication\n I write clearly and logically, adapting to different audiences as required",
                                    2
                                ),
                                new SimplifiedElement
                                (
                                    "Process Design\n I can design processes for desired outcomes",
                                    3
                                ),
                                new SimplifiedElement
                                (
                                    "Numeracy \nMy numeracy skills are appropriate for my career",
                                    4
                                ),
                                new SimplifiedElement
                                (
                                    "Critical Thinking \nI think critically about workplace issues, and articulate any concerns",
                                    5
                                ),
                                new SimplifiedElement
                                (
                                    "Resourceful \nI am resourceful, and can find alternate sources to contribute to a task",
                                    6
                                ),
                                new SimplifiedElement
                                (
                                    "Digitally Numerate \nMy IT skills are appropriate for  my career",
                                    7
                                ),
                                new SimplifiedElement
                                (
                                    "Futurist \nI understand the potential of IT in the workplace",
                                    8
                                ),
                                new SimplifiedElement
                                (
                                    "Creative \nI am creative, able to think outside the box to find innovative solutions",
                                    9
                                ),
                                new SimplifiedElement
                                (
                                    "Data Analysis \nI am able to make sense of a lot of information for a particular situation",
                                    10
                                ),
                                new SimplifiedElement
                                (
                                    "On Line Communication \nI am able to work effectively in online environments",
                                    11
                                ),
                                new TextElement
                                (
                                    "Provide evidence of your at least three of your claims: ",
                                    12
                                ),
                                new TextElement
                                (
                                    "Looking at these skills again, what could you do to develop the three areas in which you feel you have the least capacity? \n First skill development: ",
                                    13
                                ),
                                new TextElement
                                (
                                    "Second skill development: ",
                                    14
                                ),
                                new TextElement
                                (
                                    "Third skill development: ",
                                    15
                                )
                            }
                        ),
                        new CoreSkill
                        (
                            "Operating in a Workplace",
                            "A successful enterprise requires professionals to work together. How you manage in the work environment and interact with others can significantly shape your career. ",
                            new List<CoreSkillElement>
                            {
                                new SimplifiedElement
                                (
                                    "Workplace Contribution \nI think about my contribution to the workplace",
                                    1
                                ),
                                new SimplifiedElement
                                (
                                    "Workplace protocols \nI understand workplace protocols and processes",
                                    2
                                ),
                                new SimplifiedElement
                                (
                                    "Work With Others \nI am able to work effectively with others and contribute to common tasks",
                                    3
                                ),
                                new SimplifiedElement
                                (
                                    "Leadership \nI show leadership   by empowering people and directing activity",
                                    4
                                ),
                                new SimplifiedElement
                                (
                                    "Cultural Diversity \nI feel comfortable working with people with different cultural , sexual, and professional backgrounds",
                                    5
                                ),
                                new SimplifiedElement
                                (
                                    "Interpersonal Skills \nI can connect meaningfully with others to simulate reactions and desired interactions",
                                    6
                                ),
                                new SimplifiedElement
                                (
                                    "Decision making \nI am comfortable making difficult decisions that affect others",
                                    7
                                ),
                                new SimplifiedElement
                                (
                                    "Manage Conflict \nI effectively manage workplace conflict",
                                    8
                                ),
                                new SimplifiedElement
                                (
                                    "Integrity \nI treat others with honesty, fairness and respect",
                                    9
                                ),
                                new TextElement
                                (
                                    "Provide evidence of three of your claims",
                                    10
                                ),
                                new TextElement
                                (
                                    "Looking at those skills again, what could you do to build up the three areas that you feel you have the least capacity in? \n First skill development: ",
                                    11
                                ),
                                new TextElement
                                (
                                    "Second skill development: ",
                                    12
                                ),
                                new TextElement
                                (
                                    "Third skill development: ",
                                    13
                                )
                            }
                        ),
            };

            // new version of the questions
            context.SelfAssessments.Add(new SelfAssessment()
            {
                Title = "Final Draft",
                Description = "Final version of employability survey",
                CoreSkills = coreSkills
            });

            context.SaveChanges();
        }
    }
}

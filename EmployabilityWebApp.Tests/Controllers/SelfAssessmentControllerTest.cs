using EmployabilityWebApp.Controllers;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.Services;
using EmployabilityWebApp.ViewModels;
using Microsoft.AspNet.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.IO;
using System.Web;
using System.Web.Mvc;
using EmployabilityWebApp.Tests.TestHelpers;
using Xunit;
using Moq;
using System.Collections.Generic;
using EmployabilityWebApp.ViewModels;

namespace EmployabilityWebApp.Tests.Controllers
{
    public class SelfAssessmentControllerTest
    {
        internal SelfAssessmentController target;

        internal readonly Mock<EmployabilityDbContext> db =
            new Mock<EmployabilityDbContext>();

        internal Mock<IUserStore<ApplicationUser>> UserStore =
            new Mock<IUserStore<ApplicationUser>>();

        internal Mock<IReportPdfGenerateService> ReportPdfGenerateService =
            new Mock<IReportPdfGenerateService>();

        internal List<SelfAssessment> selfAssessments = new List<SelfAssessment>();

        private const string UserName = "mingyouf";
        private readonly Mock<ControllerContext> _context = new Mock<ControllerContext>();
        private readonly Mock<HttpResponseBase> _response = new Mock<HttpResponseBase>();
        private readonly Mock<IIdentity> _identity = new Mock<IIdentity>();
        internal Mock<BasicUser> User = new Mock<BasicUser>();
        internal Mock<AdminUser> Admin = new Mock<AdminUser>();

        internal Mock<SelfAssessment> SelfAssessment = new Mock<SelfAssessment>();

        public SelfAssessmentControllerTest()
        {
            var mapper = AutoMapConfig.CreateMapper();
            _context.Setup(c => c.HttpContext.Response).Returns(_response.Object);
            target = new SelfAssessmentController(db.Object, mapper, ReportPdfGenerateService.Object, UserStore.Object)
            {
                ControllerContext = _context.Object
            };
        }

        // Provides a logged in basic user.
        internal void GivenALoggedInUser()
        {
            _identity.Setup(i => i.Name).Returns(UserName);
            _context.SetupGet(c => c.HttpContext.User.Identity).Returns(_identity.Object);
            UserStore.Setup(us => us.FindByNameAsync(UserName)).ReturnsAsync(User.Object);
        }

        /// Provides a logged in admin user
        internal void GivenALoggedInAdminUser()
        {
            _identity.Setup(i => i.Name).Returns(UserName);
            _context.SetupGet(c => c.HttpContext.User.Identity).Returns(_identity.Object);
            UserStore.Setup(us => us.FindByNameAsync(UserName)).ReturnsAsync(Admin.Object);
        }

        // Provide a self assessment with ID
        internal void GivenASelfAssessmentWithId(int id)
        {
            SelfAssessment = new Mock<SelfAssessment>(id);
        }
    }

    public class SelfAssessmentController_GetSurveyReportTest : SelfAssessmentControllerTest
    {
        private readonly List<Answer> _answers = new List<Answer>();

        internal Mock<Answer> Answer = new Mock<Answer>(1, DateTime.Now);

        internal Mock<Answer> OldAnswer = new Mock<Answer>(2, DateTime.Now);
        internal Mock<Answer> NewAnswer = new Mock<Answer>(3, DateTime.Now.AddTicks(1));

        private readonly Mock<MemoryStream> _pdfOutputStream = new Mock<MemoryStream>();

        public SelfAssessmentController_GetSurveyReportTest()
        {
            SelfAssessment.SetupProperty(sa => sa.CoreSkills);
        }

        [Fact]
        public void ItShouldRedirectToTheSamePageWhenThereIsNoAnswer()
        {
            var id = 123;

            // Given a ready state
            GivenALoggedInUser();
            GivenASelfAssessmentWithId(id);

            // User has no answer
            GivenAUserHasNoAnswer();

            // Expect the result to be the correct redirection with the warning flag.
            var redirect = target.SurveyReport(id) as RedirectToRouteResult;
            Assert.NotNull(redirect);
            Assert.Equal("Start", redirect.RouteValues["action"]);
            Assert.Equal(true, target.TempData["NoAnswer"]);
        }

        [Fact]
        public void ItShouldRedirectToTheSamePageWhenReportGenerated()
        {
            var id = 123;

            // Given a ready state
            GivenALoggedInUser();
            GivenASelfAssessmentWithId(id);

            // User has an answer for the self assessment
            GivenAnAnswerOfSelfAssessmentWithCurrentUser();
            GivenAPdfOutputStreamBeingReturned(Answer.Object);

            // Generate report
            var redirect = target.SurveyReport(id);

            // Expect the report generating service being executed once
            ReportPdfGenerateService.Verify(rpgs =>
                rpgs.GenerateSurveyReport(Answer.Object, User.Object), Times.Once);

            // Expect the redirect result to be null
            Assert.Null(redirect);
        }

        [Fact]
        public void ItShouldUseTheLatestAnswerForGeneratingReport()
        {
            var id = 123;

            // Given a ready state
            GivenALoggedInUser();
            GivenASelfAssessmentWithId(id);

            // User has an answer for the self assessment
            GivenAnAnswerListOfSelfAssessmentForCurrentUser();
            GivenAPdfOutputStreamBeingReturned(NewAnswer.Object);

            // Generate report
            var redirect = target.SurveyReport(id);

            // Expect the report generating service being executed once
            ReportPdfGenerateService.Verify(rpgs =>
                rpgs.GenerateSurveyReport(NewAnswer.Object, User.Object), Times.Once);

            // Expect the redirect result to be null
            Assert.Null(redirect);
        }

        // provide a user has no answer for a given self assessment
        private void GivenAUserHasNoAnswer()
        {
            User.SetupGet(u => u.Answers).Returns(new List<Answer>());
        }

        // provide an answer for a user with a given self assessment
        private void GivenAnAnswerOfSelfAssessmentWithCurrentUser()
        {
            Answer.SetupGet(a => a.SelfAssessment).Returns(SelfAssessment.Object);
            _answers.Add(Answer.Object);
            User.SetupGet(u => u.Answers).Returns(_answers);
        }

        // provide a user has multiple answers for a given self assessment
        private void GivenAnAnswerListOfSelfAssessmentForCurrentUser()
        {
            OldAnswer.SetupGet(a => a.SelfAssessment).Returns(SelfAssessment.Object);
            NewAnswer.SetupGet(a => a.SelfAssessment).Returns(SelfAssessment.Object);
            _answers.Add(NewAnswer.Object);
            _answers.Add(OldAnswer.Object);
            User.SetupGet(u => u.Answers).Returns(_answers);
        }

        // provide a pdf output stream being returned
        private void GivenAPdfOutputStreamBeingReturned(Answer answer)
        {
            ReportPdfGenerateService.Setup(rpgs => rpgs.GenerateSurveyReport(answer, User.Object))
                .Returns(_pdfOutputStream.Object);
        }
    }

    public class SelfAssessmentController_GetSummaryReportTest : SelfAssessmentControllerTest
    {
        private readonly FakeDbSet<SelfAssessment> _saDbSet = new FakeSelfAssessmentDbSet();
        private readonly FakeDbSet<Answer> _answerDbSet = new FakeDbSet<Answer>();

        private readonly Mock<MemoryStream> _pdfOutputStream = new Mock<MemoryStream>();

        public SelfAssessmentController_GetSummaryReportTest()
        {
            SelfAssessment.SetupProperty(sa => sa.CoreSkills);
        }

        [Fact]
        public void ItShouldRedirectToTheSamePageWhenThereIsNoSelfAssessment()
        {
            // Given a ready state
            GivenALoggedInAdminUser();
            GivenSelfAssessmentsWithAnswersInSystem(0);

            // Expect the result to be the correct redirection with the warning flag.
            var redirect = target.SummaryReport() as RedirectToRouteResult;
            Assert.NotNull(redirect);
            Assert.Equal("Index", redirect.RouteValues["action"]);
            Assert.Equal(true, target.TempData["NoSelfAssessment"]);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void ItShouldRedirectToTheSamePageWhenReportGenerated(int nSelfAssessment)
        {
            // Given a ready state
            GivenALoggedInAdminUser();
            GivenSelfAssessmentsWithAnswersInSystem(nSelfAssessment);

            // Get self assessments and answers
            var selfAssessments = _saDbSet.OrderBy(s => s.TimeStamp).ToList();
            var answers = _answerDbSet.ToList();

            // User has an answer for the self assessment
            GivenAPdfOutputStreamBeingReturned(selfAssessments, answers);

            // Generate report
            var redirect = target.SummaryReport();

            // Expect the report generating service being executed once
            ReportPdfGenerateService.Verify(rpgs =>
                rpgs.GenerateSummaryReport(selfAssessments, answers, Admin.Object), Times.Once);

            // Expect the redirect result to be null
            Assert.Null(redirect);
        }

        // provide a user has no answer for a given self assessment
        private void GivenSelfAssessmentsWithAnswersInSystem(int nSelfAssessment)
        {
            var selfAssessments = new List<SelfAssessment>();
            selfAssessments.SetUpSelfAssessments(nSelfAssessment);
            var answers = new List<Answer>();
            answers.SetUpAnswersWithSelfAssessments(nSelfAssessment, selfAssessments);

            // Given that the SelfAssessments can be found
            db.SetupGet(d => d.SelfAssessments).Returns(_saDbSet);
            foreach (var selfAssessment in selfAssessments)
            {
                _saDbSet.Add(selfAssessment);
            }

            db.SetupGet(d => d.Answers).Returns(_answerDbSet);
            foreach (var answer in answers)
            {
                _answerDbSet.Add(answer);
            }
        }

        // provide a pdf output stream being returned
        private void GivenAPdfOutputStreamBeingReturned(ICollection<SelfAssessment> selfAssessments,
            ICollection<Answer> answers)
        {
            ReportPdfGenerateService.Setup(rpgs =>
                rpgs.GenerateSummaryReport(selfAssessments, answers, Admin.Object))
                    .Returns(_pdfOutputStream.Object);
        }
    }

    /// <summary>
    /// Unit test for functions other than SurveyReport
    /// </summary>
    public class SelfAssessmentController_NormalizedInterfaceTest
    {
        SelfAssessmentController target;

        private const string USER_NAME = "kaiqiy";
        private Mock<ControllerContext> context = new Mock<ControllerContext>();
        private Mock<IIdentity> identity = new Mock<IIdentity>();

        //For simulate dbset
        private FakeSelfAssessmentDbSet saDbset = new FakeSelfAssessmentDbSet();
        private Mock<EmployabilityDbContext> mockContext = new Mock<EmployabilityDbContext>();

        public void GivenALoggedInUser()
        {
            identity.Setup(i => i.Name).Returns(USER_NAME);
            context.SetupGet(c => c.HttpContext.User.Identity).Returns(identity.Object);
        }

        public SelfAssessmentController_NormalizedInterfaceTest()
        {
            mockContext.Setup(db => db.SelfAssessments).Returns(saDbset);
            SeedDataForDbsetWithID(saDbset);
            var mapper = AutoMapConfig.CreateMapper();
            target = new SelfAssessmentController(mockContext.Object, mapper, null, null);
        }

        [Fact]
        public void Index()
        {
            /*Prepare*/

            /*Act*/
            ViewResult result = target.Index() as ViewResult;

            /*Assert*/
            //view result is not null
            Assert.NotNull(result);
            //the result should be using index data
            Assert.IsType<SelfAssessmentIndexData>(result.Model);
            //the result should have been transfered to viewmodel
            Assert.IsType<List<SelfAssessmentViewModel>>
                (((SelfAssessmentIndexData)result.Model).SelfAssessments.ToList());
        }

        [Fact]
        public void DetailsReturnNotFound()
        {
            /*Prepare*/

            /*Act*/
            var result = target.Details(0);

            /*Assert*/
            //The return should be http not found
            Assert.IsType(typeof(HttpNotFoundResult), result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void DetailsReturnView(int id)
        {
            /*Prepare*/

            /*Act*/
            var result = target.Details(id) as ViewResult;

            /*Assert*/
            //view result is not null
            Assert.NotNull(result);
            //the result should be using index data
            Assert.IsType<SelfAssessmentDetailsData>(result.Model);
            //the result should have been transfered to viewmodel
            Assert.IsType<SelfAssessmentViewModel>
                (((SelfAssessmentDetailsData)result.Model).SelfAssessment);
            Assert.Equal(id, ((SelfAssessmentDetailsData)result.Model).SelfAssessment.Id);

            Assert.IsType<List<CoreSkillViewModel>>
                (((SelfAssessmentDetailsData)result.Model).CoreSkills.ToList());
        }

        [Fact]
        public void GetCreate()
        {
            /*Prepare*/

            /*Act*/
            ViewResult result = target.Create() as ViewResult;

            /*Assert*/
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void PostCreateShouldRedirectToIndexIfSucceed(int id)
        {
            /*Prepare*/
            var oldCount = mockContext.Object.SelfAssessments.Count();
            var viewmodel = new SelfAssessmentViewModel
            {
                Id = id,
                Description = "something",
                Title = "title"
            };

            /*Act*/
            var result = target.Create(viewmodel) as RedirectToRouteResult;

            /*Assert*/
            Assert.Equal(mockContext.Object.SelfAssessments.Count(), oldCount + 1);
            Assert.Equal("Index", result.RouteValues["action"]);
            Assert.Null(result.RouteValues["controller"]); // means redirected to the same controller
        }


        [Fact]
        public void PostCreateShouldReturnViewWithSameModelIfFailed()
        {
            /*Prepare*/
            var viewmodel = new SelfAssessmentViewModel
            {
                Id = 0,
                Description = "something",
                Title = "title"
            };
            //Assume the model is invalid
            target.ViewData.ModelState.AddModelError("Key", "ErrorMessage");

            /*Act*/
            var result = target.Create(viewmodel) as ViewResult;

            /*Assert*/
            Assert.NotNull(result);
            Assert.Equal(viewmodel, result.Model);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void GetEditReturnViewIfSucceed(int id)
        {
            /*Prepare*/

            /*Act*/
            var result = target.Edit(id) as ViewResult;

            /*Assert*/
            //view result is not null
            Assert.NotNull(result);
            ////the result should be using view model
            Assert.IsType<SelfAssessmentViewModel>(result.Model);
            Assert.Equal(id, ((SelfAssessmentViewModel)result.Model).Id);
        }

        [Fact]
        public void GetEditReturnHttpNotFoundIfFailed()
        {
            /*Prepare*/
            int id = 0;

            /*Act*/
            var result = target.Edit(id);

            /*Assert*/
            Assert.IsType(typeof(HttpNotFoundResult), result);
        }


        [Fact]
        public void PostEditReturnViewIfFailed()
        {
            /*Prepare*/
            int id = 0;
            var viewmodel = new SelfAssessmentViewModel
            {
                Id = id,
                Description = "ChangedDes",
                Title = "ChangedTitle"
            };
            target.ViewData.ModelState.AddModelError("Key", "ErrorMessage");

            /*Act*/
            var result = target.Edit(viewmodel) as ViewResult;

            /*Assert*/
            //view result is not null
            Assert.NotNull(result);
            ////the result should be using view model
            Assert.IsType<SelfAssessmentViewModel>(result.Model);
            Assert.Equal(id, ((SelfAssessmentViewModel)result.Model).Id);
            Assert.Equal(viewmodel.Description, ((SelfAssessmentViewModel)result.Model).Description);
            Assert.Equal(viewmodel.Title, ((SelfAssessmentViewModel)result.Model).Title);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void PostEditReturnViewIfSucceed(int id)
        {
            /*Prepare*/
            var viewmodel = new SelfAssessmentViewModel
            {
                Id = id,
                Description = "ChangedDes",
                Title = "ChangedTitle"
            };

            /*Act*/
            var result = target.Edit(viewmodel) as RedirectToRouteResult;

            /*Assert*/
            Assert.NotNull(result);
            Assert.Equal(mockContext.Object.SelfAssessments.Count(), saDbset.Count());
            Assert.Equal(saDbset.Find(id).Description, viewmodel.Description);
            Assert.Equal(saDbset.Find(id).Title, viewmodel.Title);
            Assert.Equal("Details", result.RouteValues["action"]);
            Assert.Null(result.RouteValues["controller"]); // means redirected to the same controller
            Assert.Equal(id, result.RouteValues["id"]);
        }

        [Fact]
        public void Start()
        {
            /*Prepare*/
            GivenALoggedInUser();
            target.ControllerContext = context.Object;

            /*Act*/
            var result = target.Start() as RedirectToRouteResult;

            /*Assert*/
            Assert.NotNull(result);
            var newestID = saDbset
                .OrderByDescending(sa => sa.TimeStamp)
                .ToList()
                .First()
                .Id;
            Assert.Equal(newestID, result.RouteValues["selfAssessmentId"]);
            Assert.Equal("Index", result.RouteValues["action"]);
            Assert.Equal("CoreSkill", result.RouteValues["controller"]);
        }

        internal void SeedDataForDbsetWithID(FakeDbSet<SelfAssessment> saDbset)
        {
            Random rand = new Random();
            rand.Next();
            int coreSkillID = 1;
            for (int i = 1; i < 5; i++)
            {
                Mock<SelfAssessment> selfAssessment =
                    new Mock<SelfAssessment>(i, "Title" + i, "Description" + i,
                    DateTime.Now.AddSeconds(rand.Next(0, 100)), null as List<CoreSkill>);
                selfAssessment.Setup(sa => sa.CoreSkills).Returns(new List<CoreSkill>{
                        new Mock<CoreSkill>(coreSkillID ++ , "Name", "Descrip",
                            new List<CoreSkillElement>{ new Mock<CoreSkillElement>(1, "Des", 1).Object}
                    ).Object
                    });
                saDbset.Add(selfAssessment.Object);
            }
        }
    }

    public class SelfAssessmentController_AuthenticationTest : SelfAssessmentControllerTest
    {
        private Type type = typeof(SelfAssessmentController);

        [Fact]
        public void Index_ShouldRequireAnAdmin()
        {
            // Given we're investigating the index method...
            var method = type.GetMethod("Index", new Type[] {});

            // Expect the method to have the authentication attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>();
            Assert.NotEmpty(attributes);

            // Expect the method to require an AdminUser.
            Assert.Equal(attributes.First().Roles, AdminUser.Role);
        }

        [Fact]
        public void CreateGet_ShouldRequireAnAdmin()
        {
            // Given we're investigating the create GET method...
            var method = type.GetMethod("Create", new Type[] {});

            // Expect the method to have the authentication attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>();
            Assert.NotEmpty(attributes);

            // Expect the method to require an AdminUser.
            Assert.Equal(attributes.First().Roles, AdminUser.Role);
        }

        [Fact]
        public void CreatePost_ShouldRequireAnAdmin()
        {
            // Given we're investigating the create POST method...
            var method = type.GetMethod("Create",
                new Type[] {typeof(SelfAssessmentViewModel)});

            // Expect the method to have the authentication attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>();
            Assert.NotEmpty(attributes);

            // Expect the method to require an AdminUser.
            Assert.Equal(attributes.First().Roles, AdminUser.Role);
        }

        [Fact]
        public void CreatePost_ShouldValidateAntiForgeryToken()
        {
            // Given we're investigating the edit POST method...
            var method = type.GetMethod("Create",
                new Type[] {typeof(SelfAssessmentViewModel)});

            // Expect the method to have the anti forgery attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<ValidateAntiForgeryTokenAttribute>();
            Assert.NotEmpty(attributes);
        }

        [Fact]
        public void Details_ShouldRequireAnAdmin()
        {
            // Given we're investigating the details method...
            var method = type.GetMethod("Details",
                new Type[] {typeof(int)});

            // Expect the method to have the authentication attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>();
            Assert.NotEmpty(attributes);

            // Expect the method to require an AdminUser.
            Assert.Equal(attributes.First().Roles, AdminUser.Role);
        }


        [Fact]
        public void EditGet_ShouldRequireAnAdmin()
        {
            // Given we're investigating the edit GET method...
            var method = type.GetMethod("Edit",
                new Type[] {typeof(int)});

            // Expect the method to have the authentication attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>();
            Assert.NotEmpty(attributes);

            // Expect the method to require an AdminUser.
            Assert.Equal(attributes.First().Roles, AdminUser.Role);
        }

        [Fact]
        public void EditPost_ShouldRequireAnAdmin()
        {
            // Given we're investigating the edit POST method...
            var method = type.GetMethod("Edit",
                new Type[] {typeof(SelfAssessmentViewModel)});

            // Expect the method to have the authentication attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>();
            Assert.NotEmpty(attributes);

            // Expect the method to require an AdminUser.
            Assert.Equal(attributes.First().Roles, AdminUser.Role);
        }

        [Fact]
        public void EditPost_ShouldValidateAntiForgeryToken()
        {
            // Given we're investigating the edit POST method...
            var method = type.GetMethod("Edit",
                new Type[] {typeof(SelfAssessmentViewModel)});

            // Expect the method to have the anti forgery attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<ValidateAntiForgeryTokenAttribute>();
            Assert.NotEmpty(attributes);
        }

        [Fact]
        public void Start_ShouldRequireABasicUser()
        {
            // Given we're investigating the start method...
            var method = type.GetMethod("Start");

            // Expect the method to have the authentication attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>();
            Assert.NotEmpty(attributes);

            // Expect the method to require a BasicUser.
            Assert.Equal(BasicUser.Role, attributes.First().Roles);
        }

        [Fact]
        public void SurveyReport_ShouldRequireABasicUser()
        {
            // Given we're investigating the survey report method...
            var method = type.GetMethod("SurveyReport");

            // Expect the method to have the authentication attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>();
            Assert.NotEmpty(attributes);

            // Expect the method to require a BasicUser.
            Assert.Equal(BasicUser.Role, attributes.First().Roles);
        }

        [Fact]
        public void SummaryReport_ShouldRequireAnAdminUser()
        {
            // Given we're investigating the summary report method...
            var method = type.GetMethod("SummaryReport");

            // Expect the method to have the authentication attribute.
            var attributes = method.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>();
            Assert.NotEmpty(attributes);

            // Expect the method to require a BasicUser.
            Assert.Equal(AdminUser.Role, attributes.First().Roles);
        }
    }
}

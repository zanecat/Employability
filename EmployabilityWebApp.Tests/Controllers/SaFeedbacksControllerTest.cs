using EmployabilityWebApp.Controllers;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.Services;
using EmployabilityWebApp.ViewModels;
using Microsoft.AspNet.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Xunit;

namespace EmployabilityWebApp.Tests.Controllers
{
    public class SaFeedbacksControllerTest
    {
        protected SaFeedbacksController Target;

        private readonly Mock<EmployabilityDbContext> db =
            new Mock<EmployabilityDbContext>();

        protected Mock<IUserStore<ApplicationUser>> UserStore =
            new Mock<IUserStore<ApplicationUser>>();

        protected Mock<IReportPdfGenerateService> ReportPdfGenerateService =
            new Mock<IReportPdfGenerateService>();

        private const string UserName = "mingyouf";
        private readonly Mock<ControllerContext> _context = new Mock<ControllerContext>();
        private readonly Mock<HttpResponseBase> _response = new Mock<HttpResponseBase>();
        private readonly Mock<IIdentity> _identity = new Mock<IIdentity>();
        protected Mock<BasicUser> User = new Mock<BasicUser>();

        //protected Mock<SaFeedback> SelfAssessment = new Mock<SaFeedback>();

        public SaFeedbacksControllerTest()
        {
            var mapper = AutoMapConfig.CreateMapper();
            _context.Setup(c => c.HttpContext.Response).Returns(_response.Object);
            Target = new SaFeedbacksController(db.Object, mapper)
            {
                ControllerContext = _context.Object
            };
        }

        // Provides a logged in user.
        protected void GivenALoggedInUser()
        {
            _identity.Setup(i => i.Name).Returns(UserName);
            _context.SetupGet(c => c.HttpContext.User.Identity).Returns(_identity.Object);
            UserStore.Setup(us => us.FindByNameAsync(UserName)).ReturnsAsync(User.Object);
        }

        // Provide a self assessment with ID
        //protected void GivenASelfAssessmentWithId(int id)
        //{
        //    SelfAssessment = new Mock<SelfAssessment>(id);
        //}
    }

    /// <summary>
    /// Unit test for functions other than SurveyReport
    /// </summary>
    public class SaFeedbacksControllerTest_NormalizedInterfaceTest
    {
        SaFeedbacksController target;

        private const string USER_NAME = "kaiqiy";
        private Mock<ControllerContext> context = new Mock<ControllerContext>();
        private Mock<IIdentity> identity = new Mock<IIdentity>();

        //For simulate dbset
        private FakeSaFeedbackDbSet saFeedbackDbset = new FakeSaFeedbackDbSet();
        private FakeSelfAssessmentDbSet saDbset = new FakeSelfAssessmentDbSet();
        private FakeUserDbSet userDbset = new FakeUserDbSet();


        private Mock<EmployabilityDbContext> mockContext = new Mock<EmployabilityDbContext>();

        public void GivenALoggedInUser()
        {
            identity.Setup(i => i.Name).Returns(USER_NAME);
            context.SetupGet(c => c.HttpContext.User.Identity).Returns(identity.Object);
        }

        public SaFeedbacksControllerTest_NormalizedInterfaceTest()
        {
            mockContext.Setup(db => db.SaFeedbacks).Returns(saFeedbackDbset);
            mockContext.Setup(db => db.SelfAssessments).Returns(saDbset);
            mockContext.Setup(db => db.Users).Returns(userDbset);
            SeedDataForDbsetWithID(saFeedbackDbset);
            var mapper = AutoMapConfig.CreateMapper();
            target = new SaFeedbacksController(mockContext.Object, mapper);
        }

        [Fact]
        public void Index()
        {
            /*Prepare*/
            GivenALoggedInUser();

            /*Act*/
            ViewResult result = target.Index() as ViewResult;

            /*Assert*/
            //view result is not null
            Assert.NotNull(result);
            //the result should have been transfered to viewmodel
            Assert.IsType<List<FeedbackDetailData>>(result.Model);
        }

        [Fact]
        public void GetCreate()
        {
            /*Prepare*/
            int selfAssessmentiID = 1;
            GivenALoggedInUser();

            /*Act*/
            ViewResult result = target.Create(selfAssessmentiID) as ViewResult;

            /*Assert*/
            //view result is not null
            Assert.NotNull(result);
            //the result should have been transfered to viewmodel
            Assert.IsType<FeedbackCreateData>(result.Model);
            Assert.Equal(((FeedbackCreateData)result.Model).selfAssessmentId, selfAssessmentiID);
        }

        [Fact]
        public void PostCreate()
        {
            /*Prepare*/
            var oldCount = mockContext.Object.SaFeedbacks.Count();

            var feedbackcreatedata = new FeedbackCreateData()
            {
                selfAssessmentId = 1,
                Comment = "Comment",
                Rating = SaFeedback.MinChoice
            };
            GivenALoggedInUser();

            /*Act*/
            var result = target.Create(feedbackcreatedata) as RedirectToRouteResult;

            /*Assert*/
            Assert.Equal(oldCount + 1, mockContext.Object.SaFeedbacks.Count());
            Assert.Equal("ThankYou", result.RouteValues["action"]);
            Assert.Equal("Home", result.RouteValues["controller"]); // means redirected to the same controller
        }

        protected void SeedDataForDbsetWithID(FakeSaFeedbackDbSet saDbset)
        {
            Random rand = new Random();
            for (int i = 1; i < 5; i++)
            {
                Mock<SaFeedback> safeedback = new Mock<SaFeedback>
                    (i, rand.Next(SaFeedback.MinChoice, SaFeedback.MaxChoice), "Comments");
                saDbset.Add(safeedback.Object);
            }
        }
    }


}
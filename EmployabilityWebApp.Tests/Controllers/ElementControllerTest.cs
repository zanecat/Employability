using Xunit;
using Moq;
using EmployabilityWebApp.Controllers;
using System.Web.Mvc;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.Services;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using EmployabilityWebApp.ViewModels;
using EmployabilityWebApp.Tests.TestHelpers;
using System.Data.Entity;

namespace EmployabilityWebApp.Tests.Controllers
{
    public class ElementControllerTest
    {
        protected ElementController target;

        private readonly Mock<EmployabilityDbContext> db =
            new Mock<EmployabilityDbContext>();

        protected Mock<ICoreSkillElementService> coreSkillElementService = new Mock<ICoreSkillElementService>();
        protected Mock<ICoreSkillService> coreSkillService = new Mock<ICoreSkillService>();
        protected Mock<IUserStore<ApplicationUser>> userStore =
            new Mock<IUserStore<ApplicationUser>>();

        protected Mock<CoreSkill> coreSkill = new Mock<CoreSkill>();
        private readonly Mock<SelfAssessment> selfAssessment =
            new Mock<SelfAssessment>(RIGHT_ID);
        private readonly Mock<Answer> answer =
            new Mock<Answer>();
        private const int RIGHT_ID = 123;

        private readonly List<DetailedElement> detailedElements =
            new List<DetailedElement>();
        private readonly List<DetailedOption> options =
            new List<DetailedOption>();

        private const string USER_NAME = "kimple";
        private Mock<ControllerContext> context = new Mock<ControllerContext>();
        private Mock<IIdentity> identity = new Mock<IIdentity>();
        protected Mock<BasicUser> user = new Mock<BasicUser>();

        public ElementControllerTest()
        {
            EmployabilityDbContext db = null;
            var mapper = AutoMapConfig.CreateMapper();
            target = new ElementController(db, mapper, coreSkillElementService.Object);
            target.ControllerContext = context.Object;
        }

        private void SetupCoreSkill(int nElements, int nOptions)
        {
            // Given the core skill has nElements detailed elements.
            options.SetUpOptions(nElements, nOptions);
            detailedElements.SetUpDetailedElements(nElements, nOptions, options);

            Mock<DbSet<DetailedOption>> do_dbset = new Mock<DbSet<DetailedOption>>();
            foreach (DetailedOption d_o in options)
            {
                do_dbset.Setup(d => d.Find(d_o.Id)).Returns(d_o);
            }
            db.Setup(d => d.DetailedOptions).Returns(do_dbset.Object);

            Mock<DbSet<CoreSkillElement>> coreSkillElementSet = new Mock<DbSet<CoreSkillElement>>();
            foreach (DetailedElement d_e in detailedElements)
            {
                coreSkillElementSet.Setup(c => c.Find(d_e.Id)).Returns(d_e);
            }
            db.Setup(d => d.CoreSkillElements).Returns(coreSkillElementSet.Object);

            coreSkill.SetupGet(cs => cs.Elements).Returns(
                detailedElements.OfType<CoreSkillElement>().ToList());

            Mock<DbSet<CoreSkill>> coreSkillSet = new Mock<DbSet<CoreSkill>>();
            coreSkillSet.Setup(c => c.Find(coreSkill.Object.Id)).Returns(coreSkill.Object);
            db.Setup(d => d.CoreSkills).Returns(coreSkillSet.Object);
        }

        // Make sure the original self assessment has an associated answer
        private void GivenASelfAssessmentWithAnswers()
        {
            var ans = new List<Answer>();

            ans.Add(new Mock<Answer>().Object);
            selfAssessment.SetupGet(s => s.Answers).Returns(ans);

            coreSkill.SetupGet(cs => cs.SelfAssessment).Returns(selfAssessment.Object);

            var selfAssessmentDbSet = new Mock<DbSet<SelfAssessment>>();
            selfAssessmentDbSet.Setup(s => s.Find(selfAssessment.Object.Id)).Returns(selfAssessment.Object);
            db.Setup(d => d.SelfAssessments).Returns(selfAssessmentDbSet.Object);

            var cs_list = new List<CoreSkill>();
            cs_list.Add(coreSkill.Object);
            selfAssessment.Setup(s => s.CoreSkills).Returns(cs_list);
        }

        private void GivenASelfAssessmentWithoutAnswers()
        {
            var sa = new Mock<SelfAssessment>();
            sa.SetupGet(s => s.Answers).Returns(new List<Answer>());

            coreSkill.SetupGet(cs => cs.SelfAssessment).Returns(sa.Object);

            var selfAssessmentDbSet = new Mock<DbSet<SelfAssessment>>();
            selfAssessmentDbSet.Setup(s => s.Find(sa.Object.Id)).Returns(sa.Object);
            db.Setup(d => d.SelfAssessments).Returns(selfAssessmentDbSet.Object);
        }

        [Fact]
        public void CoreSkillSimplifiedElementController_ItShouldReturnTheDefaultView()
        {
            CoreSkillSimplifiedElementViewModel viewModel
                = new CoreSkillSimplifiedElementViewModel(456, "element description", 12, coreSkill.Object.Id);

            // The SelfAssessment does not have associated answers.
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithoutAnswers();

            // Expect the result to be the correct redirection.
            var redirect = target.CreateSimplifiedElement(viewModel) as RedirectToRouteResult;
            Assert.NotNull(redirect);
            Assert.Equal("Details", redirect.RouteValues["action"]);
            Assert.Equal("CoreSkill", redirect.RouteValues["controller"]);
        }

        [Fact]
        public void CoreSkillSimplifiedElementController_ItShouldReturnToTheNewSelfAssessmentView()
        {
            CoreSkillSimplifiedElementViewModel viewModel
                = new CoreSkillSimplifiedElementViewModel(456, "element description", 13, coreSkill.Object.Id);

            // The SelfAssessment has associated answers.
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithAnswers();

            coreSkillElementService.Setup(c => c.CreateSimplifiedElement(
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<int>())).Returns(true);

            // Expect the result to be the correct redirection.
            var redirect = target.CreateSimplifiedElement(viewModel) as RedirectToRouteResult;

            var newSelfAssessmentId = coreSkillElementService.Object.GetLatestSelfAssessmentVersion();

            Assert.NotNull(redirect);
            Assert.Equal("Details", redirect.RouteValues["action"]);
            Assert.Equal("SelfAssessment", redirect.RouteValues["controller"]);
            Assert.Equal(newSelfAssessmentId, redirect.RouteValues["id"]);
        }

        [Fact]
        public void CoreSkillTextElementViewModelController_ItShouldReturnTheDefaultView()
        {
            CoreSkillTextElementViewModel viewModel
                = new CoreSkillTextElementViewModel(456, "element description", 12, coreSkill.Object.Id);

            // The SelfAssessment does not have associated answers.
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithoutAnswers();

            // Expect the result to be the correct redirection.
            var redirect = target.CreateTextElement(viewModel) as RedirectToRouteResult;
            Assert.NotNull(redirect);
            Assert.Equal("Details", redirect.RouteValues["action"]);
            Assert.Equal("CoreSkill", redirect.RouteValues["controller"]);
        }

        [Fact]
        public void CoreSkillTextElementViewModelController_ItShouldReturnToTheNewSelfAssessmentView()
        {
            CoreSkillTextElementViewModel viewModel
                = new CoreSkillTextElementViewModel(456, "element description", 13, coreSkill.Object.Id);

            // The SelfAssessment has associated answers.
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithAnswers();

            coreSkillElementService.Setup(c => c.CreateTextElement(
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<int>())).Returns(true);

            // Expect the result to be the correct redirection.
            var redirect = target.CreateTextElement(viewModel) as RedirectToRouteResult;

            var newSelfAssessmentId = coreSkillElementService.Object.GetLatestSelfAssessmentVersion();

            Assert.NotNull(redirect);
            Assert.Equal("Details", redirect.RouteValues["action"]);
            Assert.Equal("SelfAssessment", redirect.RouteValues["controller"]);
            Assert.Equal(newSelfAssessmentId, redirect.RouteValues["id"]);
        }

        [Fact]
        public void CoreSkillDetailedElementViewModelController_ItShouldReturnTheDefaultView()
        {
            Mock<List<DetailedOptionViewModel>> option = new Mock<List<DetailedOptionViewModel>>();

            CoreSkillDetailedElementViewModel viewModel
                = new CoreSkillDetailedElementViewModel(456, "element description", option.Object, 12, coreSkill.Object.Id);

            // The SelfAssessment does not have associated answers.
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithoutAnswers();

            // Expect the result to be the correct redirection.
            var redirect = target.CreateDetailedElement(viewModel) as RedirectToRouteResult;
            Assert.NotNull(redirect);
            Assert.Equal("Details", redirect.RouteValues["action"]);
            Assert.Equal("CoreSkill", redirect.RouteValues["controller"]);
        }

        [Fact]
        public void CoreSkillDetailedElementViewModelController_ItShouldReturnToTheNewSelfAssessmentView()
        {
            Mock<List<DetailedOptionViewModel>> option = new Mock<List<DetailedOptionViewModel>>();

            CoreSkillDetailedElementViewModel viewModel
                = new CoreSkillDetailedElementViewModel(456, "element description", option.Object, 13, coreSkill.Object.Id);

            // The SelfAssessment has associated answers.
            SetupCoreSkill(6, 6);
            GivenASelfAssessmentWithAnswers();

            coreSkillElementService.Setup(c => c.CreateDetailedElement(
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<List<DetailedOptionViewModel>>(), 
                It.IsAny<int>())
                ).Returns(true);

            // Expect the result to be the correct redirection.
            var redirect = target.CreateDetailedElement(viewModel) as RedirectToRouteResult;

            var newSelfAssessmentId = coreSkillElementService.Object.GetLatestSelfAssessmentVersion();

            Assert.NotNull(redirect);
            Assert.Equal("Details", redirect.RouteValues["action"]);
            Assert.Equal("SelfAssessment", redirect.RouteValues["controller"]);
            Assert.Equal(newSelfAssessmentId, redirect.RouteValues["id"]);
        }
    }
}

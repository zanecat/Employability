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

namespace EmployabilityWebApp.Tests.Controllers
{
    public class CoreSkillControllerTest
    {
        protected CoreSkillController target;

        protected Mock<ICoreSkillService> coreSkillService = new Mock<ICoreSkillService>();
        protected Mock<IUserStore<ApplicationUser>> userStore = 
            new Mock<IUserStore<ApplicationUser>>();
        
        protected Mock<CoreSkill> coreSkill = new Mock<CoreSkill>();
        protected Mock<Answer> answer = new Mock<Answer>();

        // Shouldn't be used except for certain actions.
        protected Mock<SelfAssessment> selfAssessment = null;

        private const string USER_NAME = "nedp";
        private Mock<ControllerContext> context = new Mock<ControllerContext>();
        private Mock<IIdentity> identity = new Mock<IIdentity>();
        protected Mock<BasicUser> user = new Mock<BasicUser>();

        public CoreSkillControllerTest()
        {
            EmployabilityDbContext db = null; // Ensures that the DbContext isn't used by the controller.
            var mapper = AutoMapConfig.CreateMapper();
            target = new CoreSkillController(db, mapper, coreSkillService.Object, userStore.Object);
            target.ControllerContext = context.Object;
        }

        // Provides a logged in user.
        protected void GivenALoggedInUser()
        {
            identity.Setup(i => i.Name).Returns(USER_NAME);
            context.SetupGet(c => c.HttpContext.User.Identity).Returns(identity.Object);
            userStore.Setup(us => us.FindByNameAsync(USER_NAME)).ReturnsAsync(user.Object);
        }

        // Makes the core skill have the given id.
        protected void GivenACoreSkillWithId(int id)
        {
            coreSkill = new Mock<CoreSkill>(id, "Gudness", "How good are you?", 
                null as ICollection<CoreSkillElement>);
            coreSkillService.Setup(css => css.Find(id)).Returns(coreSkill.Object);
        }
    }

    public class CoreSkillController_IndexTest : CoreSkillControllerTest
    {
        private readonly List<CoreSkill> coreSkills = new List<CoreSkill>();
        // Makes the self assessment have the given id.
        protected void GivenASelfAssessmentWithId(int id, int nCoreSkills)
        {
            selfAssessment = new Mock<SelfAssessment>(id);
            coreSkillService.Setup(css => css.FindForSelfAssessment(id)).Returns(coreSkills);
            coreSkill.SetupGet(cs => cs.SelfAssessment).Returns(selfAssessment.Object);
            selfAssessment.SetupGet(sa => sa.CoreSkills).Returns(coreSkills);
            for (var i = 0; i < nCoreSkills; i += 1)
            {
                coreSkills.Add(new Mock<CoreSkill>().Object);
            }
        }

        [Fact]
        public void ItShould404IfTheSelfAssessmentDoesntExist()
        {
            // Expect the result to be a 404.
            var result = target.Index(1) as HttpNotFoundResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void ItShouldReturnDefaultView()
        {
            int id = 1;
            int ncoreskills = 4;
            GivenASelfAssessmentWithId(id,ncoreskills);
            GivenALoggedInUser();

            var view = target.Index(id) as ViewResult;
            Assert.NotNull(view);
        }

        [Fact]
        public void ItShouldProvideCoreSkills()
        {
            int id = 1;
            int ncoreskills = 4;
            GivenASelfAssessmentWithId(id, ncoreskills);
            GivenALoggedInUser();

            var view = target.Index(id) as ViewResult;
            var viewModel = view.Model as CoreSkillIndexData;
            Assert.Equal(viewModel.CoreSkills.Count, ncoreskills);
        }

    }

    public class CoreSkillController_DetailsTest : CoreSkillControllerTest
    {
        private readonly ICollection<CoreSkill> coreSkills = new List<CoreSkill>();

        protected void GivenACoreSkillWithSelfAssessment(int id)
        {
            GivenACoreSkillWithId(id);
            selfAssessment = new Mock<SelfAssessment>(id);
            coreSkill.SetupGet(cs => cs.SelfAssessment).Returns(selfAssessment.Object);
            selfAssessment.SetupGet(sa => sa.CoreSkills).Returns(coreSkills);
        }

        [Fact]
        public void ItShould404IfTheCoreSkillDoesntExist()
        {
            // Expect the result to be a 404.
            var result = target.Details(1) as HttpNotFoundResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void ItShouldReturnDefaultView()
        {
            int id = 1;
            GivenACoreSkillWithSelfAssessment(id);
            GivenALoggedInUser();
            
            var view = target.Details(id) as ViewResult;
            Assert.NotNull(view);
        }

        [Fact]
        public void ItShouldProvideCoreSkillInfo()
        {
            int id = 1;
            GivenACoreSkillWithSelfAssessment(id);
            GivenALoggedInUser();
            var view = target.Details(id) as ViewResult;
            var viewModel = view.Model as CoreSkillDetailData;
            Assert.NotNull(viewModel.CoreSkill);
        }

        [Fact]
        public void ItShouldProvideNavBarInfo()
        {
            int id = 1;
            GivenACoreSkillWithSelfAssessment(id);
            GivenALoggedInUser();
            var view = target.Details(id) as ViewResult;
            var viewModel = view.Model as CoreSkillDetailData;
            Assert.NotNull(viewModel.CoreSkillNavBarData);
        }
    }

    // Covers the GET variant of the FillOut method, as well
    // as the relevant AutoMap configuration.
    public class CoreSkillController_GetFillOutTest : CoreSkillControllerTest
    {
        private readonly List<DetailedOption> options = new List<DetailedOption>();
        private readonly List<DetailedElement> detailedElements = new List<DetailedElement>();
        private readonly List<DetailedAnswer> detailedAnswers = new List<DetailedAnswer>();

        private readonly List<TextElement> textElements = new List<TextElement>();
        private readonly List<TextAnswer> textAnswers = new List<TextAnswer>();

        private readonly List<SimplifiedElement> simplifiedElements = new List<SimplifiedElement>();
        private readonly List<SimplifiedAnswer> simplifiedAnswers = new List<SimplifiedAnswer>();

        private readonly List<ISurveyElement> surveyElements = new List<ISurveyElement>();
        private readonly ICollection<CoreSkill> coreSkills = new List<CoreSkill>();

        public CoreSkillController_GetFillOutTest() : base()
        {
            coreSkill.SetupProperty(cs => cs.Elements);
        }   

        [Fact]
        public void ItShould404IfTheCoreSkillDoesntExist()
        {
            // Given no core skill is created,
            // Expect the result to be a 404.
            var result = target.FillOut(123) as HttpNotFoundResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void ItShouldReturnTheDefaultView()
        {
            var id = 1;

            // Given a basic ready state,
            GivenACoreSkillWithId(id);
            GivenALoggedInUser();

            // Given a basic empty survey,
            surveyElements.RemoveAll(_ => true);
            coreSkillService.Setup(css =>
                css.SurveyElements(coreSkill.Object, user.Object))
                .Returns(surveyElements);

            // Given a self assessment with core skills.
            GivenASelfAssessmentWithCoreSkills(1);

            // Expect the result to be the correct view.
            var view = target.FillOut(id) as ViewResult;
            Assert.NotNull(view);
            Assert.Equal("Survey/FillOut", view.ViewName);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(0, 5)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        public void ItShouldProvideDetailedElements(int nElements, int nOptions)
        {
            var id = nElements + nOptions;

            // Given a basic ready state,
            GivenACoreSkillWithId(id);
            GivenALoggedInUser();

            // Given a basic survey,
            coreSkillService.Setup(css => css.SurveyElements(coreSkill.Object, user.Object))
                .Returns(surveyElements);

            // Given some detailed elements,
            options.SetUpOptions(nElements, nOptions);
            detailedElements.SetUpDetailedElements(nElements, nOptions, options);
            detailedAnswers.SetUpDetailedAnswers(detailedElements, options);
            surveyElements.SetUpDetailedSurveyElements(detailedElements, detailedAnswers);

            // Given a self assessment with core skills.
            GivenASelfAssessmentWithCoreSkills(1);

            // Expect to send the right data to the view.
            var view = target.FillOut(id) as ViewResult;
            var viewModel = view.Model as SurveyViewModel;

            // Expect the right number of elements.
            Assert.Equal(nElements, viewModel.SurveyElements.Count);

            // Expect each survey element to be represented in the view model.
            foreach (var surveyElement in surveyElements.OfType<DetailedSurveyElement>())
            {
                // Find a matching element in the view model.
                var viewModelElement = viewModel.SurveyElements
                    .OfType<DetailedSurveyElementViewModel>()
                    .First(e => surveyElement.Element.Id == e.Element.Id);

                surveyElement.AssertEquivalent(viewModelElement);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void ItShouldProvideTextElements(int nElements)
        {
            var id = nElements;

            // Given a basic ready state,
            GivenACoreSkillWithId(id);
            GivenALoggedInUser();

            // Given a basic survey,
            coreSkillService.Setup(css => css.SurveyElements(coreSkill.Object, user.Object))
                .Returns(surveyElements);

            // Given some detailed elements,
            textElements.SetUpTextElements(nElements);
            textAnswers.SetUpTextAnswers(textElements);
            surveyElements.SetUpTextSurveyElements(textElements, textAnswers);

            // Given a self assessment with core skills.
            GivenASelfAssessmentWithCoreSkills(1);

            // Expect to send the right data to the view.
            var view = target.FillOut(id) as ViewResult;
            var viewModel = view.Model as SurveyViewModel;

            // Expect the right number of elements.
            Assert.Equal(nElements, viewModel.SurveyElements.Count);

            // Expect each survey element to be represented in the view model.
            foreach (var surveyElement in surveyElements.OfType<TextSurveyElement>())
            {
                // Find a matching element in the view model.
                var viewModelElement = viewModel.SurveyElements
                    .OfType<TextSurveyElementViewModel>()
                    .First(e => surveyElement.Element.Id == e.Element.Id);

                surveyElement.AssertEquivalent(viewModelElement);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void ItShouldProvideSimplifiedElements(int nElements)
        {
            var id = nElements;

            // Given a basic ready state,
            GivenACoreSkillWithId(id);
            GivenALoggedInUser();

            // Given a basic survey,
            coreSkillService.Setup(css => css.SurveyElements(coreSkill.Object, user.Object))
                .Returns(surveyElements);

            // Given some detailed elements,
            simplifiedElements.SetUpSimplifiedElements(nElements);
            simplifiedAnswers.SetUpSimplifiedAnswers(simplifiedElements);
            surveyElements.SetUpSimplifiedSurveyElements(simplifiedElements, simplifiedAnswers);

            // Given a self assessment with core skills.
            GivenASelfAssessmentWithCoreSkills(1);

            // Expect to send the right data to the view.
            var view = target.FillOut(id) as ViewResult;
            var viewModel = view.Model as SurveyViewModel;

            // Expect the right number of elements.
            Assert.Equal(nElements, viewModel.SurveyElements.Count);

            // Expect each survey element to be represented in the view model.
            foreach (var surveyElement in surveyElements.OfType<SimplifiedSurveyElement>())
            {
                // Find a matching element in the view model.
                var viewModelElement = viewModel.SurveyElements
                    .OfType<SimplifiedSurveyElementViewModel>()
                    .First(e => surveyElement.Element.Id == e.Element.Id);

                surveyElement.AssertEquivalent(viewModelElement);
            }
        }

        [Fact]
        public void ItShouldProvideACombinationOfElements()
        {
            var id = 5;

            // Given a basic ready state,
            GivenACoreSkillWithId(id);
            GivenALoggedInUser();

            // Given a basic survey,
            coreSkillService.Setup(css => css.SurveyElements(coreSkill.Object, user.Object))
                .Returns(surveyElements);

            // Given some elements of all kinds.
            options.SetUpOptions(1, 1);
            detailedElements.SetUpDetailedElements(1, 1, options);
            textElements.SetUpTextElements(1);

            surveyElements.AddRange(
                detailedElements.Select(de => new DetailedSurveyElement(de, null)));
            surveyElements.AddRange(
                textElements.Select(te => new TextSurveyElement(te, null)));

            // Given a self assessment with core skills.
            GivenASelfAssessmentWithCoreSkills(1);

            // Expect to send all three elements to the view; one per type.
            var view = target.FillOut(id) as ViewResult;
            var viewModel = view.Model as SurveyViewModel;
            Assert.Equal(2, viewModel.SurveyElements.Count);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 3)]
        [InlineData(5, 4)]
        public void ItShouldIncludeTheRightCoreSkillsInTheNavbar(int nCoreSkills, int id)
        {
            // Given a basic ready state,
            GivenACoreSkillWithId(id);
            GivenALoggedInUser();

            // Given a basic empty survey,
            surveyElements.RemoveAll(_ => true);
            coreSkillService.Setup(css =>
                css.SurveyElements(coreSkill.Object, user.Object))
                .Returns(surveyElements);

            // Given a self assessment with core skills.
            GivenASelfAssessmentWithCoreSkills(nCoreSkills);

            // Expect the result to be the correct view.
            var view = target.FillOut(id) as ViewResult;
            var model = view.Model as SurveyViewModel;
            Assert.Equal(id, model.CoreSkillNavBarData.CurrentCoreSkillId);
            Assert.Equal(nCoreSkills, model.CoreSkillNavBarData.CoreSkills.Count);
        }

        private void GivenASelfAssessmentWithCoreSkills(int nCoreSkills)
        {
            selfAssessment = new Mock<SelfAssessment>();
            coreSkill.SetupGet(cs => cs.SelfAssessment).Returns(selfAssessment.Object);
            selfAssessment.SetupGet(sa => sa.CoreSkills).Returns(coreSkills);
            for (var i = 0; i < nCoreSkills; i += 1)
            {
                coreSkills.Add(new Mock<CoreSkill>().Object);
            }
        }
    }

    // Covers the POST variant of the FillOut method.
    public class CoreSkillController_PostFillOutTest : CoreSkillControllerTest
    {
        private PostSurveyViewModel viewModel;

        [Fact]
        public void ItShould404IfTheCoreSkillIsMissing()
        {
            var id = 123;

            // Given an empty survey view model,
            GivenAnEmptySurveyViewModelWithCoreSkillId(id);

            // Given no core skill has been created on the server,
            // Expect the result to be the correct redirection.
            var result = target.FillOut(viewModel) as HttpNotFoundResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void ItShouldRedirectToTheSamePage()
        {
            var id = 123;

            // Given an empty survey view model,
            GivenAnEmptySurveyViewModelWithCoreSkillId(id);

            // Given a basic ready state,
            GivenALoggedInUser();
            GivenACoreSkillWithId(id);

            // Expect the result to be the correct redirection with the success flag.
            var redirect = target.FillOut(viewModel) as RedirectToRouteResult;
            Assert.NotNull(redirect);
            Assert.Equal("FillOut", redirect.RouteValues["action"]);
            Assert.Equal(id, redirect.RouteValues["id"]);
            Assert.Equal(true, target.TempData["Success"]);
        }

        [Fact]
        public void ItShouldUseTheMostRecentAnswerOrCreateOneIfNeeded()
        {
            var id = 123;

            // Given an empty survey view model,
            GivenAnEmptySurveyViewModelWithCoreSkillId(id);

            // Given a basic ready state,
            GivenALoggedInUser();
            GivenACoreSkillWithId(id);
            GivenTheCoreSkillHasASelfAssessmentWithId(916);
            GivenAnAnswerMayBeRetrievedOrCreated();

            // When the form is submitted,
            target.FillOut(viewModel);

            // Then expect any required answers to be created.
            coreSkillService.Verify(css =>
                css.MostRecentOrNewAnswer(coreSkill.Object, user.Object), 
                Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void ItShouldSaveTheRightDetailedAnswers(int nElements)
        {
            var id = 123;

            // Given a survey model with the right number of elements,
            GivenAFullSurveyViewModel(id, nElements);

            // Given a basic ready state,
            GivenALoggedInUser();
            GivenACoreSkillWithId(id);
            GivenTheCoreSkillHasASelfAssessmentWithId(918);
            GivenAnAnswerMayBeRetrievedOrCreated();

            // When the form is submitted,
            target.FillOut(viewModel);

            // Then expect all answers to be saved.
            foreach (var surveyElement in viewModel.DetailedSurveyElements)
            {
                // Multiple saves is ok because it's idempotent.
                coreSkillService.Verify(css => css.SaveDetailedAnswer(
                    answer.Object, surveyElement.Element.Id, surveyElement.SubAnswer.ChoiceId),
                    Times.AtLeastOnce);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void ItShouldOnlySaveFullDetailedAnswers(int nElements)
        {
            var id = 123;

            // Given a incomplete survey model with the right number of elements,
            GivenAIncompleteSurveyViewModel(id, nElements);

            // Given a basic ready state,
            GivenALoggedInUser();
            GivenACoreSkillWithId(id);
            GivenTheCoreSkillHasASelfAssessmentWithId(918);
            GivenAnAnswerMayBeRetrievedOrCreated();

            // When the form is submitted,
            target.FillOut(viewModel);

            // Then expect only complete answers to be saved.
            foreach (var surveyElement in viewModel.DetailedSurveyElements)
            {
                // Multiple saves is ok because it's idempotent.
                if (surveyElement.SubAnswer?.ChoiceId != null)
                {
                    // Only save answer when ChoiceId is not null.
                    coreSkillService.Verify(css => css.SaveDetailedAnswer(
                        answer.Object, surveyElement.Element.Id, surveyElement.SubAnswer.ChoiceId),
                        Times.AtLeastOnce);
                }
                else
                {
                    // Otherwise, don't save the answer
                    coreSkillService.Verify(css => css.SaveDetailedAnswer(
                        answer.Object, surveyElement.Element.Id, surveyElement.SubAnswer.ChoiceId),
                        Times.Never);
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void ItShouldSaveTheRightTextAnswers(int nElements)
        {
            var id = 123;

            // Given a survey model with the right number of elements,
            GivenATextFilledSurveyViewModel(id, nElements);

            // Given a basic ready state,
            GivenALoggedInUser();
            GivenACoreSkillWithId(id);
            GivenTheCoreSkillHasASelfAssessmentWithId(918);
            GivenAnAnswerMayBeRetrievedOrCreated();

            // When the form is submitted,
            target.FillOut(viewModel);

            // Then expect all answers to be saved.
            foreach (var surveyElement in viewModel.TextSurveyElements)
            {
                // Multiple saves is ok because it's idempotent.
                coreSkillService.Verify(css => css.SaveTextAnswer(
                    answer.Object, surveyElement.Element.Id, surveyElement.SubAnswer.Text),
                    Times.AtLeastOnce);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void ItShouldOnlySaveFullTextAnswers(int nElements)
        {
            var id = 123;

            // Given a incomplete survey model with the right number of elements,
            GivenAIncompleteTextSurveyViewModel(id, nElements);

            // Given a basic ready state,
            GivenALoggedInUser();
            GivenACoreSkillWithId(id);
            GivenTheCoreSkillHasASelfAssessmentWithId(918);
            GivenAnAnswerMayBeRetrievedOrCreated();

            // When the form is submitted,
            target.FillOut(viewModel);

            // Then expect all answers to be saved.
            foreach (var surveyElement in viewModel.TextSurveyElements)
            {
                // Multiple saves is ok because it's idempotent.
                if (!string.IsNullOrEmpty(surveyElement.SubAnswer.Text))
                {
                    // Only save answer when Text is not null.
                    coreSkillService.Verify(css => css.SaveTextAnswer(
                        answer.Object, surveyElement.Element.Id, surveyElement.SubAnswer.Text),
                        Times.AtLeastOnce);
                }
                else
                {
                    // Otherwise, don't save the answer.
                    coreSkillService.Verify(css => css.SaveTextAnswer(
                        answer.Object, surveyElement.Element.Id, surveyElement.SubAnswer.Text),
                        Times.Never);
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void ItShouldSaveTheRightSimplifiedAnswers(int nElements)
        {
            var id = 123;

            // Given a survey model with the right number of elements,
            GivenASimplifiedFilledSurveyViewModel(id, nElements);

            // Given a basic ready state,
            GivenALoggedInUser();
            GivenACoreSkillWithId(id);
            GivenTheCoreSkillHasASelfAssessmentWithId(918);
            GivenAnAnswerMayBeRetrievedOrCreated();

            // When the form is submitted,
            target.FillOut(viewModel);

            // Then expect all answers to be saved.
            foreach (var surveyElement in viewModel.SimplifiedSurveyElements)
            {
                // Multiple saves is ok because it's idempotent.
                coreSkillService.Verify(css => css.SaveSimplifiedAnswer(
                    answer.Object, surveyElement.Element.Id, surveyElement.SubAnswer.Choice),
                    Times.AtLeastOnce);
            }
        }

        [Fact]
        public void ItShouldRedirectToGetFillOutIfSavingFails()
        {
            var id = 123;

            // Given a survey view model with 1 element,
            GivenAFullSurveyViewModel(id, 1);

            // Given a basic ready state,
            GivenALoggedInUser();
            GivenACoreSkillWithId(id);
            GivenAnAnswerMayBeRetrievedOrCreated();

            // Given saving will fail,
            coreSkillService.Setup(css =>
                css.SaveDetailedAnswer(It.IsAny<Answer>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new SurveyConcurrencyException("some error message", null));

            // When the form is submitted,
            var redirect = target.FillOut(viewModel) as RedirectToRouteResult;

            // Then the result should redirect back to the survey page,
            Assert.NotNull(redirect);
            Assert.Equal("FillOut", redirect.RouteValues["action"]);
            Assert.Equal(id, redirect.RouteValues["id"]);
        }

        // Must be called after `GivenACoreSkillWithId`, since that resets the
        // `coreSkill` variable.
        private void GivenTheCoreSkillHasASelfAssessmentWithId(int id)
        {
            selfAssessment = new Mock<SelfAssessment>(id);
            coreSkill.SetupGet(cs => cs.SelfAssessment).Returns(selfAssessment.Object);
        }

        // Provides a view model with core skill id, but no elements/answers.
        private void GivenAnEmptySurveyViewModelWithCoreSkillId(int id)
        {
            viewModel = new PostSurveyViewModel()
            {
                CoreSkill = new CoreSkillViewModel()
                {
                    Id = id
                },
                DetailedSurveyElements = new List<DetailedSurveyElementViewModel>()
            };
        }

        // Provides a view model with core skill id and detailed elements/answers.
        // The chosen options will be the inverted diagonal; 
        // pick the (i)th answer for the (nAnswers - i)th element.
        private void GivenAFullSurveyViewModel(int coreSkillId, int nAnswers)
        {
            // Start with an empty one for DRYness.
            GivenAnEmptySurveyViewModelWithCoreSkillId(coreSkillId);

            // Add nAnswers DetailedElement/DetailedAnswer pairs.
            for (var i = 0; i < nAnswers; i += 1)
            {
                viewModel.DetailedSurveyElements.Add(
                    new DetailedSurveyElementViewModel()
                    {
                        Element = new DetailedElementViewModel() { Id = i + 1 },
                        SubAnswer = new DetailedAnswerViewModel() { ChoiceId = nAnswers - i + 1 },
                    });
            }
        }

        // Provides a view model with core skill id and detailed elements/answers.
        // Only the even-numberth (2nd, 4th,...) elements will have answer.
        // The chosen options will be the inverted diagonal; 
        // pick the (i)th answer for the (nAnswers - i)th element.
        private void GivenAIncompleteSurveyViewModel(int coreSkillId, int nAnswers)
        {
            // Start with an empty one for DRYness.
            GivenAnEmptySurveyViewModelWithCoreSkillId(coreSkillId);

            // Add nAnswers DetailedElement/DetailedAnswer pairs.
            for (var i = 0; i < nAnswers; i++)
            {
                var detailedAnswer = new DetailedAnswerViewModel();
                if (i % 2 == 0)
                {
                    detailedAnswer.ChoiceId = nAnswers - i + 1;
                }
                else
                {
                    detailedAnswer.ChoiceId = null;
                }
                viewModel.DetailedSurveyElements.Add(
                    new DetailedSurveyElementViewModel()
                    {
                        Element = new DetailedElementViewModel() { Id = i + 1 },
                        SubAnswer = detailedAnswer,
                    });
            }
        }

        // Provides a view model with core skill id and text elements/answers.
        private void GivenATextFilledSurveyViewModel(int coreSkillId, int nAnswers)
        {
            // Start with an empty one for DRYness.
            GivenAnEmptySurveyViewModelWithCoreSkillId(coreSkillId);

            // Add nAnswers TextElement/TextAnswer pairs.
            for (var i = 0; i < nAnswers; i += 1)
            {
                viewModel.TextSurveyElements.Add(
                    new TextSurveyElementViewModel()
                    {
                        Element = new CoreSkillElementViewModel() { Id = i + 1 },
                        SubAnswer = new TextAnswerViewModel() { Text = "answer " + (i + 1) },
                    });
            }
        }

        // Provides a view model with core skill id and text elements/answers.
        // Only the even-numberth (2nd, 4th,...) elements will have answer.
        private void GivenAIncompleteTextSurveyViewModel(int coreSkillId, int nAnswers)
        {
            // Start with an empty one for DRYness.
            GivenAnEmptySurveyViewModelWithCoreSkillId(coreSkillId);

            // Add nAnswers TextElement/TextAnswer pairs.
            for (var i = 0; i < nAnswers; i++)
            {
                var textAnswer = new TextAnswerViewModel();
                if (i%2 == 0)
                {
                    textAnswer.Text = "answer " + (i + 1);
                }
                else
                {
                    textAnswer.Text = null;
                }
                viewModel.TextSurveyElements.Add(
                    new TextSurveyElementViewModel()
                    {
                        Element = new CoreSkillElementViewModel() { Id = i + 1 },
                        SubAnswer = textAnswer,
                    });
            }
        }

        // Provides a view model with core skill id and simplified elements/answers.
        private void GivenASimplifiedFilledSurveyViewModel(int coreSkillId, int nAnswers)
        {
            // Start with an empty one for DRYness.
            GivenAnEmptySurveyViewModelWithCoreSkillId(coreSkillId);

            // Add nAnswers SimplifiedElement/SimplifiedAnswer pairs.
            for (var i = 0; i < nAnswers; i += 1)
            {
                viewModel.SimplifiedSurveyElements.Add(
                    new SimplifiedSurveyElementViewModel()
                    {
                        Element = new CoreSkillElementViewModel() { Id = i + 1 },
                        SubAnswer = new SimplifiedAnswerViewModel() { Choice = (i%5 + 1) },
                    });
            }
        }

        // Allows an answer to be created.
        private void GivenAnAnswerMayBeRetrievedOrCreated()
        {
            coreSkillService.Setup(css =>
                css.MostRecentOrNewAnswer(coreSkill.Object, user.Object))
                .Returns(answer.Object);
        }
    }

    static class Extensions
    {
        internal static void AssertEquivalent(this DetailedSurveyElement want, DetailedSurveyElementViewModel got)
        {
            // Expect the right description.
            Assert.Equal(want.Element.Description, got.Element.Description);

            // Expect the right number of choices per element.
            Assert.Equal(want.Element.DetailedOptions.Count, got.Element.DetailedOptions.Count);

            // Expect the right choice values per element.
            foreach (var option in want.Element.DetailedOptions)
            {
                Assert.True(got.Element.DetailedOptions.Any(o => option.Id == o.Id));
            }

            // Expect the right answers per element.
            if (want.SubAnswer != null)
            {
                Assert.Equal(want.SubAnswer.Choice.Id, got.SubAnswer.ChoiceId);
            }
        }

        internal static void AssertEquivalent(this TextSurveyElement want, TextSurveyElementViewModel got)
        {
            // Expect the right description.
            Assert.Equal(want.Element.Description, got.Element.Description);

            if (want.SubAnswer != null)
            {
                Assert.Equal(want.SubAnswer.Text, got.SubAnswer.Text);
            }
        }

        internal static void AssertEquivalent(
            this SimplifiedSurveyElement want, SimplifiedSurveyElementViewModel got)
        {
            // Expect the right description.
            Assert.Equal(want.Element.Description, got.Element.Description);

            if (want.SubAnswer != null)
            {
                Assert.Equal(want.SubAnswer.Choice, got.SubAnswer.Choice);
            }
        }
    }
}

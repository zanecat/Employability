using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.Services;
using EmployabilityWebApp.ViewModels;
using Microsoft.AspNet.Identity;

namespace EmployabilityWebApp.Controllers
{
    /// <summary>
    /// A controller to handle actions related to CoreSkills
    /// (sections of the survey).
    /// </summary>
    [RoutePrefix("CoreSkill")]
    public class CoreSkillController : Controller
    {
        // TODO: Phase out the use of db context in controllers.
        private readonly EmployabilityDbContext db;
        private readonly IMapper mapper;
        private readonly IUserStore<ApplicationUser> userStore;
        private readonly ICoreSkillService coreSkillService;

        public CoreSkillController(EmployabilityDbContext db, IMapper mapper,
            ICoreSkillService coreSkillService, IUserStore<ApplicationUser> userStore)
        {
            this.db = db;
            this.mapper = mapper;
            this.coreSkillService = coreSkillService;
            this.userStore = userStore;
        }

        /// GET: CoreSkills
        [Authorize]
        [Route("Index/{selfAssessmentId}")]
        public ActionResult Index(int selfAssessmentId)
        {
            var coreSkills = coreSkillService.FindForSelfAssessment(selfAssessmentId);
            if (coreSkills == null)
            {
                return HttpNotFound();
            }

            var viewModel = new CoreSkillIndexData
            {
                SelfAssessmentId = selfAssessmentId,
                CoreSkills = mapper.Map<List<CoreSkillViewModel>>(coreSkills)
            };
            return View(viewModel);
        }

        /// GET: CoreSkills/Details/5
        [Authorize]
        public ActionResult Details(int id)
        {
            var coreSkill = coreSkillService.Find(id);
            if (coreSkill == null)
            {
                return HttpNotFound();
            }

            var detailedElements = coreSkillService.SpecificElements<DetailedElement>(coreSkill);
            var simplifiedElements = coreSkillService.SpecificElements<SimplifiedElement>(coreSkill);
            var textElements = coreSkillService.SpecificElements<TextElement>(coreSkill);
            var selfAssessments = coreSkill.SelfAssessment;
            var viewModel = new CoreSkillDetailData
            {
                CoreSkill = mapper.Map<CoreSkillViewModel>(coreSkill),
                DetailedElements = mapper.Map<IList<DetailedElementViewModel>>(detailedElements),
                SimplifiedElements = mapper.Map<IList<SimplifiedElementViewModel>>(simplifiedElements),
                TextElements = mapper.Map<IList<TextElementViewModel>>(textElements),
                CoreSkillNavBarData = new CoreSkillNavBarData
                {
                    CoreSkills = mapper.Map<List<CoreSkillViewModel>>(selfAssessments.CoreSkills.ToList()),
                    CurrentCoreSkillId = coreSkill.Id,
                    SelfAssessmentId = id,
                }
            };

            ViewData["CoreSkillId"] = id;

            return View(viewModel);
        }
        
        /// <summary>
        /// Serves a form for completing the core skill in a survey.
        /// </summary>
        /// <remarks>
        /// Any preexisting answers for that core skill made by the user will
        /// be prefilled in the form.
        /// Example usage:
        ///     GET CoreSkills/FillOut/5
        /// </remarks>
        [Authorize(Roles = BasicUser.Role)]
        [HttpGet]
        public ActionResult FillOut(int id)
        {
            // Find the core skill, or 404 early.
            var coreSkill = coreSkillService.Find(id);
            if (coreSkill == null)
            {
                return HttpNotFound();
            }
         
            // Retrieve and show the survey with any prefilled answers the user
            // has made before.
            var surveyElements = coreSkillService.SurveyElements(coreSkill, CurrentBasicUser());
            var selfAssessment = coreSkill.SelfAssessment;
            foreach (CoreSkill coreSkillTemp in selfAssessment.CoreSkills)
            {
                coreSkillTemp.IsFinished = coreSkillService.IsFinished(coreSkillTemp, CurrentBasicUser());
            }

            var viewModel = new SurveyViewModel
            {
                CoreSkill = mapper.Map<CoreSkillViewModel>(coreSkill),
                SurveyElements = mapper.Map<List<ISurveyElementViewModel>>(surveyElements),
                CoreSkillNavBarData = new CoreSkillNavBarData
                {
                    CoreSkills = mapper.Map<List<CoreSkillViewModel>>(selfAssessment.CoreSkills.ToList()),
                    CurrentCoreSkillId = coreSkill.Id,
                    SelfAssessmentId = coreSkill.SelfAssessment.Id
                }
            };
            return View("Survey/FillOut", viewModel);
        }

        /// <summary>
        /// Saves the results of a survey completion.
        /// </summary>
        /// <remarks>
        /// The  will be saved for the logged in user, in their most recent answer
        /// for the self assessment of the specified core skill.
        /// If there is no preexisting answer for those parameters, one is created.
        ///
        /// If all goes well, the user is redirected to the core skill index.
        ///
        /// Example usage:
        ///     POST CoreSkills/FillOut { CoreSkill: { Id: 5 }, results... }
        /// </remarks>
        /// <param name="model">The completed form from GET CoreSkills/FillOut/{id}</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FillOut(PostSurveyViewModel model)
        {
            // Find the core skill, or 404 early.
            var coreSkill = coreSkillService.Find(model.CoreSkill.Id);
            if (coreSkill == null)
            {
                return HttpNotFound();
            }

            // Find the most recent Answer supplied by the user for this
            // CoreSkill's SelfAssessment, or create one if none exists yet.
            var answer = coreSkillService.MostRecentOrNewAnswer(coreSkill, CurrentBasicUser());

            // Attempt to save all types of answer.
            try
            {
                SaveDetailedAnswers(answer, model.DetailedSurveyElements);
                SaveTextAnswers(answer, model.TextSurveyElements);
                SaveSimplifiedAnswers(answer, model.SimplifiedSurveyElements);
            }
            // If the survey data was invalidated while the user filled it in,
            // then return them to the survey page.
            catch (SurveyConcurrencyException)
            {
                return RedirectToAction("FillOut", new {id = coreSkill.Id});
            }
            // Otherwise stay on the survey page with a success message displayed.
            TempData["Success"] = true;
            return RedirectToAction("FillOut", new { id = coreSkill.Id });
        }

        // Attempts to save all the detailed answers in the survey view models.
        private void SaveDetailedAnswers(Answer answer, 
            IEnumerable<DetailedSurveyElementViewModel> viewModels)
        {
            foreach (var viewModel in viewModels)
            {
                if (viewModel.SubAnswer?.ChoiceId != null)
                {
                    coreSkillService.SaveDetailedAnswer(answer,
                        viewModel.Element.Id, viewModel.SubAnswer.ChoiceId);
                }
            }
        }

        // Attempts to save all the text answers in the survey view models.
        private void SaveTextAnswers(Answer answer, 
            IEnumerable<TextSurveyElementViewModel> viewModels)
        {
            foreach (var viewModel in viewModels)
            {
                if (!string.IsNullOrWhiteSpace(viewModel.SubAnswer.Text))
                {
                    coreSkillService.SaveTextAnswer(answer,
                        viewModel.Element.Id, viewModel.SubAnswer.Text);
                }
            }
        }

        // Attempts to save all the simplified answers in the survey view models.
        private void SaveSimplifiedAnswers(Answer answer, 
            IEnumerable<SimplifiedSurveyElementViewModel> viewModels)
        {
            foreach (var viewModel in viewModels)
            {
                coreSkillService.SaveSimplifiedAnswer(answer,
                    viewModel.Element.Id, viewModel.SubAnswer.Choice);
            }
        }

        // Returns the currently logged in user.
        private BasicUser CurrentBasicUser()
        {
            // Use User.Identity.Name, not User.Identity.GetUserName(),
            // because the latter is nigh impossible to mock for testing.
            var userName = User.Identity.Name;
            var user = userStore.FindByNameAsync(userName);
            return user.Result as BasicUser;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

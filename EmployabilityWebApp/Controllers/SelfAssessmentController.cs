using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.ViewModels;
using EmployabilityWebApp.Services;
using Microsoft.AspNet.Identity;

namespace EmployabilityWebApp.Controllers
{
    public class SelfAssessmentController : Controller
    {
        private readonly EmployabilityDbContext db;
        private readonly IMapper mapper;
        private readonly IUserStore<ApplicationUser> userStore;
        private readonly IReportPdfGenerateService reportPdfGenerateService;

        public SelfAssessmentController(EmployabilityDbContext db, IMapper mapper,
            IReportPdfGenerateService reportPdfGenerateService, IUserStore<ApplicationUser> userStore)
        {
            this.db = db;
            this.mapper = mapper;
            this.reportPdfGenerateService = reportPdfGenerateService;
            this.userStore = userStore;
        }

        /// <summary>
        /// GET /SelfAssessment/
        /// Displays a list of all self assessments in the system.
        /// </summary>
        /// <remarks>
        /// Requires an admin user, not just an organisation user, because
        /// this is a list of **all** self assessments, not just those in the
        /// user's organisation.
        /// Allowing any orgnisation user to view this would be an organisational
        /// privacy violation.
        /// </remarks>
        [Authorize(Roles = AdminUser.Role)]
        public ActionResult Index()
        {
            var viewModel = new SelfAssessmentIndexData
            {
                SelfAssessments = db.SelfAssessments
                    .OrderBy(sa => sa.TimeStamp)
                    .ToList()
                    .Select(sa => mapper.Map<SelfAssessmentViewModel>(sa))
            };
            return View(viewModel);
        }

        /// GET: SelfAssessment/Details/5
        [Authorize(Roles = AdminUser.Role)]
        public ActionResult Details(int id)
        {
            var selfAssessment = db.SelfAssessments
                .Include(sa => sa.CoreSkills)
                .FirstOrDefault(sa => sa.Id == id);
            if (selfAssessment == null)
            {
                return HttpNotFound();
            }

            var viewModel = new SelfAssessmentDetailsData()
            {
                SelfAssessment = mapper.Map<SelfAssessmentViewModel>(selfAssessment),
                CoreSkills = selfAssessment.CoreSkills
                    .Select(cs => mapper.Map<CoreSkillViewModel>(cs))
            };
            return View(viewModel);
        }

        /// GET: SelfAssessment/Create
        [Authorize(Roles = AdminUser.Role)]
        public ActionResult Create()
        {
            return View();
        }

        /// POST: SelfAssessment/Create
        /// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        /// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = AdminUser.Role)]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Description")]
                                   SelfAssessmentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            var selfAssessment = mapper.Map<SelfAssessment>(viewModel);
            db.SelfAssessments.Add(selfAssessment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// GET: SelfAssessment/Edit/5
        [Authorize(Roles = AdminUser.Role)]
        public ActionResult Edit(int id)
        {
            SelfAssessment selfAssessment = db.SelfAssessments.Find(id);
            if (selfAssessment == null)
            {
                return HttpNotFound();
            }
            return View(mapper.Map<SelfAssessmentViewModel>(selfAssessment));
        }

        /// POST: SelfAssessment/Edit/5
        /// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        /// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = AdminUser.Role)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description")]
                                 SelfAssessmentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            var selfAssessment = db.SelfAssessments.Find(viewModel.Id);
            mapper.Map(viewModel, selfAssessment);
            db.SaveChanges();
            return RedirectToAction("Details", new { id = selfAssessment.Id });
        }

        /// <summary>
        /// GET: SelfAssessment/Start
        /// "Start" will be a temporary sub-route name.
        /// </summary>
        [Authorize(Roles = BasicUser.Role)]
        public ActionResult Start()
        {
            // TODO: Get latest version of self assessment based on user's organisation.
            var selfAssessment = db.SelfAssessments
                .OrderByDescending(sa => sa.TimeStamp)
                .ToList()
                .First();

            return RedirectToAction("Index", "CoreSkill", 
                new { selfAssessmentId = selfAssessment.Id });
        }

        /// <summary>
        /// Generate survey result report for a given self assessment based on
        /// current user's latest answer.
        /// </summary>
        /// <remarks>
        /// If the user has no answer for the chosen self assessment,
        /// the result report will not be generated and downloaded.
        /// A warning message will be displayed instead.
        /// Otherwise, the result report will be generated and downloaded.
        /// The user will stay on the same page.
        ///
        /// Example usage:
        ///     GET SelfAssessment/SurveyReport/:id
        /// </remarks>
        [Authorize(Roles = BasicUser.Role)]
        [HttpGet]
        public ActionResult SurveyReport(int id)
        {
            // Retrieve the answers the user has made before.
            var currentBasicUser = CurrentUser() as BasicUser;
            if (currentBasicUser == null)
            {
                return null;
            }
            var latestAnswer = currentBasicUser.Answers
                .OrderByDescending(a => a.TimeStamp)
                .FirstOrDefault(a => id == a.SelfAssessment.Id);

            // If there is no answer exists, redirect to survey start page with a generate failed flag.
            if (latestAnswer == null)
            {
                TempData["NoAnswer"] = true;
                return RedirectToAction("Start");
            }

            // Otherwise, generate result report based on latest answer.
            var pdfOutputStream = reportPdfGenerateService.GenerateSurveyReport(latestAnswer, currentBasicUser);

            // Get report output stream and export PDF report.
            string sFileName = $"MyEmployability_Report_{DateTime.Now:yyyyMMdd_hhmmss}.pdf";
            Response.ContentType = "application/pdf";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + sFileName);
            Response.BinaryWrite(pdfOutputStream.ToArray());

            // Stay on the same page.
            return null;
        }

        /// <summary>
        /// Generate summary result report all the self assessments
        /// and corresponding answers.
        /// </summary>
        /// <remarks>
        /// If there is no self assessment in the system,
        /// the result report will not be generated and downloaded.
        /// A warning message will be displayed instead.
        /// Otherwise, the result report will be generated and downloaded.
        /// The user will stay on the same page.
        ///
        /// Example usage:
        ///     GET SelfAssessment/SummaryReport
        /// </remarks>
        [Authorize(Roles = AdminUser.Role)]
        [HttpGet]
        public ActionResult SummaryReport()
        {
            // Retrieve the self assessments and answers in the system.
            var currentAdminUser = CurrentUser() as AdminUser;
            if (currentAdminUser == null)
            {
                return null;
            }
            var selfAssessments = db.SelfAssessments.OrderBy(sa => sa.TimeStamp).ToList();
            var answers = db.Answers.ToList();

            // If there is no self assessment exists, redirect to survey start page with a generate failed flag.
            if (selfAssessments.Count == 0)
            {
                TempData["NoSelfAssessment"] = true;
                return RedirectToAction("Index");
            }

            // Otherwise, generate summary report.
            var pdfOutputStream = reportPdfGenerateService
                .GenerateSummaryReport(selfAssessments, answers, currentAdminUser);

            // Get report output stream and export PDF report.
            string sFileName = $"MyEmployability_SummaryReport_{DateTime.Now:yyyyMMdd_hhmmss}.pdf";
            Response.ContentType = "application/pdf";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + sFileName);
            Response.BinaryWrite(pdfOutputStream.ToArray());

            // Stay on the same page.
            return null;
        }

        // Returns the currently logged in user.
        private ApplicationUser CurrentUser()
        {
            // Use User.Identity.Name, not User.Identity.GetUserName(),
            // because the latter is nigh impossible to mock for testing.
            var userName = User.Identity.Name;
            var user = userStore.FindByNameAsync(userName);
            return user.Result;
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

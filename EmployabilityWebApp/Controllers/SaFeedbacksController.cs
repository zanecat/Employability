using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.ViewModels;
using Microsoft.AspNet.Identity;
using AutoMapper;

namespace EmployabilityWebApp.Controllers
{
    public class SaFeedbacksController : Controller
    {
        private readonly EmployabilityDbContext db;
        private readonly IMapper mapper;

        public SaFeedbacksController(EmployabilityDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        // GET: SaFeedbacks
        [Authorize(Roles= AdminUser.Role)]
        public ActionResult Index()
        {
            var allFeedbacks = db.SaFeedbacks
                .Include(s => s.BasicUser)
                .Include(s => s.selfAssessment).ToList();

            List<FeedbackDetailData> viewmodelList = new List<FeedbackDetailData>();
            foreach (var feedback in allFeedbacks)
            {
                //viewmodelList.Add(MaptoDetailViewModel(feedback));
                viewmodelList.Add(mapper.Map<FeedbackDetailData>(feedback));
            }
            return View(viewmodelList);
        }

        // GET: SaFeedbacks/Create
        /// <summary>
        /// Createign a Feedback with a Selfassessment ID
        /// </summary>
        /// <param name="Id">Selfassessment id</param>
        /// <returns></returns>
        [Authorize(Roles= BasicUser.Role)]
        public ActionResult Create(int Id)
        {
            return View(new FeedbackCreateData() {selfAssessmentId = Id});
        }

        // POST: SaFeedbacks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = BasicUser.Role)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FeedbackCreateData viewmodel)
        {
            var saFeedback = MapFromViewmodel(viewmodel);

            db.SaFeedbacks.Add(saFeedback);
            db.SaveChanges();
            return RedirectToAction("ThankYou", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private SaFeedback MapFromViewmodel(FeedbackCreateData viewmodel)
        {
            var saFeedback = new SaFeedback();
            saFeedback.Rating = viewmodel.Rating;
            saFeedback.Comment = viewmodel.Comment;
            saFeedback.selfAssessment = db.SelfAssessments
                                        .FirstOrDefault(s => s.Id == viewmodel.selfAssessmentId);
            saFeedback.BasicUser = db.Users
                                    .OfType<BasicUser>()
                                    .FirstOrDefault(u => u.UserName == User.Identity.Name);

            return saFeedback;
        }
    }
}

using System.Linq;
using System.Web.Mvc;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.ViewModels;
using AutoMapper;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using EmployabilityWebApp.Services;

namespace EmployabilityWebApp.Controllers
{
    public class ElementController : Controller
    {
        private readonly EmployabilityDbContext db;
        private readonly IMapper mapper;
        private readonly ICoreSkillElementService coreSkillElementService;

        public ElementController(EmployabilityDbContext db, 
            IMapper mapper, ICoreSkillElementService coreSkillElementService)
        {
            this.db = db;
            this.mapper = mapper;
            this.coreSkillElementService = coreSkillElementService;
        }

        /// <summary>
        /// GET: Element/CreateSimplifiedElement/{id}?prev_action=Details
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateSimplifiedElement(int id)
        {
            ViewData["CoreSkillId"] = id;
            return View();
        }

        /// <summary>
        /// POST: Element/CreateSimplifiedElement/{id}?prev_action=Details
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSimplifiedElement([Bind(Include = "Id,Description,Position,CoreSkillId")] CoreSkillSimplifiedElementViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            
            bool hasCreatedNewSelfAssessmentVersion = 
                coreSkillElementService.CreateSimplifiedElement(
                    viewModel.Id, 
                    viewModel.Description, 
                    viewModel.CoreSkillId);

            if (hasCreatedNewSelfAssessmentVersion)
            {
                int latestSAVersion = coreSkillElementService.GetLatestSelfAssessmentVersion();
                return RedirectToAction("Details", "SelfAssessment", new { id = latestSAVersion });
            }

            return RedirectToAction("Details", "CoreSkill", new { id = viewModel.CoreSkillId });
        }

        /// <summary>
        /// GET: Element/CreateDetailedElement/{id}?prev_action=Details
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateDetailedElement(int id)
        {
            ViewData["CoreSkillId"] = id;
            return View();
        }

        /// <summary>
        /// POST: Element/CreateDetailedElement/{id}?prev_action=Details
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDetailedElement([Bind(Include = "Id,Description,DetailedOptions,Position,CoreSkillId")] CoreSkillDetailedElementViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            bool hasCreatedNewSelfAssessmentVersion =
                coreSkillElementService.CreateDetailedElement(
                    viewModel.Id, 
                    viewModel.Description, 
                    viewModel.DetailedOptions, 
                    viewModel.CoreSkillId);

            if (hasCreatedNewSelfAssessmentVersion)
            {
                int latestSAVersion = coreSkillElementService.GetLatestSelfAssessmentVersion();
                return RedirectToAction("Details", "SelfAssessment", new { id = latestSAVersion });
            }

            return RedirectToAction("Details", "CoreSkill", new { id = viewModel.CoreSkillId });
        }

        /// <summary>
        /// GET: Element/CreateTextElement/{id}?prev_action=Details
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateTextElement(int id)
        {
            ViewData["CoreSkillId"] = id;
            return View();
        }

        /// <summary>
        /// POST: Element/CreateTextElement/{id}?prev_action=Details
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTextElement([Bind(Include = "Id,Description,Position,CoreSkillId")] CoreSkillTextElementViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            bool hasCreatedNewSelfAssessmentVersion =
                coreSkillElementService.CreateTextElement(
                    viewModel.Id, 
                    viewModel.Description, 
                    viewModel.CoreSkillId);

            if (hasCreatedNewSelfAssessmentVersion)
            {
                int latestSAVersion = coreSkillElementService.GetLatestSelfAssessmentVersion();
                return RedirectToAction("Details", "SelfAssessment", new { id = latestSAVersion });
            }

            return RedirectToAction("Details", "CoreSkill", new { id = viewModel.CoreSkillId });
        }

        /// <summary>
        /// GET: Element/EditSimplifiedElement/{id}?coreSkillId={CoreSkillId}
        /// </summary>
        /// <returns></returns>
        public ActionResult EditSimplifiedElement(int id, int coreSkillId)
        {
            SimplifiedElement simplifiedElement = (SimplifiedElement)db.CoreSkillElements.Find(id);
            if (simplifiedElement == null)
            {
                return HttpNotFound();
            }

            ViewData["coreSkillId"] = coreSkillId;

            CoreSkillSimplifiedElementViewModel coreSkillSimplifiedElement =
                new CoreSkillSimplifiedElementViewModel(
                    simplifiedElement.Id,
                    simplifiedElement.Description,
                    simplifiedElement.Position,
                    coreSkillId);

            return View(coreSkillSimplifiedElement);
        }

        /// <summary>
        /// POST: Element/EditSimplifiedElement/{id}?coreSkillId={CoreSkillId}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSimplifiedElement([Bind(Include = "Id,Description,Position,CoreSkillId")] CoreSkillSimplifiedElementViewModel viewModel)
        {
            coreSkillElementService.EditSimplifiedElement(viewModel.Id, viewModel.Description);

            return RedirectToAction("Details", "CoreSkill", new { id = viewModel.CoreSkillId });
        }

        /// <summary>
        /// Delete a simplified element (no routing)
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteSimplifiedElement(int id, int coreSkillId)
        {
            coreSkillElementService.DeleteSimplifiedElement(coreSkillId, id);

            return RedirectToAction("Details", "CoreSkill", new { id = coreSkillId });
        }

        /// <summary>
        /// GET: Element/EditDetailedElement/{id}?coreSkillId={CoreSkillId}
        /// </summary>
        /// <returns></returns>
        public ActionResult EditDetailedElement(int id, int coreSkillId)
        {
            DetailedElement detailedElement = (DetailedElement)db.CoreSkillElements.Find(id);
            if (detailedElement == null)
            {
                return HttpNotFound();
            }

            ViewData["coreSkillId"] = coreSkillId;

            List<DetailedOptionViewModel> detailedOptions = new List<DetailedOptionViewModel>();

            foreach (DetailedOption option in detailedElement.DetailedOptions)
            {
                detailedOptions.Add(new DetailedOptionViewModel(option.Id, option.Description, option.Position));
            }

            CoreSkillDetailedElementViewModel coreSkillDetailedElement =
                new CoreSkillDetailedElementViewModel(
                    detailedElement.Id,
                    detailedElement.Description,
                    detailedOptions,
                    detailedElement.Position,
                    coreSkillId);

            return View(coreSkillDetailedElement);
        }

        /// <summary>
        /// POST: Element/EditDetailedElement/{id}?coreSkillId={CoreSkillId}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDetailedElement([Bind(Include = "Id,Description,DetailedOptions,Position,CoreSkillId")] CoreSkillDetailedElementViewModel viewModel)
        {
            coreSkillElementService.EditDetailedElement(viewModel.Id, viewModel.Description, viewModel.DetailedOptions);

            return RedirectToAction("Details", "CoreSkill", new { id = viewModel.CoreSkillId });
        }

        /// <summary>
        /// Delete a detailed element (no routing)
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteDetailedElement(int id, int coreSkillId)
        {
            coreSkillElementService.DeleteDetailedElement(coreSkillId, id);

            return RedirectToAction("Details", "CoreSkill", new { id = coreSkillId });
        }

        /// <summary>
        /// GET: Element/EditTextElement/{id}?coreSkillId={CoreSkillId}
        /// </summary>
        /// <returns></returns>
        public ActionResult EditTextElement(int id, int coreSkillId)
        {
            TextElement textElement = (TextElement)db.CoreSkillElements.Find(id);
            if (textElement == null)
            {
                return HttpNotFound();
            }

            ViewData["coreSkillId"] = coreSkillId;

            CoreSkillTextElementViewModel coreSkillTextElement =
                new CoreSkillTextElementViewModel(
                    textElement.Id,
                    textElement.Description,
                    textElement.Position,
                    coreSkillId);

            return View(coreSkillTextElement);
        }

        /// <summary>
        /// POST: Element/EditTextElement/{id}?coreSkillId={CoreSkillId}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTextElement([Bind(Include = "Id,Description,Position,CoreSkillId")] CoreSkillTextElementViewModel viewModel)
        {
            coreSkillElementService.EditTextElement(viewModel.Id, viewModel.Description);

            return RedirectToAction("Details", "CoreSkill", new { id = viewModel.CoreSkillId });
        }

        /// <summary>
        /// Delete a text element (no routing)
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteTextElement(int id, int coreSkillId)
        {
            coreSkillElementService.DeleteTextElement(coreSkillId, id);

            return RedirectToAction("Details", "CoreSkill", new { id = coreSkillId });
        }
    }
}
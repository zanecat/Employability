using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployabilityWebApp.Models;
using JetBrains.Annotations;
using System.Data.Entity.Validation;
using EmployabilityWebApp.ViewModels;
using AutoMapper;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;

namespace EmployabilityWebApp.Services
{
    [ImplicitNotNull]
    public class CoreSkillElementService : ICoreSkillElementService
    {
        private readonly EmployabilityDbContext db;
        private readonly IMapper mapper;

        public CoreSkillElementService(EmployabilityDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public bool CreateSimplifiedElement(
            int elementId, string elementDescription, int coreSkillId)
        {
            int maxPosition = getNextPosition(coreSkillId);
            var simplifiedElement = mapper.Map<SimplifiedElement>(new SimplifiedElementViewModel(elementId, elementDescription, maxPosition));

            return updateSelfAssessmentVersion(simplifiedElement, coreSkillId);
        }
        
        public void EditSimplifiedElement(int elementId, string elementDescription)
        {
            SimplifiedElement simplifiedElement = (SimplifiedElement)db.CoreSkillElements.Find(elementId);
            simplifiedElement.Description = elementDescription;

            db.SaveChanges();
        }

        public void DeleteSimplifiedElement(int coreSkillId, int elementId)
        {
            CoreSkill coreSkill = db.CoreSkills.Find(coreSkillId);
            CoreSkillElement element = db.CoreSkillElements.Find(elementId);

            coreSkill.Elements.Remove(element);
            db.CoreSkillElements.Remove(element);

            db.SaveChanges();
        }

        public bool CreateDetailedElement(
            int elementId, string elementDescription, List<DetailedOptionViewModel> elementDetailedOptions, int coreSkillId)
        {
            int maxPosition = getNextPosition(coreSkillId);
            var detailedElement = mapper.Map<DetailedElement>(new DetailedElementViewModel(elementId, elementDescription, elementDetailedOptions, maxPosition));

            return updateSelfAssessmentVersion(detailedElement, coreSkillId);
        }

        public void EditDetailedElement(int elementId, string elementDescription, List<DetailedOptionViewModel> detailedOptions)
        {
            DetailedElement detailedElement = (DetailedElement)db.CoreSkillElements.Find(elementId);
            detailedElement.Description = elementDescription;

            foreach (DetailedOptionViewModel optionViewModel in detailedOptions)
            {
                DetailedOption option = db.DetailedOptions.Find(optionViewModel.Id);
                option.Description = optionViewModel.Description;
            }

            db.SaveChanges();
        }

        public void DeleteDetailedElement(int coreSkillId, int elementId)
        {
            CoreSkill coreSkill = db.CoreSkills.Find(coreSkillId);
            DetailedElement element = (DetailedElement)db.CoreSkillElements.Find(elementId);

            for (int c = element.DetailedOptions.Count(); 0 < c; c = element.DetailedOptions.Count())
            {
                DetailedOption option = element.DetailedOptions.ToList()[c - 1];
                element.DetailedOptions.Remove(option);
                db.DetailedOptions.Remove(option);
            }

            coreSkill.Elements.Remove(element);
            db.CoreSkillElements.Remove(element);

            db.SaveChanges();
        }

        public bool CreateTextElement(int elementId, string elementDescription, int coreSkillId)
        {
            int maxPosition = getNextPosition(coreSkillId);
            var textElement = mapper.Map<TextElement>(new TextElementViewModel(elementId, elementDescription, maxPosition));

            return updateSelfAssessmentVersion(textElement, coreSkillId);
        }

        public void EditTextElement(int elementId, string elementDescription)
        {
            TextElement textElement = (TextElement)db.CoreSkillElements.Find(elementId);
            textElement.Description = elementDescription;

            db.SaveChanges();
        }

        public void DeleteTextElement(int coreSkillId, int elementId)
        {
            CoreSkill coreSkill = db.CoreSkills.Find(coreSkillId);
            CoreSkillElement element = db.CoreSkillElements.Find(elementId);

            coreSkill.Elements.Remove(element);
            db.CoreSkillElements.Remove(element);

            db.SaveChanges();
        }

        public int GetLatestSelfAssessmentVersion()
        {
            var selfAssessment = getLatestSelfAssessment();
            if (selfAssessment == null)
            {
                return 0;
            }
            return getLatestSelfAssessment().Id;
        }

        /// <summary>
        /// Update self-assessment version, keep the version when the version doesn't have any answer associated with
        /// it; however, create a new version if the older version has any answer associated with it.
        /// </summary>
        /// <param name="coreSkillElement"></param>
        /// <param name="coreSkillId"></param>
        private bool updateSelfAssessmentVersion(CoreSkillElement coreSkillElement, int coreSkillId)
        {
            if(hasAnswer(coreSkillId))
            {
                createNewSelfAssessmentVersion(coreSkillElement, coreSkillId);

                return true;
            }
            else
            {
                CoreSkill coreSkill = db.CoreSkills.Find(coreSkillId);
                coreSkill.Elements.Add(coreSkillElement);
                db.CoreSkillElements.Add(coreSkillElement);

                db.SaveChanges();
            }

            return false;
        }

        /// <summary>
        /// Create a new version for entire origin self-assessment, doing this in case where original self-assessment
        /// already has answer associated with it. The purpose is to allow user use the new self-assessment while
        /// reserve all answers associated with the older version of self-assessment.
        /// </summary>
        /// <param name="coreSkillElement"></param>
        /// <param name="coreSkillId"></param>
        private void createNewSelfAssessmentVersion(CoreSkillElement coreSkillElement, int coreSkillId)
        {
            // Note: this ToList().Last() works, but it's not the most efficient way as 
            // it brings back lots of records and waste loads of CPU, but for now, make
            // it work first and then improve it later.
            SelfAssessment selfAssessment = getSelfAssessment(coreSkillId);
            ICollection<CoreSkill> newCoreSkills = DeepCopyCoreSkills(selfAssessment, coreSkillElement, coreSkillId);

            // With new set of CoreSkills as parameter, create a new version of selfAssessment, and store it in database.
            var newSelfAssessment = new SelfAssessment(
                selfAssessment.Title,
                selfAssessment.Description,
                newCoreSkills);

            db.SelfAssessments.Add(newSelfAssessment);

            db.SaveChanges();

            // A new SelfAssessment is created, and amend the missing SelfAssessment field into new CoreSkills
            foreach (CoreSkill coreSkill in newCoreSkills)
            {
                db.CoreSkills.Find(coreSkill.Id).SelfAssessment = newSelfAssessment;
            }
        }

        private ICollection<CoreSkill> DeepCopyCoreSkills(SelfAssessment selfAssessment, CoreSkillElement coreSkillElement, int coreSkillId)
        {
            ICollection<CoreSkill> newCoreSkills = new List<CoreSkill>() { };

            foreach (CoreSkill coreSkill in selfAssessment.CoreSkills.ToList())
            {
                // Deep copy all elements of ith CoreSkill, all copies stored in existingElements
                ICollection<CoreSkillElement> existingElements = DeepCopyElements(selfAssessment, coreSkill);

                // Recognize the CoreSkill, and add the new element into its existingElements set. 
                if (coreSkill.Id.Equals(coreSkillId))
                {
                    existingElements.Add(coreSkillElement);
                    db.CoreSkillElements.Add(coreSkillElement);
                }

                // Copy the CoreSkill and store it into newCoreSkills set.
                // Note: Since the new SelfAssessment has not been created yet, temporarily leave it as blank
                //       that is, temporarily, SelfAsssessment field is missing.
                CoreSkill coreSkillCopy = new CoreSkill(
                    coreSkill.Name,
                    coreSkill.Description,
                    existingElements,
                    null);
                newCoreSkills.Add(coreSkillCopy);

                // Add the copy of CoreSkill into database.
                db.CoreSkills.Add(coreSkillCopy);
            }

            return newCoreSkills;
        }

        private ICollection<CoreSkillElement> DeepCopyElements(SelfAssessment selfAssessment, CoreSkill coreSkill)
        {
            ICollection<CoreSkillElement> existingElements = new List<CoreSkillElement>() { };

            existingElements = deepCopySimplifiedElements(coreSkill, existingElements);
            existingElements = deepCopyTextElements(coreSkill, existingElements);
            existingElements = deepCopyDetailedElements(coreSkill, existingElements);

            return existingElements;
        }

        private ICollection<CoreSkillElement> deepCopySimplifiedElements(CoreSkill coreSkill, ICollection<CoreSkillElement> existingElements)
        {
            foreach (SimplifiedElement element in coreSkill.Elements.OfType<SimplifiedElement>())
            {
                SimplifiedElement simplifiedElementCopy = new SimplifiedElement(
                        element.Description,
                        element.Position);

                db.CoreSkillElements.Add(simplifiedElementCopy);
                existingElements.Add(simplifiedElementCopy);
            }

            return existingElements;
        }

        private ICollection<CoreSkillElement> deepCopyTextElements(CoreSkill coreSkill, ICollection<CoreSkillElement> existingElements)
        {
            foreach (TextElement element in coreSkill.Elements.OfType<TextElement>())
            {
                TextElement textElementCopy = new TextElement(
                        element.Description,
                        element.Position);

                db.CoreSkillElements.Add(textElementCopy);
                existingElements.Add(textElementCopy);
            }

            return existingElements;
        }

        private ICollection<CoreSkillElement> deepCopyDetailedElements(CoreSkill coreSkill, ICollection<CoreSkillElement> existingElements)
        {
            foreach (DetailedElement element in coreSkill.Elements.OfType<DetailedElement>())
            {
                ICollection<DetailedOption> existingDetailedOptions = new List<DetailedOption>();

                foreach (DetailedOption option in (element.DetailedOptions))
                {
                    DetailedOption detailedOptionCopy = new DetailedOption(
                        option.Description,
                        option.Position);
                    existingDetailedOptions.Add(detailedOptionCopy);
                    db.DetailedOptions.Add(detailedOptionCopy);
                }

                DetailedElement detailedElementCopy = new DetailedElement(
                        element.Description,
                        existingDetailedOptions,
                        element.Position);

                db.CoreSkillElements.Add(detailedElementCopy);
                existingElements.Add(detailedElementCopy);
            }

            return existingElements;
        }

        private bool hasAnswer(int coreSkillId)
        {
            int associatedSelfAssessmentId = db.CoreSkills.Find(coreSkillId).SelfAssessment.Id;
            if (db.SelfAssessments.Find(associatedSelfAssessmentId).Answers.Count() == 0)
            {
                return false;
            }

            return true;
        }

        private int getNextPosition(int coreSkillId)
        {
            int maxPosition = 1;
            try
            {
                var elementWithMaxPosition = db.CoreSkills.Find(coreSkillId).Elements.OrderByDescending(element => element.Position).ToList().First();
                maxPosition = elementWithMaxPosition.Position + 1;
            }
            catch(NullReferenceException e)
            {
                Console.WriteLine(e.Message);
                return maxPosition;
            }

            return maxPosition;
        }

        private CoreSkill selectCoreSkill(SelfAssessment selfAssessment, int coreSkillId)
        {
            foreach (CoreSkill coreSkill in selfAssessment.CoreSkills)
            {
                if (coreSkill.Id.Equals(coreSkillId))
                {
                    return coreSkill;
                }
            }

            return null;
        }

        private SelfAssessment getLatestSelfAssessment()
        {
            return db.SelfAssessments.OrderByDescending(selfassessment => selfassessment.TimeStamp).ToList().FirstOrDefault();
        }

        private SelfAssessment getSelfAssessment(int coreSkillId)
        {
            return db.CoreSkills.Find(coreSkillId).SelfAssessment;
        }
    }
}

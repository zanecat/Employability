using System;
using System.Collections.Generic;
using EmployabilityWebApp.Models;
using System.Linq;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Services
{
    [ImplicitNotNull]
    public class CoreSkillService : ICoreSkillService
    {
        private readonly EmployabilityDbContext db;

        public CoreSkillService(EmployabilityDbContext db)
        {
            this.db = db;
        }

        public IList<CoreSkill> FindForSelfAssessment(int selfAssessmentId)
        {
            var selfAssessment = db.SelfAssessments.Find(selfAssessmentId);
            if (selfAssessment == null)
            {
                return null;
            }
            var coreSkills = selfAssessment.CoreSkills
                .OrderBy(cs => cs.Id)
                .ToList();
            return coreSkills;
        }

        public Answer MostRecentOrNewAnswer(CoreSkill coreSkill, BasicUser user)
        {
            var selfAssessment = coreSkill.SelfAssessment;
            var answer = MostRecentAnswer(selfAssessment, user)
                         ?? CreateNewAnswer(selfAssessment, user);
            db.SaveChanges();
            return answer;
        }

        public bool IsFinished(CoreSkill coreSkill, BasicUser user)
        {
            var selfAssessment = coreSkill.SelfAssessment;
            Answer answer = MostRecentAnswer(selfAssessment, user);
            if (answer != null)
            {
                var answersToElements = answer.SubAnswers.
                    Where(answerLocal => coreSkill.Elements.
                    Any(element => element.Id.Equals(answerLocal.Element.Id))).ToList();
                if (answersToElements.Count() != 0)
                {
                    return answersToElements.All(a => a.IsFinished);
                }
              
            }

            return false;
        }

        public TElement FindElement<TElement>(int id) where TElement : CoreSkillElement
        {
            return db.CoreSkillElements
                .OfType<TElement>()
                .FirstOrDefault(subAnswer => id == subAnswer.Id);
        }

        public void SaveDetailedAnswer(Answer answer, int elementId, int? choiceId)
        {
            var element = (DetailedElement) db.CoreSkillElements.Find(elementId);
            var choice = db.DetailedOptions.Find(choiceId);

            // Try to change and validate the choice of an existing DetailedAnswer.
            // If there was no DetailedAnswer already, create one.
            var detailedAnswer = FindSubAnswer<DetailedAnswer>(answer, element)
                                 ?? NewDetailedAnswer(answer, element);
            try
            {
                detailedAnswer.SetChoiceAndValidate(choice);
            }
            catch (ArgumentException e)
            {
                throw new SurveyConcurrencyException("error saving detailed answer", e);
            }

            db.SaveChanges();
        }

        public void SaveTextAnswer(Answer answer, int elementId, string text)
        {
            var element = (TextElement) db.CoreSkillElements.Find(elementId);

            // Try to change and validate the text of an existing TextAnswer.
            // If there was no TextAnswer already, create one.
            var textAnswer = FindSubAnswer<TextAnswer>(answer, element)
                             ?? NewTextAnswer(answer, element);
            textAnswer.SetTextAndValidate(text);
            db.SaveChanges();
        }

        public void SaveSimplifiedAnswer(Answer answer, int elementId, int choice)
        {
            var element = (SimplifiedElement) db.CoreSkillElements.Find(elementId);

            // Try to change and validate the text of an existing SimplifiedAnswer.
            // If there was no SimplifiedAnswer already, create one.
            var textAnswer = FindSubAnswer<SimplifiedAnswer>(answer, element)
                             ?? NewSimplifiedAnswer(answer, element);
            textAnswer.SetChoiceAndValidate(choice);
            db.SaveChanges();
        }

        public IList<TElement> SpecificElements<TElement>(CoreSkill coreSkill)
            where TElement : CoreSkillElement
        {
            return coreSkill.Elements.OfType<TElement>().ToList();
        }

        public IList<ISurveyElement> SurveyElements(CoreSkill coreSkill, BasicUser user)
        {
            // Exit early if there are no core skill elements.
            if (!coreSkill.Elements.Any())
            {
                return new List<ISurveyElement>();
            }

            // Find the most recent Answer supplied by the user for this
            // CoreSkill's SelfAssessment, or an empty one if none exists.
            var answer = MostRecentAnswer(coreSkill.SelfAssessment, user)
                         ?? new Answer(coreSkill.SelfAssessment);

            // Find the survey elements of all types.
            return DetailedSurveyElements(coreSkill, answer)
                .Concat(TextSurveyElements(coreSkill, answer))
                .Concat(SimplifiedSurveyElements(coreSkill, answer))
                .ToList();
        }

        // Creates DetailedSurveyElements for all DetailedElements
        // in the CoreSkill, with DetailedAnswers from the Answer.
        private static IEnumerable<ISurveyElement> 
            DetailedSurveyElements(CoreSkill coreSkill, Answer answer)
        {
            return from element in coreSkill.Elements.OfType<DetailedElement>()
                   let subAnswer = FindSubAnswer<DetailedAnswer>(answer, element)
                   select new DetailedSurveyElement(element, subAnswer);
        }

        // Creates TextSurveyElements for all TextElements
        // in the CoreSkill, with TextAnswers from the Answer.
        private static IEnumerable<ISurveyElement> 
            TextSurveyElements(CoreSkill coreSkill, Answer answer)
        {
            return from element in coreSkill.Elements.OfType<TextElement>()
                   let subAnswer = FindSubAnswer<TextAnswer>(answer, element)
                   select new TextSurveyElement(element, subAnswer);
        }

        // Creates SimplifiedSurveyElements for all SimplifiedElements
        // in the CoreSkill, with SimplifiedAnswers from the Answer.
        private static IEnumerable<ISurveyElement> 
            SimplifiedSurveyElements(CoreSkill coreSkill, Answer answer)
        {
            return from element in coreSkill.Elements.OfType<SimplifiedElement>()
                   let subAnswer = FindSubAnswer<SimplifiedAnswer>(answer, element)
                   select new SimplifiedSurveyElement(element, subAnswer);
        }

        public CoreSkill Find(int id)
        {
            return db.CoreSkills.Find(id);
        }

        public SelfAssessment FindSelfAssessment(int id)
        {
            return db.SelfAssessments.Find(id);
        }

        // Registers a new detailed answer with the Answer, returning it.
        private DetailedAnswer NewDetailedAnswer(Answer answer, DetailedElement element)
        {
            var subAnswer = new DetailedAnswer(element);
            db.SubAnswers.Add(subAnswer);
            answer.AddSubAnswer(subAnswer);
            return subAnswer;
        }

        // Registers a new text answer with the Answer, returning it.
        private TextAnswer NewTextAnswer(Answer answer, TextElement element)
        {
            var subAnswer = new TextAnswer(element);
            db.SubAnswers.Add(subAnswer);
            answer.AddSubAnswer(subAnswer);
            return subAnswer;
        }

        // Registers a new simplified answer with the Answer, returning it.
        private SimplifiedAnswer NewSimplifiedAnswer(Answer answer, SimplifiedElement element)
        {
            var subAnswer = new SimplifiedAnswer(element);
            db.SubAnswers.Add(subAnswer);
            answer.AddSubAnswer(subAnswer);
            return subAnswer;
        }

        // Find a SubAnswer in the answer for the specified element,
        // of the specified type.
        //
        // TSubAnswer is the desired type of SubAnswer.
        // answer is the answer in which to look.
        // element is the element which the found SubAnswer must answer.
        // Eeturns the found SubAnswer, or null if none is found.
        private static TSubAnswer FindSubAnswer<TSubAnswer>(Answer answer, CoreSkillElement element)
            where TSubAnswer : SubAnswer
        {
            return answer.SubAnswers
                .OfType<TSubAnswer>()
                .FirstOrDefault(subAnswer => element.Id == subAnswer.Element.Id);
        }

        // Finds the user's most recent Answer for a given SelfAssessment.
        //
        // selfAssessment is the SelfAssessment which the found Answer must be a response to
        // user is the User which must have submitted the Answer
        // Returns the found Answer, or null if no such Answer can be found.
        [CanBeNull]
        private static Answer MostRecentAnswer(SelfAssessment selfAssessment, BasicUser user)
        {
            return user.Answers
                .OrderByDescending(a => a.TimeStamp)
                .FirstOrDefault(a => selfAssessment.Id == a.SelfAssessment.Id);
        }

        // Creates an Answer for the specified self assessment and user.
        //
        // selfAssessment is the SelfAssessment which the Answer should be a response to.
        // user is the user who is creating the Answer.
        // Returns the new Answer.
        private Answer CreateNewAnswer(SelfAssessment selfAssessment, BasicUser user)
        {
            var answer = new Answer(selfAssessment);
            db.Answers.Add(answer);
            user.AddAnswer(answer);
            return answer;
        }

    }
}
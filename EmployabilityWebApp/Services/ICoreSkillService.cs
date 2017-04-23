using System.Collections.Generic;
using EmployabilityWebApp.Models;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Services
{
    [ImplicitNotNull]
    public interface ICoreSkillService
    {
        /// <summary>
        /// Returns a ISurveyElement for each CoreSkillElement in coreSkill
        /// looking for SubAnswers in the most recent Answer the user submitted for the
        /// coreSkill's SelfAssessment.
        /// </summary>
        /// <param name="coreSkill">The source of the CoreSkillElements in the result</param>
        /// <param name="user">The source of the SubAnswers in the result</param>
        /// <returns>The created ISurveyElements</returns>
        IList<ISurveyElement> SurveyElements(CoreSkill coreSkill, BasicUser user);

        /// <summary>
        /// Returns the CoreSkill with the specified id, or null if it doesn't exist.
        /// </summary>
        [CanBeNull]
        CoreSkill Find(int id);

        /// <summary>
        /// Find all the CoreSkills of a SelfAssessment specified by id
        /// </summary>
        /// <param name="selfAssessmentId">Id of the specific SelfAssessment</param>
        /// <returns>
        /// A list of coreskills of the specific SelfAssessment,
        /// or null if the SelfAssessment cannot be found.
        /// </returns>
        [CanBeNull]
        IList<CoreSkill> FindForSelfAssessment(int selfAssessmentId);

        /// <summary>
        /// Finds the user's most recent Answer for a given CoreSkill.
        /// </summary>
        /// If no such Answer exists, a new one is created.
        /// <param name="coreSkill">The coreSkill for which the answer must exist</param>
        /// <param name="user">The User which must have submitted the Answer</param>
        /// <returns>The found Answer, or null if no such Answer can be found.</returns>
        [CanBeNull]
        Answer MostRecentOrNewAnswer(CoreSkill coreSkill, BasicUser user);

        /// <summary>
        /// Save a user's choice for the DetailedELement in the Answer.
        /// </summary>
        /// If there already exists a DetailedAnswer for the specified Answer
        /// and DetailedElement, it will be updated.
        /// Otherwise, a new DetailedAnswer will be created and the choice saved in it.
        /// <param name="answer">The Answer to be updated</param>
        /// <param name="elementId">The id of the DetailedElement to be answered</param>
        /// <param name="choiceId">The id of the DetailedOption the user chose for the DetailedElement</param>
        void SaveDetailedAnswer(Answer answer, int elementId, int? choiceId);

        /// <summary>
        /// Save a user's choice for the TextElement in the Answer.
        /// </summary>
        /// If there already exists a TextAnswer for the specified Answer
        /// and TextElement, it will be updated.
        /// Otherwise, a new TextAnswer will be created and the text saved in it.
        /// <param name="answer">The Answer to be updated</param>
        /// <param name="elementId">The id of the TextElement to be answered</param>
        /// <param name="text">The text the user input to answer the TextElement</param>
        void SaveTextAnswer(Answer answer, int elementId, string text);

        /// <summary>
        /// Save a user's choice for the SimplifiedElement in the Answer.
        /// </summary>
        /// If there already exists a SimplifiedAnswer for the specified Answer
        /// and SimplifiedElement, it will be updated.
        /// Otherwise, a new SimplifiedAnswer will be created and the choice saved in it.
        /// <param name="answer">The Answer to be updated</param>
        /// <param name="elementId">The id of the SimplifiedElement to be answered</param>
        /// <param name="choice">The choice the user input to answer the SimplifiedElement</param>
        void SaveSimplifiedAnswer(Answer answer, int elementId, int choice);

        /// <summary>
        /// Retrieve all the elements with a particular type, from a CoreSkill.
        /// </summary>
        /// <typeparam name="TElement">The subtype of elements we want</typeparam>
        /// <param name="coreSkill">The CoreSkill from which to collect elements.</param>
        /// <returns>A list of the TElements of that coreSkill</returns>
        IList<TElement> SpecificElements<TElement>(CoreSkill coreSkill)
            where TElement : CoreSkillElement;

        /// <summary>
        /// Check if a self assessment of a user has been finished or not. 
        /// Return true if it is finished, otherwise false.
        /// </summary>
        /// <param name="coreSkill">The coreSkill to check if it is finished</param>
        /// <param name="user">The target users.</param>
        /// <returns></returns>
        bool IsFinished(CoreSkill coreSkill, BasicUser user);
    }
}
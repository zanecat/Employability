using System;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Models
{
    /// <summary>
    /// Base interface for SurveyElement, useful for collections.
    /// </summary>
    public interface ISurveyElement
    {
        CoreSkillElement Element { get; }
        SubAnswer SubAnswer { get; }
    }

    /// <summary>
    /// A simple tuple of a survey question and a user's answer for it.
    /// </summary>
    /// <remarks>
    /// Validation is performed to ensure the subAnswer is actually one
    /// of the element's subAnswers.
    /// </remarks>
    /// <typeparam name="TElement">The type of CoreSkillElement in this tuple</typeparam>
    /// <typeparam name="TSubAnswer">The type of SubAnswer in this tuple</typeparam>
    [NotMapped]
    public abstract class SurveyElement<TElement, TSubAnswer> : ISurveyElement
        where TElement : CoreSkillElement
        where TSubAnswer : SubAnswer
    {
        protected SurveyElement([NotNull] TElement element, [CanBeNull] TSubAnswer subAnswer)
        {
            if (subAnswer != null && subAnswer.Element.Id != element.Id)
            {
                throw new ArgumentException(
                    "element " + element.Id + " and subAnswer " 
                    + subAnswer.Id + " do not match.");
            }
            Element = element;
            SubAnswer = subAnswer;
        }

        public TElement Element { get; }
        public TSubAnswer SubAnswer { get; }

        CoreSkillElement ISurveyElement.Element => Element;
        SubAnswer ISurveyElement.SubAnswer => SubAnswer;
    }

    /// <summary>
    /// A specialiation of SurveyElement for DetailedElements.
    /// </summary>
    [NotMapped]
    public class DetailedSurveyElement : SurveyElement<DetailedElement, DetailedAnswer>
    {
        public DetailedSurveyElement(DetailedElement element, DetailedAnswer subAnswer)
            : base(element, subAnswer) { }
    }

    /// <summary>
    /// A specialiation of SurveyElement for TextElements.
    /// </summary>
    [NotMapped]
    public class TextSurveyElement : SurveyElement<TextElement, TextAnswer>
    {
        public TextSurveyElement(TextElement element, TextAnswer subAnswer)
            : base(element, subAnswer) { }
    }

    /// <summary>
    /// A specialiation of SurveyElement for SimplifiedElements.
    /// </summary>
    [NotMapped]
    public class SimplifiedSurveyElement : SurveyElement<SimplifiedElement, SimplifiedAnswer>
    {
        public SimplifiedSurveyElement(SimplifiedElement element, SimplifiedAnswer subAnswer)
            : base(element, subAnswer) { }
    }
}
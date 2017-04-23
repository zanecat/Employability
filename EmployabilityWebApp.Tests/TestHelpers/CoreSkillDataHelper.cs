using System.Collections.Generic;
using System.Linq;
using EmployabilityWebApp.Models;
using Moq;

namespace EmployabilityWebApp.Tests.TestHelpers
{
    internal static class CoreSkillDataHelper
    {
        internal static void SetUpOptions(
            this List<DetailedOption> options, int nElements, int nOptions)
        {
            for (var i = 0; i < nOptions * nElements; i += 1)
            {
                var option = new Mock<DetailedOption>(i + 1, "option" + i, i + 1);
                options.Add(option.Object);
            }
        }

        internal static void SetUpDetailedSurveyElements(this List<ISurveyElement> surveyElements,
            List<DetailedElement> elements, List<DetailedAnswer> answers)
        {
            foreach (var item in elements.Select((element, i) => new {element, i}))
            {
                var answer = answers[item.i];
                surveyElements.Add(new DetailedSurveyElement(item.element, answer));
            }
        }

        internal static void SetUpDetailedElements(this List<DetailedElement> elements,
            int nElements, int nOptions, List<DetailedOption> options)
        {
            for (var i = 0; i < nElements; i += 1)
            {
                var element = new Mock<DetailedElement>(i + 1, "detailedElement" + i, options, i + 1);
                element.SetupGet(e => e.DetailedOptions).Returns(options.GetRange(i*nOptions, nOptions));
                elements.Add(element.Object);
            }
        }

        // Set up diagonalised subAnswers.
        // The nth DetailedAnswer uses the nth DetailedOption of the nth DetailedElement.
        internal static void SetUpDetailedAnswers(this List<DetailedAnswer> subAnswers,
            List<DetailedElement> elements, List<DetailedOption> options)
        {
            foreach (var item in elements.Select((element, i) => new { element, i }))
            {
                if (item.i*(elements.Count + 1) >= options.Count)
                {
                    subAnswers.Add(null);
                    continue;
                }
                var subAnswer = new Mock<DetailedAnswer>();
                subAnswer.Setup(sa => sa.Element).Returns(item.element);
                var choice = options[item.i*(elements.Count + 1)];
                subAnswer.Setup(sa => sa.Choice).Returns(choice);
                subAnswers.Add(subAnswer.Object);
            }
        }

        internal static void SetUpTextSurveyElements(this List<ISurveyElement> surveyElements,
            List<TextElement> elements, List<TextAnswer> answers)
        {
            foreach (var item in elements.Select((element, i) => new {element, i}))
            {
                var answer = answers[item.i];
                surveyElements.Add(new TextSurveyElement(item.element, answer));
            }
        }

        internal static void SetUpTextElements(this List<TextElement> elements, int nElements)
        {
            for (var i = 0; i < nElements; i += 1)
            {
                var element = new Mock<TextElement>(100 + i + 1, "textElement" + i, i + 1);
                elements.Add(element.Object);
            }
        }

        internal static void SetUpTextAnswers(this List<TextAnswer> subAnswers,
            List<TextElement> elements)
        {
            foreach (var item in elements.Select((element, i) => new { element, i }))
            {
                var subAnswer = new Mock<TextAnswer>();
                subAnswer.Object.SetTextAndValidate("answer for element " + item.i);
                subAnswer.Setup(sa => sa.Element).Returns(item.element);
                subAnswers.Add(subAnswer.Object);
            }
        }

        internal static void SetUpSimplifiedSurveyElements(this List<ISurveyElement> surveyElements,
            List<SimplifiedElement> elements, List<SimplifiedAnswer> answers)
        {
            foreach (var item in elements.Select((element, i) => new {element, i}))
            {
                var answer = answers[item.i];
                surveyElements.Add(new SimplifiedSurveyElement(item.element, answer));
            }
        }

        internal static void SetUpSimplifiedElements(this List<SimplifiedElement> elements, int nElements)
        {
            for (var i = 0; i < nElements; i += 1)
            {
                var element = new Mock<SimplifiedElement>(100 + i + 1, "simplifiedElement" + i, i + 1);
                elements.Add(element.Object);
            }
        }

        internal static void SetUpSimplifiedAnswers(this List<SimplifiedAnswer> subAnswers,
            List<SimplifiedElement> elements)
        {
            foreach (var item in elements.Select((element, i) => new { element, i }))
            {
                var subAnswer = new Mock<SimplifiedAnswer>();
                subAnswer.Object.SetChoiceAndValidate(item.i%5 + 1);
                subAnswer.Setup(sa => sa.Element).Returns(item.element);
                subAnswers.Add(subAnswer.Object);
            }
        }
    }
}
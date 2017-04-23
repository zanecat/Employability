using System;
using EmployabilityWebApp.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace EmployabilityWebApp.Tests.Models
{
    class SurveyElementsTest
    {
        private class SurveyElementSubClass : SurveyElement<CoreSkillElement, SubAnswer>
        {
            public SurveyElementSubClass(CoreSkillElement element, SubAnswer subAnswer)
                : base(element, subAnswer)
            { }
        }

        private const int RIGHT_ID = 826;
        private readonly Mock<CoreSkillElement> element = 
            new Mock<CoreSkillElement>(RIGHT_ID, "description", 123);
        private readonly Mock<SubAnswer> subAnswer = new Mock<SubAnswer>();

        private readonly List<DetailedOption> options = new List<DetailedOption>();

        public SurveyElementsTest() { }

        [Fact]
        public void TheConstructorShouldAllowCorrectPairs()
        {
            // Given a subAnswer is for the element,
            subAnswer.SetupGet(sa => sa.Element).Returns(element.Object);

            // Expect the constructor to set the element and subAnswer.
            var target = new SurveyElementSubClass(element.Object, subAnswer.Object);
            Assert.Equal(element.Object, target.Element);
            Assert.Equal(subAnswer.Object, target.SubAnswer);
        }

        [Fact]
        public void TheConstructorShouldNotAllowInvalidPairs()
        {
            // Given a subAnswer is for the element,
            var otherElement = new Mock<CoreSkillElement>(RIGHT_ID + 1, "description", 123);
            subAnswer.SetupGet(sa => sa.Element).Returns(otherElement.Object);

            // When the SurveyElement is constructed,
            // Then an ArgumentException should be thrown.
            Assert.Throws<ArgumentException>(() =>
                new SurveyElementSubClass(element.Object, subAnswer.Object));
        }
    }
}

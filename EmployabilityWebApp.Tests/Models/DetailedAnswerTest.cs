using System;
using EmployabilityWebApp.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace EmployabilityWebApp.Tests.Models
{
    class DetailedAnswerTest
    {
        private readonly DetailedAnswer target;
        private readonly Mock<DetailedElement> detailedElement = new Mock<DetailedElement>();
        private readonly Mock<DetailedOption> choice = new Mock<DetailedOption>(RIGHT_ID);
        private const int RIGHT_ID = 543;

        private readonly List<DetailedOption> options = new List<DetailedOption>();

        public DetailedAnswerTest()
        {
            target = new DetailedAnswer(detailedElement.Object);
        }

        [Fact]
        public void TheConstructorShouldSetTheDetailedElement()
        {
            // Given a detailed element is passed to the constructor,
            var target = new DetailedAnswer(detailedElement.Object);

            // Expect DetailedElement to be set.
            Assert.Equal(detailedElement.Object, target.DetailedElement);
        }

        [Fact]
        public void ItShouldAllowValidChoices()
        {
            // Given a detailed element which has the choice,
            detailedElement.Setup(de => de.HasOption(choice.Object)).Returns(true);

            // Expect the choice to pass validation and be set.
            target.SetChoiceAndValidate(choice.Object);
            Assert.Equal(choice.Object, target.Choice);
        }

        [Fact]
        public void ItShouldNotAllowChoicesNotOwnedByTheElement()
        {
            // Given a detailed element which does not have the choice,
            detailedElement.Setup(de => de.HasOption(choice.Object)).Returns(false);

            // When the choice is set,
            // Then an ArgumentException should be thrown.
            Assert.Throws<ArgumentException>(() =>
                target.SetChoiceAndValidate(choice.Object));
        }
    }
}

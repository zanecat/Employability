using System.Collections.Generic;
using System.Linq;
using EmployabilityWebApp.Models;
using Xunit;
using Moq;
using EmployabilityWebApp.Services;
using EmployabilityWebApp.Tests.TestHelpers;
using System;
using System.Data.Entity;
using System.Net.Mime;

namespace EmployabilityWebApp.Tests.Services
{
    public class CoreSkillServiceTest
    {
        private readonly CoreSkillService target;

        private readonly Mock<EmployabilityDbContext> db = 
            new Mock<EmployabilityDbContext>();

        private readonly Mock<CoreSkill> coreSkill =
            new Mock<CoreSkill>();
        private readonly Mock<SelfAssessment> selfAssessment =
            new Mock<SelfAssessment>(RIGHT_ID);
        private const int RIGHT_ID = 123;

        private readonly Mock<BasicUser> user = new Mock<BasicUser>();

        private readonly List<DetailedAnswer> detailedAnswers = 
            new List<DetailedAnswer>();
        private readonly List<DetailedElement> detailedElements = 
            new List<DetailedElement>();
        private readonly List<DetailedOption> options = 
            new List<DetailedOption>();

        private readonly List<TextAnswer> textAnswers = 
            new List<TextAnswer>();
        private readonly List<TextElement> textElements = 
            new List<TextElement>();
        private readonly List<SimplifiedAnswer> simplifiedAnswers = 
            new List<SimplifiedAnswer>();
        private readonly List<SimplifiedElement> simplifiedElements = 
            new List<SimplifiedElement>();

        private readonly Mock<Answer> oldAnswer = new Mock<Answer>(1, DateTime.Now);
        private readonly Mock<Answer> newAnswer = new Mock<Answer>(2, DateTime.Now.AddTicks(1));

        private readonly List<Answer> answers = new List<Answer>();

        private readonly List<CoreSkill> coreSkills = new List<CoreSkill>();

        public CoreSkillServiceTest()
        {
            target = new CoreSkillService(db.Object);

            user.SetupGet(u => u.Answers).Returns(answers);
        }

        [Fact]
        public void FindForSelfAssessment_ShouldReturnNullIfNoSelfAssessment()
        {
            SetupSelfAssessmentDbSet();
            Assert.Null(target.FindForSelfAssessment(1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(4)]
        public void FindForSelfAssessment_ShouldReturnCorrectNumberofCoreSkills(int nCoreSkill)
        {
            GivenASelfAssessmentWithCoreSkills(nCoreSkill);
            SetupSelfAssessmentDbSet();
            var saCoreSkills = target.FindForSelfAssessment(RIGHT_ID);
            Assert.Equal(nCoreSkill, saCoreSkills.Count);
        }

        [Fact]
        public void SurveyElements_ShouldBeEmptyIfNoElementsInCoreSkill()
        {
            // Given the core skill has a self assessment.
            coreSkill.SetupGet(cs => cs.SelfAssessment)
                .Returns(selfAssessment.Object);

            // Given the core skill has no elements.
            coreSkill.SetupGet(cs => cs.Elements).Returns(new List<CoreSkillElement>());

            // Expect the result to be an empty list.
            Assert.Empty(target.SurveyElements(coreSkill.Object, user.Object));
        }

        [Fact]
        public void SurveyElements_ShouldContainElementsButNoAnswersIfNoAnswerExists()
        {
            GivenABasicSelfAssessment(6, 6);

            // Given the user has no answers.
            user.SetupGet(u => u.Answers).Returns(new List<Answer>());

            // Expect the result to have no answers.
            var surveyElements = target.SurveyElements(coreSkill.Object, user.Object);
            Assert.All(surveyElements, se => Assert.Null(se.SubAnswer));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(0, 2)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 0)]
        [InlineData(2, 1)]
        [InlineData(2, 2)]
        public void SurveyElements_ShouldContainTheRightSurveyElements(int nElements, int nOptions)
        {
            GivenABasicSelfAssessment(nElements, nOptions);

            // Given the user has an answer for the selfAssessment,
            // and it has subAnswers,
            GivenAnswerIsForSelfAssessment(newAnswer);
            GivenOnlyDetailedAnswersExistIn(newAnswer);

            // Expect the result to have the right elements and answers.
            var surveyElements = target.SurveyElements(coreSkill.Object, user.Object);
            foreach (var item in surveyElements.Select((se, i) => new {se, i}))
            {
                Assert.Equal(detailedElements[item.i], item.se.Element);
                Assert.Equal(detailedAnswers[item.i], item.se.SubAnswer);
            }
        }

        [Fact]
        public void SurveyElements_ShouldUseTheMostRecentAnswer()
        {
            GivenABasicSelfAssessment(5, 5);

            // Given the user has two answers for the selfAssessment,
            // but only the new one has sub answers.
            GivenAnswerIsForSelfAssessment(newAnswer);
            GivenAnswerIsForSelfAssessment(oldAnswer);
            GivenOnlyDetailedAnswersExistIn(newAnswer);

            // Expect the right answer's subAnswers to be found, 
            // not the wrong answer's null subAnswers.
            var surveyElements = target.SurveyElements(coreSkill.Object, user.Object);
            Assert.NotNull(surveyElements[0].SubAnswer);
        }

        [Fact]
        public void SurveyElements_ShouldUseAnAnswerFromTheRightSelfAssessment()
        {
            GivenABasicSelfAssessment(3, 3);

            // Given the user has two answers,
            // but only the old answer has the right self assessment,
            // and it has subAnswers,
            GivenAnswerHasADifferentSelfAssessment(newAnswer);
            GivenAnswerIsForSelfAssessment(oldAnswer);
            GivenOnlyDetailedAnswersExistIn(oldAnswer);

            // Expect the right answer's subAnswers to be found, 
            // not the wrong answer's null subAnswers.
            var surveyElements = target.SurveyElements(coreSkill.Object, user.Object);
            Assert.NotNull(surveyElements[0].SubAnswer);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(7)]
        public void SurveyElements_ShouldIncludeTextElements(int nTextElements)
        {
            // Given the self assessment has various kinds of elements.
            GivenASelfAssessmentWithTextElements(nTextElements);

            // Given the user has an answer with TextAnswers.
            GivenAnswerIsForSelfAssessment(newAnswer);
            GivenOnlyTextAnswersExistIn(newAnswer);

            // Expect the text elements and answers to be found.
            var surveyElements = target.SurveyElements(coreSkill.Object, user.Object);
            var textElements = surveyElements.Where(se => se.Element is TextElement);
            Assert.Equal(nTextElements, textElements.Count());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(7)]
        public void SurveyElements_ShouldIncludeSimplifiedElements(int nSimplifiedElements)
        {
            // Given the self assessment has various kinds of elements.
            GivenASelfAssessmentWithSimplifiedElements(nSimplifiedElements);

            // Given the user has an answer with SimplifiedAnswers.
            GivenAnswerIsForSelfAssessment(newAnswer);
            GivenOnlySimplifiedAnswersExistIn(newAnswer);

            // Expect the simplified elements and answers to be found.
            var surveyElements = target.SurveyElements(coreSkill.Object, user.Object);
            var simplifiedElements = surveyElements.Where(se => se.Element is SimplifiedElement);
            Assert.Equal(nSimplifiedElements, simplifiedElements.Count());
        }

        [Fact]
        public void SaveDetailedAnswer_ShouldWorkWithAnExistingAnswer()
        {
            GivenABasicSelfAssessment(1, 1);

            // Given that the detailedElement can be found,
            var coreSkillElementDbSet = new Mock<DbSet<CoreSkillElement>>();
            db.SetupGet(db => db.CoreSkillElements).Returns(coreSkillElementDbSet.Object);
            coreSkillElementDbSet.Setup(cses => cses.Find(detailedElements[0].Id))
                .Returns(detailedElements[0]);

            // Given that the detailedOption can be found,
            var detailedOptionsDbSet = new Mock<DbSet<DetailedOption>>();
            db.SetupGet(db => db.DetailedOptions).Returns(detailedOptionsDbSet.Object);
            detailedOptionsDbSet.Setup(dos => dos.Find(options[0].Id))
                .Returns(options[0]);

            // Given that a sub answer exists in the answer,
            var subAnswer = new Mock<DetailedAnswer>();
            var subAnswers = new List<SubAnswer>();
            subAnswers.Add(subAnswer.Object);
            newAnswer.SetupGet(a => a.SubAnswers).Returns(subAnswers);

            // Given that the sub answer is for the detailedElement,
            subAnswer.SetupGet(sa => sa.Element).Returns(detailedElements[0]);

            // Given that the choice of the sub answer can be saved,
            subAnswer.SetupProperty(sa => sa.Choice);

            // When the detailed answer is saved,
            target.SaveDetailedAnswer(newAnswer.Object, detailedElements[0].Id, options[0].Id);

            // Then the existing answer should have its choice set and validated,
            Assert.Equal(options[0], subAnswer.Object.Choice);

            // Then the database changes should at least have been saved.
            db.Verify(db => db.SaveChanges());
        }

        [Fact]
        public void SaveTextAnswer_ShouldWorkWithAnExistingAnswer()
        {
            GivenASelfAssessmentWithTextElements(1);

            // Given that the textElement can be found,
            var coreSkillElementDbSet = new Mock<DbSet<CoreSkillElement>>();
            db.SetupGet(db => db.CoreSkillElements).Returns(coreSkillElementDbSet.Object);
            coreSkillElementDbSet.Setup(cses => cses.Find(textElements[0].Id))
                .Returns(textElements[0]);

            // Given that a sub answer exists in the answer,
            var subAnswer = new Mock<TextAnswer>();
            var subAnswers = new List<SubAnswer>();
            subAnswers.Add(subAnswer.Object);
            newAnswer.SetupGet(a => a.SubAnswers).Returns(subAnswers);

            // Given that the sub answer is for the textElement,
            subAnswer.SetupGet(sa => sa.Element).Returns(textElements[0]);

            // When the text answer is saved,
            var text = "some textual answer";
            target.SaveTextAnswer(newAnswer.Object, textElements[0].Id, text);

            // Then the existing answer should have its text set and validated,
            Assert.Equal(text, subAnswer.Object.Text);

            // Then the database changes should at least have been saved.
            db.Verify(db => db.SaveChanges());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void SaveSimplifiedAnswer_ShouldWorkWithAnExistingAnswer(int choice)
        {
            GivenASelfAssessmentWithSimplifiedElements(1);

            // Given that the simplifiedElement can be found,
            var coreSkillElementDbSet = new Mock<DbSet<CoreSkillElement>>();
            db.SetupGet(db => db.CoreSkillElements).Returns(coreSkillElementDbSet.Object);
            coreSkillElementDbSet.Setup(cses => cses.Find(simplifiedElements[0].Id))
                .Returns(simplifiedElements[0]);

            // Given that a sub answer exists in the answer,
            var subAnswer = new Mock<SimplifiedAnswer>();
            var subAnswers = new List<SubAnswer>();
            subAnswers.Add(subAnswer.Object);
            newAnswer.SetupGet(a => a.SubAnswers).Returns(subAnswers);

            // Given that the sub answer is for the simplifiedElement,
            subAnswer.SetupGet(sa => sa.Element).Returns(simplifiedElements[0]);

            // When the simplified answer is saved,
            target.SaveSimplifiedAnswer(newAnswer.Object, simplifiedElements[0].Id, choice);

            // Then the existing answer should have its choice set and validated,
            Assert.Equal(choice, subAnswer.Object.Choice);

            // Then the database changes should at least have been saved.
            db.Verify(db => db.SaveChanges());
        }

        [Fact]
        public void CoreSkillIsFinishedOnlyWhenAllElementsHaveAnswers()
        {
            GivenASelfAssessmentWithSimplifiedElements(1);
         
            // Given that a sub answer exists in the answer,
            var subAnswer = new Mock<SimplifiedAnswer>();
            subAnswer.SetupGet(sa => sa.Element).Returns(simplifiedElements[0]);
            var subAnswers = new List<SubAnswer>();
            subAnswers.Add(subAnswer.Object);
            newAnswer.SetupGet(a => a.SubAnswers).Returns(subAnswers);
            newAnswer.SetupGet(t => t.SelfAssessment).Returns(coreSkill.Object.SelfAssessment);            
           
            var answers = new List<Answer>();
            answers.Add(newAnswer.Object);
            user.SetupGet(t => t.Answers).Returns(answers);
            subAnswer.SetupGet(t => t.IsFinished).Returns(true);

            Assert.True(target.IsFinished(coreSkill.Object,user.Object));
        }

        [Fact]
        public void CoreSkillIsNotFinishedWhenSomeElementsHaveNoAnswers()
        {
            // Given that two sub answer exists in the answer,
            GivenASelfAssessmentWithSimplifiedElements(2);
            
            var subAnswer1 = new Mock<SimplifiedAnswer>();
            subAnswer1.SetupGet(sa => sa.Element).Returns(simplifiedElements[0]);

            var subAnswer2 = new Mock<SimplifiedAnswer>();
            subAnswer2.SetupGet(sa => sa.Element).Returns(simplifiedElements[1]);

            var subAnswers = new List<SubAnswer>();
            subAnswers.Add(subAnswer1.Object);
            subAnswers.Add(subAnswer2.Object);
            newAnswer.SetupGet(a => a.SubAnswers).Returns(subAnswers);
            newAnswer.SetupGet(t => t.SelfAssessment).Returns(coreSkill.Object.SelfAssessment);

            var answers = new List<Answer>();
            answers.Add(newAnswer.Object);
            user.SetupGet(t => t.Answers).Returns(answers);
            subAnswer1.SetupGet(t => t.IsFinished).Returns(true);
            subAnswer2.SetupGet(t => t.IsFinished).Returns(false);

            Assert.True(!(target.IsFinished(coreSkill.Object, user.Object)));
        }

        [Fact]
        public void CoreSkillIsNotFinishedWhenNoElementsHasAnswers()
        {
            // Given that a sub answer exists in the answer,
            GivenASelfAssessmentWithSimplifiedElements(1);
            var subAnswer = new Mock<SimplifiedAnswer>();
            subAnswer.SetupGet(sa => sa.Element).Returns(simplifiedElements[0]);
            var subAnswers = new List<SubAnswer>();
            subAnswers.Add(subAnswer.Object);
            newAnswer.SetupGet(a => a.SubAnswers).Returns(subAnswers);
            newAnswer.SetupGet(t => t.SelfAssessment).Returns(coreSkill.Object.SelfAssessment);

            var answers = new List<Answer>();
            answers.Add(newAnswer.Object);
            user.SetupGet(t => t.Answers).Returns(answers);
            subAnswer.SetupGet(t => t.IsFinished).Returns(false);

            Assert.True(!(target.IsFinished(coreSkill.Object, user.Object)));
        }

        /*************************** Private helper methods *******************************************/


        private void GivenASelfAssessmentWithTextElements(int nElements)
        {
            // Given the core skill has a self assessment,
            coreSkill.SetupGet(cs => cs.SelfAssessment)
                .Returns(selfAssessment.Object);

            // Given the core skill has text elements only.
            textElements.SetUpTextElements(nElements);
            coreSkill.SetupGet(cs => cs.Elements).Returns(
                    textElements.OfType<CoreSkillElement>().ToList());
        }

        private void GivenASelfAssessmentWithSimplifiedElements(int nElements)
        {
            // Given the core skill has a self assessment,
            coreSkill.SetupGet(cs => cs.SelfAssessment)
                .Returns(selfAssessment.Object);

            // Given the core skill has simplified elements only.
            simplifiedElements.SetUpSimplifiedElements(nElements);
            coreSkill.SetupGet(cs => cs.Elements).Returns(
                    simplifiedElements.OfType<CoreSkillElement>().ToList());
        }

        private void GivenAnswerIsForSelfAssessment(Mock<Answer> answerMock)
        {
            answers.Add(answerMock.Object);
            answerMock.SetupGet(a => a.SelfAssessment).Returns(selfAssessment.Object);
        }

        private void GivenAnswerHasADifferentSelfAssessment(Mock<Answer> answerMock)
        {
            answers.Add(answerMock.Object);
            answerMock.SetupGet(a => a.SelfAssessment).Returns(new Mock<SelfAssessment>(RIGHT_ID + 1).Object);
        }

        private void GivenOnlyDetailedAnswersExistIn(Mock<Answer> answerMock)
        {
            detailedAnswers.SetUpDetailedAnswers(detailedElements, options);
            answerMock.SetupGet(a => a.SubAnswers)
                .Returns(detailedAnswers.Cast<SubAnswer>().ToList());
        }

        private void GivenOnlyTextAnswersExistIn(Mock<Answer> answerMock)
        {
            textAnswers.SetUpTextAnswers(textElements);
            answerMock.SetupGet(a => a.SubAnswers)
                .Returns(textAnswers.Cast<SubAnswer>().ToList());
        }

        private void GivenOnlySimplifiedAnswersExistIn(Mock<Answer> answerMock)
        {
            simplifiedAnswers.SetUpSimplifiedAnswers(simplifiedElements);
            answerMock.SetupGet(a => a.SubAnswers)
                .Returns(simplifiedAnswers.Cast<SubAnswer>().ToList());
        }

        private void GivenABasicSelfAssessment(int nElements, int nOptions)
        {
            // Given the core skill has a self assessment,
            coreSkill.SetupGet(cs => cs.SelfAssessment)
                .Returns(selfAssessment.Object);

            // Given the core skill has nElements detailed elements.
            options.SetUpOptions(nElements, nOptions);
            detailedElements.SetUpDetailedElements(nElements, nOptions, options);
            coreSkill.SetupGet(cs => cs.Elements).Returns(
                detailedElements.OfType<CoreSkillElement>().ToList());
        }

        private void GivenASelfAssessmentWithCoreSkills(int nCoreSkills)
        {
            for (int i = 0; i < nCoreSkills; i++)
            {
                Mock<CoreSkill> coreSkill = new Mock<CoreSkill>("Gudness", "How good are you?",
                null as ICollection<CoreSkillElement>);
                coreSkill.SetupGet(cs => cs.SelfAssessment).Returns(selfAssessment.Object);
                coreSkills.Add(coreSkill.Object);
            }
            selfAssessment.SetupGet(sa => sa.CoreSkills).Returns(coreSkills);
        }

        private void SetupSelfAssessmentDbSet()
        {
            // Given that the SelfAssessments can be found,
            var selfAssessmentDbSet = new Mock<DbSet<SelfAssessment>>();
            db.SetupGet(db => db.SelfAssessments).Returns(selfAssessmentDbSet.Object);
            selfAssessmentDbSet.Setup(sas => sas.Find(RIGHT_ID))
               .Returns(selfAssessment.Object);
        }
    }
}

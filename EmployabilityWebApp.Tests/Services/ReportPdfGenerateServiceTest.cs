using System.Collections.Generic;
using System.Linq;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.Services;
using EmployabilityWebApp.Tests.TestHelpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Moq;
using Xunit;

namespace EmployabilityWebApp.Tests.Services
{
    /// <summary>
    /// Unit tests for ReportPdfGenerateService
    /// </summary>
    public class ReportPdfGenerateServiceTest
    {
        protected readonly ReportPdfGenerateService Target;

        protected readonly Mock<Document> ReportDocument = 
            new Mock<Document>(PageSize.A4, 50, 50, 25, 25);

        protected readonly Mock<IReportPdfHelper> ReportPdfHelper = 
            new Mock<IReportPdfHelper>();

        private readonly Mock<Answer> _answer = new Mock<Answer>();
        private readonly Mock<BasicUser> _user = new Mock<BasicUser>();
        protected readonly List<Answer> Answers = new List<Answer>();
        private List<SubAnswer> _subAnswers = new List<SubAnswer>();

        private readonly Mock<AdminUser> _admin = new Mock<AdminUser>();

        private readonly List<SelfAssessment> _selfAssessments =
            new List<SelfAssessment>();

        private const int SelfAssessmentId = 123;
        protected readonly Mock<SelfAssessment> SelfAssessment = 
            new Mock<SelfAssessment>(SelfAssessmentId);

        protected readonly List<CoreSkill> CoreSkills = 
            new List<CoreSkill>();

        protected readonly Mock<CoreSkill> CoreSkill = new Mock<CoreSkill>();
        private List<CoreSkillElement> _elements = new List<CoreSkillElement>();

        private readonly List<DetailedAnswer> _detailedAnswers =
           new List<DetailedAnswer>();
        private readonly List<DetailedElement> _detailedElements =
            new List<DetailedElement>();
        private readonly List<DetailedOption> _options =
            new List<DetailedOption>();

        protected List<SimplifiedAnswer> SimplifiedAnswers =
            new List<SimplifiedAnswer>();

        protected readonly List<SimplifiedElement> SimplifiedElements =
            new List<SimplifiedElement>();

        public ReportPdfGenerateServiceTest()
        {
            Target = new ReportPdfGenerateService()
            {
                ReportDocument = ReportDocument.Object,
                ReportPdfHelper = ReportPdfHelper.Object
            };
            _admin.Setup(a => a.Id);
        }

        [Fact]
        public void GenerateSurveyReport_ItShouldOpenAndCloseTheDocumentObject()
        {
            // Given a user and an answer
            GivenABasicUsersAnswerForSelfAssessment();
            // Generate survey report
            Target.GenerateSurveyReport(_answer.Object, _user.Object);
            // Verify
            ReportDocument.Verify(rd => rd.Open(), Times.Once);
            ReportDocument.Verify(rd => rd.Close(), Times.Once);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void GenerateSummary_ItShouldGenerateSummaryTitleOnce(int nAnswers)
        {
            // Given a user and an answer
            GivenABasicUsersAnswerForSelfAssessment();
            // Given a self assessment with core skill
            GivenAnAnswerWithSimplifiedSubAnswers(nAnswers);
            // Generate survey report
            Target.GenerateSurveyReport(_answer.Object, _user.Object);
            // Verify
            ReportPdfHelper.Verify(rph =>
                rph.GenerateSectionTitle("1. Summary of your self assessment"),
                    Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void GenerateSummary_ItShouldGenerateSummaryImage(int nAnswers)
        {
            // Given a user and an answer
            GivenABasicUsersAnswerForSelfAssessment();
            // Given An Answer WithSimplifiedSubAnswers
            GivenAnAnswerWithSimplifiedSubAnswers(nAnswers);
            // Generate survey report
            Target.GenerateSurveyReport(_answer.Object, _user.Object);
            // Verify
            ReportPdfHelper.Verify(rph => 
                rph.GenerateSummaryChart(SimplifiedAnswers, CoreSkills), Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void GenerateSummary_ItShouldGenerateCoreSkillLinks(int nAnswers)
        {
            // Given a user and an answer
            GivenABasicUsersAnswerForSelfAssessment();
            // Given a self assessment with core skill
            GivenAnAnswerWithSimplifiedSubAnswers(nAnswers);
            // Generate survey report
            Target.GenerateSurveyReport(_answer.Object, _user.Object);
            // Verify
            ReportPdfHelper.Verify(rph =>
                rph.GenerateCoreSkillLinks(CoreSkills, _user.Object.Id), Times.Once);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 0)]
        [InlineData(0, 10, 0)]
        [InlineData(0, 0, 3)]
        [InlineData(0, 0, 5)]
        [InlineData(0, 1, 3)]
        [InlineData(0, 10, 3)]
        [InlineData(0, 1, 5)]
        [InlineData(0, 10, 5)]
        [InlineData(1, 0, 0)]
        [InlineData(1, 1, 0)]
        [InlineData(1, 10, 0)]
        [InlineData(1, 0, 3)]
        [InlineData(1, 0, 5)]
        [InlineData(1, 1, 3)]
        [InlineData(1, 10, 3)]
        [InlineData(1, 1, 5)]
        [InlineData(1, 10, 5)]
        [InlineData(10, 0, 0)]
        [InlineData(10, 1, 0)]
        [InlineData(10, 10, 0)]
        [InlineData(10, 0, 3)]
        [InlineData(10, 0, 5)]
        [InlineData(10, 1, 3)]
        [InlineData(10, 10, 3)]
        [InlineData(10, 1, 5)]
        [InlineData(10, 10, 5)]
        public void ListCoreSkillResult_ItShouldGenerateCoreSkilTitle(int nCoreSkills, int nAnswers, int nOptions)
        {
            // Given a user and an answer
            GivenABasicUsersAnswerForSelfAssessment();
            // Given a self assessment with core skill
            GivenAnAnswerWithMixedSubAnswers(nCoreSkills, nAnswers, nOptions);
            // Generate survey report
            Target.GenerateSurveyReport(_answer.Object, _user.Object);
            // Verify
            foreach (var coreSkill in CoreSkills)
            {
                ReportPdfHelper.Verify(rph =>
                    rph.GenerateTitleWithDescription(coreSkill.Name, coreSkill.Description, _user.Object.Id), 
                        Times.Once);
            }
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 0)]
        [InlineData(0, 10, 0)]
        [InlineData(0, 0, 3)]
        [InlineData(0, 0, 5)]
        [InlineData(0, 1, 3)]
        [InlineData(0, 10, 3)]
        [InlineData(0, 1, 5)]
        [InlineData(0, 10, 5)]
        [InlineData(1, 0, 0)]
        [InlineData(1, 1, 0)]
        [InlineData(1, 10, 0)]
        [InlineData(1, 0, 3)]
        [InlineData(1, 0, 5)]
        [InlineData(1, 1, 3)]
        [InlineData(1, 10, 3)]
        [InlineData(1, 1, 5)]
        [InlineData(1, 10, 5)]
        [InlineData(10, 0, 0)]
        [InlineData(10, 1, 0)]
        [InlineData(10, 10, 0)]
        [InlineData(10, 0, 3)]
        [InlineData(10, 0, 5)]
        [InlineData(10, 1, 3)]
        [InlineData(10, 10, 3)]
        [InlineData(10, 1, 5)]
        [InlineData(10, 10, 5)]
        public void ListCoreSkillResult_MustAddDetialedAnswerTable(int nCoreSkills, int nAnswers, int nOptions)
        {
            // Given a user and an answer
            GivenABasicUsersAnswerForSelfAssessment();

            // The answer includes nAnswers number of mix types of answer
            GivenAnAnswerWithMixedSubAnswers(nCoreSkills, nAnswers, nOptions);

            // List results
            Target.GenerateSurveyReport(_answer.Object, _user.Object);

            // Must try to add answer cells once
            foreach (var detailedElement in _detailedElements)
            {
                ReportPdfHelper.Verify(
                    rph => rph.AddDetailedAnswerTable(_answer.Object, nOptions, 
                        CoreSkill.Object, detailedElement), Times.Once);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void GenerateSummaryReport_ItShouldOpenAndCloseTheDocumentObject(int nSelfAssessment)
        {
            // Given a list of self assessments and answers
            GivenAListOfSelfAssessmentsAndAnswers(nSelfAssessment);
            // Generate survey report
            Target.GenerateSummaryReport(_selfAssessments, Answers, _admin.Object);
            // Verify
            ReportDocument.Verify(rd => rd.Open(), Times.Once);
            ReportDocument.Verify(rd => rd.Close(), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void GenerateSummaryReport_ItShouldGenerateSummaryForEachSelfAssessment(int nAnswer)
        {
            // Given a list of self assessments with corresponding answers
            GivenAListOfSelfAssessmentsAndAnswers(nAnswer);
            // Generate survey report
            Target.GenerateSummaryReport(_selfAssessments, Answers, _admin.Object);
            // Verify
            foreach (var selfAssessment in _selfAssessments)
            {
                ReportPdfHelper.Verify(rph =>
                    rph.GenerateSectionTitle($"Survey Name: {selfAssessment.Title}"),
                        Times.Once);
            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 0)]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        public void GenerateSaSummary_ItShouldGenerateSummaryReportForAllSelfAssessments(int nAnswers,
            int nSimplifiedAnswers)
        {
            // Given a list of self assessments with simplified answers
            GivenAListOfAnswersWithSimplifiedSubAnswers(nAnswers, nSimplifiedAnswers);
            // Generate summary report
            Target.GenerateSummaryReport(_selfAssessments, Answers, _admin.Object);
            var saAnswers = Answers
                .Where(a => a.SelfAssessment.Id == SelfAssessment.Object.Id)
                .OrderBy(a => a.TimeStamp)
                .ToList();
            var simplifiedAnswerList = new List<SimplifiedAnswer>();
            simplifiedAnswerList = saAnswers.Aggregate(simplifiedAnswerList,
                (current, answer) =>
                    current.Concat(answer.SubAnswers.OfType<SimplifiedAnswer>().ToList()).ToList());
            // Verify
            if (saAnswers.Count == 0)
            {
                ReportPdfHelper.Verify(rph =>
                    rph.GenerateSummaryChart(simplifiedAnswerList, CoreSkills), Times.Never);
            }
            else
            {
                ReportPdfHelper.Verify(rph =>
                    rph.GenerateSummaryChart(simplifiedAnswerList, CoreSkills), Times.AtLeastOnce);
            }
            
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 0)]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        public void GenerateSaSummary_ItShouldGenerateAnswerLinks(int nAnswers,
            int nSimplifiedAnswers)
        {
            // Given a list of self assessments with simplified answers
            GivenAListOfAnswersWithSimplifiedSubAnswers(nAnswers, nSimplifiedAnswers);
            // Generate summary report
            Target.GenerateSummaryReport(_selfAssessments, Answers, _admin.Object);
            var saAnswers = Answers
                .Where(a => a.SelfAssessment.Id == SelfAssessment.Object.Id)
                .OrderBy(a => a.TimeStamp)
                .ToList();
            // Verify
            if (saAnswers.Count == 0)
            {
                ReportPdfHelper.Verify(rph =>
                    rph.GenerateAnswerLinks(saAnswers), Times.Never);
            }
            else
            {
                ReportPdfHelper.Verify(rph =>
                    rph.GenerateAnswerLinks(saAnswers), Times.Once);
            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 0)]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        public void GenerateSaSummary_ItShouldResultForEachAnswer(int nAnswers,
            int nSimplifiedAnswers)
        {
            // Given a list of self assessments with simplified answers
            GivenAListOfAnswersWithSimplifiedSubAnswers(nAnswers, nSimplifiedAnswers);
            // Generate summary report
            Target.GenerateSummaryReport(_selfAssessments, Answers, _admin.Object);
            var saAnswers = Answers
                .Where(a => a.SelfAssessment.Id == SelfAssessment.Object.Id)
                .OrderBy(a => a.TimeStamp)
                .ToList();
            // Verify
            foreach (var saAnswer in saAnswers)
            {
                ReportPdfHelper.Verify(rph =>
                    rph.GenerateTitleWithDescription("Answer from",
                        $"{saAnswer.BasicUser.GivenName} {saAnswer.BasicUser.FamilyName}", 
                        saAnswer.BasicUser.Id), Times.Once);
            }
        }
        
        /// Provide a user and an corresponding answer to the self assessment
        private void GivenABasicUsersAnswerForSelfAssessment()
        {
            _user.Setup(u => u.Id);
            _answer.SetupGet(a => a.SelfAssessment).Returns(SelfAssessment.Object);
            _answer.SetupGet(a => a.BasicUser).Returns(_user.Object);
            Answers.Add(_answer.Object);
            _user.SetupGet(u => u.Answers).Returns(Answers);
        }
        
        /// Provide an answer with answers for simplified elements
        private void GivenAnAnswerWithSimplifiedSubAnswers(int nAnswers)
        {
            CoreSkill.SetupGet(c => c.SelfAssessment).Returns(SelfAssessment.Object);
            SimplifiedElements.SetUpSimplifiedElements(nAnswers);
            SimplifiedAnswers.SetUpSimplifiedAnswers(SimplifiedElements);

            _answer.SetupGet(a => a.SubAnswers)
                .Returns(SimplifiedAnswers.OfType<SubAnswer>().ToList());

            CoreSkill.SetupGet(c => c.Elements)
                .Returns(SimplifiedElements.OfType<CoreSkillElement>().ToList());
            CoreSkills.Add(CoreSkill.Object);
            SelfAssessment.SetupGet(s => s.CoreSkills).Returns(CoreSkills);
        }

        /// Provide an answer with mix types of elements
        private void GivenAnAnswerWithMixedSubAnswers(int nCoreSkills, int nAnswers, int nOptions)
        {
            CoreSkills.SetUpCoreSkills(nCoreSkills);
            CoreSkill.SetupGet(c => c.SelfAssessment).Returns(SelfAssessment.Object);

            SimplifiedElements.SetUpSimplifiedElements(nAnswers);
            SimplifiedAnswers.SetUpSimplifiedAnswers(SimplifiedElements);

            _options.SetUpOptions(nAnswers, nOptions);
            _detailedElements.SetUpDetailedElements(nAnswers, nOptions, _options);
            _detailedAnswers.SetUpDetailedAnswers(_detailedElements, _options);

            _subAnswers = _subAnswers.Concat(SimplifiedAnswers.OfType<SubAnswer>().ToList()).ToList();
            _subAnswers = _subAnswers.Concat(_detailedAnswers.OfType<SubAnswer>().ToList()).ToList();

            _elements = _elements.Concat(SimplifiedElements.OfType<CoreSkillElement>().ToList()).ToList();
            _elements = _elements.Concat(_detailedElements.OfType<CoreSkillElement>().ToList()).ToList();

            _answer.SetupGet(a => a.SubAnswers).Returns(_subAnswers);
            CoreSkill.SetupGet(c => c.Elements)
                .Returns(_detailedElements.OfType<CoreSkillElement>().ToList());
            CoreSkills.Add(CoreSkill.Object);
            SelfAssessment.SetupGet(s => s.CoreSkills).Returns(CoreSkills);
        }

        /// Provide a list of self assessments with corresponding answers
        private void GivenAListOfSelfAssessmentsAndAnswers(int nSelfAssessment)
        {
            _selfAssessments.SetUpSelfAssessments(nSelfAssessment);
            Answers.SetUpAnswersWithSelfAssessments(nSelfAssessment, _selfAssessments);
        }

        /// Provide a list of answers for a self assessment with simplified answers
        private void GivenAListOfAnswersWithSimplifiedSubAnswers(int nAnswers, int nSimplifiedAnswers)
        {
            SimplifiedElements.SetUpSimplifiedElements(nSimplifiedAnswers);
            SimplifiedAnswers.SetUpSimplifiedAnswers(SimplifiedElements);

            Answers.SetUpAnswersWithSimplifiedAnswers(nAnswers, SimplifiedAnswers, SelfAssessment);

            CoreSkill.SetupGet(c => c.Elements)
                .Returns(SimplifiedElements.OfType<CoreSkillElement>().ToList());
            CoreSkill.SetupGet(c => c.SelfAssessment).Returns(SelfAssessment.Object);
            CoreSkills.Add(CoreSkill.Object);

            SelfAssessment.SetupGet(sa => sa.Answers).Returns(Answers);
            SelfAssessment.SetupGet(sa => sa.CoreSkills).Returns(CoreSkills);
            _selfAssessments.Add(SelfAssessment.Object);
        }
    }
}
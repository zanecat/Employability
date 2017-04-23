using System;
using System.Collections.Generic;
using System.IO;
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
    /// Unit tests for ReportPdfHelper
    /// </summary>
    public class ReportPdfHelperTest
    {
        private readonly ReportPdfHelper _target;

        private readonly Mock<IReportChartService> _reportChartService = 
            new Mock<IReportChartService>();

        private readonly Mock<BasicUser> _user = new Mock<BasicUser>();
        private readonly Mock<Answer> _answer = new Mock<Answer>(1, DateTime.Now);
        private readonly List<Answer> _answers = new List<Answer>();
        
        private readonly List<CoreSkill> _coreSkills =
            new List<CoreSkill>();
        private readonly Mock<CoreSkill> _coreSkill = new Mock<CoreSkill>();

        private readonly List<DetailedAnswer> _detailedAnswers =
           new List<DetailedAnswer>();
        private readonly List<DetailedElement> _detailedElements =
            new List<DetailedElement>();
        private readonly List<DetailedOption> _options =
            new List<DetailedOption>();

        private readonly List<SimplifiedAnswer> _simplifiedAnswers =
            new List<SimplifiedAnswer>();
        private readonly List<SimplifiedElement> _simplifiedElements =
            new List<SimplifiedElement>();

        private readonly List<TextAnswer> _textAnswers =
            new List<TextAnswer>();
        private readonly List<TextElement> _textElements =
            new List<TextElement>();

        private readonly Font _titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
        private readonly Font _subTitleFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
        private readonly Font _boldFont = FontFactory.GetFont("Arial", 10, Font.BOLD);
        private readonly Font _textFont = FontFactory.GetFont("Arial", 10, Font.NORMAL);
        private readonly Font _linkFont = FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLUE);
        
        public ReportPdfHelperTest()
        {
            _target = new ReportPdfHelper()
            {
                ReportChartService = _reportChartService.Object
            };
        }

        [Fact]
        public void ItShouldReturnCorrectFontObject()
        {
            Assert.Equal(_titleFont, _target.GetFont("TitleFont"));
            Assert.Equal(_subTitleFont, _target.GetFont("SubTitleFont"));
            Assert.Equal(_boldFont, _target.GetFont("BoldFont"));
            Assert.Equal(_textFont, _target.GetFont("TextFont"));
            Assert.Equal(_linkFont, _target.GetFont("LinkFont"));
            // If font doesn't exist, return default font - text font
            Assert.Equal(_textFont, _target.GetFont("InvalidFont"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(10)]
        public void LinkTableShouldHaveFourColumnsAndIncludeAllCoreSkills(int nCoreSkills)
        {
            // Get a list of core skills
            GivenAListOfCoreSkills(nCoreSkills);
            // Generate link table
            var linkTable = _target.GenerateCoreSkillLinks(_coreSkills, _user.Object.Id);
            // Check columns
            Assert.Equal(4, linkTable.NumberOfColumns);
            // Check rows
            Assert.Equal(nCoreSkills == 0 ? 0 : 
                (nCoreSkills % 4 == 0 ? (nCoreSkills / 4) : (nCoreSkills / 4 + 1)), 
                    linkTable.Rows.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void SimplifiedAnswerTableShouldHaveSameNumberOfRowsWithSimplifiedElements(int nAnswers)
        {
            // Get an answer with a list of simplified answers
            GivenAnAnswerWithSimplifiedSubAnswers(nAnswers);
            // Generate result table for simplified elements
            var resultTable = new PdfPTable(3);
            _target.AddSimplifiedAnswerCells(_answer.Object, _simplifiedElements, resultTable);
            // Verify number of rows
            Assert.Equal(_simplifiedElements.Count, resultTable.Rows.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void TextAnswerTableShouldHaveSameNumberOfRowsWithIsolatedTextElements(int nAnswers)
        {
            // Get an answer with a list of simplified answers
            GivenAnAnswerWithTextSubAnswers(nAnswers);
            // Generate result table for simplified elements
            var resultTable = new PdfPTable(3);
            _target.AddTextAnswerCells(_answer.Object, _coreSkill.Object, resultTable);
            // Verify number of rows
            Assert.Equal(_textElements.Count, resultTable.Rows.Count);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(0, 5)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 5)]
        [InlineData(10, 0)]
        [InlineData(10, 1)]
        [InlineData(10, 5)]
        public void DetailedAnswerTableShouldHaveTwiceNumberOfRowsWithIsolatedDetailedElements(int nAnswers, int nOptions)
        {
            // Get an answer with a list of simplified answers
            GivenAnAnswerWithDetailedSubAnswers(nAnswers, nOptions);
            // Generate result table for simplified elements
            foreach (var detailedElement in _detailedElements)
            {
                var detailedTable = _target.AddDetailedAnswerTable(_answer.Object, nOptions, _coreSkill.Object, detailedElement);
                // Verify number of columns
                Assert.Equal((nOptions == 0 ? 5 : nOptions), detailedTable.NumberOfColumns);
                // Verify number of rows
                Assert.Equal((nOptions == 0 ? 1 : 2), detailedTable.Rows.Count);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(10)]
        public void AnswerLinkTableShouldHaveFourColumnsAndIncludeAllAnswers(int nAnswers)
        {
            // Get a list of core skills
            GivenAListOfAnswers(nAnswers);
            // Generate link table
            var linkTable = _target.GenerateAnswerLinks(_answers);
            // Check columns
            Assert.Equal(4, linkTable.NumberOfColumns);
            // Check rows
            Assert.Equal(nAnswers == 0 ? 0 :
                (nAnswers % 4 == 0 ? (nAnswers / 4) : (nAnswers / 4 + 1)),
                    linkTable.Rows.Count);
        }

        /// Provide a list of core skills
        private void GivenAListOfCoreSkills(int nCoreSkills)
        {
            _coreSkills.SetUpCoreSkills(nCoreSkills);
        }

        private void GivenAListOfAnswers(int nAnswers)
        {
            _answers.SetUpAnswersWithBasicUsers(nAnswers);
        }

        /// Provide an answer with answers for simplified elements
        private void GivenAnAnswerWithSimplifiedSubAnswers(int nAnswers)
        {
            _simplifiedElements.SetUpSimplifiedElements(nAnswers);
            _simplifiedAnswers.SetUpSimplifiedAnswers(_simplifiedElements);

            _answer.SetupGet(a => a.SubAnswers)
                .Returns(_simplifiedAnswers.OfType<SubAnswer>().ToList());

            _coreSkill.SetupGet(c => c.Elements)
                .Returns(_simplifiedElements.OfType<CoreSkillElement>().ToList());
        }

        /// Provide an answer with answers for simplified elements
        private void GivenAnAnswerWithTextSubAnswers(int nAnswers)
        {
            _textElements.SetUpTextElements(nAnswers);
            _textAnswers.SetUpTextAnswers(_textElements);

            _answer.SetupGet(a => a.SubAnswers)
                .Returns(_textAnswers.OfType<SubAnswer>().ToList());

            _coreSkill.SetupGet(c => c.Elements)
                .Returns(_textElements.OfType<CoreSkillElement>().ToList());
        }

        /// Provide an answer with answers for detailed elements
        private void GivenAnAnswerWithDetailedSubAnswers(int nAnswers, int nOptions)
        {
            _options.SetUpOptions(nAnswers, nOptions);
            _detailedElements.SetUpDetailedElements(nAnswers, nOptions, _options);
            _detailedAnswers.SetUpDetailedAnswers(_detailedElements, _options);

            _answer.SetupGet(a => a.SubAnswers)
                .Returns(_detailedAnswers.OfType<SubAnswer>().ToList());

            _coreSkill.SetupGet(c => c.Elements)
                .Returns(_detailedElements.OfType<CoreSkillElement>().ToList());
        }
    }
}

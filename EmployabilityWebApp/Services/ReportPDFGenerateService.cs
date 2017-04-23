using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmployabilityWebApp.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Services
{
    [ImplicitNotNull]
    public class ReportPdfGenerateService : IReportPdfGenerateService
    {
        // Make it public for testing
        public Document ReportDocument;

        // Make it public for testing
        public IReportPdfHelper ReportPdfHelper;

        private const int TableCellPadding = 7;

        public ReportPdfGenerateService()
        {
            // Initialise PDF document
            ReportDocument = new Document(PageSize.A4, 50, 50, 25, 25);
            // Initialise report PDF helper
            ReportPdfHelper = new ReportPdfHelper();
        }

        public MemoryStream GenerateSurveyReport(Answer latestAnswer, BasicUser user)
        {
            // Create a new PdfWrite object, writing the output to a MemoryStream
            var outputMemoryStream = new MemoryStream();
            PdfWriter.GetInstance(ReportDocument, outputMemoryStream);
            // Open the Document for writing
            ReportDocument.Open();
            // Write report content
            GenerateReportTitle(user);
            // Get core skills for the given selfassessment, generate summary section
            if (latestAnswer.SelfAssessment.CoreSkills != null)
            {
                GenerateSurveyResult(latestAnswer);
            }
            // Close the Document after writing
            ReportDocument.Close();
            return outputMemoryStream;
        }

        public MemoryStream GenerateSummaryReport(ICollection<SelfAssessment> selfAssessments
            , ICollection<Answer> answers, AdminUser user)
        {
            // Create a new PdfWrite object, writing the output to a MemoryStream
            var outputMemoryStream = new MemoryStream();
            PdfWriter.GetInstance(ReportDocument, outputMemoryStream);
            // Open the Document for writing
            ReportDocument.Open();
            // Write report content
            GenerateReportTitle(user);
            // For each self assessment, generate a report like result report
            foreach (var selfAssessment in selfAssessments)
            {
                // Add self assessment title
                ReportDocument.Add(
                    ReportPdfHelper.GenerateSectionTitle($"Survey Name: {selfAssessment.Title}"));
                // Get answers for current report
                var saAnswers = answers
                    .Where(a => a.SelfAssessment.Id == selfAssessment.Id)
                    .OrderBy(sa => sa.TimeStamp)
                    .ToList();
                // If core skills exist in the self assessment 
                // and there are some answers in the self assessment,
                // generate result report based on each answer
                if (selfAssessment.CoreSkills == null || saAnswers.Count == 0)
                {
                    continue;
                }
                // Add self assessment summary section
                GenerateSaSummary(selfAssessment, saAnswers);
                // List result for each answer
                foreach (var saAnswer in saAnswers)
                {
                    // Add answer title with user information
                    ReportDocument.Add(ReportPdfHelper.GenerateTitleWithDescription("Answer from",
                        $"{saAnswer.BasicUser.GivenName} {saAnswer.BasicUser.FamilyName}", saAnswer.BasicUser.Id));
                    // Get result
                    GenerateSurveyResult(saAnswer);
                }
            }
            // Close the Document after writing
            ReportDocument.Close();
            return outputMemoryStream;
        }

        /// <summary>
        /// Generate title section for the report, 
        /// including title, date, user's full name and user's email
        /// </summary>
        /// <param name="user">Current logged in user</param>
        private void GenerateReportTitle(ApplicationUser user)
        {
            // Add report title
            var titleParagraph = new Paragraph("My Employability Report", 
                ReportPdfHelper.GetFont("TitleFont"))
            {
                SpacingAfter = TableCellPadding
            };
            ReportDocument.Add(titleParagraph);

            // Add report generation date
            var dateParagraph = new Paragraph($"Date: {DateTime.Now.ToLongDateString()}",
                ReportPdfHelper.GetFont("SubTitleFont"));
            ReportDocument.Add(dateParagraph);

            if (user.GetType().FullName == "BasicUser")
            {
                var basicUser = user as BasicUser;
                if (basicUser != null)
                {
                    //Add student's name
                    var nameParagraph = new Paragraph("")
                    {
                        new Phrase("Name: ", ReportPdfHelper.GetFont("BoldFont")),
                        new Phrase($"{basicUser.GivenName} {basicUser.FamilyName}", ReportPdfHelper.GetFont("TextFont"))
                    };
                    ReportDocument.Add(nameParagraph);
                }
            }
            
            //Add student's email
            var emailParagraph = new Paragraph("")
            {
                new Phrase("Email: ", ReportPdfHelper.GetFont("BoldFont")),
                new Phrase(user.Email, ReportPdfHelper.GetFont("TextFont"))
            };
            ReportDocument.Add(emailParagraph);
        }

        /// <summary>
        /// Generate result report for a single answer
        /// </summary>
        /// <param name="answer">The chosen answer</param>
        private void GenerateSurveyResult(Answer answer)
        {
            // Get core skill list of the self assessment
            var coreSkillList = answer.SelfAssessment.CoreSkills.ToList();
            GenerateSummary(answer, coreSkillList);

            // List all the answered results for all the core skills
            foreach (var coreSkill in coreSkillList)
            {
                ListCoreSkillResult(coreSkill, answer);
            }
        }

        /// <summary>
        /// Generate summary section of the report,
        /// including title, summary chart and links to detailed core skill results
        /// </summary>
        /// <param name="latestAnswer">The user's latest answer for the chosen self assessment</param>
        /// <param name="coreSkillList">The list of core skills of the chosen self assessment</param>
        private void GenerateSummary(Answer latestAnswer, List<CoreSkill> coreSkillList)
        {
            // Add section title
            ReportDocument.Add(
                ReportPdfHelper.GenerateSectionTitle("1. Summary of your self assessment"));

            // Add summary chart
            var simplifiedAnswers = latestAnswer.SubAnswers.OfType<SimplifiedAnswer>().ToList();
            ReportDocument.Add(
                ReportPdfHelper.GenerateSummaryChart(simplifiedAnswers, coreSkillList));

            // Add sub-section and links to result part
            var subSectionHeader = new Paragraph("Results from each section", 
                ReportPdfHelper.GetFont("BoldFont"))
            {
                SpacingAfter = TableCellPadding
            };
            ReportDocument.Add(subSectionHeader);
            ReportDocument.Add(ReportPdfHelper.GenerateCoreSkillLinks(coreSkillList, latestAnswer.BasicUser.Id));
        }

        /// <summary>
        /// List the answers of a chosen core skill
        /// </summary>
        /// <param name="coreSkill">The given core skill</param>
        /// <param name="latestAnswer">The user's latest answer for the chosen self assessment</param>
        private void ListCoreSkillResult(CoreSkill coreSkill, Answer latestAnswer)
        {
            // Add title with description
            ReportDocument.Add(ReportPdfHelper.GenerateTitleWithDescription(coreSkill.Name, 
                coreSkill.Description, latestAnswer.BasicUser.Id));

            // If there is no element in the core skill, skip
            if (coreSkill.Elements == null)
            {
                return;
            }
            // Create table for the results
            var resultTable = new PdfPTable(3)
            {
                WidthPercentage = 90,
                HorizontalAlignment = 1,
                SpacingBefore = TableCellPadding,
                SpacingAfter = TableCellPadding
            };
            var columnWidths = new[] { 1f, 3f, 3f };
            resultTable.SetWidths(columnWidths);

            // Add simplified answer cells into the table
            ReportPdfHelper.AddSimplifiedAnswerCells(latestAnswer,
                coreSkill.Elements.OfType<SimplifiedElement>(), resultTable);
            // Add text answer cells into the table
            ReportPdfHelper.AddTextAnswerCells(latestAnswer, coreSkill, resultTable);
            // Add the table into the document
            ReportDocument.Add(resultTable);
            // Create a new table for displaying detailed answers.
            AddDetailedAnswerTables(coreSkill, latestAnswer);
        }

        /// <summary>
        /// Create a table to display the detailed answers for a chosen core skill
        /// </summary>
        /// <param name="coreSkill">The chosen core skill</param>
        /// <param name="latestAnswer">The user's latest answer for the chosen self assessment</param>
        private void AddDetailedAnswerTables(CoreSkill coreSkill, Answer latestAnswer)
        {
            foreach (var detailedElement in coreSkill.Elements.OfType<DetailedElement>()
                .Select((value, i) => new { i, value }))
            {
                // Generate the result table,
                // the width of the table depends on the amount of options of the detailed element
                var tableWidth = detailedElement.value.DetailedOptions.Count;

                // Add table title
                var tableTitle = new Paragraph($"Reflection {detailedElement.i + 1}", 
                    ReportPdfHelper.GetFont("BoldFont"))
                {
                    IndentationLeft = 20
                };
                ReportDocument.Add(tableTitle);

                // Add detailed option answers and corresponding text answer
                ReportDocument.Add(ReportPdfHelper.AddDetailedAnswerTable(latestAnswer, 
                    tableWidth, coreSkill, detailedElement.value));
            }
        }

        /// <summary>
        /// Add summary section for a given self assessment, 
        /// including a summary chart and links to each answer
        /// </summary>
        /// <param name="selfAssessment">The given self assessment</param>
        /// <param name="answers">The answers of the given self assessment</param>
        private void GenerateSaSummary(SelfAssessment selfAssessment, List<Answer> answers)
        {
            // Add section title
            ReportDocument.Add(
                ReportPdfHelper.GenerateSectionTitle("Summary"));
            // Get all simplified answers from self assessment's answers
            var simplifiedAnswers = new List<SimplifiedAnswer>();
            simplifiedAnswers = answers.Aggregate(simplifiedAnswers,
                (current, answer) => 
                    current.Concat(answer.SubAnswers.OfType<SimplifiedAnswer>().ToList()).ToList());
            // Add summary chart
            ReportDocument.Add(
                ReportPdfHelper.GenerateSummaryChart(simplifiedAnswers, selfAssessment.CoreSkills.ToList()));
            // Add sub-section and links to result part
            var subSectionHeader = new Paragraph("Results of each answer",
                ReportPdfHelper.GetFont("BoldFont"))
            {
                SpacingAfter = TableCellPadding
            };
            ReportDocument.Add(subSectionHeader);
            // Add answer link table
            ReportDocument.Add(ReportPdfHelper.GenerateAnswerLinks(answers));
        }
    }
}
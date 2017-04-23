using System.Collections.Generic;
using EmployabilityWebApp.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Services
{
    [ImplicitNotNull]
    public interface IReportPdfHelper
    {
        /// <summary>
        /// Get a font defined for the PDF generator
        /// It returns the specified font
        /// </summary>
        /// <param name="fontKey">The key of chosen font</param>
        /// <returns>The font object corresponding to the given font key</returns>
        Font GetFont(string fontKey);

        /// <summary>
        /// Given a title string, generate a section title paragraph in PDF format
        /// </summary>
        /// <param name="title">The string displayed in the section title</param>
        /// <returns>The paragraph object of the given section title</returns>
        Paragraph GenerateSectionTitle(string title);

        /// <summary>
        /// Given a list of core skills, 
        /// generate a table of links to the detailed core skill answer list
        /// </summary>
        /// <param name="coreSkillList">A list of core skills in the chosen self assessment</param>
        /// <param name="userId">The user id of the owner of the answer</param>
        /// <returns>The table of links to detailed answer list</returns>
        PdfPTable GenerateCoreSkillLinks(IEnumerable<CoreSkill> coreSkillList, string userId);

        /// <summary>
        /// Given a name and a description string, 
        /// generate a paragraph including both of the information
        /// </summary>
        /// <param name="name">The name of section</param>
        /// <param name="description">The description of section</param>
        /// <param name="userId">The user id of the owner of the answer</param>
        /// <returns>The paragraph object of the given section title</returns>
        Paragraph GenerateTitleWithDescription(string name, string description, string userId);

        /// <summary>
        /// Create cells for all simplified answers in user's answer for a chosen core skill
        /// </summary>
        /// <param name="latestAnswer">The user's latest answer of given self assessment</param>
        /// <param name="simplifiedElements">The list of simplified elements in the core skill</param>
        /// <param name="resultTable">The table where the generated cell should be put in</param>
        void AddSimplifiedAnswerCells(Answer latestAnswer, IEnumerable<SimplifiedElement> simplifiedElements,
            PdfPTable resultTable);

        /// <summary>
        /// Create cells for all text answers in user's answer for a chosen core skill
        /// </summary>
        /// <param name="latestAnswer">The user's latest answer of given self assessment</param>
        /// <param name="coreSkill">The core skill specified for current section</param>
        /// <param name="resultTable">The table where the generated cell should be put in</param>
        void AddTextAnswerCells(Answer latestAnswer, CoreSkill coreSkill, PdfPTable resultTable);

        /// <summary>
        /// Create a table to display detailed answers 
        /// and their corresponding text answers for a core skill
        /// </summary>
        /// <param name="latestAnswer">The user's latest answer of given self assessment</param>
        /// <param name="tableWidth">The width of the table, which is the amount of options</param>
        /// <param name="coreSkill">The core skill specified for current section</param>
        /// <param name="detailedElement">The list of detailed elements in the core skill</param>
        /// <returns>The table generated for the detailed answer</returns>
        PdfPTable AddDetailedAnswerTable(Answer latestAnswer, int tableWidth, CoreSkill coreSkill,
            DetailedElement detailedElement);

        /// <summary>
        /// Generate a summary chart based on user's latest answer 
        /// and the core skill list of a given self assessment
        /// </summary>
        /// <param name="simplifiedAnswers">The list of simplified answers in the given answer</param>
        /// <param name="coreSkillList">The list of core skills in the given self assessment</param>
        /// <returns>The summary chart object for the given self assessment</returns>
        Image GenerateSummaryChart(List<SimplifiedAnswer> simplifiedAnswers, List<CoreSkill> coreSkillList);

        /// <summary>
        /// Given a list of answers, 
        /// generate a table of links to the detailed answer result
        /// </summary>
        /// <param name="answers">The list of answers of chosen self assessment</param>
        /// <returns>The table of links to answer result</returns>
        PdfPTable GenerateAnswerLinks(IEnumerable<Answer> answers);
    }
}

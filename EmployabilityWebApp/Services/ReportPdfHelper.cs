using System.Collections.Generic;
using System.Linq;
using EmployabilityWebApp.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Services
{
    [ImplicitNotNull]
    public class ReportPdfHelper : IReportPdfHelper
    {
        private const int TableCellPadding = 7;
        
        private readonly Dictionary<string, Font> _fontList = new Dictionary<string, Font>();

        // Make it public for unit tests
        public IReportChartService ReportChartService;

        public ReportPdfHelper()
        {
            // Initialise report chart service
            ReportChartService = new ReportChartService();

            // Initialise font list
            _fontList.Add("TitleFont", FontFactory.GetFont("Arial", 16, Font.BOLD));
            _fontList.Add("SubTitleFont", FontFactory.GetFont("Arial", 12, Font.BOLD));
            _fontList.Add("BoldFont", FontFactory.GetFont("Arial", 10, Font.BOLD));
            _fontList.Add("TextFont", FontFactory.GetFont("Arial", 10, Font.NORMAL));
            _fontList.Add("LinkFont", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLUE));
        }

        public Font GetFont(string fontKey)
        {
            // If font exists, return the font; otherwise, return the default font
            return _fontList.ContainsKey(fontKey) ? _fontList[fontKey] : _fontList["TextFont"];
        }

        public Paragraph GenerateSectionTitle(string title)
        {
            // Create and return the section title object
            var sectionTitle = new Paragraph(title, _fontList["TitleFont"])
            {
                IndentationLeft = 1,
                SpacingAfter = TableCellPadding,
                SpacingBefore = TableCellPadding
            };
            return sectionTitle;
        }

        public PdfPTable GenerateCoreSkillLinks(IEnumerable<CoreSkill> coreSkillList, string userId)
        {
            // Initialise the link table with 4 cells per row
            var linkTable = CreateLinkTable();

            // For every core skill, generate a link to its detailed answer list
            foreach (var coreSkill in coreSkillList)
            {
                var linkCell = new PdfPCell(new Phrase()
                {
                    new Chunk(coreSkill.Name, _fontList["LinkFont"]).SetLocalGoto($"{coreSkill.Name}_{userId}")
                })
                {
                    Border = 0
                };
                linkTable.AddCell(linkCell);
            }
            // If there is an incomplete row, padding it with empty cells
            linkTable.CompleteRow();
            
            // Return the generated table
            return linkTable;
        }

        public Paragraph GenerateTitleWithDescription(string name, string description, string userId)
        {
            // Generate section title object
            var titleWithDescription = new Paragraph("")
            {
                new Chunk($"{name}. ", _fontList["SubTitleFont"]).SetLocalDestination($"{name}_{userId}"),
                new Phrase(description, _fontList["TextFont"])
            };
            // Setup paragraph format
            titleWithDescription.IndentationLeft = 1;
            titleWithDescription.SpacingAfter = TableCellPadding;
            titleWithDescription.SpacingBefore = TableCellPadding;
            // Return the generated object
            return titleWithDescription;
        }

        public void AddSimplifiedAnswerCells(Answer latestAnswer, 
            IEnumerable<SimplifiedElement> simplifiedElements, PdfPTable resultTable)
        {
            // For every simplified element
            foreach (var simplifiedElement in simplifiedElements
                .Select((value, i) => new { i, value }))
            {
                // Get the corresponding answer
                var answer = latestAnswer.SubAnswers.OfType<SimplifiedAnswer>()
                    .FirstOrDefault(a => a.Element.Id == simplifiedElement.value.Id);

                // Add index cell
                resultTable.AddCell(new PdfPCell(new Phrase($"{simplifiedElement.i + 1}", _fontList["TextFont"])));

                // Add title cell, including the description of the element
                var titleCell = new PdfPCell(new Phrase(simplifiedElement.value.Description, _fontList["BoldFont"]));
                resultTable.AddCell(titleCell);

                // Add value cell, display the user's choice in the format of percentage
                var answerPercentage = answer?.Choice / (float)SimplifiedAnswer.MaxChoice ?? 0;
                var percentage = $"{answerPercentage:0%}";
                var resultCell = new PdfPCell(new Phrase(percentage, _fontList["TextFont"]));

                // Add the cell to the table object
                resultTable.AddCell(resultCell);
            }
        }

        public void AddTextAnswerCells(Answer latestAnswer, CoreSkill coreSkill, PdfPTable resultTable)
        {
            // For every text element
            foreach (var textElement in coreSkill.Elements.OfType<TextElement>())
            {
                // If the element on its previous positions is a detialed element,
                // do not add it into the table object
                var previousElement = coreSkill.Elements.OfType<DetailedElement>()
                    .FirstOrDefault(e => e.Position == textElement.Position - 1);
                if (previousElement != null)
                {
                    continue;
                }

                // Otherwise, get the corresponding answer
                var answer = latestAnswer.SubAnswers.OfType<TextAnswer>()
                    .FirstOrDefault(a => a.Element.Id == textElement.Id);
                resultTable.AddCell(new PdfPCell());

                // Add title cell, including the description of the element
                var titleCell = new PdfPCell(new Phrase(textElement.Description, _fontList["BoldFont"]));
                resultTable.AddCell(titleCell);

                // Add answer cell
                var answerText = answer?.Text ?? " ";
                var resultCell = new PdfPCell(new Phrase(answerText, _fontList["TextFont"]));

                // Add the cell to the table object
                resultTable.AddCell(resultCell);
            }
        }

        public PdfPTable AddDetailedAnswerTable(Answer latestAnswer, int tableWidth, 
            CoreSkill coreSkill, DetailedElement detailedElement)
        {
            // If there is no option, generate a table with 5 columns
            var realWidth = tableWidth == 0 ? 5 : tableWidth;
            // Initialise the detailed element table object with the given width
            var detailedElementTable = new PdfPTable(realWidth)
            {
                WidthPercentage = 90,
                HorizontalAlignment = 1,
                SpacingBefore = TableCellPadding,
                SpacingAfter = TableCellPadding
            };

            // Create and add a title cell with the table's width
            var titleCell = new PdfPCell(new Phrase(detailedElement.Description, _fontList["BoldFont"]))
            {
                Colspan = realWidth
            };
            detailedElementTable.AddCell(titleCell);

            // Add option cells
            AddDetailedOptionsCell(latestAnswer, detailedElement, detailedElementTable);

            // Check whether there is a related text element
            var relatedTextElement = coreSkill.Elements.OfType<TextElement>()
                .FirstOrDefault(e => e.Position == detailedElement.Position + 1);
            if (relatedTextElement != null)
            {
                // If exists, add corresponding text answer under the table
                AddDetailedElementRelatedText(latestAnswer, relatedTextElement, 
                    detailedElementTable, realWidth);
            }
            // Return the table object
            return detailedElementTable;
        }

        public Image GenerateSummaryChart(List<SimplifiedAnswer> simplifiedAnswers, 
            List<CoreSkill> coreSkillList)
        {
            // Calculate the count of core skills
            var coreSkillCount = coreSkillList.Count();

            // Both x and y data array will has the same length as the amount of core skills
            var xArray = new string[coreSkillCount];
            var yArrayBar = new double[coreSkillCount];

            // For every core skill
            foreach (var coreSkill in coreSkillList.Select((value, i) => new { i, value }))
            {
                // The value of x array will be the name of core skill
                xArray[coreSkill.i] = coreSkill.value.Name;
                // The value of y array will be the average score of 
                // user's answer of simplified elements in the core skill
                yArrayBar[coreSkill.i] = GetAvgSimplifiedChoice(simplifiedAnswers, coreSkill.value);
            }

            // Generate the summary chart by given x and y data arrays
            var chartImage = ReportChartService.GenerateSummaryChart(xArray, yArrayBar);
            var image = Image.GetInstance(chartImage.GetBuffer());
            image.ScalePercent(75f);
            // Return the image object
            return image;
        }

        public PdfPTable GenerateAnswerLinks(IEnumerable<Answer> answers)
        {
            // Initialise the link table with 4 cells per row
            var linkTable = CreateLinkTable();

            // For every answer, generate a link to its detailed result list
            foreach (var answer in answers)
            {
                var linkCell = new PdfPCell(new Phrase()
                {
                    new Chunk($"Answer from {answer.BasicUser.GivenName} {answer.BasicUser.FamilyName}", 
                        _fontList["LinkFont"]).SetLocalGoto($"Answer from_{answer.BasicUser.Id}")
                })
                {
                    Border = 0
                };
                linkTable.AddCell(linkCell);
            }
            // If there is an incomplete row, padding it with empty cells
            linkTable.CompleteRow();

            // Return the generated table
            return linkTable;
        }

        /// <summary>
        /// Create link table
        /// </summary>
        /// <returns>The created link table</returns>
        private PdfPTable CreateLinkTable()
        {
            // Initialise the link table with 4 cells per row
            var linkTable = new PdfPTable(4)
            {
                WidthPercentage = 90,
                HorizontalAlignment = 1,
                SpacingBefore = TableCellPadding,
                SpacingAfter = TableCellPadding
            };
            linkTable.DefaultCell.Border = 0;
            return linkTable;
        }

        /// <summary>
        /// Generate cells for options of a given detialed element
        /// Also high light the user's choice if exists
        /// </summary>
        /// <param name="latestAnswer">The user's latest answer of given self assessment</param>
        /// <param name="detailedElement">The chosen detailed element</param>
        /// <param name="detailedTable">The table object for the answer of given detialed element</param>
        private void AddDetailedOptionsCell(Answer latestAnswer, DetailedElement detailedElement, 
            PdfPTable detailedTable)
        {
            // Get user's answer
            var answer = latestAnswer.SubAnswers.OfType<DetailedAnswer>()
                .FirstOrDefault(a => a.Element.Id == detailedElement.Id);

            // For each option of the given detailed element
            foreach (var option in detailedElement.DetailedOptions)
            {
                // Check whether it is the one chosen by the user
                var isChosen = answer != null && answer.Choice.Equals(option);
                // If yes, high light the generated cell; otherwise, just generate the cell
                var optionCell = new PdfPCell(new Phrase(option.Description, _fontList["TextFont"]))
                {
                    BackgroundColor = isChosen ? BaseColor.LIGHT_GRAY : BaseColor.WHITE
                };
                // Add the cell into the table object
                detailedTable.AddCell(optionCell);
            }
            detailedTable.CompleteRow();
        }

        /// <summary>
        /// Generate table cells for the text answer corresponding to the given detailed element
        /// </summary>
        /// <param name="latestAnswer">The user's latest answer of given self assessment</param>
        /// <param name="textElement">The corresponding text element</param>
        /// <param name="detailedTable">The table object for the answer of given detialed element</param>
        /// <param name="tableWidth">The width of the table</param>
        private void AddDetailedElementRelatedText(Answer latestAnswer, CoreSkillElement textElement, 
            PdfPTable detailedTable, int tableWidth)
        {
            // Add the title cell, includes the description of the text element
            var textTitleCell = new PdfPCell(new Phrase(textElement.Description, _fontList["BoldFont"]))
            {
                Colspan = tableWidth
            };
            detailedTable.AddCell(textTitleCell);

            // Get the answer of the text element
            var relatedTextAnswer = latestAnswer.SubAnswers.OfType<TextAnswer>()
                .FirstOrDefault(a => a.Element.Id == textElement.Id);
            var answerText = relatedTextAnswer?.Text ?? " ";
            // Add the answer cell
            var textResultCell = new PdfPCell(new Phrase(answerText, _fontList["TextFont"]))
            {
                Colspan = tableWidth
            };
            detailedTable.AddCell(textResultCell);
        }

        /// <summary>
        /// Get the average score of user's choice of all the simplified elements in a given core skill
        /// </summary>
        /// <param name="simplifiedAnswers">The list of simplified answers</param>
        /// <param name="coreSkill">The given core skill</param>
        /// <returns>The user's average score in all the simplified elements</returns>
        private static double GetAvgSimplifiedChoice(List<SimplifiedAnswer> simplifiedAnswers, CoreSkill coreSkill)
        {
            // If there is no element in the core skill, return 0.0
            if (coreSkill.Elements == null || simplifiedAnswers == null)
            {
                return 0.0;
            }
            // Get the list of simplified elements
            var simplifiedElements = coreSkill.Elements.OfType<SimplifiedElement>();
            // Calculate the user's average score
            // If the user does not have an answer for an element, add 0 to the sum
            return
                simplifiedElements.Select(
                    simplifiedElement =>
                        simplifiedAnswers.FirstOrDefault(a => 
                            a.Element.Id == simplifiedElement.Id))
                    .Select(answer => answer?.Choice ?? 0.0)
                    .Average();
        }
    }
}
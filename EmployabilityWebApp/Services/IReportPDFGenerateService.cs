using System.Collections.Generic;
using System.IO;
using EmployabilityWebApp.Models;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Services
{
    [ImplicitNotNull]
    public interface IReportPdfGenerateService
    {
        /// <summary>
        /// Returns an output memory stream for the PDF report generated for 
        /// the given answer and user.
        /// </summary>
        /// <param name="latestAnswer">The user's latest answer for a chosen self assessment</param>
        /// <param name="user">The current logged in basic user</param>
        /// <returns>A memory output stream includes the content of pdf report</returns>
        MemoryStream GenerateSurveyReport(Answer latestAnswer, BasicUser user);

        /// <summary>
        /// Returns an output memory stream for the PDF summary report.
        /// </summary>
        /// <param name="selfAssessments">All the self assessmsents in the system</param>
        /// <param name="answers">All the answers in the system</param>
        /// <param name="user">The current logged in admin user</param>
        /// <returns>A memory output stream includes the content of summary report</returns>
        MemoryStream GenerateSummaryReport(ICollection<SelfAssessment> selfAssessments, 
            ICollection<Answer> answers, AdminUser user);
    }
}

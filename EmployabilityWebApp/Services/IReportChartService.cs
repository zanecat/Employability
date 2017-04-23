using System.IO;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Services
{
    [ImplicitNotNull]
    public interface IReportChartService
    {
        /// <summary>
        /// Generate a summary chart based on the given data arrays.
        /// The chart will be a bar chart.
        /// It returns an output memory stream of generated chart.
        /// </summary>
        /// <param name="xArray">The x-axis value of the chart</param>
        /// <param name="yArrayBar">The y-axis value of the chart</param>
        /// <returns>The output stream of the generated chart</returns>
        MemoryStream GenerateSummaryChart(string[] xArray, double[] yArrayBar);
    }
}

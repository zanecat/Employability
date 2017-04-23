using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web.UI.DataVisualization.Charting;
using JetBrains.Annotations;

namespace EmployabilityWebApp.Services
{
    [ImplicitNotNull]
    public class ReportChartService : IReportChartService
    {
        public MemoryStream GenerateSummaryChart(string[] xArray, double[] yArrayBar)
        {
            // Initialise chart object
            var chart = InitChart(xArray.Length + 1, 10);
            
            // Add data series into the chart object
            chart.Series.Add("");
            ConfigureDataSeries(chart.Series[0], xArray, yArrayBar);

            // Turn chart object into output memory stream
            var chartImage = new MemoryStream();
            chart.SaveImage(chartImage, ChartImageFormat.Png);
            return chartImage;
        }

        /// <summary>
        /// Initialise the chart object based on a given x-axis length and y-axix length
        /// </summary>
        /// <param name="xLength">The length of x axis</param>
        /// <param name="yLength">The length of y axis</param>
        /// <returns>The generated chart object</returns>
        private static Chart InitChart(int xLength, int yLength)
        {
            // Create a new 650*350 chart
            var chart = new Chart
            {
                Width = 650,
                Height = 350,
                RenderType = RenderType.ImageTag,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };
            // Add chart titles and chart area
            chart.Titles.Add("The Average Score of Each Core Skill");
            chart.ChartAreas.Add("Overall");

            // Set alignment of chart
            chart.ChartAreas["Overall"].AlignmentOrientation = AreaAlignmentOrientations.Vertical;
            chart.ChartAreas["Overall"].AlignmentStyle = AreaAlignmentStyles.All;

            // Configure X and Y axises
            ConfigureAxisX(chart.ChartAreas["Overall"], xLength);
            ConfigureAxisY(chart.ChartAreas["Overall"], yLength);

            return chart;
        }

        /// <summary>
        /// Configure x axis
        /// </summary>
        /// <param name="chartArea">The chart area where the axis belongs to</param>
        /// <param name="xLength">The length of the axis</param>
        private static void ConfigureAxisX(ChartArea chartArea, int xLength)
        {
            // Set up max/min values of the axis
            chartArea.AxisX.Maximum = xLength;
            chartArea.AxisX.Minimum = 0;
            // Set up axis interval
            chartArea.AxisX.Interval = 1;
            // Set up color
            chartArea.AxisX.LineColor = Color.Gray;

            // Disable major grid for this axis
            chartArea.AxisX.MajorGrid.Enabled = false;
        }

        /// <summary>
        /// Configure y axis
        /// </summary>
        /// <param name="chartArea">The chart area where the axis belongs to</param>
        /// <param name="yLength">The length of the axis</param>
        private static void ConfigureAxisY(ChartArea chartArea, int yLength)
        {
            // Set up max/min values of the axis
            chartArea.AxisY.Maximum = yLength;
            chartArea.AxisY.Minimum = 1;
            // Set up axis interval
            chartArea.AxisY.Interval = 1;
            // Set up color
            chartArea.AxisY.LineColor = Color.Gray;

            //Display the MajorGrid lines
            chartArea.AxisY.MajorGrid.LineColor = Color.Gray;
        }

        /// <summary>
        /// Configure chart data series
        /// </summary>
        /// <param name="chartSeries">The chart series where the data belong to</param>
        /// <param name="xArray">The x-array of the data</param>
        /// <param name="yArrayBar">The y-array of the data</param>
        private static void ConfigureDataSeries(Series chartSeries, IEnumerable<string> xArray, IEnumerable<double> yArrayBar)
        {
            // Set the chart as a bar chart
            chartSeries.ChartType = SeriesChartType.Bar;

            // Setup the color of bars
            chartSeries.Color = Color.DeepSkyBlue;
            chartSeries.SmartLabelStyle.Enabled = true;

            // Setup null value bars
            chartSeries["EmptyPointValue"] = "Zero";

            chartSeries.EmptyPointStyle.Label = "no selection";
            chartSeries.EmptyPointStyle.LabelForeColor = Color.Red;

            chartSeries.EmptyPointStyle.LabelBackColor = Color.Silver;
            chartSeries.EmptyPointStyle.LabelBorderWidth = 1;
            chartSeries.EmptyPointStyle.LabelBorderColor = Color.Blue;
            chartSeries.EmptyPointStyle.LabelBorderDashStyle = ChartDashStyle.Dot;

            // Setup width of bars
            chartSeries["PixelPointWidth"] = "20";

            // Plot data points on the chart series
            chartSeries.Points.DataBindXY(xArray, yArrayBar);
        }
    }
}
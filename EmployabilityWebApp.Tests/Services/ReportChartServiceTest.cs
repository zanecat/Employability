using System.Collections.Generic;
using System.Linq;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.Services;
using EmployabilityWebApp.Tests.TestHelpers;
using Xunit;

namespace EmployabilityWebApp.Tests.Services
{
    /// <summary>
    /// Unit tests for ReportChartService
    /// </summary>
    public class ReportChartServiceTest
    {
        private readonly ReportChartService _target;

        private readonly List<CoreSkill> _coreSkills =
           new List<CoreSkill>();

        private string[] _xArray;
        private double[] _yArrayBar;

        public ReportChartServiceTest()
        {
            _target = new ReportChartService();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void ItShouldAlwaysGenerateChartImage(int nCoreSkills)
        {
            // Initialise variables
            GivenAListOfCoreSkills(nCoreSkills);
            // Generate summary chart
            var chartImage = _target.GenerateSummaryChart(_xArray, _yArrayBar);
            // Verify
            Assert.NotNull(chartImage);
        }

        /// Provide a list of core skills
        private void GivenAListOfCoreSkills(int nCoreSkills)
        {
            _coreSkills.SetUpCoreSkills(nCoreSkills);
            // Generate corresponding data arrays
            _xArray = new string[nCoreSkills];
            _yArrayBar = new double[nCoreSkills];
            foreach (var coreSkill in _coreSkills.Select((value, i) => new {i, value}))
            {
                _xArray[coreSkill.i] = coreSkill.value.Name;
                _yArrayBar[coreSkill.i] = 0.0;
            }
        }
    }
}

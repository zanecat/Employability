using System.Web.Mvc;
using Xunit;
using EmployabilityWebApp.Controllers;

namespace EmployabilityWebApp.Tests.Controllers
{
    public class HomeControllerTest
    {
        [Fact]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void About()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.About() as ViewResult;

            // Assert
            Assert.Equal("Your application description page.", result.ViewBag.Message);
        }

        [Fact]
        public void Error()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }
    }
}

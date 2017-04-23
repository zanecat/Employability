using EmployabilityWebApp.ViewModels;
using System.Web.Mvc;

namespace EmployabilityWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult ThankYou()
        {
            ViewBag.Message = "ThankYou page.";

            return View();
        }

        public ActionResult Error()
        {
            ViewBag.Message = "404 page";

            return View();
        }
    }
}
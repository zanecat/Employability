using System.Web.Mvc;
using System.Web.Routing;

namespace EmployabilityWebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Use attribute routing because it puts route definitions close
            // to their handlers.
            routes.MapMvcAttributeRoutes();

            // TODO Phase this out because attribute routing is prefered.
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

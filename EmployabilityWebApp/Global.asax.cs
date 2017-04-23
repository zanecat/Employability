using EmployabilityWebApp.Migrations;
using EmployabilityWebApp.Models;
using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace EmployabilityWebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BundleTable.EnableOptimizations = true;

            // Automatically run pending migrations when the database
            // is initialised.
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion
                <EmployabilityDbContext, Configuration>());

            // Force initialisation of the database at startup.
            EmployabilityDbContext.Create().Database.Initialize(true);
        }
    }
}

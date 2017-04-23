using System;
using System.Data.Entity;
using System.Web;
using EmployabilityWebApp.Models;
using EmployabilityWebApp.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Practices.Unity;

// ReSharper disable once CheckNamespace
namespace EmployabilityWebApp.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static readonly Lazy<IUnityContainer> Container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return Container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // Register types with a single instance for the life of the server.
            container.RegisterInstance(AutoMapConfig.CreateMapper());

            // Register types for a single instance per request.
            container.RegisterType<DbContext, EmployabilityDbContext>(new PerRequestLifetimeManager());

            // Register types which are recreated every time they are used.
            container.RegisterType<IAccountService, AccountService>();
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>();
            container.RegisterType<ICoreSkillService, CoreSkillService>();
            container.RegisterType<ICoreSkillElementService, CoreSkillElementService>();
            container.RegisterType<IReportPdfGenerateService, ReportPdfGenerateService>();
            container.RegisterType<IAuthenticationManager>(
                    new InjectionFactory(c => HttpContext.Current.GetOwinContext().Authentication));
            container.RegisterType<ApplicationUserManager>(new InjectionFactory(c =>
                HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()));
        }
    }
}

using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EmployabilityWebApp.Startup))]
namespace EmployabilityWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

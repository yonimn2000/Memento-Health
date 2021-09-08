using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MementoHealth.Startup))]
namespace MementoHealth
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

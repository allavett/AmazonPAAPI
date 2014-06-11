using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AmazonPAAPI.Startup))]
namespace AmazonPAAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

using System.Web.Http;
using Owin;

namespace BlockchainApp.Source
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "BlockchainApp",
                routeTemplate: "{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);
        }
    }
}

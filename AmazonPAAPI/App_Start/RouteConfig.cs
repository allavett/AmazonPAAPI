using System.Web.Mvc;
using System.Web.Routing;

namespace AmazonPAAPI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "ItemSearch", action = "Index", id = UrlParameter.Optional }
            );

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "Search",
                url: "ItemSearch/Search",
                defaults: new { controller = "ItemSearch", action = "Search" }
                );            
        }
    }
}

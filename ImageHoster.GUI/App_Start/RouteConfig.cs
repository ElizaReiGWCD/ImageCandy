using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ImageHoster.GUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "photopage",
                url: "user/{username}/photo/{filename}",
                defaults: new { controller = "User", action = "Photo", username = "" }
                );

            routes.MapRoute(
                name: "account",
                url: "user/{username}/{action}",
                defaults: new { controller = "User", action = "Index", username = ""}
                );

            routes.MapRoute(
                name: "images",
                url: "image/{action}/{filename}",
                defaults: new { controller = "Photo", action = "GetImage", filename="" }
                );

            routes.MapRoute(
                name: "imageactions",
                url: "image/{action}/{id}",
                defaults: new { controller = "Photo", action="Detail" }
                );

            routes.MapRoute(
                name: "albums",
                url: "album/{action}/{id}",
                defaults: new { controller = "Album", action = "Album", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
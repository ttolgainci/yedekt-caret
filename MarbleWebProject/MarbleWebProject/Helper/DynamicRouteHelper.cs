using Azure;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace MarbleWebProject.Helper
{
    public class DynamicRouteHelper
    {
        public static IConfiguration _configuration;
        public static void AppSetting(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }
        public static RouteDefinitionModel GenerateRouteAll(List<RouteListModel> routeList)
        {
            return BuildRouteModel(new RouteDefinitionModel(), routeList);
        }

        /// <summary>API kapalıyken bile Web ayağa kalksın (cart, checkout, home).</summary>
        public static RouteDefinitionModel GenerateStaticRoutesOnly()
        {
            return BuildRouteModel(new RouteDefinitionModel(), new List<RouteListModel>());
        }

        private static RouteDefinitionModel BuildRouteModel(RouteDefinitionModel returnRoute, List<RouteListModel> routeList)
        {
            #region Route Without Area Name
            var routeDefinitions = new List<RouteDefinition>();
            routeDefinitions.Add(new RouteDefinition
            {
                Name = "Wishlist",
                Pattern = "wishlist",
                Defaults = new Info { controller = "Wishlist", action = "Index" }
            });
            routeDefinitions.Add(new RouteDefinition
            {
                Name = "Static",
                Pattern = "pages/{id}",
                Defaults = new Info { controller = "StaticPage", action = "Index" }
            });
            routeDefinitions.Add(new RouteDefinition
            {
                Name = "Cart",
                Pattern = "cart",
                Defaults = new Info { controller = "Cart", action = "Index" }
            });
            routeDefinitions.Add(new RouteDefinition
            {
                Name = "Checkout",
                Pattern = "checkout",
                Defaults = new Info { controller = "Checkout", action = "Index" }
            });
            //routeDefinitions.Add(new RouteDefinition
            //{
            //    Name = "SiteMap",
            //    Pattern = "sitemap.xml",
            //    Defaults = new Info { controller = "Sitemap", action = "Index" }
            //});

            FilterParametersHelper.SiteMapUrlList = routeList.Select(c => new SiteMapUrlModel { Type = c.PageType, Url = c.RouteUrl, LanguageCode = c.LanguageCode }).ToList();
            foreach (var item in routeList)
            {

                switch (item.PageType)
                {
                    case "CATEGORY":
                        routeDefinitions.Add(new RouteDefinition
                        {
                            Name = item.RouteName,
                            Pattern = item.RouteUrl,
                            Defaults = new Info { controller = "Category", action = "Index", id = item.ID.GetValueOrDefault() }
                        });
                        break;
                    case "PRODUCT":
                        routeDefinitions.Add(new RouteDefinition
                        {
                            Name = item.RouteName,
                            Pattern = item.RouteUrl,
                            Defaults = new Info { controller = "ProductDetail", action = "Index",id=item.ID.GetValueOrDefault(),catID=item.CatID.GetValueOrDefault() }
                        });
                        break;
                    case "INFORMATION":
                        routeDefinitions.Add(new RouteDefinition
                        {
                            Name = item.RouteName,
                            Pattern = "pages/" + item.RouteUrl,
                            Defaults = new Info { controller = "StaticPage", action = "Index" }
                        });
                        break;
                    case "FAQ":
                        routeDefinitions.Add(new RouteDefinition
                        {
                            Name = item.RouteName,
                            Pattern = item.RouteUrl,
                            Defaults = new Info { controller = "Faq", action = "Index" }
                        });
                        break;
                }




            }
            #endregion

            returnRoute.GenerateRoute = routeDefinitions;
            return returnRoute;
        }

    }
}

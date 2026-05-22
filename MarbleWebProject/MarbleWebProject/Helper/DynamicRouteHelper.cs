using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Helper
{
    public class DynamicRouteHelper
    {
        public static IConfiguration _configuration;

        /// <summary>Startup'ta ürünler için SEO indeks + catch-all kullanıldıysa true.</summary>
        public static bool LastBuildUsedProductIndex { get; private set; }

        public static void AppSetting(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }
        public static RouteDefinitionModel GenerateRouteAll()
        {
            var returnRoute = new RouteDefinitionModel();

            var routeList = getRouteList(AppConfig.CMSService.CustomName);
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
                        if (!LastBuildUsedProductIndex)
                        {
                            routeDefinitions.Add(new RouteDefinition
                            {
                                Name = item.RouteName,
                                Pattern = item.RouteUrl,
                                Defaults = new Info { controller = "ProductDetail", action = "Index", id = item.ID.GetValueOrDefault(), catID = item.CatID.GetValueOrDefault() }
                            });
                        }
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

            if (LastBuildUsedProductIndex)
            {
                routeDefinitions.Add(new RouteDefinition
                {
                    Name = "catalogSeoCatchAll",
                    Pattern = "{**seoPath}",
                    Defaults = new Info { controller = "CatalogDispatch", action = "Index" }
                });
            }

            returnRoute.GenerateRoute = routeDefinitions;
            return returnRoute;
        }
        public static List<RouteListModel> getRouteList(string customName)
        {
            LastBuildUsedProductIndex = false;
            var routeList = new List<RouteListModel>();
            //TokenResponse loginResponse = new TokenResponse();
            BaseResponse<List<CategoryRouteModel>> routeResponse = new BaseResponse<List<CategoryRouteModel>>();

            //using (var client = new CmsClient())
            //{
            //    loginResponse = client.getSession();
            //    routeResponse = client.GetRouteList(customName, loginResponse.Token);
            //}

            AccountLoginRequest loginRequest = new AccountLoginRequest();
            TokenResponse loginResponse = new TokenResponse();

            var client = new WebRequest("/AccountManager/login", RestSharp.Method.POST, "");
            client.Endpoint = AppConfig.StorefrontApiBaseUrl;
            client.LogEnable = true;

            loginRequest.UserName = AppConfig.CMSService.UserName;
            loginRequest.Password = AppConfig.CMSService.Password;
            loginRequest.CustomName = AppConfig.CMSService.CustomName;
            client.AddJsonBody(loginRequest);
            loginResponse = client.Execute<TokenResponse>("login");
            AppConfig.CMSService.LanguageCode = loginResponse.LanguageCode;
            AppConfig.CMSService.LanguageCulture = loginResponse.LanguageCulture;
            using (var cms = new CmsClient())
            {
                routeResponse = cms.GetCagetoryRoute(loginResponse.Token);
            }
            if (routeResponse.Status)
            {
                int indexCategory = 1;
                foreach (var item in routeResponse.Data)
                {
                    routeList.Add(new RouteListModel { PageType = "CATEGORY", RouteName = item.Name + " " + indexCategory, RouteUrl = item.Url, LanguageCode = item.LanguageCode, ID = item.ID });
                    //routeList.Add(new RouteListModel { PageType = "CATEGORY", RouteName = item.Name + " " + indexCategory, RouteUrl = item.Url + "/{id}" });
                    indexCategory++;
                }
            }
            var usedIndexForProducts = false;
            if (AppConfig.UseIndexedProductRouting)
            {
                BaseResponse<List<SitemapPathItemDto>> sitemapProducts;
                using (var cms = new CmsClient())
                {
                    sitemapProducts = cms.GetSitemapProductPaths(loginResponse.Token);
                }

                if (sitemapProducts.Status && sitemapProducts.Data != null && sitemapProducts.Data.Count > 0)
                {
                    usedIndexForProducts = true;
                    var indexProduct = 1;
                    foreach (var item in sitemapProducts.Data)
                    {
                        routeList.Add(new RouteListModel
                        {
                            PageType = "PRODUCT",
                            RouteName = "product " + indexProduct,
                            RouteUrl = item.Url,
                            LanguageCode = item.LanguageCode,
                            ID = item.EntityId,
                            CatID = item.CatId
                        });
                        indexProduct++;
                    }
                }
            }

            if (!usedIndexForProducts)
            {
                using (var cms = new CmsClient())
                {
                    routeResponse = cms.GetProductRoute(loginResponse.Token);
                }
                if (routeResponse.Status && routeResponse.Data != null)
                {
                    var indexProduct = 1;
                    foreach (var item in routeResponse.Data)
                    {
                        routeList.Add(new RouteListModel { PageType = "PRODUCT", RouteName = item.Name + " " + indexProduct, RouteUrl = item.Url, LanguageCode = item.LanguageCode, ID = item.ID, CatID = item.CatID });
                        indexProduct++;
                    }
                }
            }

            LastBuildUsedProductIndex = AppConfig.UseIndexedProductRouting && usedIndexForProducts;
            BaseResponse<List<InformationRouteModel>> routeResponseInfo = new BaseResponse<List<InformationRouteModel>>();
            using (var cms = new CmsClient())
            {
                routeResponseInfo = cms.GetInformationForRoute(loginResponse.Token);
            }
            if (routeResponseInfo.Status)
            {
                int indexInfo = 1;
                foreach (var item in routeResponseInfo.Data)
                {
                    routeList.Add(new RouteListModel { PageType = item.Type, RouteName = item.Name + " " + indexInfo, RouteUrl = item.Url, LanguageCode = item.LanguageCode });

                    indexInfo++;
                }
            }
            #region Translate
            BaseResponse<List<TranslateAllResponse>> translateAllResponse = new BaseResponse<List<TranslateAllResponse>>();
            using (var clientCms = new CmsClient())
            {
                translateAllResponse = clientCms.GetTranslateAll(loginResponse.Token);
            }
            if (translateAllResponse.Status)
            {
                var returnTranslateAll = translateAllResponse.Data.Select(c => new TranslateAllResponse
                {
                    Key = c.Key,
                    KeyLang = c.KeyLang,
                    RetLang = c.RetLang,
                    Translation = c.Translation,
                }).ToList();
                FilterParametersHelper.TranslateFullList = returnTranslateAll;
            }

            #endregion

            //#region UserID
            //if (FilterParametersHelper.UserInfo == null)
            //{
            //    var userModel=new UserModel();
            //    userModel.UserID = Guid.NewGuid();
            //    FilterParametersHelper.UserInfo = userModel;
            //}

            //#endregion


            return routeList;
        }
    }
}

using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents
{
    [ViewComponent]
    public class HomeManufacturersBannerViewComponent : ViewComponent
    {
        private readonly IMemoryCache _cache;

        public HomeManufacturersBannerViewComponent(IMemoryCache cache)
        {
            _cache = cache;
        }
        public async Task<IViewComponentResult> InvokeAsync(List<AllBannerResponse> models)
        {
            BaseResponse<List<AllBannerResponse>> routeResponse = new BaseResponse<List<AllBannerResponse>>();
            var returnData = new List<AllBannerResponse>();
            TokenResponse loginResponse = new TokenResponse();
            CategoryRequest request = new CategoryRequest();
            CacheHelper cacheHelper = new CacheHelper(_cache);
            string Key = "HomeManufacturersBanner" + AppConfig.CMSService.CustomName + AppConfig.CMSService.LanguageCode;
            string returnHtml = "";

            if (!cacheHelper.IfCache(Key))
            {
                using (var cms = new CmsClient())
                {
                    loginResponse = cms.getSession();
                    var contentRequest = new BannerAllRequest { LanguageCode = loginResponse.LanguageCode, Type = "MANUFACTURESBANNER" };
                    routeResponse = cms.GetBannerAll(contentRequest, loginResponse.Token);
                }
                if (routeResponse.Status)
                {
                    returnData = routeResponse.Data;
                    cacheHelper.SetCache(Key, returnData);
                    return View(returnData);
                }
                cacheHelper.SetCache(Key, returnData);
                return Content(string.Empty);
            }
            returnData = cacheHelper.GetCache(Key) as List<AllBannerResponse>;
            return View(returnData);
        }
    }

}

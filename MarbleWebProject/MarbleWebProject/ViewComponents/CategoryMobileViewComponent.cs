using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class CategoryMobileViewComponent : ViewComponent
    {
        private readonly IMemoryCache _cache;

        public CategoryMobileViewComponent(IMemoryCache cache)
        {
            _cache = cache;
        }
        public async Task<IViewComponentResult> InvokeAsync(List<CategoryListModel> models)
        {
            BaseResponse<List<CategoryListModel>> routeResponse = new BaseResponse<List<CategoryListModel>>();
            var returnData = new List<CategoryListModel>();
            TokenResponse loginResponse = new TokenResponse();
            CategoryRequest request = new CategoryRequest();
            CacheHelper cacheHelper = new CacheHelper(_cache);
            string Key = "CategoryMobileListHeader" + AppConfig.CMSService.CustomName + AppConfig.CMSService.MarketCode + AppConfig.CMSService.LanguageCode;
            string returnHtml = "";

            if (!cacheHelper.IfCache(Key))
            {
                using (var cms = new CmsClient())
                {
                    loginResponse = cms.getSession();
                    var contentRequest = new CategoryRequest { languageCode = loginResponse.LanguageCode };
                    routeResponse = cms.GetCagetory(contentRequest, loginResponse.Token);
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
            returnData = cacheHelper.GetCache(Key) as List<CategoryListModel>;
            return View(returnData);
        }
    }
}

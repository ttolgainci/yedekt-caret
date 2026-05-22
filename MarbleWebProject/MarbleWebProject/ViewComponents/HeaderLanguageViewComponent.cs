using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class HeaderLanguageViewComponent : ViewComponent
    {
        private readonly IMemoryCache _cache;

        public HeaderLanguageViewComponent(IMemoryCache cache)
        {
            _cache = cache;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            TokenResponse loginResponse = new TokenResponse();
            BaseResponse<List<LanguageCultureResponse>> languageResponse = new BaseResponse<List<LanguageCultureResponse>>();

            var returnData = new LanguageReturnModel();
            CacheHelper cacheHelper = new CacheHelper(_cache);

            string Key = "LanguageMenuV1" + AppConfig.CMSService.CustomName + AppConfig.CMSService.LanguageCode;
            if (!cacheHelper.IfCache(Key))
            {
                using (var client = new CmsClient())
                {
                    loginResponse = client.getSession();
                    // loginResponse = client.login(new AccountLoginRequest() { UserName = AppConfig.CMSService.UserName, Password = AppConfig.CMSService.Password, CustomName = AppConfig.CMSService.CustomName });
                    languageResponse = client.GetLanguageCulture(loginResponse.Token);

                }
                if (languageResponse.Status)
                {

                    string strData = "";

                    string lowerLanguage = loginResponse.LanguageCode.ToLower();




                    strData += "   <div class='header-dropdown'>";
                    strData += ""+ loginResponse.LanguageName + "";
                    strData += " <div class='header-menu'>";
                    strData += "    <ul>";
                    foreach (var item in languageResponse.Data)
                    {
                        strData += " <li>";
                        strData += "    <a href='#' data-id='" + item.stCode + "' data-name='" + item.stName + "'  data-culture='" + item.stCulture + "' onclick='setLang_Click(this)' class='langLink'>";
                        strData += "      " + item.stName + "";
                        strData += "    </a>";
                        strData += " </li>";
                    }
                    strData += "  </ul>";
                    strData += " </div>";
                    strData += " </div>";
                    returnData.SetLangContent = strData;
                    cacheHelper.SetCache(Key, returnData);
                    return View(returnData);
                }
                else
                {
                    cacheHelper.SetCache(Key, returnData);
                    return Content(string.Empty);
                }
            }
            else
            {
                returnData = cacheHelper.GetCache(Key) as LanguageReturnModel;
                return View(returnData);
            }

        }
    }
}

using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarbleWebProject.Helper
{
    public static class TranslateHelper
    {
        public static string Translate(this IHtmlHelper htmlHelper, string key)
        {
            TranslateRequest requestTranslate = new TranslateRequest();
            BaseResponse<TranslateResponse> translateResponse = new BaseResponse<TranslateResponse>();
            var returnData = new TranslateResponse();
            TokenResponse loginResponse = new TokenResponse();
            //CacheHelper cacheHelper = new CacheHelper(memoryCache);
            var localizedString = "";
            var translateAllList = FilterParametersHelper.TranslateFullList;
            if (translateAllList.Count > 0)
            {
                var hasTranslate = translateAllList.Where(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase) && c.AgencyGroupID == AppConfig.CMSService.MarketCode.GetValueOrDefault() && c.RetLang == AppConfig.CMSService.LanguageCode).FirstOrDefault();
                if (hasTranslate != null)
                {
                    return hasTranslate.Translation;
                }
                else
                {
                    using (var cms = new CmsClient())
                    {
                        loginResponse = cms.getSession();
                        requestTranslate.Text = key;
                        requestTranslate.Language = AppConfig.CMSService.LanguageCode;
                        translateResponse = cms.GetTranslate(requestTranslate, loginResponse.Token);
                    }

                    if (translateResponse.Status)
                    {
                        returnData = translateResponse.Data;
                        //cacheHelper.SetCache(Key, localizedString);
                        localizedString = returnData.Text;
                        FilterParametersHelper.TranslateFullList = returnData.GetFullList;
                    }
                    return localizedString;
                }

            }
            else
            {
                using (var cms = new CmsClient())
                {
                    loginResponse = cms.getSession();
                    requestTranslate.Text = key;
                    
                    requestTranslate.Language = AppConfig.CMSService.LanguageCode;
                    translateResponse = cms.GetTranslate(requestTranslate, loginResponse.Token);
                }

                if (translateResponse.Status)
                {
                    returnData = translateResponse.Data;
                    //cacheHelper.SetCache(Key, localizedString);
                    localizedString = returnData.Text;
                    FilterParametersHelper.TranslateFullList = returnData.GetFullList;
                }
                return localizedString;
            }


        }
    }
}

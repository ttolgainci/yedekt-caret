using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MarbleWebProject.Controllers
{
    public class LanguageController : Controller
    {
        [HttpPost]
        public IActionResult SetLanguageToUser(string id, string name, string culture)
        {
            HttpContextSessionHelper sessionHelper = new HttpContextSessionHelper(HttpContext);
            if (AppConfig.CMSService.LanguageCode != id)
            {
                TokenResponse response = new TokenResponse();
                var getTokenSession = sessionHelper.GetSession("CmsApiToken");
                if (!string.IsNullOrEmpty(getTokenSession))
                {
                    var rtn = JsonSerializer.Deserialize<TokenResponse>(getTokenSession);
                    if (rtn == null)
                    {
                        return Json(new { redirectToUrl = Url.Action("Index", "Home") });
                    }

                    response.Token = rtn.Token;
                    response.Market = rtn.Market;
                    response.LanguageName = name;
                    response.LanguageCode = id;
                    response.LanguageCulture = culture;
                    AppConfig.CMSService.LanguageCode = id;
                    AppConfig.CMSService.LanguageCulture = culture;
                    AppConfig.CMSService.MarketCode = rtn.Market;
                    string setTokenResponse = JsonSerializer.Serialize(response);
                    sessionHelper.SetSession("CmsApiToken", setTokenResponse);
                }

                return Json(new { redirectToUrl = Url.Action("Index", "Home") });
            }
            return Json(new { redirectToUrl = "" });
        }
    }
}

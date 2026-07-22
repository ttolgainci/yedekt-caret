using MarbleWebProject.Helper;
using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MarbleWebProject.Controllers
{
    public class LanguageController : Controller
    {
        private readonly IStoreCatalogApi _catalog;

        public LanguageController(IStoreCatalogApi catalog)
        {
            _catalog = catalog;
        }

        [HttpPost]
        public async Task<IActionResult> SetLanguageToUser(string id, string name, string culture, CancellationToken cancellationToken = default)
        {
            HttpContextSessionHelper sessionHelper = new HttpContextSessionHelper(HttpContext);
            if (AppConfig.Storefront.StoreAuth.LanguageCode != id)
            {
                TokenResponse response = new TokenResponse();
                var getTokenSession = sessionHelper.GetSession("CmsApiToken");
                if (!string.IsNullOrEmpty(getTokenSession))
                {
                    var rtn = JsonSerializer.Deserialize<TokenResponse>(getTokenSession);

                    response.Token = rtn.Token;
                    response.Market = rtn.Market;
                    response.LanguageName = name;
                    response.LanguageCode = id;
                    response.LanguageCulture = culture;
                    AppConfig.Storefront.StoreAuth.LanguageCode = id;
                    AppConfig.Storefront.StoreAuth.LanguageCulture = culture;
                    AppConfig.Storefront.StoreAuth.MarketCode = rtn.Market;
                    string setTokenResponse = JsonSerializer.Serialize(response);
                    sessionHelper.SetSession("CmsApiToken", setTokenResponse);
                }

                // Prefer same product in the new language (cookie/session already updated).
                var referer = Request.Headers.Referer.ToString();
                if (!string.IsNullOrWhiteSpace(referer)
                    && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri)
                    && UrlSlugHelper.TryParseProductId(refererUri.AbsolutePath, out var productId))
                {
                    var canonical = await _catalog.GetProductCanonicalUrlAsync(productId, id, cancellationToken);
                    if (canonical.Status && !string.IsNullOrWhiteSpace(canonical.Data))
                    {
                        var path = canonical.Data!;
                        if (!path.StartsWith('/'))
                            path = "/" + path;
                        return Json(new { redirectToUrl = path });
                    }
                }

                return Json(new { redirectToUrl = Url.Action("Index", "Home") });
            }
            return Json(new { redirectToUrl = "" });
        }
    }
}

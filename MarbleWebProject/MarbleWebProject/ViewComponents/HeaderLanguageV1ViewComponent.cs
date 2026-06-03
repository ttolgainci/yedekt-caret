using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderLanguageV1ViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;
    private readonly IStoreContentApi _content;
    private readonly IStoreAuthService _auth;

    public HeaderLanguageV1ViewComponent(IMemoryCache cache, IStoreContentApi content, IStoreAuthService auth)
    {
        _cache = cache;
        _content = content;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var cacheHelper = new CacheHelper(_cache);
        var key = "LanguageMenuV1" + AppConfig.CMSService.CustomName + AppConfig.CMSService.LanguageCode;

        if (cacheHelper.IfCache(key))
        {
            var cached = cacheHelper.GetCache(key) as LanguageReturnModel;
            return View(cached ?? new LanguageReturnModel());
        }

        var loginResponse = await _auth.GetSessionAsync(cancellationToken);
        var languageResponse = await _content.GetLanguageCultureAsync(cancellationToken);

        if (!languageResponse.Status)
        {
            cacheHelper.SetCache(key, new LanguageReturnModel());
            return Content(string.Empty);
        }

        var strData = "   <div class='header-dropdown'>";
        strData += loginResponse.LanguageName;
        strData += " <div class='header-menu'><ul>";
        foreach (var item in languageResponse.Data)
        {
            strData += " <li><a href='#' data-id='" + item.stCode + "' data-name='" + item.stName
                + "' data-culture='" + item.stCulture + "' onclick='setLang_Click(this)' class='langLink'>"
                + item.stName + "</a></li>";
        }
        strData += "  </ul></div></div>";

        var returnData = new LanguageReturnModel { SetLangContent = strData };
        cacheHelper.SetCache(key, returnData);
        return View(returnData);
    }
}

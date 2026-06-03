using MarbleWebProject.Models;
using MarbleWebProject.Services;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace MarbleWebProject.Helper;

public static class TranslateHelper
{
    public static async Task<string> TranslateAsync(IHtmlHelper htmlHelper, string key, CancellationToken cancellationToken = default)
    {
        var translateAllList = FilterParametersHelper.TranslateFullList;
        if (translateAllList.Count > 0)
        {
            var hasTranslate = translateAllList.FirstOrDefault(c =>
                c.Key.Equals(key, StringComparison.OrdinalIgnoreCase)
                && c.AgencyGroupID == AppConfig.CMSService.MarketCode.GetValueOrDefault()
                && c.RetLang == AppConfig.CMSService.LanguageCode);
            if (hasTranslate != null)
                return hasTranslate.Translation;
        }

        var services = htmlHelper.ViewContext.HttpContext.RequestServices;
        var auth = services.GetRequiredService<IStoreAuthService>();
        var content = services.GetRequiredService<IStoreContentApi>();
        var session = await auth.GetSessionAsync(cancellationToken);
        var translateResponse = await content.GetTranslateAsync(new TranslateRequest
        {
            Text = key,
            Language = session.LanguageCode
        }, cancellationToken);

        if (!translateResponse.Status)
            return key;

        FilterParametersHelper.TranslateFullList = translateResponse.Data.GetFullList;
        return translateResponse.Data.Text;
    }

    public static string Translate(this IHtmlHelper htmlHelper, string key)
    {
        return TranslateAsync(htmlHelper, key).GetAwaiter().GetResult();
    }
}

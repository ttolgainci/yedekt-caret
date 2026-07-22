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
        var lang = AppConfig.Storefront.StoreAuth.LanguageCode;
        if (string.IsNullOrWhiteSpace(lang))
            lang = "tr";

        if (translateAllList.Count > 0)
        {
            var hasTranslate = translateAllList.FirstOrDefault(c =>
                string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase)
                && string.Equals(c.RetLang, lang, StringComparison.OrdinalIgnoreCase));
            if (hasTranslate != null && !string.IsNullOrWhiteSpace(hasTranslate.Translation))
                return hasTranslate.Translation;
        }

        var services = htmlHelper.ViewContext.HttpContext.RequestServices;
        var auth = services.GetRequiredService<IStoreAuthService>();
        var content = services.GetRequiredService<IStoreContentApi>();
        var session = await auth.GetSessionAsync(cancellationToken);
        var translateResponse = await content.GetTranslateAsync(new TranslateRequest
        {
            Text = key,
            Language = string.IsNullOrWhiteSpace(session.LanguageCode) ? lang : session.LanguageCode
        }, cancellationToken);

        if (!translateResponse.Status || translateResponse.Data == null)
            return key;

        if (translateResponse.Data.GetFullList is { Count: > 0 } fullList)
            FilterParametersHelper.TranslateFullList = fullList;

        return string.IsNullOrWhiteSpace(translateResponse.Data.Text) ? key : translateResponse.Data.Text;
    }

    public static string Translate(this IHtmlHelper htmlHelper, string key)
    {
        return TranslateAsync(htmlHelper, key).GetAwaiter().GetResult();
    }
}

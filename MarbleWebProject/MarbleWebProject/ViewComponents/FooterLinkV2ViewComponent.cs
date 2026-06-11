using MarbleWebProject.Models;
using MarbleWebProject.Services;
using MarbleWebProject.Services.Api;
using MarbleWebProject.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class FooterLinkV2ViewComponent : ViewComponent
{
    private static readonly TimeSpan MemoryTtl = TimeSpan.FromMinutes(5);

    private readonly IStoreContentApi _content;
    private readonly IStoreAuthService _auth;
    private readonly IWebContentCache _webCache;
    private readonly IGlobalContentCacheVersion _version;
    private readonly IMemoryCache _memory;

    public FooterLinkV2ViewComponent(
        IStoreContentApi content,
        IStoreAuthService auth,
        IWebContentCache webCache,
        IGlobalContentCacheVersion version,
        IMemoryCache memory)
    {
        _content = content;
        _auth = auth;
        _webCache = webCache;
        _version = version;
        _memory = memory;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var tenant = AppConfig.ProjectName;
        var session = await _auth.GetSessionAsync(cancellationToken);
        var languageCode = string.IsNullOrWhiteSpace(session.LanguageCode) ? "tr" : session.LanguageCode;
        var cacheVersion = _version.GetCurrent(tenant);
        var memoryKey = $"FooterLinks:{tenant}:{languageCode}:{cacheVersion}";

        if (_memory.TryGetValue(memoryKey, out StorefrontFooterLinksModel? cached) && cached != null)
            return View("~/Views/Shared/Components/FooterLinkV1/Default.cshtml", cached);

        var links = await _webCache.TryGetFooterLinksAsync(tenant, languageCode, cacheVersion, cancellationToken);
        if (links == null)
        {
            try
            {
                var response = await _content.GetFooterLinksAsync(languageCode, cancellationToken);
                links = response.Status && response.Data != null
                    ? response.Data
                    : new StorefrontFooterLinksModel();
                await _webCache.SetFooterLinksAsync(tenant, languageCode, cacheVersion, links, cancellationToken);
            }
            catch
            {
                links = new StorefrontFooterLinksModel();
            }
        }

        _memory.Set(memoryKey, links, MemoryTtl);
        return View("~/Views/Shared/Components/FooterLinkV1/Default.cshtml", links);
    }
}

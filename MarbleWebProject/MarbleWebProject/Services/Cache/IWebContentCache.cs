using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Cache;

public interface IWebContentCache
{
    bool IsEnabled();
    Task<StorefrontFooterLinksModel?> TryGetFooterLinksAsync(string tenant, string languageCode, int version, CancellationToken cancellationToken = default);
    Task SetFooterLinksAsync(string tenant, string languageCode, int version, StorefrontFooterLinksModel data, CancellationToken cancellationToken = default);
    Task<StorefrontCheckoutLegalModel?> TryGetCheckoutLegalAsync(string tenant, string languageCode, int version, CancellationToken cancellationToken = default);
    Task SetCheckoutLegalAsync(string tenant, string languageCode, int version, StorefrontCheckoutLegalModel data, CancellationToken cancellationToken = default);
}

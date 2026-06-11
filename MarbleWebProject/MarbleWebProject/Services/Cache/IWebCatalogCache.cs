using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Cache;

public interface IWebCatalogCache
{
    bool IsEnabled();

    Task<List<CategoryListModel>?> TryGetHeaderCategoriesAsync(
        string tenant,
        string languageCode,
        int catalogVersion,
        CancellationToken cancellationToken = default);

    Task SetHeaderCategoriesAsync(
        string tenant,
        string languageCode,
        int catalogVersion,
        List<CategoryListModel> categories,
        CancellationToken cancellationToken = default);
}

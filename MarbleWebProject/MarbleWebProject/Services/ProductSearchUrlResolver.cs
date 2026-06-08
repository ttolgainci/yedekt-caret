using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.Services;

public sealed record VehicleSearchPath(
    int MakeId,
    int ModelId,
    int GenerationId,
    int EngineId,
    string MakeSlug,
    string ModelSlug,
    string GenerationSlug,
    string EngineSlug);

public interface IProductSearchUrlResolver
{
    Task<int?> ResolveCategoryIdAsync(string categorySlug, CancellationToken cancellationToken = default);
    Task<string?> ResolveCategorySlugAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<VehicleSearchPath?> BuildVehiclePathAsync(
        int makeId, int modelId, int generationId, int engineId,
        CancellationToken cancellationToken = default);
    Task<VehicleSearchPath?> ResolveVehicleByEngineIdAsync(int engineId, CancellationToken cancellationToken = default);
    Task<VehicleSearchPath?> ResolveVehiclePathAsync(
        string makeSlug, string modelSlug, string generationSlug, string engineSlug,
        CancellationToken cancellationToken = default);
}

public sealed class ProductSearchUrlResolver : IProductSearchUrlResolver
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;
    private readonly IMemoryCache _cache;

    public ProductSearchUrlResolver(IStoreCatalogApi catalog, IStoreAuthService auth, IMemoryCache cache)
    {
        _catalog = catalog;
        _auth = auth;
        _cache = cache;
    }

    public async Task<int?> ResolveCategoryIdAsync(string categorySlug, CancellationToken cancellationToken = default)
    {
        if (UrlSlugHelper.TryParseCategoryPath(categorySlug, out var categoryId, out _))
            return categoryId;

        var target = UrlSlugHelper.NormalizeSlug(categorySlug);
        if (string.IsNullOrEmpty(target))
            return null;

        var routes = await _catalog.GetCategoryRoutesAsync(cancellationToken);
        if (routes.Status && routes.Data != null)
        {
            foreach (var route in routes.Data)
            {
                if (UrlSlugHelper.Equals(route.Url, target))
                    return route.ID;
            }
        }

        var session = await _auth.GetSessionAsync(cancellationToken);
        var tree = await _catalog.GetCategoriesAsync(
            new CategoryRequest { languageCode = session.LanguageCode },
            cancellationToken);

        if (!tree.Status || tree.Data == null)
            return null;

        return FindCategoryIdInTree(tree.Data, target);
    }

    public async Task<string?> ResolveCategorySlugAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var routes = await _catalog.GetCategoryRoutesAsync(cancellationToken);
        if (routes.Status && routes.Data != null)
        {
            var route = routes.Data.FirstOrDefault(r => r.ID == categoryId);
            if (!string.IsNullOrWhiteSpace(route?.Url))
                return UrlSlugHelper.NormalizeSlug(route.Url);
        }

        var session = await _auth.GetSessionAsync(cancellationToken);
        var tree = await _catalog.GetCategoriesAsync(
            new CategoryRequest { languageCode = session.LanguageCode },
            cancellationToken);

        if (!tree.Status || tree.Data == null)
            return null;

        return FindCategorySlugInTree(tree.Data, categoryId);
    }

    public async Task<VehicleSearchPath?> BuildVehiclePathAsync(
        int makeId, int modelId, int generationId, int engineId,
        CancellationToken cancellationToken = default)
    {
        var makes = await _catalog.GetVehicleMakesAsync(cancellationToken);
        if (!makes.Status || makes.Data == null)
            return null;

        var make = makes.Data.FirstOrDefault(m => m.Id == makeId);
        if (make == null)
            return null;

        var models = await _catalog.GetVehicleModelsByMakeAsync(makeId, cancellationToken);
        if (!models.Status || models.Data == null)
            return null;

        var model = models.Data.FirstOrDefault(m => m.Id == modelId);
        if (model == null)
            return null;

        var generations = await _catalog.GetVehicleGenerationsByModelAsync(modelId, cancellationToken);
        if (!generations.Status || generations.Data == null)
            return null;

        var generation = generations.Data.FirstOrDefault(g => g.Id == generationId);
        if (generation == null)
            return null;

        var engines = await _catalog.GetVehicleEnginesByGenerationAsync(generationId, cancellationToken);
        if (!engines.Status || engines.Data == null)
            return null;

        var engine = engines.Data.FirstOrDefault(e => e.Id == engineId);
        if (engine == null)
            return null;

        return CreateVehiclePath(make, model, generation, engine);
    }

    public Task<VehicleSearchPath?> ResolveVehicleByEngineIdAsync(int engineId, CancellationToken cancellationToken = default)
    {
        if (engineId <= 0)
            return Task.FromResult<VehicleSearchPath?>(null);

        return _cache.GetOrCreateAsync($"vehicle-engine-path:{engineId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);
            return await FindVehicleByEngineIdAsync(engineId, cancellationToken);
        });
    }

    public async Task<VehicleSearchPath?> ResolveVehiclePathAsync(
        string makeSlug, string modelSlug, string generationSlug, string engineSlug,
        CancellationToken cancellationToken = default)
    {
        var makes = await _catalog.GetVehicleMakesAsync(cancellationToken);
        if (!makes.Status || makes.Data == null)
            return null;

        var make = makes.Data.FirstOrDefault(m =>
            UrlSlugHelper.Equals(m.Name, makeSlug) || UrlSlugHelper.Equals(m.Slug, makeSlug));
        if (make == null)
            return null;

        var models = await _catalog.GetVehicleModelsByMakeAsync(make.Id, cancellationToken);
        if (!models.Status || models.Data == null)
            return null;

        var model = models.Data.FirstOrDefault(m =>
            UrlSlugHelper.Equals(m.Name, modelSlug) || UrlSlugHelper.Equals(m.Slug, modelSlug));
        if (model == null)
            return null;

        var generations = await _catalog.GetVehicleGenerationsByModelAsync(model.Id, cancellationToken);
        if (!generations.Status || generations.Data == null)
            return null;

        var generation = generations.Data.FirstOrDefault(g =>
            UrlSlugHelper.Equals(g.Name, generationSlug)
            || UrlSlugHelper.Equals(g.Slug, generationSlug));
        if (generation == null)
            return null;

        var engines = await _catalog.GetVehicleEnginesByGenerationAsync(generation.Id, cancellationToken);
        if (!engines.Status || engines.Data == null)
            return null;

        var engine = engines.Data.FirstOrDefault(e =>
            UrlSlugHelper.Equals(e.EngineCode, engineSlug));
        if (engine == null)
            return null;

        return CreateVehiclePath(make, model, generation, engine);
    }

    private async Task<VehicleSearchPath?> FindVehicleByEngineIdAsync(int engineId, CancellationToken cancellationToken)
    {
        var makes = await _catalog.GetVehicleMakesAsync(cancellationToken);
        if (!makes.Status || makes.Data == null)
            return null;

        foreach (var make in makes.Data)
        {
            var models = await _catalog.GetVehicleModelsByMakeAsync(make.Id, cancellationToken);
            if (!models.Status || models.Data == null)
                continue;

            foreach (var model in models.Data)
            {
                var generations = await _catalog.GetVehicleGenerationsByModelAsync(model.Id, cancellationToken);
                if (!generations.Status || generations.Data == null)
                    continue;

                foreach (var generation in generations.Data)
                {
                    var engines = await _catalog.GetVehicleEnginesByGenerationAsync(generation.Id, cancellationToken);
                    if (!engines.Status || engines.Data == null)
                        continue;

                    var engine = engines.Data.FirstOrDefault(e => e.Id == engineId);
                    if (engine != null)
                        return CreateVehiclePath(make, model, generation, engine);
                }
            }
        }

        return null;
    }

    private static VehicleSearchPath CreateVehiclePath(
        VehicleMakeListItem make,
        VehicleModelListItem model,
        VehicleGenerationListItem generation,
        VehicleEngineListItem engine) =>
        new(
            make.Id,
            model.Id,
            generation.Id,
            engine.Id,
            UrlSlugHelper.NameSegment(make.Name),
            UrlSlugHelper.NameSegment(model.Name),
            UrlSlugHelper.GenerationSegment(generation.Name),
            UrlSlugHelper.EngineSegment(engine.EngineCode));

    private static int? FindCategoryIdInTree(IEnumerable<CategoryListModel> nodes, string target)
    {
        foreach (var node in nodes)
        {
            if (UrlSlugHelper.Equals(node.Url, target))
                return node.CategoryID > 0 ? node.CategoryID : node.ID;

            if (node.SubCat is { Count: > 0 })
            {
                var found = FindCategoryIdInTree(node.SubCat, target);
                if (found is > 0)
                    return found;
            }
        }

        return null;
    }

    private static string? FindCategorySlugInTree(IEnumerable<CategoryListModel> nodes, int categoryId)
    {
        foreach (var node in nodes)
        {
            var id = node.CategoryID > 0 ? node.CategoryID : node.ID;
            if (id == categoryId && !string.IsNullOrWhiteSpace(node.Url))
                return UrlSlugHelper.NormalizeSlug(node.Url);

            if (node.SubCat is { Count: > 0 })
            {
                var found = FindCategorySlugInTree(node.SubCat, categoryId);
                if (!string.IsNullOrEmpty(found))
                    return found;
            }
        }

        return null;
    }
}

using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

public class ProductResultController : Controller
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;
    private readonly IProductSearchUrlResolver _urlResolver;

    public ProductResultController(
        IStoreCatalogApi catalog,
        IStoreAuthService auth,
        IProductSearchUrlResolver urlResolver)
    {
        _catalog = catalog;
        _auth = auth;
        _urlResolver = urlResolver;
    }

    public Task<IActionResult> Category(
        string categorySlug,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default) =>
        RenderCategoryResultsAsync(
            categorySlug, brandId, minPrice, maxPrice, pageNumber, pageSize, cancellationToken);

    public Task<IActionResult> Vehicle(
        string vehiclePath,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default) =>
        RenderVehicleResultsByPathAsync(
            vehiclePath, categoryId, brandId, minPrice, maxPrice,
            pageNumber, pageSize, cancellationToken);

    public Task<IActionResult> VehicleLegacy(
        string makeSlug,
        string modelSlug,
        string generationSlug,
        string engineSlug,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber = 1,
        CancellationToken cancellationToken = default) =>
        RenderVehicleLegacyRedirectAsync(
            makeSlug, modelSlug, generationSlug, engineSlug,
            categoryId, brandId, minPrice, maxPrice, pageNumber, cancellationToken);

    public async Task<IActionResult> Index(
        int? vehicleEngineId,
        int? vehicleMakeId,
        int? vehicleModelId,
        int? vehicleGenerationId,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        if (vehicleEngineId is > 0
            && vehicleMakeId is > 0
            && vehicleModelId is > 0
            && vehicleGenerationId is > 0)
        {
            var path = await _urlResolver.BuildVehiclePathAsync(
                vehicleMakeId.Value,
                vehicleModelId.Value,
                vehicleGenerationId.Value,
                vehicleEngineId.Value,
                cancellationToken);

            if (path != null)
            {
                var redirectUrl = BuildVehicleRedirectUrl(path, categoryId, brandId, minPrice, maxPrice, pageNumber);
                return RedirectPermanent(redirectUrl);
            }
        }

        if (categoryId is > 0 && vehicleEngineId is not > 0)
        {
            var slug = await _urlResolver.ResolveCategorySlugAsync(categoryId.Value, cancellationToken);
            if (!string.IsNullOrWhiteSpace(slug))
            {
                var redirectUrl = UrlSlugHelper.BuildCategoryPath(slug, categoryId.Value);
                if (pageNumber > 1)
                    redirectUrl += $"?pageNumber={pageNumber}";
                return RedirectPermanent(redirectUrl);
            }
        }

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 48 ? 12 : pageSize;

        var model = new ProductResultSearchViewModel
        {
            Mode = ProductSearchMode.None,
            Title = "Arama Sonuçları",
            Summary = "Araç seçerek uyumlu ürünleri listeleyin veya kategorilerden ürünlere göz atın.",
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        TempData["Title"] = model.Title;
        TempData["Description"] = model.Summary;
        return View(model);
    }

    private async Task<IActionResult> RenderCategoryResultsAsync(
        string categorySlug,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 48 ? 12 : pageSize;

        int categoryId;
        if (UrlSlugHelper.TryParseCategoryPath(categorySlug, out var parsedId, out _))
        {
            categoryId = parsedId;
        }
        else
        {
            var resolvedId = await _urlResolver.ResolveCategoryIdAsync(categorySlug, cancellationToken);
            if (resolvedId is not > 0)
                return RedirectToAction("PageNotFound", "Error");

            var slug = UrlSlugHelper.NormalizeSlug(categorySlug);
            var canonical = UrlSlugHelper.BuildCategoryPath(slug, resolvedId.Value);
            return RedirectPermanent(AppendFilterQuery(canonical, null, brandId, minPrice, maxPrice, pageNumber));
        }

        var session = await _auth.GetSessionAsync(cancellationToken);
        var filterCategories = new List<VehicleSearchCategoryListItem>();
        var filterBrands = new List<VehicleSearchBrandListItem>();
        var products = new List<ProductList>();
        var totalCount = 0;
        var totalPages = 0;
        var currencySymbol = "₺";
        VehicleSearchPriceRangeModel? priceRange = null;

        var catFilterResponse = await _catalog.GetSubcategoriesByCategoryAsync(
            categoryId, session.LanguageCode, brandId, cancellationToken);
        if (catFilterResponse.Status && catFilterResponse.Data != null)
            filterCategories = catFilterResponse.Data;

        var brandFilterResponse = await _catalog.GetBrandsByCategoryAsync(
            categoryId, session.LanguageCode, brandId, cancellationToken);
        if (brandFilterResponse.Status && brandFilterResponse.Data != null)
            filterBrands = brandFilterResponse.Data;

        var productResponse = await _catalog.GetProductsByCategorySearchAsync(
            categoryId,
            session.LanguageCode,
            brandId,
            minPrice,
            maxPrice,
            pageNumber,
            pageSize,
            cancellationToken);

        if (productResponse.Status && productResponse.Data != null)
        {
            products = productResponse.Data.Products ?? new List<ProductList>();
            totalCount = productResponse.Data.TotalCount;
            totalPages = productResponse.Data.TotalPages;
            pageNumber = productResponse.Data.PageNumber;
            if (!string.IsNullOrWhiteSpace(productResponse.Data.CurrencySymbol))
                currencySymbol = productResponse.Data.CurrencySymbol.Trim();
        }

        var priceRangeResponse = await _catalog.GetPriceRangeByCategoryAsync(
            categoryId, session.LanguageCode, brandId, minPrice, maxPrice, cancellationToken);
        if (priceRangeResponse.Status && priceRangeResponse.Data != null && priceRangeResponse.Data.HasRange)
        {
            priceRange = priceRangeResponse.Data;
            if (!string.IsNullOrWhiteSpace(priceRange.CurrencySymbol))
                currencySymbol = priceRange.CurrencySymbol.Trim();
        }

        var title = "Kategori";
        string? metaDesc = null;
        string? metaKeyword = null;
        var categorySlugPart = UrlSlugHelper.NormalizeSlug(categorySlug);

        var metaResponse = await _catalog.GetProductsByCategoryAsync(new ProductsByCageoryRequest
        {
            ID = categoryId,
            LanguageCode = session.LanguageCode,
            pageNumber = 1,
            pageSize = 1
        }, cancellationToken);

        if (metaResponse.Status && metaResponse.Data != null)
        {
            title = string.IsNullOrWhiteSpace(metaResponse.Data.MetaTitle)
                ? metaResponse.Data.CategoryName
                : metaResponse.Data.MetaTitle;
            metaDesc = metaResponse.Data.MetaDesc;
            metaKeyword = metaResponse.Data.MetaKeyword;
            categorySlugPart = UrlSlugHelper.NormalizeSlug(metaResponse.Data.Url);
            if (string.IsNullOrEmpty(categorySlugPart))
                categorySlugPart = UrlSlugHelper.NormalizeSlug(categorySlug);
        }
        else
        {
            var resolvedSlug = await _urlResolver.ResolveCategorySlugAsync(categoryId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(resolvedSlug))
                categorySlugPart = resolvedSlug;
        }

        var canonicalPath = UrlSlugHelper.BuildCategoryPath(categorySlugPart, categoryId);
        if (!string.Equals(Request.Path.Value, canonicalPath, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectPermanent(AppendFilterQuery(canonicalPath, null, brandId, minPrice, maxPrice, pageNumber));
        }

        var model = new ProductResultSearchViewModel
        {
            Mode = ProductSearchMode.Category,
            Title = title,
            Summary = BuildCategorySummary(products.Count, totalCount),
            CategorySlug = categorySlugPart,
            CategoryId = categoryId,
            BrandId = brandId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            CurrencySymbol = currencySymbol,
            PriceRange = priceRange,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages > 0 ? totalPages : (pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0),
            Products = products,
            FilterCategories = filterCategories,
            FilterBrands = filterBrands,
        };

        TempData["Title"] = model.Title;
        TempData["Description"] = metaDesc ?? model.Summary ?? model.Title;
        TempData["Keywords"] = metaKeyword;

        return View("Index", model);
    }

    private async Task<IActionResult> RenderVehicleResultsByPathAsync(
        string vehiclePath,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (!UrlSlugHelper.TryParseVehiclePath(vehiclePath, out var engineId, out _))
            return RedirectToAction("PageNotFound", "Error");

        var vehicle = await _urlResolver.ResolveVehicleByEngineIdAsync(engineId, cancellationToken);
        if (vehicle == null)
            return RedirectToAction("PageNotFound", "Error");

        var canonicalPath = UrlSlugHelper.BuildVehicleSearchPath(
            vehicle.MakeSlug,
            vehicle.ModelSlug,
            vehicle.GenerationSlug,
            vehicle.EngineSlug,
            vehicle.EngineId);

        if (!string.Equals(Request.Path.Value, canonicalPath, StringComparison.OrdinalIgnoreCase))
        {
            var redirect = AppendFilterQuery(canonicalPath, categoryId, brandId, minPrice, maxPrice, pageNumber);
            return RedirectPermanent(redirect);
        }

        return await RenderVehicleResultsCoreAsync(
            vehicle, categoryId, brandId, minPrice, maxPrice, pageNumber, pageSize, cancellationToken);
    }

    private async Task<IActionResult> RenderVehicleLegacyRedirectAsync(
        string makeSlug,
        string modelSlug,
        string generationSlug,
        string engineSlug,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var vehicle = await _urlResolver.ResolveVehiclePathAsync(
            makeSlug, modelSlug, generationSlug, engineSlug, cancellationToken);
        if (vehicle == null)
            return RedirectToAction("PageNotFound", "Error");

        var redirectUrl = BuildVehicleRedirectUrl(vehicle, categoryId, brandId, minPrice, maxPrice, pageNumber);
        return RedirectPermanent(redirectUrl);
    }

    private async Task<IActionResult> RenderVehicleResultsCoreAsync(
        VehicleSearchPath vehicle,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 48 ? 12 : pageSize;

        var session = await _auth.GetSessionAsync(cancellationToken);
        var filterCategories = new List<VehicleSearchCategoryListItem>();
        var filterBrands = new List<VehicleSearchBrandListItem>();
        var products = new List<ProductList>();
        var totalCount = 0;
        var totalPages = 0;
        var currencySymbol = "₺";
        VehicleSearchPriceRangeModel? priceRange = null;

        var catResponse = await _catalog.GetCategoriesByVehicleEngineAsync(
            vehicle.EngineId, session.LanguageCode, brandId, cancellationToken);
        if (catResponse.Status && catResponse.Data != null)
            filterCategories = catResponse.Data;

        var brandResponse = await _catalog.GetBrandsByVehicleEngineAsync(
            vehicle.EngineId, session.LanguageCode, categoryId, cancellationToken);
        if (brandResponse.Status && brandResponse.Data != null)
            filterBrands = brandResponse.Data;

        var productResponse = await _catalog.GetProductsByVehicleEngineAsync(
            vehicle.EngineId,
            session.LanguageCode,
            categoryId,
            brandId,
            minPrice,
            maxPrice,
            pageNumber,
            pageSize,
            cancellationToken);

        if (productResponse.Status && productResponse.Data != null)
        {
            products = productResponse.Data.Products ?? new List<ProductList>();
            totalCount = productResponse.Data.TotalCount;
            totalPages = productResponse.Data.TotalPages;
            pageNumber = productResponse.Data.PageNumber;
            if (!string.IsNullOrWhiteSpace(productResponse.Data.CurrencySymbol))
                currencySymbol = productResponse.Data.CurrencySymbol.Trim();
        }

        var priceRangeResponse = await _catalog.GetPriceRangeByVehicleEngineAsync(
            vehicle.EngineId,
            session.LanguageCode,
            categoryId,
            brandId,
            minPrice,
            maxPrice,
            cancellationToken);

        if (priceRangeResponse.Status && priceRangeResponse.Data != null && priceRangeResponse.Data.HasRange)
        {
            priceRange = priceRangeResponse.Data;
            if (!string.IsNullOrWhiteSpace(priceRange.CurrencySymbol))
                currencySymbol = priceRange.CurrencySymbol.Trim();
        }

        var model = new ProductResultSearchViewModel
        {
            Mode = ProductSearchMode.Vehicle,
            Title = "Araç Arama Sonuçları",
            Summary = BuildVehicleSummary(filterCategories.Count, filterBrands.Count, totalCount),
            VehicleMakeId = vehicle.MakeId,
            VehicleModelId = vehicle.ModelId,
            VehicleGenerationId = vehicle.GenerationId,
            VehicleEngineId = vehicle.EngineId,
            VehicleMakeSlug = vehicle.MakeSlug,
            VehicleModelSlug = vehicle.ModelSlug,
            VehicleGenerationSlug = vehicle.GenerationSlug,
            VehicleEngineSlug = vehicle.EngineSlug,
            CategoryId = categoryId,
            BrandId = brandId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            CurrencySymbol = currencySymbol,
            PriceRange = priceRange,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages > 0 ? totalPages : (pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0),
            Products = products,
            FilterCategories = filterCategories,
            FilterBrands = filterBrands,
        };

        TempData["Title"] = model.Title;
        TempData["Description"] = model.Summary ?? model.Title;

        return View("Index", model);
    }

    private static string BuildVehicleRedirectUrl(
        VehicleSearchPath path,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber)
    {
        var url = UrlSlugHelper.BuildVehicleSearchPath(
            path.MakeSlug,
            path.ModelSlug,
            path.GenerationSlug,
            path.EngineSlug,
            path.EngineId);

        return AppendFilterQuery(url, categoryId, brandId, minPrice, maxPrice, pageNumber);
    }

    private static string AppendFilterQuery(
        string url,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber)
    {
        var parts = new List<string>();
        if (categoryId is > 0) parts.Add($"categoryId={categoryId}");
        if (brandId is > 0) parts.Add($"brandId={brandId}");
        if (minPrice.HasValue)
            parts.Add($"minPrice={minPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (maxPrice.HasValue)
            parts.Add($"maxPrice={maxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (pageNumber > 1) parts.Add($"pageNumber={pageNumber}");

        return parts.Count == 0 ? url : url + "?" + string.Join("&", parts);
    }

    private static string BuildCategorySummary(int showing, int total)
    {
        if (total <= 0)
            return "Bu kategoride ürün bulunamadı.";
        return $"{total} ürün";
    }

    private static string BuildVehicleSummary(int categoryCount, int brandCount, int productCount)
    {
        if (productCount > 0)
        {
            var parts = new List<string> { $"{productCount} ürün" };
            if (categoryCount > 0) parts.Add($"{categoryCount} kategori");
            if (brandCount > 0) parts.Add($"{brandCount} marka");
            return string.Join(" · ", parts);
        }

        if (categoryCount > 0 || brandCount > 0)
            return "Bu filtrede ürün bulunamadı.";

        return "Bu motor için ürün bulunamadı.";
    }
}

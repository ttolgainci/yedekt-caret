using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using MarbleWebProject.Services.Api;
using MarbleWebProject.Services.Storefront;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

public class ProductResultController : Controller
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;
    private readonly IProductSearchUrlResolver _urlResolver;
    private readonly IStorefrontRuntimeProvider _storefront;

    public ProductResultController(
        IStoreCatalogApi catalog,
        IStoreAuthService auth,
        IProductSearchUrlResolver urlResolver,
        IStorefrontRuntimeProvider storefront)
    {
        _catalog = catalog;
        _auth = auth;
        _urlResolver = urlResolver;
        _storefront = storefront;
    }

    public Task<IActionResult> Category(
        string categorySlug,
        string? b,
        string? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default) =>
        RenderCategoryResultsAsync(
            categorySlug,
            FilterIdListHelper.ParsePrefer(b, brandId),
            minPrice, maxPrice, pageNumber, pageSize, cancellationToken);

    public Task<IActionResult> Brand(
        string brandSlug,
        string? c,
        string? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default) =>
        RenderBrandResultsAsync(
            brandSlug,
            FilterIdListHelper.ParsePrefer(c, categoryId),
            minPrice, maxPrice, pageNumber, pageSize, cancellationToken);

    public Task<IActionResult> Vehicle(
        string vehiclePath,
        string? c,
        string? b,
        string? categoryId,
        string? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default) =>
        RenderVehicleResultsByPathAsync(
            vehiclePath,
            FilterIdListHelper.ParsePrefer(c, categoryId),
            FilterIdListHelper.ParsePrefer(b, brandId),
            minPrice, maxPrice,
            pageNumber, pageSize, cancellationToken);

    public Task<IActionResult> VehicleLegacy(
        string makeSlug,
        string modelSlug,
        string generationSlug,
        string engineSlug,
        string? c,
        string? b,
        string? categoryId,
        string? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber = 1,
        CancellationToken cancellationToken = default) =>
        RenderVehicleLegacyRedirectAsync(
            makeSlug, modelSlug, generationSlug, engineSlug,
            FilterIdListHelper.ParsePrefer(c, categoryId),
            FilterIdListHelper.ParsePrefer(b, brandId),
            minPrice, maxPrice, pageNumber, cancellationToken);

    public async Task<IActionResult> Index(
        string? q,
        string? c,
        string? b,
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
        var query = (q ?? string.Empty).Trim();
        if (query.Length >= 2
            && vehicleEngineId is not > 0
            && categoryId is not > 0
            && brandId is not > 0)
        {
            return await RenderTextSearchResultsAsync(
                query,
                FilterIdListHelper.Parse(c),
                FilterIdListHelper.Parse(b),
                minPrice, maxPrice, pageNumber, pageSize, cancellationToken);
        }

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
                var redirectUrl = BuildVehicleRedirectUrl(
                    path,
                    categoryId is > 0 ? new[] { categoryId.Value } : Array.Empty<int>(),
                    brandId is > 0 ? new[] { brandId.Value } : Array.Empty<int>(),
                    minPrice, maxPrice, pageNumber);
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

        if (brandId is > 0 && vehicleEngineId is not > 0 && categoryId is not > 0)
        {
            var brand = await _catalog.GetBrandAsync(brandId.Value, (await _auth.GetSessionAsync(cancellationToken)).LanguageCode, cancellationToken);
            if (brand.Status && brand.Data != null)
            {
                var slug = !string.IsNullOrWhiteSpace(brand.Data.Slug)
                    ? brand.Data.Slug
                    : brand.Data.Name;
                var redirectUrl = AppendFilterQuery(
                    UrlSlugHelper.BuildBrandPath(slug, brandId.Value),
                    categoryIds: null,
                    brandIds: null,
                    minPrice, maxPrice, pageNumber);
                return RedirectPermanent(redirectUrl);
            }
        }

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 48 ? 12 : pageSize;

        var model = new ProductResultSearchViewModel
        {
            Mode = ProductSearchMode.None,
            Title = "Arama Sonuçları",
            Summary = "Araç seçerek uyumlu ürünleri listeleyin, kategorilerden ürünlere göz atın veya üst aramayı kullanın.",
            PageNumber = pageNumber,
            PageSize = pageSize,
            Query = string.IsNullOrWhiteSpace(query) ? null : query,
        };

        await ApplySearchResultLayoutAsync(model, cancellationToken);

        TempData["Title"] = model.Title;
        TempData["Description"] = model.Summary;
        return View(model);
    }

    private async Task<IActionResult> RenderTextSearchResultsAsync(
        string query,
        IReadOnlyList<int> categoryIds,
        IReadOnlyList<int> brandIds,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 48 ? 12 : pageSize;
        categoryIds = FilterIdListHelper.Normalize(categoryIds);
        brandIds = FilterIdListHelper.Normalize(brandIds);

        var session = await _auth.GetSessionAsync(cancellationToken);
        var products = new List<ProductList>();
        var filterCategories = new List<VehicleSearchCategoryListItem>();
        var filterBrands = new List<VehicleSearchBrandListItem>();
        VehicleSearchPriceRangeModel? priceRange = null;
        var totalCount = 0;
        var totalPages = 0;
        var currencySymbol = "₺";

        var catFilterResponse = await _catalog.GetCategoriesByTextSearchAsync(
            query, session.LanguageCode, brandIds, cancellationToken);
        if (catFilterResponse.Status && catFilterResponse.Data != null)
            filterCategories = catFilterResponse.Data;

        var brandFilterResponse = await _catalog.GetBrandsByTextSearchAsync(
            query, session.LanguageCode, categoryIds, cancellationToken);
        if (brandFilterResponse.Status && brandFilterResponse.Data != null)
            filterBrands = brandFilterResponse.Data;

        var productResponse = await _catalog.GetProductsByTextSearchAsync(
            query, session.LanguageCode, categoryIds, brandIds, minPrice, maxPrice, pageNumber, pageSize, cancellationToken);
        if (productResponse.Status && productResponse.Data != null)
        {
            products = productResponse.Data.Products ?? new List<ProductList>();
            totalCount = productResponse.Data.TotalCount;
            totalPages = productResponse.Data.TotalPages;
            pageNumber = productResponse.Data.PageNumber;
            if (!string.IsNullOrWhiteSpace(productResponse.Data.CurrencySymbol))
                currencySymbol = productResponse.Data.CurrencySymbol.Trim();
        }

        var priceRangeResponse = await _catalog.GetPriceRangeByTextSearchAsync(
            query, session.LanguageCode, categoryIds, brandIds, minPrice, maxPrice, cancellationToken);
        if (priceRangeResponse.Status && priceRangeResponse.Data != null && priceRangeResponse.Data.HasRange)
        {
            priceRange = priceRangeResponse.Data;
            if (!string.IsNullOrWhiteSpace(priceRange.CurrencySymbol))
                currencySymbol = priceRange.CurrencySymbol.Trim();
        }

        var model = new ProductResultSearchViewModel
        {
            Mode = ProductSearchMode.Text,
            Title = $"\"{query}\" arama sonuçları",
            Summary = totalCount > 0
                ? $"{totalCount} ürün bulundu."
                : "Aramanızla eşleşen ürün bulunamadı.",
            Query = query,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            SelectedCategoryIds = categoryIds,
            SelectedBrandIds = brandIds,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            CurrencySymbol = currencySymbol,
            PriceRange = priceRange,
            Products = products,
            FilterCategories = filterCategories,
            FilterBrands = filterBrands,
        };

        await ApplySearchResultLayoutAsync(model, cancellationToken);

        TempData["Title"] = model.Title;
        TempData["Description"] = model.Summary;
        return View("Index", model);
    }

    private async Task<IActionResult> RenderCategoryResultsAsync(
        string categorySlug,
        IReadOnlyList<int> brandIds,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 48 ? 12 : pageSize;
        brandIds = FilterIdListHelper.Normalize(brandIds);

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
            return RedirectPermanent(AppendFilterQuery(canonical, null, brandIds, minPrice, maxPrice, pageNumber));
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
            categoryId, session.LanguageCode, brandId: null, cancellationToken);
        if (catFilterResponse.Status && catFilterResponse.Data != null)
            filterCategories = catFilterResponse.Data;

        var brandFilterResponse = await _catalog.GetBrandsByCategoryAsync(
            categoryId, session.LanguageCode, brandId: null, cancellationToken);
        if (brandFilterResponse.Status && brandFilterResponse.Data != null)
            filterBrands = brandFilterResponse.Data;

        var productResponse = await _catalog.GetProductsByCategorySearchAsync(
            categoryId,
            session.LanguageCode,
            brandIds,
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
            categoryId, session.LanguageCode, brandId: null, minPrice, maxPrice, cancellationToken);
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
            return RedirectPermanent(AppendFilterQuery(canonicalPath, null, brandIds, minPrice, maxPrice, pageNumber));
        }

        var categoryPicture = await ResolveCategoryPictureAsync(categoryId, session.LanguageCode, cancellationToken);

        var model = new ProductResultSearchViewModel
        {
            Mode = ProductSearchMode.Category,
            Title = title,
            Summary = BuildCategorySummary(products.Count, totalCount),
            CategorySlug = categorySlugPart,
            CategoryId = categoryId,
            CategoryPicture = categoryPicture,
            SelectedBrandIds = brandIds,
            BrandId = brandIds.Count == 1 ? brandIds[0] : null,
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

        await ApplySearchResultLayoutAsync(model, cancellationToken);

        TempData["Title"] = model.Title;
        TempData["Description"] = metaDesc ?? model.Summary ?? model.Title;
        TempData["Keywords"] = metaKeyword;

        return View("Index", model);
    }

    private async Task<IActionResult> RenderBrandResultsAsync(
        string brandSlug,
        IReadOnlyList<int> categoryIds,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 48 ? 12 : pageSize;
        categoryIds = FilterIdListHelper.Normalize(categoryIds);

        if (!UrlSlugHelper.TryParseBrandPath(brandSlug, out var brandId, out _))
            return RedirectToAction("PageNotFound", "Error");

        var session = await _auth.GetSessionAsync(cancellationToken);
        var brandResponse = await _catalog.GetBrandAsync(brandId, session.LanguageCode, cancellationToken);
        if (!brandResponse.Status || brandResponse.Data == null)
            return RedirectToAction("PageNotFound", "Error");

        var brand = brandResponse.Data;
        var brandSlugPart = UrlSlugHelper.NormalizeSlug(
            !string.IsNullOrWhiteSpace(brand.Slug) ? brand.Slug : brand.Name);
        if (string.IsNullOrEmpty(brandSlugPart))
            brandSlugPart = UrlSlugHelper.NormalizeSlug(brandSlug);

        var canonicalPath = UrlSlugHelper.BuildBrandPath(brandSlugPart, brandId);
        if (!string.Equals(Request.Path.Value, canonicalPath, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectPermanent(AppendFilterQuery(canonicalPath, categoryIds, null, minPrice, maxPrice, pageNumber));
        }

        var filterCategories = new List<VehicleSearchCategoryListItem>();
        var filterBrands = new List<VehicleSearchBrandListItem>();
        var products = new List<ProductList>();
        var totalCount = 0;
        var totalPages = 0;
        var currencySymbol = "₺";
        VehicleSearchPriceRangeModel? priceRange = null;

        var catFilterResponse = await _catalog.GetCategoriesByBrandAsync(
            brandId, session.LanguageCode, categoryId: null, cancellationToken);
        if (catFilterResponse.Status && catFilterResponse.Data != null)
            filterCategories = catFilterResponse.Data;

        var productResponse = await _catalog.GetProductsByBrandAsync(
            brandId,
            session.LanguageCode,
            categoryIds,
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

        var priceRangeResponse = await _catalog.GetPriceRangeByBrandAsync(
            brandId, session.LanguageCode, categoryId: null, minPrice, maxPrice, cancellationToken);
        if (priceRangeResponse.Status && priceRangeResponse.Data != null && priceRangeResponse.Data.HasRange)
        {
            priceRange = priceRangeResponse.Data;
            if (!string.IsNullOrWhiteSpace(priceRange.CurrencySymbol))
                currencySymbol = priceRange.CurrencySymbol.Trim();
        }

        var brandFacetCount = totalCount;
        if (categoryIds.Count > 0 || minPrice.HasValue || maxPrice.HasValue)
        {
            var brandTotalResponse = await _catalog.GetProductsByBrandAsync(
                brandId, session.LanguageCode, null, null, null, 1, 1, cancellationToken);
            if (brandTotalResponse.Status && brandTotalResponse.Data != null)
                brandFacetCount = brandTotalResponse.Data.TotalCount;
        }

        filterBrands =
        [
            new VehicleSearchBrandListItem
            {
                BrandId = brandId,
                Name = brand.Name,
                Url = canonicalPath,
                ProductCount = brandFacetCount
            }
        ];

        var model = new ProductResultSearchViewModel
        {
            Mode = ProductSearchMode.Brand,
            Title = brand.Name,
            Summary = BuildBrandSummary(totalCount),
            BrandSlug = brandSlugPart,
            BrandName = brand.Name,
            BrandPicture = brand.Picture,
            BrandId = brandId,
            SelectedCategoryIds = categoryIds,
            CategoryId = categoryIds.Count == 1 ? categoryIds[0] : null,
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

        await ApplySearchResultLayoutAsync(model, cancellationToken);

        TempData["Title"] = model.Title;
        TempData["Description"] = model.Summary ?? model.Title;

        return View("Index", model);
    }

    private async Task<IActionResult> RenderVehicleResultsByPathAsync(
        string vehiclePath,
        IReadOnlyList<int> categoryIds,
        IReadOnlyList<int> brandIds,
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
            var redirect = AppendFilterQuery(canonicalPath, categoryIds, brandIds, minPrice, maxPrice, pageNumber);
            return RedirectPermanent(redirect);
        }

        return await RenderVehicleResultsCoreAsync(
            vehicle, categoryIds, brandIds, minPrice, maxPrice, pageNumber, pageSize, cancellationToken);
    }

    private async Task<IActionResult> RenderVehicleLegacyRedirectAsync(
        string makeSlug,
        string modelSlug,
        string generationSlug,
        string engineSlug,
        IReadOnlyList<int> categoryIds,
        IReadOnlyList<int> brandIds,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var vehicle = await _urlResolver.ResolveVehiclePathAsync(
            makeSlug, modelSlug, generationSlug, engineSlug, cancellationToken);
        if (vehicle == null)
            return RedirectToAction("PageNotFound", "Error");

        var redirectUrl = BuildVehicleRedirectUrl(vehicle, categoryIds, brandIds, minPrice, maxPrice, pageNumber);
        return RedirectPermanent(redirectUrl);
    }

    private async Task<IActionResult> RenderVehicleResultsCoreAsync(
        VehicleSearchPath vehicle,
        IReadOnlyList<int> categoryIds,
        IReadOnlyList<int> brandIds,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 48 ? 12 : pageSize;
        categoryIds = FilterIdListHelper.Normalize(categoryIds);
        brandIds = FilterIdListHelper.Normalize(brandIds);

        var session = await _auth.GetSessionAsync(cancellationToken);
        var filterCategories = new List<VehicleSearchCategoryListItem>();
        var filterBrands = new List<VehicleSearchBrandListItem>();
        var products = new List<ProductList>();
        var totalCount = 0;
        var totalPages = 0;
        var currencySymbol = "₺";
        VehicleSearchPriceRangeModel? priceRange = null;

        var catResponse = await _catalog.GetCategoriesByVehicleEngineAsync(
            vehicle.EngineId, session.LanguageCode, brandIds, cancellationToken);
        if (catResponse.Status && catResponse.Data != null)
            filterCategories = catResponse.Data;

        var brandResponse = await _catalog.GetBrandsByVehicleEngineAsync(
            vehicle.EngineId, session.LanguageCode, categoryIds, cancellationToken);
        if (brandResponse.Status && brandResponse.Data != null)
            filterBrands = brandResponse.Data;

        var productResponse = await _catalog.GetProductsByVehicleEngineAsync(
            vehicle.EngineId,
            session.LanguageCode,
            categoryIds,
            brandIds,
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
            categoryIds: null,
            brandIds: null,
            minPrice,
            maxPrice,
            cancellationToken);

        if (priceRangeResponse.Status && priceRangeResponse.Data != null && priceRangeResponse.Data.HasRange)
        {
            priceRange = priceRangeResponse.Data;
            if (!string.IsNullOrWhiteSpace(priceRange.CurrencySymbol))
                currencySymbol = priceRange.CurrencySymbol.Trim();
        }

        var vehicleMakes = new List<VehicleMakeListItem>();
        var makesResponse = await _catalog.GetVehicleMakesAsync(cancellationToken);
        if (makesResponse.Status && makesResponse.Data != null)
            vehicleMakes = makesResponse.Data;

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
            VehicleMakeName = vehicle.MakeName,
            VehicleModelName = vehicle.ModelName,
            VehicleGenerationName = vehicle.GenerationName,
            VehicleEngineCode = vehicle.EngineCode,
            VehicleGenerationStartYear = vehicle.GenerationStartYear,
            VehicleGenerationEndYear = vehicle.GenerationEndYear,
            VehiclePowerHp = vehicle.PowerHp,
            VehicleFuelType = vehicle.FuelType,
            VehiclePicture = vehicle.MakePicture,
            VehicleMakes = vehicleMakes,
            SelectedCategoryIds = categoryIds,
            SelectedBrandIds = brandIds,
            CategoryId = categoryIds.Count == 1 ? categoryIds[0] : null,
            BrandId = brandIds.Count == 1 ? brandIds[0] : null,
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

        await ApplySearchResultLayoutAsync(model, cancellationToken);

        TempData["Title"] = model.Title;
        TempData["Description"] = model.Summary ?? model.Title;

        return View("Index", model);
    }

    private async Task ApplySearchResultLayoutAsync(ProductResultSearchViewModel model, CancellationToken cancellationToken)
    {
        var runtime = await _storefront.GetAsync(cancellationToken);
        model.SearchResultLayout = StorefrontSearchLayoutKeys.Normalize(runtime.SearchResultLayout);
    }

    private static string BuildVehicleRedirectUrl(
        VehicleSearchPath path,
        IReadOnlyList<int>? categoryIds,
        IReadOnlyList<int>? brandIds,
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

        return AppendFilterQuery(url, categoryIds, brandIds, minPrice, maxPrice, pageNumber);
    }

    private static string AppendFilterQuery(
        string url,
        IReadOnlyList<int>? categoryIds,
        IReadOnlyList<int>? brandIds,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber)
    {
        var parts = new List<string>();
        var catQ = FilterIdListHelper.ToQueryValue(categoryIds);
        var brandQ = FilterIdListHelper.ToQueryValue(brandIds);
        if (catQ != null) parts.Add($"{FilterIdListHelper.CategoryQueryKey}={catQ}");
        if (brandQ != null) parts.Add($"{FilterIdListHelper.BrandQueryKey}={brandQ}");
        if (minPrice.HasValue)
            parts.Add($"minPrice={minPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (maxPrice.HasValue)
            parts.Add($"maxPrice={maxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (pageNumber > 1) parts.Add($"pageNumber={pageNumber}");

        return parts.Count == 0 ? url : url + "?" + string.Join("&", parts);
    }

    private static string AppendFilterQuery(
        string url,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber)
    {
        var categoryIds = categoryId is > 0 ? new[] { categoryId.Value } : Array.Empty<int>();
        var brandIds = brandId is > 0 ? new[] { brandId.Value } : Array.Empty<int>();
        return AppendFilterQuery(url, categoryIds, brandIds, minPrice, maxPrice, pageNumber);
    }

    private async Task<string?> ResolveCategoryPictureAsync(
        int categoryId,
        string languageCode,
        CancellationToken cancellationToken)
    {
        var tree = await _catalog.GetCategoriesAsync(
            new CategoryRequest { languageCode = languageCode },
            cancellationToken);
        if (!tree.Status || tree.Data == null)
            return null;

        return FindCategoryPicture(tree.Data, categoryId);
    }

    private static string? FindCategoryPicture(IEnumerable<CategoryListModel> nodes, int categoryId)
    {
        foreach (var node in nodes)
        {
            var id = node.CategoryID > 0 ? node.CategoryID : node.ID;
            if (id == categoryId && !string.IsNullOrWhiteSpace(node.Picture))
                return node.Picture;

            if (node.SubCat is { Count: > 0 })
            {
                var found = FindCategoryPicture(node.SubCat, categoryId);
                if (!string.IsNullOrWhiteSpace(found))
                    return found;
            }
        }

        return null;
    }

    private static string BuildCategorySummary(int showing, int total)
    {
        if (total <= 0)
            return "Bu kategoride ürün bulunamadı.";
        return $"{total} ürün";
    }

    private static string BuildBrandSummary(int total)
    {
        if (total <= 0)
            return "Bu markada ürün bulunamadı.";
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

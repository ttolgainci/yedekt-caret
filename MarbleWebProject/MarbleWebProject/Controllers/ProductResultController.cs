using MarbleWebProject.Models;
using MarbleWebProject.Services;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

public class ProductResultController : Controller
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public ProductResultController(IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _catalog = catalog;
        _auth = auth;
    }

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

        if (vehicleEngineId is > 0)
        {
            var catResponse = await _catalog.GetCategoriesByVehicleEngineAsync(
                vehicleEngineId.Value, session.LanguageCode, brandId, cancellationToken);
            if (catResponse.Status && catResponse.Data != null)
                filterCategories = catResponse.Data;

            var brandResponse = await _catalog.GetBrandsByVehicleEngineAsync(
                vehicleEngineId.Value, session.LanguageCode, categoryId, cancellationToken);
            if (brandResponse.Status && brandResponse.Data != null)
                filterBrands = brandResponse.Data;

            var productResponse = await _catalog.GetProductsByVehicleEngineAsync(
                vehicleEngineId.Value,
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
                vehicleEngineId.Value,
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
        }

        var model = new ProductResultSearchViewModel
        {
            Title = vehicleEngineId is > 0 ? "Araç Arama Sonuçları" : "Arama Sonuçları",
            Summary = BuildSummary(vehicleEngineId, filterCategories.Count, filterBrands.Count, totalCount),
            VehicleEngineId = vehicleEngineId,
            VehicleMakeId = vehicleMakeId,
            VehicleModelId = vehicleModelId,
            VehicleGenerationId = vehicleGenerationId,
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

        return View(model);
    }

    private static string BuildSummary(int? vehicleEngineId, int categoryCount, int brandCount, int productCount)
    {
        if (vehicleEngineId is not > 0)
            return "Araç seçerek uyumlu ürün kategorilerini listeleyin.";

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

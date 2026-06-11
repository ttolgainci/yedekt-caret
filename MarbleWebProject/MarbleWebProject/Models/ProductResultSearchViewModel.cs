using MarbleWebProject.Helpers;

namespace MarbleWebProject.Models;

/// <summary>Ürün arama sonuç sayfası (/category/...-c{id} ve /arac/...-v{id}).</summary>
public class ProductResultSearchViewModel
{
    public ProductSearchMode Mode { get; set; } = ProductSearchMode.None;
    public string Title { get; set; } = "Arama Sonuçları";
    public string? Summary { get; set; }

    public string? CategorySlug { get; set; }
    public string? CategoryPicture { get; set; }

    public string? VehicleMakeName { get; set; }
    public string? VehicleModelName { get; set; }
    public string? VehicleGenerationName { get; set; }
    public string? VehicleEngineCode { get; set; }
    public int? VehicleGenerationStartYear { get; set; }
    public int? VehicleGenerationEndYear { get; set; }
    public int? VehiclePowerHp { get; set; }
    public string? VehicleFuelType { get; set; }
    public string? VehiclePicture { get; set; }
    public IReadOnlyList<VehicleMakeListItem> VehicleMakes { get; set; } = Array.Empty<VehicleMakeListItem>();
    public string? VehicleMakeSlug { get; set; }
    public string? VehicleModelSlug { get; set; }
    public string? VehicleGenerationSlug { get; set; }
    public string? VehicleEngineSlug { get; set; }

    public int? VehicleMakeId { get; set; }
    public int? VehicleModelId { get; set; }
    public int? VehicleGenerationId { get; set; }
    public int? VehicleEngineId { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage => PageNumber;

    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string CurrencySymbol { get; set; } = "₺";
    public VehicleSearchPriceRangeModel? PriceRange { get; set; }
    public string SearchResultLayout { get; set; } = StorefrontSearchLayoutKeys.Default;
    public IReadOnlyList<ProductList> Products { get; set; } = Array.Empty<ProductList>();
    public IReadOnlyList<VehicleSearchCategoryListItem> FilterCategories { get; set; } = Array.Empty<VehicleSearchCategoryListItem>();
    public IReadOnlyList<VehicleSearchBrandListItem> FilterBrands { get; set; } = Array.Empty<VehicleSearchBrandListItem>();

    public string BasePath => Mode switch
    {
        ProductSearchMode.Category => UrlSlugHelper.BuildCategoryPath(CategorySlug, CategoryId ?? 0),
        ProductSearchMode.Vehicle => UrlSlugHelper.BuildVehicleSearchPath(
            VehicleMakeSlug,
            VehicleModelSlug,
            VehicleGenerationSlug,
            VehicleEngineSlug,
            VehicleEngineId ?? 0),
        _ => "/arac"
    };

    public string BuildAramaUrl(
        int? categoryId = null,
        int? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? pageNumber = null,
        bool includeCurrentCategory = true,
        bool includeCurrentBrand = true,
        bool includeCurrentPrice = true)
    {
        var parts = new List<string>();

        var cat = categoryId;
        if (cat == null && includeCurrentCategory)
            cat = CategoryId;
        if (cat is > 0 && Mode == ProductSearchMode.Vehicle)
            parts.Add($"categoryId={cat}");

        var brand = brandId;
        if (brand == null && includeCurrentBrand)
            brand = BrandId;
        if (brand is > 0 && Mode is ProductSearchMode.Vehicle or ProductSearchMode.Category)
            parts.Add($"brandId={brand}");

        var min = minPrice;
        if (min == null && includeCurrentPrice)
            min = MinPrice;
        var max = maxPrice;
        if (max == null && includeCurrentPrice)
            max = MaxPrice;
        if (min.HasValue && Mode is ProductSearchMode.Vehicle or ProductSearchMode.Category)
            parts.Add($"minPrice={min.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (max.HasValue && Mode is ProductSearchMode.Vehicle or ProductSearchMode.Category)
            parts.Add($"maxPrice={max.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        var page = pageNumber ?? PageNumber;
        if (page is > 1)
            parts.Add($"pageNumber={page}");

        var path = BasePath;
        return parts.Count == 0 ? path : path + "?" + string.Join("&", parts);
    }

    public string BuildCategoryToggleUrl(int categoryId) =>
        CategoryId == categoryId
            ? BuildAramaUrl(includeCurrentCategory: false, pageNumber: 1)
            : BuildAramaUrl(categoryId: categoryId, pageNumber: 1);

    public string BuildBrandToggleUrl(int brandId) =>
        BrandId == brandId
            ? BuildAramaUrl(includeCurrentBrand: false, pageNumber: 1)
            : BuildAramaUrl(brandId: brandId, pageNumber: 1);

    public string BuildAramaUrlWithoutPrice() =>
        BuildAramaUrl(pageNumber: 1, includeCurrentPrice: false);

    public string BuildPriceFilterUrl(decimal min, decimal max) =>
        BuildAramaUrl(minPrice: min, maxPrice: max, pageNumber: 1, includeCurrentPrice: false);

    public string FormatPrice(decimal? price) =>
        price.HasValue ? price.Value.ToString("N2") : string.Empty;

    public string FormatPriceWithCurrency(decimal? price, string? currencySymbol = null)
    {
        if (!price.HasValue)
            return string.Empty;
        var sym = string.IsNullOrWhiteSpace(currencySymbol) ? CurrencySymbol : currencySymbol;
        return $"{FormatPrice(price)} {sym}".Trim();
    }

    public string VehicleContextTitle
    {
        get
        {
            var parts = new[] { VehicleMakeName, VehicleModelName, VehicleEngineCode }
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p!.Trim())
                .ToList();
            if (parts.Count == 0)
                return Title;
            return string.Join(" ", parts) + " Uyumlu Yedek Parçalar";
        }
    }

    public string? VehicleYearRangeLabel
    {
        get
        {
            if (VehicleGenerationStartYear is > 0 && VehicleGenerationEndYear is > 0)
                return $"{VehicleGenerationStartYear} - {VehicleGenerationEndYear}";
            if (VehicleGenerationStartYear is > 0)
                return $"{VehicleGenerationStartYear}+";
            return null;
        }
    }
}

using MarbleWebProject.Helpers;

namespace MarbleWebProject.Models;

/// <summary>Ürün arama sonuç sayfası (/category/...-c{id}, /brand/...-b-{id} ve /arac/...-v{id}).</summary>
public class ProductResultSearchViewModel
{
    public ProductSearchMode Mode { get; set; } = ProductSearchMode.None;
    public string Title { get; set; } = "Arama Sonuçları";
    public string? Summary { get; set; }

    public string? CategorySlug { get; set; }
    public string? CategoryPicture { get; set; }

    public string? BrandSlug { get; set; }
    public string? BrandName { get; set; }
    public string? BrandPicture { get; set; }

    /// <summary>Serbest metin arama sorgusu (/arama?q=).</summary>
    public string? Query { get; set; }

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

    /// <summary>Kategori sayfası scope ID.</summary>
    public int? CategoryId { get; set; }
    /// <summary>Marka sayfası scope ID.</summary>
    public int? BrandId { get; set; }

    public IReadOnlyList<int> SelectedCategoryIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> SelectedBrandIds { get; set; } = Array.Empty<int>();

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
        ProductSearchMode.Brand => UrlSlugHelper.BuildBrandPath(BrandSlug, BrandId ?? 0),
        ProductSearchMode.Vehicle => UrlSlugHelper.BuildVehicleSearchPath(
            VehicleMakeSlug,
            VehicleModelSlug,
            VehicleGenerationSlug,
            VehicleEngineSlug,
            VehicleEngineId ?? 0),
        ProductSearchMode.Text => "/arama",
        _ => "/arac"
    };

    public bool IsCategorySelected(int categoryId) =>
        Mode switch
        {
            ProductSearchMode.Category => CategoryId == categoryId,
            ProductSearchMode.Vehicle or ProductSearchMode.Brand or ProductSearchMode.Text => SelectedCategoryIds.Contains(categoryId),
            _ => CategoryId == categoryId
        };

    public bool IsBrandSelected(int brandId) =>
        Mode switch
        {
            ProductSearchMode.Brand => BrandId == brandId,
            ProductSearchMode.Vehicle or ProductSearchMode.Category or ProductSearchMode.Text => SelectedBrandIds.Contains(brandId),
            _ => BrandId == brandId
        };

    public string BuildAramaUrl(
        IReadOnlyList<int>? categoryIds = null,
        IReadOnlyList<int>? brandIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? pageNumber = null,
        bool includeCurrentCategory = true,
        bool includeCurrentBrand = true,
        bool includeCurrentPrice = true)
    {
        var parts = new List<string>();

        if (Mode == ProductSearchMode.Vehicle)
        {
            var cats = categoryIds ?? (includeCurrentCategory ? SelectedCategoryIds : Array.Empty<int>());
            var brands = brandIds ?? (includeCurrentBrand ? SelectedBrandIds : Array.Empty<int>());
            AppendBc(parts, cats, brands);
        }
        else if (Mode == ProductSearchMode.Text)
        {
            if (!string.IsNullOrWhiteSpace(Query))
                parts.Add($"q={Uri.EscapeDataString(Query.Trim())}");
            var cats = categoryIds ?? (includeCurrentCategory ? SelectedCategoryIds : Array.Empty<int>());
            var brands = brandIds ?? (includeCurrentBrand ? SelectedBrandIds : Array.Empty<int>());
            AppendBc(parts, cats, brands);
        }
        else if (Mode == ProductSearchMode.Category)
        {
            // Category scope fixed; only brand multi-filter in query.
            var brands = brandIds ?? (includeCurrentBrand ? SelectedBrandIds : Array.Empty<int>());
            AppendBc(parts, categoryIds: null, brands);
        }
        else if (Mode == ProductSearchMode.Brand)
        {
            // Brand scope fixed; only category multi-filter in query.
            var cats = categoryIds ?? (includeCurrentCategory ? SelectedCategoryIds : Array.Empty<int>());
            AppendBc(parts, cats, brandIds: null);
        }

        var min = minPrice;
        if (min == null && includeCurrentPrice)
            min = MinPrice;
        var max = maxPrice;
        if (max == null && includeCurrentPrice)
            max = MaxPrice;
        if (min.HasValue && Mode is ProductSearchMode.Vehicle or ProductSearchMode.Category or ProductSearchMode.Brand or ProductSearchMode.Text)
            parts.Add($"minPrice={min.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (max.HasValue && Mode is ProductSearchMode.Vehicle or ProductSearchMode.Category or ProductSearchMode.Brand or ProductSearchMode.Text)
            parts.Add($"maxPrice={max.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        var page = pageNumber ?? PageNumber;
        if (page is > 1)
            parts.Add($"pageNumber={page}");

        var path = BasePath;
        return parts.Count == 0 ? path : path + "?" + string.Join("&", parts);
    }

    private static void AppendBc(List<string> parts, IReadOnlyList<int>? categoryIds, IReadOnlyList<int>? brandIds)
    {
        var catQ = FilterIdListHelper.ToQueryValue(categoryIds);
        var brandQ = FilterIdListHelper.ToQueryValue(brandIds);
        if (catQ != null) parts.Add($"{FilterIdListHelper.CategoryQueryKey}={catQ}");
        if (brandQ != null) parts.Add($"{FilterIdListHelper.BrandQueryKey}={brandQ}");
    }

    public string BuildCategoryToggleUrl(int categoryId)
    {
        if (Mode is ProductSearchMode.Vehicle or ProductSearchMode.Brand or ProductSearchMode.Text)
        {
            var next = FilterIdListHelper.Toggle(SelectedCategoryIds, categoryId);
            return BuildAramaUrl(
                categoryIds: next,
                brandIds: Mode is ProductSearchMode.Vehicle or ProductSearchMode.Text ? SelectedBrandIds : null,
                pageNumber: 1);
        }

        // Category mode: navigate via facet Url (handled in view).
        return BasePath;
    }

    public string BuildBrandToggleUrl(int brandId)
    {
        if (Mode is ProductSearchMode.Vehicle or ProductSearchMode.Category or ProductSearchMode.Text)
        {
            var next = FilterIdListHelper.Toggle(SelectedBrandIds, brandId);
            return BuildAramaUrl(
                categoryIds: Mode is ProductSearchMode.Vehicle or ProductSearchMode.Text ? SelectedCategoryIds : null,
                brandIds: next,
                pageNumber: 1);
        }

        return BasePath;
    }

    /// <summary>Marka sonuç sayfasında başka markaya geçiş (kategori/fiyat filtrelerini korur).</summary>
    public string BuildBrandSwitchUrl(string? brandPath)
    {
        if (string.IsNullOrWhiteSpace(brandPath) || brandPath == "#")
            return BuildAramaUrl(pageNumber: 1);

        var parts = new List<string>();
        var catQ = FilterIdListHelper.ToQueryValue(SelectedCategoryIds);
        if (catQ != null)
            parts.Add($"{FilterIdListHelper.CategoryQueryKey}={catQ}");
        if (MinPrice.HasValue)
            parts.Add($"minPrice={MinPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (MaxPrice.HasValue)
            parts.Add($"maxPrice={MaxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        return parts.Count == 0 ? brandPath : brandPath + "?" + string.Join("&", parts);
    }

    public string BuildAramaUrlWithoutPrice() =>
        BuildAramaUrl(pageNumber: 1, includeCurrentPrice: false);

    public string BuildPriceFilterUrl(decimal min, decimal max) =>
        BuildAramaUrl(minPrice: min, maxPrice: max, pageNumber: 1, includeCurrentPrice: false);

    public string FormatPrice(decimal? price) =>
        CurrencyDisplayHelper.FormatAmount(price, CurrencySymbol);

    public string FormatPriceWithCurrency(decimal? price, string? currencySymbol = null) =>
        CurrencyDisplayHelper.FormatAmount(price, currencySymbol ?? CurrencySymbol);

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

namespace MarbleWebProject.Models;

/// <summary>Ürün arama sonuç sayfası (/arama).</summary>
public class ProductResultSearchViewModel
{
    public string Title { get; set; } = "Arama Sonuçları";
    public string? Summary { get; set; }

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
    public IReadOnlyList<ProductList> Products { get; set; } = Array.Empty<ProductList>();
    public IReadOnlyList<VehicleSearchCategoryListItem> FilterCategories { get; set; } = Array.Empty<VehicleSearchCategoryListItem>();
    public IReadOnlyList<VehicleSearchBrandListItem> FilterBrands { get; set; } = Array.Empty<VehicleSearchBrandListItem>();

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
        if (VehicleMakeId is > 0) parts.Add($"vehicleMakeId={VehicleMakeId}");
        if (VehicleModelId is > 0) parts.Add($"vehicleModelId={VehicleModelId}");
        if (VehicleGenerationId is > 0) parts.Add($"vehicleGenerationId={VehicleGenerationId}");
        if (VehicleEngineId is > 0) parts.Add($"vehicleEngineId={VehicleEngineId}");

        var cat = categoryId;
        if (cat == null && includeCurrentCategory)
            cat = CategoryId;
        if (cat is > 0)
            parts.Add($"categoryId={cat}");

        var brand = brandId;
        if (brand == null && includeCurrentBrand)
            brand = BrandId;
        if (brand is > 0)
            parts.Add($"brandId={brand}");

        var min = minPrice;
        if (min == null && includeCurrentPrice)
            min = MinPrice;
        var max = maxPrice;
        if (max == null && includeCurrentPrice)
            max = MaxPrice;
        if (min.HasValue)
            parts.Add($"minPrice={min.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (max.HasValue)
            parts.Add($"maxPrice={max.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        var page = pageNumber ?? PageNumber;
        if (page is > 1)
            parts.Add($"pageNumber={page}");

        return parts.Count == 0 ? "/arama" : "/arama?" + string.Join("&", parts);
    }

    /// <summary>Seçili kategoriye tekrar tıklanırsa filtreyi kaldırır.</summary>
    public string BuildCategoryToggleUrl(int categoryId) =>
        CategoryId == categoryId
            ? BuildAramaUrl(includeCurrentCategory: false, pageNumber: 1)
            : BuildAramaUrl(categoryId: categoryId, pageNumber: 1);

    /// <summary>Seçili markaya tekrar tıklanırsa filtreyi kaldırır.</summary>
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
}

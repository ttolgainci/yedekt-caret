namespace MarbleWebProject.Models;

public class VehicleMakeListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Slug { get; set; }
    public string? Picture { get; set; }
    public bool Status { get; set; }
}

public class VehicleModelListItem
{
    public int Id { get; set; }
    public int VehicleMakeID { get; set; }
    public string Name { get; set; } = "";
    public string? Slug { get; set; }
    public bool Status { get; set; }
}

public class VehicleGenerationListItem
{
    public int Id { get; set; }
    public int VehicleModelID { get; set; }
    public string Name { get; set; } = "";
    public string? Slug { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public bool Status { get; set; }
}

public class VehicleEngineListItem
{
    public int Id { get; set; }
    public int VehicleGenerationID { get; set; }
    public string EngineCode { get; set; } = "";
    public string? FuelType { get; set; }
    public int? PowerHp { get; set; }
    public bool Status { get; set; }
}

public class VehicleSearchCategoryListItem
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string Url { get; set; } = "#";
    public int ProductCount { get; set; }
}

public class VehicleSearchBrandListItem
{
    public int BrandId { get; set; }
    public string Name { get; set; } = "";
    public string Url { get; set; } = "#";
    public int ProductCount { get; set; }
}

public class CatalogSuggestResponse
{
    public string Query { get; set; } = "";
    public List<CatalogSuggestItem> Brands { get; set; } = new();
    public List<CatalogSuggestItem> Categories { get; set; } = new();
    public List<CatalogSuggestItem> Products { get; set; } = new();
    public List<CatalogSuggestItem> Vehicles { get; set; } = new();
}

public class CatalogSuggestItem
{
    public string Type { get; set; } = "";
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public string? SubLabel { get; set; }
    public string Url { get; set; } = "#";
    public string? Picture { get; set; }
    public int Rank { get; set; }
}

public class VehicleSearchProductsResponse
{
    public List<ProductList> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string CurrencySymbol { get; set; } = "₺";
}

public class VehicleSearchPriceRangeModel
{
    public decimal RangeMin { get; set; }
    public decimal RangeMax { get; set; }
    public decimal? SelectedMin { get; set; }
    public decimal? SelectedMax { get; set; }
    public string CurrencySymbol { get; set; } = "₺";
    public bool HasRange => RangeMax > RangeMin;
}

public class MainBannerV2ViewModel
{
    public List<AllBannerResponse> Slides { get; set; } = new();
    public List<VehicleMakeListItem> VehicleMakes { get; set; } = new();
}

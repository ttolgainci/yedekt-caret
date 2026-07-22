namespace MarbleWebProject.Models;

/// <summary>CMS configuration/currencies — vitrin fiyat formatı.</summary>
public sealed class StoreCurrencyModel
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public string Symbol { get; set; } = "";
    /// <summary>true = sembol sağda (100€), false = solda (€100). API: position.</summary>
    public bool Position { get; set; }
    public bool SymbolOnRight => Position;
    public string? DisplayLocale { get; set; }
    public string? PriceFormat { get; set; }
    public int DisplayOrder { get; set; }
}

namespace MarbleWebProject.Helpers;

public static class StorefrontSearchLayoutKeys
{
    public const string Grid1 = "grid1";
    public const string Grid2 = "grid2";
    public const string Grid3 = "grid3";
    public const string Grid4 = "grid4";
    public const string Default = Grid4;

    public static string Normalize(string? value)
    {
        var key = (value ?? "").Trim().ToLowerInvariant();
        if (key == "list")
            return Grid1;

        return key switch
        {
            Grid1 or Grid2 or Grid3 or Grid4 => key,
            _ => Default
        };
    }
}

public static class StorefrontSearchLayoutHelper
{
    public static bool IsListLayout(string? layout) =>
        string.Equals(StorefrontSearchLayoutKeys.Normalize(layout), StorefrontSearchLayoutKeys.Grid1, StringComparison.Ordinal);

    public static string GetColumnClass(string? layout) => StorefrontSearchLayoutKeys.Normalize(layout) switch
    {
        StorefrontSearchLayoutKeys.Grid1 => "col-12",
        StorefrontSearchLayoutKeys.Grid2 => "col-6",
        StorefrontSearchLayoutKeys.Grid3 => "col-6 col-md-4 col-lg-4",
        _ => "col-6 col-md-4 col-lg-4 col-xl-3"
    };

    public static string GetProductClass(string? layout) =>
        IsListLayout(layout) ? "product product-list" : "product product-7 text-center";

    public static string GetContainerClass(string? layout) =>
        IsListLayout(layout) ? "products mb-3 product-result-list" : "products mb-3 product-result-grid";
}

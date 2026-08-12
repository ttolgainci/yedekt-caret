using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MarbleWebProject.Helpers;

public static class UrlSlugHelper
{
    private static readonly Regex NonAlphaNumeric = new(@"[^a-z0-9]+", RegexOptions.Compiled);
    private static readonly Regex CategoryIdSuffix = new(@"-c(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BrandIdSuffix = new(@"-b-(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex VehicleEngineIdSuffix = new(@"-v(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ProductIdSuffix = new(@"-p-(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public const string BrandlessProductSegment = "product";

    public static string NormalizeSlug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var text = value.Trim().ToLowerInvariant();
        text = text.Replace('ı', 'i').Replace('İ', 'i')
            .Replace('ş', 's').Replace('Ş', 's')
            .Replace('ğ', 'g').Replace('Ğ', 'g')
            .Replace('ü', 'u').Replace('Ü', 'u')
            .Replace('ö', 'o').Replace('Ö', 'o')
            .Replace('ç', 'c').Replace('Ç', 'c');

        text = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        text = sb.ToString().Normalize(NormalizationForm.FormC);
        text = NonAlphaNumeric.Replace(text, "-").Trim('-');
        return text;
    }

    public static string ToSlug(string? text) => NormalizeSlug(text);

    public static string NameSegment(string? name) => NormalizeSlug(name);

    public static string GenerationSegment(string? name) => NormalizeSlug(name);

    public static string EngineSegment(string? engineName) => NormalizeSlug(engineName);

    public static bool Equals(string? left, string? right) =>
        NormalizeSlug(left) == NormalizeSlug(right);

    public static string BuildCategoryPath(string? categorySlug, int categoryId)
    {
        var slug = NormalizeSlug(categorySlug);
        if (string.IsNullOrEmpty(slug) || categoryId <= 0)
            return "/category";

        return $"/category/{slug}-c{categoryId}";
    }

    public static bool TryParseCategoryPath(string? path, out int categoryId, out string slugPart)
    {
        categoryId = 0;
        slugPart = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var normalized = NormalizeSlug(path.Trim('/').Split('/').LastOrDefault());
        var match = CategoryIdSuffix.Match(normalized);
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out categoryId) || categoryId <= 0)
            return false;

        slugPart = normalized[..match.Index].TrimEnd('-');
        return true;
    }

    public static string BuildBrandPath(string? brandSlug, int brandId)
    {
        var slug = NormalizeSlug(brandSlug);
        if (string.IsNullOrEmpty(slug) || brandId <= 0)
            return "/brand";

        return $"/brand/{slug}-b-{brandId}";
    }

    public static bool TryParseBrandPath(string? path, out int brandId, out string slugPart)
    {
        brandId = 0;
        slugPart = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var normalized = NormalizeSlug(path.Trim('/').Split('/').LastOrDefault());
        var match = BrandIdSuffix.Match(normalized);
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out brandId) || brandId <= 0)
            return false;

        slugPart = normalized[..match.Index].TrimEnd('-');
        return true;
    }

    public static string BuildVehicleSearchPath(
        string? makeName,
        string? modelName,
        string? generationName,
        string? engineName,
        int engineId)
    {
        var make = NameSegment(makeName);
        var model = NameSegment(modelName);
        var generation = GenerationSegment(generationName);
        var engine = EngineSegment(engineName);
        if (string.IsNullOrEmpty(make) || string.IsNullOrEmpty(model)
            || string.IsNullOrEmpty(generation) || string.IsNullOrEmpty(engine) || engineId <= 0)
            return "/arac";

        var slug = string.Join("-", new[] { make, model, generation, engine }.Where(p => !string.IsNullOrEmpty(p)));
        return $"/arac/{slug}-v{engineId}";
    }

    public static bool TryParseVehiclePath(string? path, out int engineId, out string slugPart)
    {
        engineId = 0;
        slugPart = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var normalized = NormalizeSlug(path.Trim('/').Split('/').LastOrDefault());
        var match = VehicleEngineIdSuffix.Match(normalized);
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out engineId) || engineId <= 0)
            return false;

        slugPart = normalized[..match.Index].TrimEnd('-');
        return true;
    }

    public static string BuildProductPath(string? brandSlug, string? productSlug, int productId)
    {
        if (productId <= 0)
            return "/";

        var brand = NormalizeSlug(brandSlug);
        if (string.IsNullOrEmpty(brand))
            brand = BrandlessProductSegment;

        var slug = NormalizeSlug(productSlug);
        if (string.IsNullOrEmpty(slug))
            slug = productId.ToString();

        if (TryParseProductId(slug, out var existingId) && existingId == productId)
        {
            var m = ProductIdSuffix.Match(slug);
            if (m.Success)
                slug = slug[..m.Index].TrimEnd('-');
        }

        if (string.IsNullOrEmpty(slug))
            slug = productId.ToString();

        return $"/{brand}/{slug}-p-{productId}";
    }

    public static bool TryParseProductId(string? urlSegmentOrPath, out int productId)
    {
        productId = 0;
        if (string.IsNullOrWhiteSpace(urlSegmentOrPath))
            return false;

        var segment = urlSegmentOrPath.Trim().Trim('/');
        var slash = segment.LastIndexOf('/');
        if (slash >= 0)
            segment = segment[(slash + 1)..];

        var match = ProductIdSuffix.Match(segment);
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out productId) || productId <= 0)
            return false;

        return true;
    }

    public static string NormalizeRequestPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var value = path.Trim();
        if (!value.StartsWith('/'))
            value = "/" + value;
        while (value.Contains("//", StringComparison.Ordinal))
            value = value.Replace("//", "/", StringComparison.Ordinal);
        if (value.Length > 1)
            value = value.TrimEnd('/');
        return value.ToLowerInvariant();
    }

    public static bool PathsEqual(string? left, string? right) =>
        string.Equals(NormalizeRequestPath(left), NormalizeRequestPath(right), StringComparison.Ordinal);
}

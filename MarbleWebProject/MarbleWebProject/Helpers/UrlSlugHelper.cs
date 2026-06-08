using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MarbleWebProject.Helpers;

public static class UrlSlugHelper
{
    private static readonly Regex NonAlphaNumeric = new(@"[^a-z0-9]+", RegexOptions.Compiled);
    private static readonly Regex CategoryIdSuffix = new(@"-c(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex VehicleEngineIdSuffix = new(@"-v(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
}
